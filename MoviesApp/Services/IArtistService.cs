using System.Collections.Generic;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public interface IArtistService
    {
        ArtistDto GetArtist(int id, bool apiFlag);
        IEnumerable<ArtistDto> GetAllArtists(bool apiFlag);
        ArtistDto UpdateArtist(ArtistDto artistDto, bool apiFlag);
        ArtistDto AddArtist(ArtistDto artistDto, bool apiFlag);
        ArtistDto DeleteArtist(int id);

        public List<OptionVMArtist> PopulateAssignedMovieData(InputArtistViewModel artist);
    }
}