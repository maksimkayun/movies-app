using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MoviesApp.Models;
using MoviesApp.Validation;

namespace MoviesApp.Services.Dto
{
    public class ArtistDtoApi
    {
        public int? Id { get; set; }
        
        [Required]
        [UserName(4)]
        public string FirstName { get; set; }
        
        [Required]
        [UserNameAttribute(4)]
        public string LastName { get; set; }
        
        [DataType(DataType.Date)]
        public DateTime Birthday { get; set; }

        public ICollection<int> MoviesArtistsIds { get; set; }
    }
}