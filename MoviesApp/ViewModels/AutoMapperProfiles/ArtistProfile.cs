using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MoviesApp.Models;
using MoviesApp.Services.Dto;

namespace MoviesApp.ViewModels.AutoMapperProfiles
{
    public class ArtistProfile:Profile
    {
        public ArtistProfile()
        {
            CreateMap<ArtistDto, InputArtistViewModel>().ReverseMap();
            CreateMap<ArtistDto, DeleteArtistViewModel>();
            CreateMap<ArtistDto, EditArtistViewModel>().ReverseMap();
            CreateMap<ArtistDto, ArtistViewModel>().ReverseMap();
        }
    }
}