using System.Collections.Generic;
using MoviesApp.Services.Dto;

namespace MoviesApp.Services
{
    public interface IArtistService
    {
        ArtistDto GetArtist(int id);
        IEnumerable<ArtistDto> GetAllArtists();
        ArtistDto UpdateArtist(ArtistDto artistDto);
        ArtistDto AddArtist(ArtistDto artistDto);
        ArtistDto DeleteArtist(int id);

        #region API

        IEnumerable<ArtistDtoApi> GetAllArtistApi();
        ArtistDtoApi GetArtistApi(int id);
        ArtistDtoApi AddArtistApi(ArtistDtoApi inputDtoApi);
        ArtistDtoApi UpdateArtistApi(ArtistDtoApi artistDto);

        #endregion
    }
}