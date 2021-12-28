using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesApp.Data;
using MoviesApp.Models;
using MoviesApp.Services;
using MoviesApp.Services.Dto;
using MoviesApp.ViewModels;

namespace MoviesApp.Controllers
{
    public class MoviesController : Controller
    {
        private readonly MoviesContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IMovieService _service;


        public MoviesController(MoviesContext context, ILogger<HomeController> logger, IMapper mapper, IMovieService service)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _service = service;
        }

        // GET: Movies
        [HttpGet]
        [Authorize]
        public IActionResult Index()
        {
            var movies = _mapper.Map<IEnumerable<MovieDto>, IEnumerable<MovieViewModel>>(_service.GetAllMovies(false).ToList());

            #region without mapper

            /*return View(_context.Movies.Select(m => new MovieViewModel
            {
                Id = m.Id,
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate
            }).ToList());*/

            #endregion

            return View(movies);
        }

        [HttpGet]
        [Authorize]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*MovieViewModel viewModel = _mapper.Map<Movie, MovieViewModel>(
                _context.Movies
                    .Include(ng => ng.MoviesArtists)
                    .ThenInclude(ng => ng.Artist)
                    .FirstOrDefault(m => m.Id == id)
            );*/

            var viewModel = _mapper.Map<MovieViewModel>(_service.GetMovie((int) id, false));

            #region without mapper

            /*var viewModel = _context.Movies.Where(m => m.Id == id).Select(m => new MovieViewModel
            {
                Id = m.Id,
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate,
                MoviesArtists = (ICollection<MoviesArtist>) m.MoviesArtists.Where(ma => ma.MovieId == id)
                    .Select(m => m)
            }).FirstOrDefault();*/

            // or we can do this:

            /*var viewModel = _context.Movies
                .Include(m => m.MoviesArtists)
                .ThenInclude(ma => ma.Artist).SingleOrDefault(m=>m.Id == id);*/

            #endregion

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            InputMovieViewModel movie = new InputMovieViewModel();
            PopulateAssignedMovieDataView(movie);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([Bind("Title,ReleaseDate,Genre,Price")] InputMovieViewModel inputModel,
            string[] selectedOptions)
        {
            if (ModelState.IsValid)
            {
                MovieDto newMovie = _mapper.Map<MovieDto>(inputModel);
                newMovie.SelectOptions = selectedOptions.Select(int.Parse).ToList();
                _service.AddMovie(newMovie, false);
                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedMovieDataView(inputModel);
            return View(inputModel);
        }

        private void PopulateAssignedMovieDataView(InputMovieViewModel movie)
        {
            ViewData["ArtistOptions"] = _service.PopulateAssignedMovieData(movie);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EditMovieViewModel editModel = _mapper.Map<EditMovieViewModel>(_service.GetMovie((int) id, false));

            #region without mapper

            /*var editModel = _context.Movies
                .Include(m => m.MoviesArtists)
                .ThenInclude(ma => ma.Artist).AsNoTracking().SingleOrDefault(m => m.Id == id);*/
            
            /*EditMovieViewModel editModel = _mapper.Map<Movie, EditMovieViewModel>(
                _context.Movies.Include(ng => ng.MoviesArtists)
                    .ThenInclude(ng => ng.Artist)
                    .FirstOrDefault(m => m.Id == id)
            );*/

            // or we can do this:
            /*var editModel = _context.Movies.Where(m => m.Id == id).AsNoTracking()
                .Select(m => new EditMovieViewModel
                {
                    Title = m.Title,
                    Genre = m.Genre,
                    ReleaseDate = m.ReleaseDate,
                    Price = m.Price,
                    MoviesArtists = m.MoviesArtists
                }).FirstOrDefault();*/

            #endregion

            if (editModel == null)
            {
                return NotFound();
            }

            PopulateAssignedMovieDataView(editModel);
            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] 
        public IActionResult Edit(int id, [Bind("Title,ReleaseDate,Genre,Price")] EditMovieViewModel editModel,
            string[] selectedOptions)
        {
            var movieToUpdate = _mapper.Map<EditMovieViewModel>(editModel);
            if (ModelState.IsValid)
            {
                try
                {
                    var movieDto = _mapper.Map<MovieDto>(editModel);
                    movieDto.SelectOptions = selectedOptions.Select(int.Parse).ToList();
                    movieDto.Id = id;
                    movieToUpdate = _mapper.Map<EditMovieViewModel>(_service.UpdateMovie(movieDto, false));
                }
                catch (DbUpdateException)
                {
                    if (!MovieExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedMovieDataView(movieToUpdate);
            return View(movieToUpdate);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")] 
        // GET: Movies/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeleteMovieViewModel deleteModel = _mapper.Map<MovieDto, DeleteMovieViewModel>(_service.GetMovie((int) id, false));

            if (deleteModel == null)
            {
                return NotFound();
            }

            return View(deleteModel);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")] 
        public IActionResult DeleteConfirmed(int id)
        {
            _mapper.Map<MovieDto>(_service.DeleteMovie(id , false));
            _logger.LogInformation($"Movie with id {id} has been deleted!");
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}