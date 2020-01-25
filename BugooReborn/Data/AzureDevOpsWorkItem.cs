//----------------------------------------------------------------------------------------------
// <copyright file="AzureDevOpsWorkItem.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Data
{
    using Newtonsoft.Json;
    using System;

    public class AzureDevOpsWorkItem
    {
        public int Id { get; set; }

        public AzureDevOpsWorkItemFields Fields { get; set; }

        [JsonProperty("_links")]
        public AzureDevOpsWorkItemLinks Links { get; set; }

        public string Url { get; set; }
    }

    public class AzureDevOpsWorkItemFields
    {
        [JsonProperty("System.AreaPath")]
        public string AreaPath { get; set; }

        [JsonProperty("System.IterationPath")]
        public string IterationPath { get; set; }

        [JsonProperty("System.State")]
        public string State { get; set; }

        [JsonProperty("System.AssignedTo")]
        public AzureDevOpsPerson AssignedTo { get; set; }

        [JsonProperty("System.CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("System.CreatedBy")]
        public AzureDevOpsPerson CreatedBy { get; set; }

        [JsonProperty("System.ChangedDate")]
        public DateTime ChangedDate { get; set; }

        [JsonProperty("System.ChangedBy")]
        public AzureDevOpsPerson ChangedBy { get; set; }

        [JsonProperty("System.Title")]
        public string Title { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Severity")]
        public string Severity { get; set; }

        [JsonProperty("Microsoft.VSTS.Common.Priority")]
        public int Priority { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI.TeamTriage")]
        public string TeamTriage { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI.GEMOwner")]
        public AzureDevOpsPerson GEMOwner { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI.GPMOwner")]
        public AzureDevOpsPerson GPMOwner { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI.PMOwner")]
        public AzureDevOpsPerson PMOwner { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI.EMOwner")]
        public AzureDevOpsPerson EMOwner { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI-Copy.Team")]
        public string TeamName { get; set; }

        [JsonProperty("MicrosoftTeamsCMMI-Copy.BugAgeDate")]
        public DateTime BugAgeData { get; set; }
    }

    public class AzureDevOpsPerson
    {
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public AzureDevOpsPersonLinks _links { get; set; }
        public string Id { get; set; }
        public string Uniquename { get; set; }
        public string ImageUrl { get; set; }
        public string Descriptor { get; set; }
        public string TeamsUserId { get; set; }
    }

    public class AzureDevOpsPersonLinks
    {
        public Avatar Avatar { get; set; }
    }

    public class Avatar
    {
        public string Href { get; set; }
    }

    public class AzureDevOpsWorkItemLinks
    {
        public Html Html { get; set; }
    }

    public class Html
    {
        public string Href { get; set; }
    }

}
