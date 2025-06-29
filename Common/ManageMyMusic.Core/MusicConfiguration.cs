using ManageMyMusic.Core.Configuration;
using Microsoft.Extensions.Options;

namespace ManageMyMusic.Core
{
    public class MusicConfiguration : IMusicConfiguration
    {
        public AppSettings AppSettings { get; }
        public ConnectionStrings ConnectionStrings { get; }

        public MusicConfiguration(IOptions<AppSettings> appSettingsOptions, IOptions<ConnectionStrings> connectionStringsOptions)
        {
            AppSettings = appSettingsOptions.Value;
            ConnectionStrings = connectionStringsOptions.Value;
        }
    }
}
