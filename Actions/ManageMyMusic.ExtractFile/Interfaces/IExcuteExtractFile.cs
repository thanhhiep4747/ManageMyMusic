namespace ManageMyMusic.ExtractFile.Interfaces
{
    public interface IExcuteExtractFile
    {
        IEnumerable<string> GetAllZipFilesPath();
    }
}
