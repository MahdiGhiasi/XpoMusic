namespace ModernSpotifyUWP.SpotifyApi.Model
{
    public class Track
    {
        public int duration_ms;
        public string id;
        public string name;
        public ArtistSimplified[] artists;
        public AlbumSimplified album;
    }
}