using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Data;
using MoviesApp.Models;
using MoviesApp.Services.Dto;

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
                _context.Artists.Include(e=>e.MoviesArtists).ToList()
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
            throw new System.NotImplementedException();
        }

        public IEnumerable<ArtistDtoApi> GetAllArtistApi()
        {
            throw new System.NotImplementedException();
        }

        public ArtistDtoApi GetArtistApi(int id)
        {
            throw new System.NotImplementedException();
        }

        public ArtistDtoApi AddArtistApi(ArtistDtoApi inputDtoApi)
        {
            throw new System.NotImplementedException();
        }

        public ArtistDtoApi UpdateArtistApi(ArtistDtoApi artistDto)
        {
            throw new System.NotImplementedException();
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
        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}