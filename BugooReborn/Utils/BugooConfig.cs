namespace BugooReborn.Utils
{
    public class BugooConfig
    {
        public string ADOPAT { get; set; }
        public string BugJairQueryId { get; set; }
        public string BugJailQueryUrlPrefix { get; set; }
        public BugooConfigWhitelistedConversation[] ConversationWhitelist { get; set; }
        public string[] UserWhitelist { get; set; }
        public string BotContact { get; set; }
    }

    public class BugooConfigWhitelistedConversation
    {
        public string ConversationId { get; set; }
        public string DefaultTeam { get; set; }
    }

}
