using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using MoviesApp.Models;

namespace MoviesApp.ViewModels.AutoMapperProfiles
{
    public class ArtistProfile:Profile
    {
        public ArtistProfile()
        {
            CreateMap<Artist, InputArtistViewModel>().ReverseMap();
            CreateMap<Artist, DeleteArtistViewModel>();
            CreateMap<Artist, EditArtistViewModel>().ReverseMap();
            CreateMap<Artist, ArtistViewModel>();
        }
    }
}