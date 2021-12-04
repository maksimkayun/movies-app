using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MoviesApp.Models
{
    public class Artist
    {
        public Artist()
        {
            MoviesArtists = new HashSet<MoviesArtist>();
        }

        public int Id { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        public virtual ICollection<MoviesArtist> MoviesArtists { get; set; }
    }
}