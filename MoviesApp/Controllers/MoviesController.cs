using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MoviesApp.Data;
using MoviesApp.Models;
using MoviesApp.ViewModels;

namespace MoviesApp.Controllers
{
    public class MoviesController: Controller
    {
        private readonly MoviesContext _context;
        private readonly ILogger<HomeController> _logger;


        public MoviesController(MoviesContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Movies
        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.Movies.Select(m => new MovieViewModel
            {
                Id = m.Id,
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate
            }).ToList());
        }
        
        [HttpGet]
        public IActionResult Artists(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = _context.MoviesArtists.Where(ma => ma.MovieId == id)
                .Select(a => new ArtistsViewModel
                {
                    FirstName = a.Artist.FirstName,
                    LastName = a.Artist.LastName,
                    Birthday = a.Artist.Birthday
                }).ToList();

            
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*var viewModel = _context.Movies.Where(m => m.Id == id).Select(m => new MovieViewModel
            {
                Id = m.Id,
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate
            }).FirstOrDefault();*/
            var viewModel = _context.Movies
                .Include(m => m.MoviesArtists)
                .ThenInclude(ma => ma.Artist).SingleOrDefault(m=>m.Id == id);

            
            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            Movie movie = new Movie();
            PopulateAssignedMovieData(movie);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Title,ReleaseDate,Genre,Price")] 
            Movie inputModel, string[] selectedOptions)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inputModel);
                _context.SaveChanges();
                if (selectedOptions != null)
                {
                    foreach (var artist in selectedOptions)
                    {
                        var artistToAdd = new MoviesArtist
                        {
                            ArtistId = int.Parse(artist),
                            MovieId = inputModel.Id
                        };
                        _context.MoviesArtists.Add(artistToAdd);
                        _context.SaveChanges();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedMovieData(inputModel);
            return View(inputModel);
        }
        
        private void PopulateAssignedMovieData(Movie movie)
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

        private void UpdateMoviesArtists(string[] selectedOptions, Movie movieToUpdate)
        {
            if (selectedOptions == null)
            {
                movieToUpdate.MoviesArtists = new List<MoviesArtist>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var movieOptionsHS = new HashSet<int>(movieToUpdate.MoviesArtists
                .Select(m => m.ArtistId));
            foreach (var option in _context.Artists)
            {
                if (selectedOptionsHS.Contains(option.Id.ToString())) // чекбокс выделен
                {
                    if (!movieOptionsHS.Contains(option.Id)) // но не отображено в таблице многие-ко-многим
                    {
                        movieToUpdate.MoviesArtists.Add(new MoviesArtist
                        {
                            MovieId = movieToUpdate.Id,
                            ArtistId = option.Id
                        });
                    }
                }
                else
                {
                    // чекбокс не выделен
                    if (movieOptionsHS.Contains(option.Id)) // но в таблице многие-ко-многим такое отношение было
                    {
                        MoviesArtist movieToRemove = movieToUpdate.MoviesArtists
                            .SingleOrDefault(m => m.ArtistId == option.Id);
                        _context.MoviesArtists.Remove(movieToRemove ?? throw new InvalidOperationException());
                    }
                }
            }
        }
        
        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var editModel = _context.Movies
                .Include(m => m.MoviesArtists)
                .ThenInclude(ma => ma.Artist).AsNoTracking().SingleOrDefault(m => m.Id == id);
            
            if (editModel == null)
            {
                return NotFound();
            }
            PopulateAssignedMovieData(editModel);
            return View(editModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Title,ReleaseDate,Genre,Price")] 
            Movie editModel, string[] selectedOptions)
        {
            var movieToUpdate = _context.Movies
                .Include(m => m.MoviesArtists)
                .ThenInclude(am => am.Artist)
                .SingleOrDefault(m => m.Id == id);
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateMoviesArtists(selectedOptions, movieToUpdate);
                    if (movieToUpdate != null)
                    {
                        movieToUpdate.Title = editModel.Title;
                        movieToUpdate.Genre = editModel.Genre;
                        movieToUpdate.ReleaseDate = editModel.ReleaseDate;
                        movieToUpdate.Price = editModel.Price;
                        _context.Update(movieToUpdate);
                    }
                    _context.SaveChanges();
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
            PopulateAssignedMovieData(editModel);
            return View(editModel);
        }
        
        [HttpGet]
        // GET: Movies/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deleteModel = _context.Movies.Where(m => m.Id == id).Select(m => new DeleteMovieViewModel
            {
                Genre = m.Genre,
                Price = m.Price,
                Title = m.Title,
                ReleaseDate = m.ReleaseDate
            }).FirstOrDefault();
            
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
            var movie = _context.Movies.Find(id);
            var сommunications = _context.MoviesArtists.Where(ma => ma.MovieId == id)
                .Select(ma => ma).ToList();
            foreach (var elem in сommunications)
            {
                _context.MoviesArtists.Remove(elem);
            }
            _context.Movies.Remove(movie);
            _context.SaveChanges();
            _logger.LogError($"Movie with id {movie.Id} has been deleted!");
            return RedirectToAction(nameof(Index));
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }
    }
}