namespace ManageMyMusic.Models
{
    public class MusicStructureResponse
    {
        public int RegionId { get; set; }
        public string? RegionName { get; set; }

        public int TypeId { get; set; }
        public string? TypeName { get; set; }

        public int ArtirstId { get; set; }
        public string? ArtirstName { get; set; }

        public int AlbumId { get; set; }
        public string? AlbumName { get; set; }

        public string? RelativePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(RegionName)
                    && !string.IsNullOrWhiteSpace(TypeName)
                    && !string.IsNullOrWhiteSpace(ArtirstName)
                    && !string.IsNullOrWhiteSpace(AlbumName))
                {
                    return $"{RegionName}//{TypeName}//{ArtirstName}//{AlbumName}";
                }
                return string.Empty;
            }
        }
    }
}
