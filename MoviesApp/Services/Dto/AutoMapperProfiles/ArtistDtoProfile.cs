using System.Linq;
using AutoMapper;
using MoviesApp.Models;

namespace MoviesApp.Services.Dto.AutoMapperProfiles
{
    public class ArtistDtoProfile: Profile
    {
        public ArtistDtoProfile()
        {
            CreateMap<Artist, ArtistDto>().ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists))
                .ReverseMap()
                .ForMember(e => e.MoviesArtists,
                    opt => opt.MapFrom(m => m.MoviesArtists));
        }
    }
}