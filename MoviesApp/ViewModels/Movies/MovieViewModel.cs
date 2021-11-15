using System;

namespace MoviesApp.ViewModels
{
    public class MovieViewModel:InputMovieViewModel
    {
        public MovieViewModel() : base()
        {
        }

        public int Id { get; set; }
    }
}