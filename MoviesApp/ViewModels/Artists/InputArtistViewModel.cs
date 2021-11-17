using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MoviesApp.Models;
using MoviesApp.Validation;

namespace MoviesApp.ViewModels
{
    public class InputArtistViewModel
    {
        public InputArtistViewModel()
        {
            MoviesArtists = new HashSet<MoviesArtist>();
            Movies = new HashSet<Movie>();
        }
        
        [UserNameAttribute(4)]
        public string FirstName { get; set; }
        
        [UserNameAttribute(4)]
        public string LastName  { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }
        
        public ICollection<MoviesArtist> MoviesArtists { get; set; }
        public ICollection<Movie> Movies { get; set; }
    }
}