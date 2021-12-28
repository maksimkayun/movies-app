using System.Linq;
using AutoMapper;
using MoviesApp.Data;
using MoviesApp.Models;

namespace MoviesApp.Services.Dto.AutoMapperProfiles
{
    public class MovieDtoProfile : Profile
    {
        public MovieDtoProfile()
        {
            CreateMap<Movie, MovieDto>().ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists))
                .ReverseMap()
                .ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists));
        }
    }
}