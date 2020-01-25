//----------------------------------------------------------------------------------------------
// <copyright file="BugooConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Utils
{
    public class BugooConfig
    {
        public string ADOPAT { get; set; }
        public string BugJairQueryId { get; set; }
        public string BugJailQueryUrlPrefix { get; set; }
        public string[] ConversationWhitelist { get; set; }
        public string[] UserWhitelist { get; set; }
        public string BotContact { get; set; }
    }

}
