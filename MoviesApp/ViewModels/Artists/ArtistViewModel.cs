using System;
using System.Collections;
using System.Linq;

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