using ManageMyMusic.Enums;
using ManageMyMusic.Models;

namespace ManageMyMusic.Interfaces
{
    public interface IMusicDataExcute
    {
        string GetJsonFilePathByType(FileDataType type);
        Task<bool> SaveJsonData(string jsonData, FileDataType type);

        Task<MusicDataModel> GetMusicDataModel();
        Task<IList<MusicRegionModel>> GetMusicRegionData();
        Task<IList<MusicTypeModel>> GetMusicTypeData();
        Task<IList<MusicAlbumModel>> GetMusicAlbumData();
        Task<IList<MusicArtistModel>> GetMusicArtistData();

        MusicStructureResponse MergeAlbumIntoStructure(string artistName, string albumName, MusicDataModel musicData);
    }
}
