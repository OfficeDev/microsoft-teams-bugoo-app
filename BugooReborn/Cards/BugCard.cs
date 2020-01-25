//----------------------------------------------------------------------------------------------
// <copyright file="BugCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------

namespace BugooReborn.Cards
{
    using BugooReborn.Data;
    using Mustache;
    using System.IO;

    public static class BugCard
    {
        public static string GetCard(AzureDevOpsWorkItem workItem)
        {
            var filePath = $"Cards\\{typeof(BugCard).Name}.mustache";
            var fileContentsWithMustachePlaceholders = File.ReadAllText(filePath);
            var compiler = new FormatCompiler();
            var generator = compiler.Compile(fileContentsWithMustachePlaceholders);
            var processedCard = generator.Render(BugViewModel.CreateFrom(workItem));
            return processedCard;
        }
    }
}
