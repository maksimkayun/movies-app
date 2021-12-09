namespace MoviesApp.ViewModels
{
    public class DeleteMovieViewModel:InputMovieViewModel
    {
        public DeleteMovieViewModel() : base() {}
        public int? Id { get; set; }
    }
}