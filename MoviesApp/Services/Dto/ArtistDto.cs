using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MoviesApp.Models;
using MoviesApp.Validation;

namespace MoviesApp.Services.Dto
{
    public class ArtistDto
    {
        public int? Id { get; set; }
        
        [Required]
        [UserNameAttribute(4)]
        public string FirstName { get; set; }
        
        [Required]
        [UserNameAttribute(4)]
        public string LastName { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        public virtual ICollection<MoviesArtist> MoviesArtists { get; set; }
        
        public ICollection<string> SelectOptions { get; set; }
    }
}