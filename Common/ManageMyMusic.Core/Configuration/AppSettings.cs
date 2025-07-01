namespace ManageMyMusic.Core.Configuration
{
    public class AppSettings
    {
        public string? SourceFolder { get; set; }
        public string? DestinationFolder { get; set; }
        public string? ExtensionZipFolder { get; set; }
        public IEnumerable<string> ExtensionMusicFiles { get; set; }
    }
}
