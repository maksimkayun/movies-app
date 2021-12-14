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
        [ProducesResponseType(200, Type = typeof(IEnumerable<MovieDtoApi>))]  
        [ProducesResponseType(404)]
        public ActionResult<IEnumerable<MovieDtoApi>> GetMovies()
        {
            return Ok(_service.GetAllMoviesApi());
        }
        
        [HttpGet("{id}")] // GET: /api/movies/5
        [ProducesResponseType(200, Type = typeof(MovieDto))]  
        [ProducesResponseType(404)]
        public IActionResult GetById(int id)
        {
            var movie = _service.GetMovieApi(id);
            if (movie == null) return NotFound();  
            return Ok(movie);
        }
        
        [HttpPost] // POST: api/movies
        public ActionResult<MovieDto> PostMovie(MovieDtoApi inputDtoApi)
        {
            var movie = _service.AddMovieApi(inputDtoApi);
            return CreatedAtAction("GetById", new { id = movie.Id }, movie);
        }
        
        [HttpPut("{id}")] // PUT: api/movies/5
        public IActionResult UpdateMovie(int id, MovieDtoApi editDto)
        {
            editDto.Id = id;
            var movie = _service.UpdateMovieApi(editDto);

            if (movie==null)
            {
                return BadRequest();
            }

            return Ok(movie);
        }
        
        [HttpDelete("{id}")] // DELETE: api/movie/5
        public ActionResult<MovieDto> DeleteMovie(int id)
        {
            var movie = _mapper.Map<MovieDtoApi>(_service.DeleteMovie(id));
            if (movie == null) return NotFound();  
            return Ok(movie);
        }
    }
}