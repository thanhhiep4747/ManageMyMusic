using ManageMyMusic.Core;
using ManageMyMusic.Core.Extensions;
using ManageMyMusic.Enums;
using ManageMyMusic.Interfaces;
using ManageMyMusic.Models;
using Newtonsoft.Json;
using System.ComponentModel;

namespace ManageMyMusic
{
    public class MusicDataExcute : IMusicDataExcute
    {
        private readonly IMusicConfiguration m_configuration;
        private readonly char m_CharReplace = '-';
        public MusicDataExcute(IMusicConfiguration configuration)
        {
            m_configuration = configuration;
        }

        public string GetJsonFilePathByType(FileDataType type)
        {
            string jsonFileName = string.Empty;
            switch (type)
            {
                case FileDataType.Region:
                    jsonFileName = "MusicRegion.json";
                    break;
                case FileDataType.Type:
                    jsonFileName = "MusicType.json";
                    break;
                case FileDataType.Album:
                    jsonFileName = "MusicAlbum.json";
                    break;
                case FileDataType.Artirst:
                    jsonFileName = "MusicArtist.json";
                    break;
            }

            return Path.Combine(AppContext.BaseDirectory, $"Data/{jsonFileName}"); ;
        }

        public async Task<MusicDataModel> GetMusicDataModel()
        {
            MusicDataModel model = new MusicDataModel();

            model.Regions = await GetMusicRegionData();
            model.Types = await GetMusicTypeData();
            model.Artists = await GetMusicArtistData();
            model.Albums = await GetMusicAlbumData();

            return model;
        }

        public async Task<IList<MusicRegionModel>> GetMusicRegionData()
        {
            try
            {
                var jsonFilePath = GetJsonFilePathByType(FileDataType.Region);
                if (string.IsNullOrWhiteSpace(jsonFilePath))
                {
                    Console.WriteLine($"GetMusicRegionData: Can not find json file path.");
                    return new List<MusicRegionModel>();
                }

                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

                var response = JsonConvert.DeserializeObject<IList<MusicRegionModel>>(jsonContent);

                return response ?? new List<MusicRegionModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMusicRegionData: Exceoption Message: {ex.Message}.");
                return new List<MusicRegionModel>();
            }
        }

        public async Task<IList<MusicTypeModel>> GetMusicTypeData()
        {
            try
            {
                var jsonFilePath = GetJsonFilePathByType(FileDataType.Type);
                if (string.IsNullOrWhiteSpace(jsonFilePath))
                {
                    Console.WriteLine($"GetMusicTypeData: Can not find json file path.");
                    return new List<MusicTypeModel>();
                }

                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

                var response = JsonConvert.DeserializeObject<IList<MusicTypeModel>>(jsonContent);

                return response ?? new List<MusicTypeModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMusicTypeData: Exceoption Message: {ex.Message}.");
                return new List<MusicTypeModel>();
            }
        }

        public async Task<IList<MusicArtistModel>> GetMusicArtistData()
        {
            try
            {
                var jsonFilePath = GetJsonFilePathByType(FileDataType.Artirst);
                if (string.IsNullOrWhiteSpace(jsonFilePath))
                {
                    Console.WriteLine($"GetMusicArtistData: Can not find json file path.");
                    return new List<MusicArtistModel>();
                }

                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

                var response = JsonConvert.DeserializeObject<IList<MusicArtistModel>>(jsonContent);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMusicArtistData: Exceoption Message: {ex.Message}.");
                return new List<MusicArtistModel>();
            }
        }

        public async Task<IList<MusicAlbumModel>> GetMusicAlbumData()
        {
            try
            {
                var jsonFilePath = GetJsonFilePathByType(FileDataType.Album);
                if (string.IsNullOrWhiteSpace(jsonFilePath))
                {
                    Console.WriteLine($"GetMusicAlbumData: Can not find json file path.");
                    return new List<MusicAlbumModel>();
                }

                string jsonContent = await File.ReadAllTextAsync(jsonFilePath);

                var response = JsonConvert.DeserializeObject<IList<MusicAlbumModel>>(jsonContent);

                return response ?? new List<MusicAlbumModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetMusicAlbumData: Exceoption Message: {ex.Message}.");
                return new List<MusicAlbumModel>();
            }
        }

        public async Task<bool> SaveJsonData(string jsonData, FileDataType type)
        {
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                Console.WriteLine("SaveJsonData: JsonData is empty?");
                return false;
            }

            var jsonFilePath = GetJsonFilePathByType(type);
            if (string.IsNullOrWhiteSpace(jsonFilePath))
            {
                Console.WriteLine("SaveJsonData: jsonFilePath is null?");
                return false;
            }

            try
            {
                await File.WriteAllTextAsync(jsonFilePath, jsonData);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SaveJsonData: Exception Message: {ex.Message}");
                return false;
            }
        }

