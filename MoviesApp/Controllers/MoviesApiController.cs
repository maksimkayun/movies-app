using System.Collections.Generic;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using MoviesApp.Services;
using MoviesApp.Services.Dto;

namespace MoviesApp.Controllers
{
    [Route("api/movies")]
    [ApiController]
    public class MoviesApiController : ControllerBase
    {
        private readonly IMovieService _service;
        private readonly IMapper _mapper;
        
        public MoviesApiController(IMovieService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet] // GET: /api/movies
        [ProducesResponseType(200, Type = typeof(IEnumerable<MovieDto>))]  
        [ProducesResponseType(404)]
        public ActionResult<IEnumerable<MovieDto>> GetMovies()
        {
            return Ok(_service.GetAllMovies(true));
        }
        
        [HttpGet("{id}")] // GET: /api/movies/5
        [ProducesResponseType(200, Type = typeof(MovieDto))]  
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            var movie = _service.GetMovie(id, true);
            if (movie == null) return NotFound();  
            return Ok(movie);
        }
        
        [HttpPost] // POST: api/movies
        public ActionResult<MovieDto> PostMovie(MovieDto inputDto)
        {
            var movie = _service.AddMovie(inputDto, true);
            return CreatedAtAction("GetById", new { id = movie.Id }, movie);
        }
        
        [HttpPut("{id}")] // PUT: api/movies/5
        public IActionResult UpdateMovie(int id, MovieDto editDto)
        {
            editDto.Id = id;
            var movie = _service.UpdateMovie(editDto, true);

            if (movie==null)
            {
                return BadRequest();
            }

            return Ok(movie);
        }
        
        [HttpDelete("{id}")] // DELETE: api/movie/5
        public ActionResult<MovieDto> DeleteMovie(int id)
        {
            var movie = _mapper.Map<MovieDto>(_service.DeleteMovie(id, true));
            if (movie == null) return NotFound();  
            return Ok(movie);
        }
    }
}