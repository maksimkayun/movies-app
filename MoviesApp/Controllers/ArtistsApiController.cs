using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Services;
using MoviesApp.Services.Dto;

namespace MoviesApp.Controllers
{
    [Route("api/artists")]
    [ApiController]
    public class ArtistsApiController : Controller
    {
        private readonly IArtistService _service;
        private readonly IMapper _mapper;

        public ArtistsApiController(IArtistService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }
        
        [HttpGet] // GET: /api/artists
        [ProducesResponseType(200, Type = typeof(IEnumerable<ArtistDto>))]  
        [ProducesResponseType(404)]
        public ActionResult<IEnumerable<ArtistDto>> GetArtists()
        {
            return Ok(_service.GetAllArtists(true));
        }
        
        [HttpGet("{id}")] // GET: /api/artists/5
        [ProducesResponseType(200, Type = typeof(ArtistDto))]  
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            var artist = _service.GetArtist(id, true);
            if (artist == null) return NotFound();  
            return Ok(artist);
        }
        
        [HttpPost] // POST: api/artists
        public ActionResult<ArtistDto> PostArtist(ArtistDto inputDto)
        {
            var artist = _service.AddArtist(inputDto, true);
            return CreatedAtAction("GetById", new { id = artist.Id }, artist);
        }
        
        [HttpPut("{id}")] // PUT: api/artists/5
        public IActionResult UpdateArtist(int id, ArtistDto editDto)
        {
            editDto.Id = id;
            var artist = _service.UpdateArtist(editDto, true);

            if (artist==null)
            {
                return BadRequest();
            }

            return Ok(artist);
        }
        
        [HttpDelete("{id}")] // DELETE: api/artists/5
        public ActionResult<MovieDto> DeleteArtist(int id)
        {
            var artist = _mapper.Map<ArtistDtoApi>(_service.DeleteArtist(id));
            if (artist == null) return NotFound();  
            return Ok(artist);
        }
    }
}