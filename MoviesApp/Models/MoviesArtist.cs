namespace MoviesApp.Models
{
    public class MoviesArtist
    {
        public int MovieId { get; set; }
        public int ArtistId { get; set; }

        public virtual Movie Movie { get; set; }
        public virtual Artist Artist { get; set; }
    }
}