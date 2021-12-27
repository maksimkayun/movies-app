using System.Collections.Generic;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Services
{
    public interface IArtistService
    {
        ArtistDto GetArtist(int id, bool apiFlag);
        IEnumerable<ArtistDto> GetAllArtists();
        ArtistDto UpdateArtist(ArtistDto artistDto, bool apiFlag);
        ArtistDto AddArtist(ArtistDto artistDto);
        ArtistDto DeleteArtist(int id);

        #region API

        IEnumerable<ArtistDtoApi> GetAllArtistApi();
        ArtistDtoApi GetArtistApi(int id);
        ArtistDtoApi AddArtistApi(ArtistDtoApi inputDtoApi);
        ArtistDtoApi UpdateArtistApi(ArtistDtoApi artistDto);

        #endregion

        public List<OptionVMArtist> PopulateAssignedMovieData(InputArtistViewModel artist);
    }
}