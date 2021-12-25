﻿using System;
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

        #region Controllers visual

        public MovieDto GetMovie(int id)
        {
            return _mapper.Map<MovieDto>(
                _context.Movies
                    .Include(e => e.MoviesArtists)
                    .ThenInclude(e => e.Artist)
                    .FirstOrDefault(m => m.Id == id)
            );
        }

        public IEnumerable<MovieDto> GetAllMovies()
        {
            return _mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDto>>(
                _context.Movies.Include(e => e.MoviesArtists).ToList()
            );
        }

        public MovieDto UpdateMovie(MovieDto movieDto)
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
                UpdateMoviesArtists(movieDto.SelectOptions.ToArray(), movieToUpdate);
                if (movieToUpdate != null)
                {
                    movieToUpdate.Title = movieDto.Title;
                    movieToUpdate.Genre = movieDto.Genre;
                    movieToUpdate.ReleaseDate = movieDto.ReleaseDate;
                    movieToUpdate.Price = movieDto.Price;
                    _context.Update(movieToUpdate);
                    _context.SaveChanges();
                }

                return _mapper.Map<MovieDto>(movieToUpdate);
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

        public MovieDto AddMovie(MovieDto movieDto)
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
                            ArtistId = int.Parse(artist),
                            MovieId = (int) id
                        };
                        _context.MoviesArtists.Add(artistToAdd);
                    }
                }
            }

            _context.SaveChanges();
            return _mapper.Map<MovieDto>(movie);
        }

        public MovieDto DeleteMovie(int id)
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

            return _mapper.Map<MovieDto>(movie);
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

        #endregion

        #region APIs

        public IEnumerable<MovieDtoApi> GetAllMoviesApi()
        {
            return _mapper.Map<IEnumerable<Movie>, IEnumerable<MovieDtoApi>>(
                _context.Movies.Include(e => e.MoviesArtists).ToList()
            );
        }

        public MovieDtoApi GetMovieApi(int id)
        {
            return _mapper.Map<MovieDtoApi>(
                _context.Movies
                    .Include(e => e.MoviesArtists)
                    .FirstOrDefault(m => m.Id == id)
            );
        }

        public MovieDtoApi AddMovieApi(MovieDtoApi inputDtoApi)
        {
            var movie = _context.Add((object) _mapper.Map<Movie>(inputDtoApi)).Entity;
            _context.SaveChanges();
            if (inputDtoApi.MoviesArtistsIds != null)
            {
                foreach (var artist in inputDtoApi.MoviesArtistsIds)
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
            return _mapper.Map<MovieDtoApi>(movie);
        }

        public MovieDtoApi UpdateMovieApi(MovieDtoApi movieDto)
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
                UpdateMoviesArtists(movieDto.MoviesArtistsIds.Select(e => e.ToString()).ToArray(), movieToUpdate);
                if (movieToUpdate != null)
                {
                    movieToUpdate.Title = movieDto.Title;
                    movieToUpdate.Genre = movieDto.Genre;
                    movieToUpdate.ReleaseDate = movieDto.ReleaseDate;
                    movieToUpdate.Price = movieDto.Price;
                    _context.Update(movieToUpdate);
                    _context.SaveChanges();
                }

                return _mapper.Map<MovieDtoApi>(movieToUpdate);
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

        #endregion

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