namespace ManageMyMusic.Models
{
    public class MusicDataModel
    {
        public IList<MusicRegionModel> Regions { get; set; } = new List<MusicRegionModel>();
        public IList<MusicTypeModel> Types { get; set; } = new List<MusicTypeModel>();
        public IList<MusicArtistModel> Artists { get; set; } = new List<MusicArtistModel>();
        public IList<MusicAlbumModel> Albums { get; set; } = new List<MusicAlbumModel>();
        public IList<MusicSong> Songs { get; set; } = new List<MusicSong>();
    }

    public class MusicRegionModel
    {
        public int Id { get; set; }
        public string? RegionName { get; set; }
    }

    public class MusicTypeModel
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string? TypeName { get; set; }
    }

    public class MusicArtistModel
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string? ArtistName { get; set; }
    }

    public class MusicAlbumModel
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }
        public string? AlbumName { get; set; }
    }

    public class MusicSong
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public string? SongName { get; set; }
        public string? SongPath { get; set; }
    }
}
