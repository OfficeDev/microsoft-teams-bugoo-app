//----------------------------------------------------------------------------------------------
// <copyright file="BugViewModel.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Cards
{
    using BugooReborn.Data;

    public class BugViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Priority { get; set; }
        public string State { get; set; }
        public string Severity { get; set; }
        public BuggablePerson AssignedTo { get; set; }
        public BuggablePerson EMOwner { get; set; }
        public BuggablePerson PMOwner { get; set; }
        public string BugLink { get; set; }

        public BugViewModel() { }

        public static BugViewModel CreateFrom(AzureDevOpsWorkItem workItem)
        {
            var result = new BugViewModel()
            {
                Id = workItem.Id,
                Title = workItem.Fields.Title,
                Priority = workItem.Fields.Priority,
                State = workItem.Fields.State,
                Severity = workItem.Fields.Severity,
                BugLink = workItem.Links.Html.Href
            };

            if (workItem.Fields.AssignedTo != null)
            {
                result.AssignedTo = new BuggablePerson()
                {
                    DisplayName = workItem.Fields.AssignedTo.DisplayName,
                    TeamsUserId = workItem.Fields.AssignedTo.TeamsUserId
                };
            }

            if (workItem.Fields.EMOwner != null)
            {
                result.EMOwner = new BuggablePerson()
                {
                    DisplayName = workItem.Fields.EMOwner.DisplayName,
                    TeamsUserId = workItem.Fields.EMOwner.TeamsUserId
                };
            }

            if (workItem.Fields.PMOwner != null)
            {
                result.PMOwner = new BuggablePerson()
                {
                    DisplayName = workItem.Fields.PMOwner.DisplayName,
                    TeamsUserId = workItem.Fields.PMOwner.TeamsUserId
                };
            }

            return result;
        }
    }

    public class BuggablePerson
    {
        public string TeamsUserId { get; set; }
        public string DisplayName { get; set; }
    }
}
