namespace NiekoBoard.Server
{
    public class ServerConfig
    {
        public string AllowedHosts { get; set; } = string.Empty;
        public string AppPool { get; set; } = string.Empty;
        public string AppPoolServiceAccount { get; set; } = string.Empty;
        public bool AppPoolAccountIsGSMA { get; set; } = false;
        public string WebAppFolder { get; set; } = string.Empty;
        public int ListenPort { get; set; } = 5123;
        public string WebSite { get; set; } = string.Empty;
        public string WebAppUrl { get; set; } = string.Empty;
    }
}
