using System.Collections.Generic;
using MoviesApp.Models;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public interface IMovieService
    {
        MovieDto GetMovie(int id);
        IEnumerable<MovieDto> GetAllMovies();
        MovieDto UpdateMovie(MovieDto movieDto);
        MovieDto AddMovie(MovieDto movieDto);
        MovieDto DeleteMovie(int id);

        #region API

        IEnumerable<MovieDtoApi> GetAllMoviesApi();

        MovieDtoApi GetMovieApi(int id);

        MovieDtoApi AddMovieApi(MovieDtoApi inputDtoApi);
        
        MovieDtoApi UpdateMovieApi(MovieDtoApi movieDto);

        #endregion

        public List<OptionVModelMovie> PopulateAssignedMovieData(InputMovieViewModel movie);
    }
}