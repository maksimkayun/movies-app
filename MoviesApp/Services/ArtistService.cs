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

        public ArtistDto GetArtist(int id, bool apiFlag)
        {
            if (apiFlag)
            {
                var artist = _mapper.Map<ArtistDto>(
                    _context.Artists.FirstOrDefault(e => e.Id == id)
                );
                artist.SelectOptions = _context.MoviesArtists.Where(e => e.ArtistId == id)
                    .Select(e => e.MovieId).ToList();
                return artist;
            }
            else
            {
                var artist = _mapper.Map<ArtistDto>(
                    _context.Artists
                        .Include(e => e.MoviesArtists)
                        .ThenInclude(e => e.Movie)
                        .FirstOrDefault(a => a.Id == id)
                );
                return artist;
            }
        }

        public IEnumerable<ArtistDto> GetAllArtists(bool apiFlag)
        {
            if (apiFlag)
            {
                IEnumerable<ArtistDto> artists = _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDto>>(
                    _context.Artists.AsNoTracking().ToList()
                ).ToList();
                foreach (var artist in artists)
                {
                    artist.SelectOptions = _context.MoviesArtists.Where(e => e.ArtistId == artist.Id)
                        .Select(e => e.MovieId).ToList();
                    artist.MoviesArtists = new List<MoviesArtist>();
                }

                return artists;
            }

            return _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistDto>>(
                _context.Artists.Include(e => e.MoviesArtists).ToList()
            );
        }

        public ArtistDto UpdateArtist(ArtistDto artistDto, bool apiFlag)
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
                UpdateMoviesArtists(artistDto.SelectOptions.Select(e => e.ToString()).ToArray(), artistToUpdate);
                if (artistToUpdate != null)
                {
                    artistToUpdate.FirstName = artistDto.FirstName;
                    artistToUpdate.LastName = artistDto.LastName;
                    artistToUpdate.Birthday = artistDto.Birthday;
                    _context.Update(artistToUpdate);
                    _context.SaveChanges();
                }

                if (apiFlag)
                {
                    var result = _mapper.Map<ArtistDto>(artistToUpdate);
                    result.SelectOptions = _context.MoviesArtists.Where(e => e.ArtistId == artistDto.Id)
                        .Select(e => e.MovieId).ToList();
                    result.MoviesArtists = new List<MoviesArtist>();
                    return result;
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

        public ArtistDto AddArtist(ArtistDto artistDto, bool apiFlag)
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
                            MovieId = movie
                        };
                        _context.MoviesArtists.Add(movieToAdd);
                    }
                }
            }

            _context.SaveChanges();
            var resultArtist = _mapper.Map<ArtistDto>(artist);
            if (apiFlag)
            {
                resultArtist.MoviesArtists = new List<MoviesArtist>();
                resultArtist.SelectOptions = _context.MoviesArtists.Where(e => e.ArtistId == resultArtist.Id)
                    .Select(e => e.MovieId).ToList();
            }
            return resultArtist;
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