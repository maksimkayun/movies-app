using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Controllers;
using MoviesApp.Data;
using MoviesApp.Models;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public class MovieService : IMovieService
    {
        private readonly MoviesContext _context;
        private readonly IMapper _mapper;

        public MovieService(MoviesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public MovieDto GetMovie(int id, bool apiFlag)
        {
            if (apiFlag)
            {
                var movie = _mapper.Map<MovieDto>(
                    _context.Movies
                        .Include(e=>e.MoviesArtists).FirstOrDefault(e => e.Id == id)
                );
                movie.MoviesArtists = new List<MoviesArtist>();
                return movie;
            }

            return _mapper.Map<MovieDto>(
                _context.Movies
                    .Include(e => e.MoviesArtists)
                    .ThenInclude(e => e.Artist)
                    .FirstOrDefault(m => m.Id == id)
            );
        }

        public IEnumerable<MovieDto> GetAllMovies(bool apiFlag)
        {
            if (apiFlag)
            {
                var movies = _mapper.Map<ICollection<MovieDto>>(
                    _context.Movies.AsNoTracking().Select(e=>e)
                        .Include(e=>e.MoviesArtists)
                );
                movies.ToList().ForEach(e=>
                {
                    e.MoviesArtists = new List<MoviesArtist>();
                });
                return movies;
            }

            return _mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDto>>(
                _context.Movies.Include(e => e.MoviesArtists).ToList()
            );
        }

        public MovieDto UpdateMovie(MovieDto movieDto, bool apiFlag)
        {
            if (movieDto.Id == null)
            {
                //упрощение для примера
                //лучше всего генерировать ошибки и обрабатывать их на уровне конроллера
                return null;
            }

            try
            {
                var movieToUpdate = _context.Movies
                    .Include(m => m.MoviesArtists)
                    .ThenInclude(a => a.Artist)
                    .FirstOrDefault(m => m.Id == movieDto.Id);
                UpdateMoviesArtists(movieDto.SelectOptions.Select(e => e.ToString()).ToArray(), movieToUpdate);
                if (movieToUpdate != null)
                {
                    movieToUpdate.Title = movieDto.Title;
                    movieToUpdate.Genre = movieDto.Genre;
                    movieToUpdate.ReleaseDate = movieDto.ReleaseDate;
                    movieToUpdate.Price = movieDto.Price;
                    _context.Update(movieToUpdate);
                    _context.SaveChanges();
                }
                
                var returnResult = _mapper.Map<MovieDto>(movieToUpdate);
                if (apiFlag)
                {
                    returnResult.MoviesArtists = new List<MoviesArtist>();
                }

                return returnResult;
            }
            catch (DbUpdateException)
            {
                if (!MovieExists((int) movieDto.Id))
                {
                    //упрощение для примера
                    //лучше всего генерировать ошибки и обрабатывать их на уровне конроллера
                    return null;
                }
                else
                {
                    //упрощение для примера
                    //лучше всего генерировать ошибки и обрабатывать их на уровне конроллера
                    return null;
                }
            }
        }

        public MovieDto AddMovie(MovieDto movieDto, bool apiFlag)
        {
            var movie = _context.Add((object) _mapper.Map<Movie>(movieDto)).Entity;
            _context.SaveChanges();
            if (movieDto.SelectOptions != null)
            {
                foreach (var artist in movieDto.SelectOptions)
                {
                    var id = _mapper.Map<MovieDto>(movie).Id;
                    if (id != null)
                    {
                        var artistToAdd = new MoviesArtist
                        {
                            ArtistId = artist,
                            MovieId = (int) id
                        };
                        _context.MoviesArtists.Add(artistToAdd);
                    }
                }
            }

            _context.SaveChanges();
            
            var returnResult = _mapper.Map<MovieDto>(movie);
            if (apiFlag)
            {
                returnResult.MoviesArtists = new List<MoviesArtist>();
            }
            return returnResult;
        }

        public MovieDto DeleteMovie(int id, bool apiFlag)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null)
            {
                //упрощение для примера
                //лучше всего генерировать ошибки и обрабатывать их на уровне конроллера
                return null;
            }

            var сommunications = _context.MoviesArtists.Where(ma => ma.MovieId == id)
                .Select(ma => ma).ToList();
            foreach (var elem in сommunications)
            {
                _context.MoviesArtists.Remove(elem);
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();
            
            var returnResult = _mapper.Map<MovieDto>(movie);
            if (apiFlag)
            {
                returnResult.MoviesArtists = new List<MoviesArtist>();
            }

            return returnResult;
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }

        private void UpdateMoviesArtists(string[] selectedOptions, Movie movieToUpdate)
        {
            if (selectedOptions == null)
            {
                movieToUpdate.MoviesArtists = new List<MoviesArtist>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var movieOptionsHS = new HashSet<int>(movieToUpdate.MoviesArtists
                .Select(m => m.ArtistId));
            foreach (var option in _context.Artists)
            {
                if (selectedOptionsHS.Contains(option.Id.ToString())) // чекбокс выделен
                {
                    if (!movieOptionsHS.Contains(option.Id)) // но не отображено в таблице многие-ко-многим
                    {
                        if (movieToUpdate.Id != null)
                            movieToUpdate.MoviesArtists.Add(new MoviesArtist
                            {
                                MovieId = (int) movieToUpdate.Id,
                                ArtistId = option.Id
                            });
                    }
                }
                else
                {
                    // чекбокс не выделен
                    if (movieOptionsHS.Contains(option.Id)) // но в таблице многие-ко-многим такое отношение было
                    {
                        MoviesArtist movieToRemove = movieToUpdate.MoviesArtists
                            .SingleOrDefault(m => m.ArtistId == option.Id);
                        _context.MoviesArtists.Remove(movieToRemove ?? throw new InvalidOperationException());
                    }
                }
            }
        }

        public List<OptionVModelMovie> PopulateAssignedMovieData(InputMovieViewModel movie)
        {
            var allOptions = _context.Artists;
            var currentOptionIDs = new HashSet<int>(movie.MoviesArtists.Select(m => m.ArtistId));
            var checkBoxes = new List<OptionVModelMovie>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVModelMovie
                {
                    Id = option.Id,
                    Name = option.FirstName + " " + option.LastName,
                    Assigned = currentOptionIDs.Contains(option.Id)
                });
            }

            return checkBoxes;
        }
    }
}