using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MoviesApp.Models;

namespace MoviesApp.ViewModels
{
    public class InputMovieViewModel
    {
        public InputMovieViewModel()
        {
            MoviesArtists = new HashSet<MoviesArtist>();
            Artists = new HashSet<Artist>();
        }
        public string Title { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }
        public string Genre { get; set; }
        public decimal Price { get; set; }
        public ICollection<MoviesArtist> MoviesArtists { get; set; }
        public ICollection<Artist> Artists { get; set; }
    }
}