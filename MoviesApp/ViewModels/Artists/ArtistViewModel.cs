using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoviesApp.Models;

namespace MoviesApp.ViewModels
{
    public class ArtistViewModel:InputArtistViewModel
    {
        public ArtistViewModel() : base()
        {
        }
        public int Id { get; set; }
    }
}