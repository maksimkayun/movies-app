using System.Linq;
using AutoMapper;
using MoviesApp.Models;

namespace MoviesApp.Services.Dto.AutoMapperProfiles
{
    public class ArtistDtoProfile : Profile
    {
        public ArtistDtoProfile()
        {
            CreateMap<Artist, ArtistDto>().ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists))
                .ForMember(e => e.SelectOptions,
                    opt => opt.MapFrom(m => 
                        m.MoviesArtists.Select(e=>e.MovieId).ToList()))
                .ReverseMap()
                .ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists));
        }
    }
}