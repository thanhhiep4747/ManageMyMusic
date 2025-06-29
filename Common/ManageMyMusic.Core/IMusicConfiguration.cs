using ManageMyMusic.Core.Configuration;

namespace ManageMyMusic.Core
{
    public interface IMusicConfiguration
    {
        AppSettings AppSettings { get; }
        ConnectionStrings ConnectionStrings { get; }
    }
}