        public MusicStructureResponse MergeAlbumIntoStructure(string artistName, string albumName, MusicDataModel musicData)
        {
            var response = MergeArtistIntoStructure(artistName, musicData);
            if (response == null || string.IsNullOrWhiteSpace(response.RegionName)
                || string.IsNullOrWhiteSpace(response.TypeName) || string.IsNullOrWhiteSpace(response.ArtirstName))
                return response ?? new MusicStructureResponse();

            if (string.IsNullOrWhiteSpace(albumName))
            {
                Console.WriteLine($"MergeAlbumIntoStructure albumName is empty");
                return response;
            }

            if (string.IsNullOrWhiteSpace(artistName))
            {
                Console.WriteLine($"MergeAlbumIntoStructure artistName is empty");
                return response;
            }

            if (musicData == null)
            {
                Console.WriteLine($"MergeAlbumIntoStructure musicData is empty");
                return response;
            }

            var beautiAlbumName = ReplaceSpecialCharInName(albumName);

            foreach (var album in musicData.Albums)
            {
                if (string.IsNullOrWhiteSpace(album.AlbumName)) continue;

                var existing = CompareEqualStringIgnoreSpecialChar(albumName, album.AlbumName);
                if (existing)
                {
                    var beautiAlbumName_CountCharReplace = beautiAlbumName.Count(c => c == m_CharReplace);
                    var album_AlbumName_CountCharReplace = album.AlbumName.Count(c => c == m_CharReplace);

                    if (album_AlbumName_CountCharReplace > beautiAlbumName_CountCharReplace)
                        response.AlbumName = beautiAlbumName;
                    else
                        response.AlbumName = album.AlbumName;

                    response.AlbumId = album.Id;


                    break;
                }
            }

            if (response.AlbumId == 0 && string.IsNullOrWhiteSpace(response.AlbumName))
            {
                int maxAlbumId = 0;
                if (musicData.Albums.Any())
                {
                    maxAlbumId = musicData.Albums.Max(x => x.Id);
                }

                response.AlbumId = maxAlbumId + 1;
                response.AlbumName = beautiAlbumName;

                var album = new MusicAlbumModel();
                album.Id = maxAlbumId + 1;
                album.ArtistId = response.ArtirstId;
                album.AlbumName = beautiAlbumName;

                musicData.Albums.Add(album);
            }

            return response;
        }

        private MusicStructureResponse MergeArtistIntoStructure(string artistName, MusicDataModel musicData)
        {
            var response = new MusicStructureResponse();

            if (string.IsNullOrWhiteSpace(artistName))
            {
                Console.WriteLine($"VerifyArtistExistsInStructure artistName is empty");
                return response;
            }

            if (musicData == null)
            {
                Console.WriteLine($"VerifyArtistExistsInStructure musicData is empty");
                return response;
            }

            if (musicData.Artists == null || !musicData.Artists.Any())
            {
                Console.WriteLine($"VerifyArtistExistsInStructure Artirsts is empty");
                return response;
            }

            foreach (var artist in musicData.Artists)
            {
                if (string.IsNullOrWhiteSpace(artist.ArtistName)) continue;

                var existing = CompareEqualStringIgnoreSpecialChar(artistName, artist.ArtistName);
                if (existing)
                {
                    response.ArtirstId = artist.Id;
                    response.ArtirstName = artist.ArtistName;
                    var type = musicData.Types.FirstOrDefault(x => x.Id == artist.TypeId);
                    if (type != null)
                    {
                        response.TypeId = type.Id;
                        response.TypeName = type.TypeName;

                        var region = musicData.Regions.FirstOrDefault(x => x.Id == type.RegionId);
                        if (region != null)
                        {
                            response.RegionId = region.Id;
                            response.RegionName = region.RegionName;
                        }
                    }


                    break;
                }
            }

            return response;
        }

        private bool CompareEqualStringIgnoreSpecialChar(string sourceString, string compareString)
        {
            var sourceStrings = (sourceString.Trim()).RemoveDiacriticsVietnamese().ToLower().ToCharArray();
            var compareStrings = (compareString.Trim()).RemoveDiacriticsVietnamese().ToLower().ToCharArray();

            if (sourceStrings.Count() != compareStrings.Count())
                return false;

            for (int i = 0; i < compareStrings.Count(); i++)
            {
                var sourceChar = sourceStrings[i];
                var compareChar = compareStrings[i];

                if (!char.IsLetterOrDigit(sourceChar) || !char.IsLetterOrDigit(compareChar))
                    continue;

                if (sourceChar == compareChar) continue;
                else
                    return false;
            }

            return true;
        }

        private string ReplaceSpecialCharInName(string sourceString)
        {
            var sourceStrings = sourceString.ToCharArray();
            for (int i = 0; i < sourceStrings.Count(); i++)
            {
                var sourceChar = sourceStrings[i];

                if (!char.IsLetterOrDigit(sourceChar) && sourceChar != ' ')
                {
                    sourceStrings[i] = m_CharReplace;
                }
            }

            var result = string.Join("", sourceStrings);

            return result;
        }
    }
}
