using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
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
        public IActionResult Index()
        {
            var movies = _mapper.Map<IEnumerable<MovieDto>, IEnumerable<MovieViewModel>>(_service.GetAllMovies().ToList());

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

            var viewModel = _mapper.Map<MovieViewModel>(_service.GetMovie((int) id));

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
        public IActionResult Create()
        {
            InputMovieViewModel movie = new InputMovieViewModel();
            PopulateAssignedMovieData(movie);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Title,ReleaseDate,Genre,Price")] InputMovieViewModel inputModel,
            string[] selectedOptions)
        {
            #region without mapper

            /*var newMovie = new Movie
            {
                Title = inputModel.Title,
                Genre = inputModel.Genre,
                ReleaseDate = inputModel.ReleaseDate,
                Price = inputModel.Price,
                MoviesArtists = inputModel.MoviesArtists
            };*/

            #endregion

            if (ModelState.IsValid)
            {
                MovieDto newMovie = _mapper.Map<MovieDto>(inputModel);
                newMovie.SelectOptions = selectedOptions;
                _service.AddMovie(newMovie);
                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedMovieData(inputModel);
            return View(inputModel);
        }

        private void PopulateAssignedMovieData(InputMovieViewModel movie)
        {
            var allOptions = _context.Artists;
            var currentOptionIDs = new HashSet<int>(movie.MoviesArtists.Select(m => m.ArtistId));
            var checkBoxes = new List<OptionVModelMovie>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVModelMovie
                {
                    Id = option.Id,
                    Name = option.FirstName + " " + option.LastName,
                    Assigned = currentOptionIDs.Contains(option.Id)
                });
            }

            ViewData["ArtistOptions"] = checkBoxes;
        }

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EditMovieViewModel editModel = _mapper.Map<EditMovieViewModel>(_service.GetMovie((int) id));

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

            PopulateAssignedMovieData(editModel);
            return View(editModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Title,ReleaseDate,Genre,Price")] EditMovieViewModel editModel,
            string[] selectedOptions)
        {
            var movieToUpdate = _mapper.Map<EditMovieViewModel>(editModel);
            if (ModelState.IsValid)
            {
                try
                {
                    var movieDto = _mapper.Map<MovieDto>(editModel);
                    movieDto.SelectOptions = selectedOptions.ToList();
                    movieDto.Id = id;
                    movieToUpdate = _mapper.Map<EditMovieViewModel>(_service.UpdateMovie(movieDto));
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

            PopulateAssignedMovieData(movieToUpdate);
            return View(movieToUpdate);
        }

        [HttpGet]
        // GET: Movies/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeleteMovieViewModel deleteModel = _mapper.Map<MovieDto, DeleteMovieViewModel>(_service.DeleteMovie((int) id));

            #region without mapper

            /*var deleteModel = _context.Movies.Where(m => m.Id == id).Select(m => new DeleteMovieViewModel
            {
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate
            }).FirstOrDefault();*/

            #endregion


            if (deleteModel == null)
            {
                return NotFound();
            }

            return View(deleteModel);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _logger.LogError($"Movie with id {id} has been deleted!");
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}