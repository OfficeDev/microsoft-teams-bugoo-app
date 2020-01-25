//----------------------------------------------------------------------------------------------
// <copyright file="AzureDevOpsDataProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Data
{
    using BugooReborn.Utils;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;

    public class AzureDevOpsDataProvider
    {
        public static async Task<List<AzureDevOpsWorkItem>> GetWorkItemsFromQuery(BugooConfig bugooConfig, string queryId)
        {
            AzureDevOpsQueryResult queryResult = null;
            var result = new List<AzureDevOpsWorkItem>();

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(
                        System.Text.ASCIIEncoding.ASCII.GetBytes(
                            string.Format("{0}:{1}", "", bugooConfig.ADOPAT))));

                // Run the query to get the links to individul work items (yeah, VSTS API is weird!)
                using (HttpResponseMessage response = await client.GetAsync(
                            $"https://domoreexp.visualstudio.com/MSTeams/Platform/_apis/wit/wiql/{queryId}?api-version=4.1"))
                {
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    queryResult = JsonConvert.DeserializeObject<AzureDevOpsQueryResult>(responseBody);
                }

                if (queryResult != null)
                {
                    // For each work item in the query results, fetch it
                    foreach (var w in queryResult.WorkItems)
                    {
                        using (var response = await client.GetAsync(w.Url))
                        {
                            response.EnsureSuccessStatusCode();
                            string responseBody = await response.Content.ReadAsStringAsync();
                            var workItemDetails = JsonConvert.DeserializeObject<AzureDevOpsWorkItem>(responseBody);
                            result.Add(workItemDetails);
                        }
                    }
                }
            }

            return result;
        }

    }
}
