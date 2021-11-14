using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MoviesApp.Models;

namespace MoviesApp.ViewModels
{
    public class InputArtistViewModel
    {
        public InputArtistViewModel()
        {
            MoviesArtists = new HashSet<MoviesArtist>();
            Movies = new HashSet<Movie>();
        }
        public string FirstName { get; set; }
        
        public string LastName  { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
        
        public ICollection<MoviesArtist> MoviesArtists { get; set; }
        public ICollection<Movie> Movies { get; set; }
    }
}