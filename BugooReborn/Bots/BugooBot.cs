//----------------------------------------------------------------------------------------------
// <copyright file="BugooBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Bots
{
    using BugooReborn.Cards;
    using BugooReborn.Data;
    using BugooReborn.Utils;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Microsoft.Practices.TransientFaultHandling;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class BugooBot : TeamsActivityHandler
    {
        #region Private fields

        private readonly IOptions<BugooConfig> config;

        public BugooBot(IOptions<BugooConfig> config)
        {
            this.config = config;
        }

        #endregion

        #region Protected overrides

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Check whitelist and return if user not in whitelist
            if (!CheckWhitelist(turnContext, cancellationToken))
            {
                await turnContext.SendActivityAsync(CreateActivityWithTextAndSpeak($"You are not authorized to perform this operation. Please contact {this.config.Value.BotContact}."), cancellationToken);
                return;
            }

            // Supported commands:
            // <nag> <teamname>

            // Get rid of the mention
            turnContext.Activity.RemoveRecipientMention();
            var message = turnContext.Activity.Text;
            var parts = message.Split(' ');

            switch (parts[0].ToLowerInvariant())
            {
                case "nag":
                    var teamName = string.Join(' ', parts.Skip(1)); // Some team names can have spaces in them
                    if (string.IsNullOrEmpty(teamName))
                    {
                        await turnContext.SendActivityAsync(CreateActivityWithTextAndSpeak($"Please provide a team-name."), cancellationToken);
                    }
                    else
                    {
                        await NagTeamName(teamName, turnContext, cancellationToken);
                    }

                    break;
                default:
                    await turnContext.SendActivityAsync(CreateActivityWithTextAndSpeak($"Sorry, my little 🤖 🧠 doesn't understand this yet! To nag a team about it's bug-jail bugs, use `@bugoo nag teamname`."), cancellationToken);
                    break;
            }
            
        }

        #endregion

        #region Private helpers

        private async Task NagTeamName(string teamName, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var bugJailQueryId = this.config.Value.BugJairQueryId;
            var queryUrlPrefix = this.config.Value.BugJailQueryUrlPrefix;

            // ACK that we are working on it since processing takes time.
            await turnContext.SendActivityAsync(MessageFactory.Text($"👍 Working on it! Checking the bugs for {teamName} team in [this query]({queryUrlPrefix + bugJailQueryId})..."), cancellationToken);

            // Get bugs from the bug-jail query
            var bugs = await AzureDevOpsDataProvider.GetWorkItemsFromQuery(this.config.Value, bugJailQueryId);
            var bugsForSpecifiedTeam = new List<AzureDevOpsWorkItem>();

            string replyText = null;

            var similarTeamNames = new HashSet<string>();

            foreach (var b in bugs)
            {
                try
                {
                    if (! string.IsNullOrEmpty(b.Fields.TeamName))
                    {
                        if (b.Fields.TeamName.Equals(teamName, StringComparison.InvariantCultureIgnoreCase))
                        {
                            bugsForSpecifiedTeam.Add(b);
                        }
                        else if (LevenshteinDistance.Compute(teamName.ToLowerInvariant(), b.Fields.TeamName.ToLowerInvariant()) <= 2)
                        {
                            similarTeamNames.Add(teamName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Bug parsing breaks when the underlying ADO schema changes
                    Trace.TraceError(ex.ToString());
                }
            }

            if (bugsForSpecifiedTeam.Count == 0)
            {
                // No matching bugs found so maybe the user did a typo in team-name. Show them suggestions
                if (similarTeamNames.Count > 0)
                {
                    replyText = $"Hmm, I couldn't find bugs in the exact team you specified. Did you mean to check for any of {string.Join(',', "`" + similarTeamNames.ToArray() + "`")}?";
                }
                else
                {
                    replyText = "Great news! It appears there are no bug jail bugs to nag about! 🎉";
                }
            }
            else
            {
                await HydrateTeamsData(bugsForSpecifiedTeam, turnContext, cancellationToken);

                foreach (var b in bugsForSpecifiedTeam)
                {
                    try
                    {
                        await CheckAndPostBug(b, turnContext, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError(ex.ToString());
                    }
                }

                replyText = $"Nagged for {bugsForSpecifiedTeam.Count} bugs upon your request 🙏.";
            }

            var replyActivity = MessageFactory.Text(replyText);

            replyActivity.ReplyToId = turnContext.Activity.Id;

            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
        }

        private async Task CheckAndPostBug(AzureDevOpsWorkItem workItem, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var bugAdaptiveCard = BugCard.GetCard(workItem);

            var exponentialBackoffRetryStrategy = new ExponentialBackoff(3, TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(1));


            // Define the Retry Policy
            var retryPolicy = new RetryPolicy(new BotSdkTransientExceptionDetectionStrategy(), exponentialBackoffRetryStrategy);

            var replyActivity = MessageFactory.Attachment(new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = JsonConvert.DeserializeObject(bugAdaptiveCard),
            });

            // Set summary text so make sure users have more useful information in the notifcation feed in Teams instead of just "Sent a card".
            replyActivity.Summary = GetActivitySummaryText(workItem);

            await retryPolicy.ExecuteAsync(() => turnContext.SendActivityAsync(replyActivity));
        }

        private async Task HydrateTeamsData(List<AzureDevOpsWorkItem> workItems, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // Get the roster one time in the current team and look up people inside it as needed
            var members = await TeamsInfo.GetMembersAsync(turnContext, cancellationToken);

            foreach (var wi in workItems)
            {
                // Plug the Team user-id for assigned-to
                if (wi.Fields.AssignedTo != null)
                {
                    wi.Fields.AssignedTo.TeamsUserId = members.Where(m =>
                        m.UserPrincipalName.Equals(wi.Fields.AssignedTo.Uniquename, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault()?.Id
                        ?? turnContext.Activity.From.Id;
                }

                // Plug the Team user-id for EM owner
                if (wi.Fields.EMOwner != null)
                {
                    wi.Fields.EMOwner.TeamsUserId = members.Where(m =>
                        m.UserPrincipalName.Equals(wi.Fields.EMOwner.Uniquename, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault()?.Id
                        ?? turnContext.Activity.From.Id;
                }

                // Plug the Team user-id for PM owner
                if (wi.Fields.PMOwner != null)
                {
                    wi.Fields.PMOwner.TeamsUserId = members.Where(m =>
                        m.UserPrincipalName.Equals(wi.Fields.PMOwner.Uniquename, StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault()?.Id
                        ?? turnContext.Activity.From.Id;
                }

            }

        }

        private bool CheckWhitelist(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var senderId = turnContext.Activity.From.AadObjectId;
            var conversationId = turnContext.Activity.Conversation.Id;

            var userWhitelist = this.config.Value.UserWhitelist;
            var conversationWhitelist = this.config.Value.ConversationWhitelist;

            return userWhitelist.Contains(senderId, StringComparer.OrdinalIgnoreCase) || conversationWhitelist.Contains(conversationId, StringComparer.OrdinalIgnoreCase);
        }

        private static string GetActivitySummaryText(AzureDevOpsWorkItem workItem)
        {
            return $"BUG {workItem.Id} - {workItem.Fields.Title}";
        }

        private IActivity CreateActivityWithTextAndSpeak(string message)
        {
            var activity = MessageFactory.Text(message);
            string speak = @"<speak version='1.0' xmlns='https://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
              <voice name='Microsoft Server Speech Text to Speech Voice (en-US, JessaRUS)'>" +
              $"{message}" + "</voice></speak>";
            activity.Speak = speak;
            return activity;
        }

        #endregion
    }
}
