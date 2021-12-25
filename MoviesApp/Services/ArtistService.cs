using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Scaffolding;
using MoviesApp.Controllers;
using MoviesApp.Data;
using MoviesApp.Models;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public class ArtistService : IArtistService
    {
        private readonly MoviesContext _context;
        private readonly IMapper _mapper;

        public ArtistService(MoviesContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public ArtistDto GetArtist(int id)
        {
            return _mapper.Map<ArtistDto>(
                _context.Artists
                    .Include(e => e.MoviesArtists)
                    .ThenInclude(e => e.Movie)
                    .FirstOrDefault(a => a.Id == id)
            );
        }

        public IEnumerable<ArtistDto> GetAllArtists()
        {
            return _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDto>>(
                _context.Artists.Include(e => e.MoviesArtists).ToList()
            );
        }

        public ArtistDto UpdateArtist(ArtistDto artistDto)
        {
            if (artistDto.Id == null)
            {
                return null;
            }

            try
            {
                var artistToUpdate = _context.Artists
                    .Include(e => e.MoviesArtists)
                    .ThenInclude(e => e.Movie)
                    .FirstOrDefault(e => e.Id == artistDto.Id);
                UpdateMoviesArtists(artistDto.SelectOptions.ToArray(), artistToUpdate);
                if (artistToUpdate != null)
                {
                    artistToUpdate.FirstName = artistDto.FirstName;
                    artistToUpdate.LastName = artistDto.LastName;
                    artistToUpdate.Birthday = artistDto.Birthday;
                    _context.Update(artistToUpdate);
                    _context.SaveChanges();
                }

                return _mapper.Map<ArtistDto>(artistToUpdate);
            }
            catch (DbUpdateException)
            {
                if (!ArtistExists((int) artistDto.Id))
                {
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public ArtistDto AddArtist(ArtistDto artistDto)
        {
            var artist = _context.Artists.Add(_mapper.Map<Artist>(artistDto)).Entity;
            _context.SaveChanges();
            if (artistDto.SelectOptions != null)
            {
                foreach (var movie in artistDto.SelectOptions)
                {
                    var id = _mapper.Map<ArtistDto>(artist).Id;
                    if (id != null)
                    {
                        var movieToAdd = new MoviesArtist
                        {
                            ArtistId = (int) id,
                            MovieId = int.Parse(movie)
                        };
                        _context.MoviesArtists.Add(movieToAdd);
                    }
                }
            }

            _context.SaveChanges();
            return _mapper.Map<ArtistDto>(artist);
        }

        public ArtistDto DeleteArtist(int id)
        {
            var artist = _context.Artists.Find(id);
            if (artist == null)
            {
                return null;
            }

            var coommunications = _context.MoviesArtists.Where(ma => ma.ArtistId == id)
                .Select(ma => ma).ToList();
            foreach (var elem in coommunications)
            {
                _context.MoviesArtists.Remove(elem);
            }

            _context.Artists.Remove(artist);
            _context.SaveChanges();

            return _mapper.Map<ArtistDto>(artist);
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }

        #region API

        public IEnumerable<ArtistDtoApi> GetAllArtistApi()
        {
            return _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDtoApi>>(
                _context.Artists.Include(e => e.MoviesArtists).ToList()
            );
        }

        public ArtistDtoApi GetArtistApi(int id)
        {
            return _mapper.Map<ArtistDtoApi>(
                _context.Artists
                    .Include(e => e.MoviesArtists)
                    .FirstOrDefault(a => a.Id == id)
            );
        }

        public ArtistDtoApi AddArtistApi(ArtistDtoApi inputDtoApi)
        {
            var artist = _context.Artists.Add(_mapper.Map<Artist>(inputDtoApi)).Entity;
            _context.SaveChanges();
            if (inputDtoApi.MoviesArtistsIds != null)
            {
                foreach (var movie in inputDtoApi.MoviesArtistsIds)
                {
                    var id = _mapper.Map<ArtistDto>(artist).Id;
                    if (id != null)
                    {
                        var movieToAdd = new MoviesArtist
                        {
                            ArtistId = (int) id,
                            MovieId = movie
                        };
                        _context.MoviesArtists.Add(movieToAdd);
                    }
                }
            }

            _context.SaveChanges();
            return _mapper.Map<ArtistDtoApi>(artist);
        }

        public ArtistDtoApi UpdateArtistApi(ArtistDtoApi artistDto)
        {
            if (artistDto.Id == null)
            {
                return null;
            }

            try
            {
                var artistToUpdate = _context.Artists
                    .Include(e => e.MoviesArtists)
                    .ThenInclude(e => e.Movie)
                    .FirstOrDefault(e => e.Id == artistDto.Id);
                UpdateMoviesArtists(artistDto.MoviesArtistsIds.Select(e=>e.ToString()).ToArray(), artistToUpdate);
                if (artistToUpdate != null)
                {
                    artistToUpdate.FirstName = artistDto.FirstName;
                    artistToUpdate.LastName = artistDto.LastName;
                    artistToUpdate.Birthday = artistDto.Birthday;
                    _context.Artists.Update(artistToUpdate);
                    _context.SaveChanges();
                }

                return _mapper.Map<ArtistDtoApi>(artistToUpdate);
            }
            catch (DbUpdateException)
            {
                if (!ArtistExists((int) artistDto.Id))
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


        private void UpdateMoviesArtists(string[] selectedOptions, Artist artistToUpdate)
        {
            if (selectedOptions == null)
            {
                artistToUpdate.MoviesArtists = new List<MoviesArtist>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var artistOptionsHS = new HashSet<int>(artistToUpdate.MoviesArtists
                .Select(m => m.MovieId));
            foreach (var option in _context.Movies)
            {
                if (selectedOptionsHS.Contains(option.Id.ToString())) // чекбокс выделен
                {
                    if (!artistOptionsHS.Contains(option.Id)) // но не отображено в таблице многие-ко-многим
                    {
                        artistToUpdate.MoviesArtists.Add(new MoviesArtist
                        {
                            ArtistId = artistToUpdate.Id,
                            MovieId = option.Id
                        });
                    }
                }
                else
                {
                    // чекбокс не выделен
                    if (artistOptionsHS.Contains(option.Id)) // но в таблице многие-ко-многим такое отношение было
                    {
                        MoviesArtist movieToRemove = artistToUpdate.MoviesArtists
                            .SingleOrDefault(m => m.MovieId == option.Id);
                        _context.MoviesArtists.Remove(movieToRemove ?? throw new InvalidOperationException());
                    }
                }
            }
        }
        
        public List<OptionVMArtist> PopulateAssignedMovieData(InputArtistViewModel artist)
        {
            var allOptions = _context.Movies;
            var currentOptionIDs = new HashSet<int>(artist.MoviesArtists.Select(m => m.MovieId));
            var checkBoxes = new List<OptionVMArtist>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVMArtist
                {
                    Id = option.Id,
                    Name = option.Title,
                    Assigned = currentOptionIDs.Contains(option.Id)
                });
            }

            return checkBoxes;
        }
    }
}