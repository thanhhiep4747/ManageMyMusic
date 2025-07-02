namespace ManageMyMusic.Models
{
    public class MusicData
    {
        public IList<MusicRegion> Regions { get; set; } = new List<MusicRegion>();
        public IList<MusicType> Types { get; set; } = new List<MusicType>();
        public IList<MusicArtist> Artists { get; set; } = new List<MusicArtist>();
        public IList<MusicAlbum> Albums { get; set; } = new List<MusicAlbum>();
        public IList<MusicSong> Songs { get; set; } = new List<MusicSong>();
    }

    public class MusicRegion
    {
        public int Id { get; set; }
        public string? RegionName { get; set; }
    }

    public class MusicType
    {
        public int Id { get; set; }
        public int RegionId { get; set; }
        public string? TypeName { get; set; }
    }

    public class MusicArtist
    {
        public int Id { get; set; }
        public int TypeId { get; set; }
        public string? ArtistName { get; set; }
    }

    public class MusicAlbum
    {
        public int Id { get; set; }
        public int ArtistId { get; set; }
        public int AlbumName { get; set; }
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
