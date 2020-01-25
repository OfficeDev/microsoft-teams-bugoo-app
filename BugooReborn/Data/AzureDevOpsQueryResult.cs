//----------------------------------------------------------------------------------------------
// <copyright file="AzureDevOpsQueryResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Data
{
    using Newtonsoft.Json;

    public class AzureDevOpsQueryResult
    {
        [JsonProperty("columns")]
        public AzureDevOpsQueryColumn[] QueryColumns { get; set; }

        [JsonProperty("workItems")]
        public AzureDevOpsWorkItemSummary[] WorkItems { get; set; }
    }

    public class AzureDevOpsQueryColumn
    {
        public string ReferenceName { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }
    }

    public class AzureDevOpsWorkItemSummary
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }

}
