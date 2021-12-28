using System.Collections.Generic;
using MoviesApp.Models;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public interface IMovieService
    {
        MovieDto GetMovie(int id, bool apiFlag);
        IEnumerable<MovieDto> GetAllMovies(bool apiFlag);
        MovieDto UpdateMovie(MovieDto movieDto, bool apiFlag);
        MovieDto AddMovie(MovieDto movieDto, bool apiFlag);
        MovieDto DeleteMovie(int id, bool apiFlag);

        public List<OptionVModelMovie> PopulateAssignedMovieData(InputMovieViewModel movie);
    }
}