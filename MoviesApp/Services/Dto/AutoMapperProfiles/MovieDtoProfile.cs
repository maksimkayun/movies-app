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
            CreateMap<Movie, MovieDtoApi>().ForMember(e => e.MoviesArtistsIds,
                    opt =>
                        opt.MapFrom(m => m.MoviesArtists.Select(a => a.ArtistId)))
                .ReverseMap()
                .ForMember(e => e.MoviesArtists,
                    opt => opt.Ignore());
            CreateMap<MovieDto, MovieDtoApi>().ForMember(e => e.MoviesArtistsIds,
                arg => arg.MapFrom(
                    opt => opt.SelectOptions.Select(int.Parse)));
        }
    }
}