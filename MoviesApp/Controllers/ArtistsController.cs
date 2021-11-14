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
    public class ArtistsController: Controller
    {
        private readonly MoviesContext _context;
        private readonly ILogger<HomeController> _logger;


        public ArtistsController(MoviesContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return View(_context.Artists.Select(m => new ArtistViewModel
            {
                Id = m.Id, 
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday
            }).ToList());
        }
        
        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
        
            var viewModel = _context.Artists.Where(m => m.Id == id).Select(m => new ArtistViewModel
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday,
                Movies = (ICollection<Movie>) m.MoviesArtists.Where(ma => ma.ArtistId == id).Select(m => m.Movie),
                MoviesArtists = (ICollection<MoviesArtist>) m.MoviesArtists.Where(ma => ma.ArtistId == id).Select(m => m)
            }).FirstOrDefault();
        
            
            if (viewModel == null)
            {
                return NotFound();
            }
        
            return View(viewModel);
        }
        
        [HttpGet]
        public IActionResult Create()
        {
            var artist = new Artist();
            PopulateAssignedMovieData(artist);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("FirstName,LastName,Birthday")] 
            Artist artist, string[] selectedOptions)
        {
            if (ModelState.IsValid)
            {
                _context.Artists.Add(artist);
                _context.SaveChanges();
                if (selectedOptions != null)
                {
                    foreach (var movie in selectedOptions)
                    {
                        var movieToAdd = new MoviesArtist
                        {
                            ArtistId = artist.Id,
                            MovieId = int.Parse(movie)
                        };
                        _context.MoviesArtists.Add(movieToAdd);
                        _context.SaveChanges();
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateAssignedMovieData(artist);
            return View(artist);
        }
        
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            
            var editModel = _context.Artists.Include(a => a.MoviesArtists)
                .ThenInclude(ma => ma.Movie).AsNoTracking().SingleOrDefault(a => a.Id == id);

            if (editModel == null)
            {
                return NotFound();
            }
            PopulateAssignedMovieData(editModel);
            return View(editModel);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("FirstName,LastName,Birthday")] 
            Artist editModel, string[] selectedOptions)
        {
            var artistToUpdate = _context.Artists
                .Include(a => a.MoviesArtists)
                .ThenInclude(am => am.Movie)
                .SingleOrDefault(a => a.Id == id);
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateMoviesArtists(selectedOptions, artistToUpdate);
                    if (artistToUpdate != null)
                    {
                        artistToUpdate.FirstName = editModel.FirstName;
                        artistToUpdate.LastName = editModel.LastName;
                        artistToUpdate.Birthday = editModel.Birthday;
                        _context.Update(artistToUpdate);
                    }
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    if (!ArtistExists(id))
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

        private void PopulateAssignedMovieData(Artist artist)
        {
            var allOptions = _context.Movies;
            var currentOptionIDs = new HashSet<int>(artist.MoviesArtists.Select(m => m.MovieId));
            var checkBoxes = new List<OptionVMArtist>();
            foreach (var option in allOptions)
            {
                checkBoxes.Add(new OptionVMArtist
                {
                    Id = option.Id,
                    Name = option.Title,
                    Assigned = currentOptionIDs.Contains(option.Id)
                });
            }
            
            ViewData["MovieOptions"] = checkBoxes;
        }

        private void UpdateMoviesArtists(string[] selectedOptions, Artist artistToUpdate)
        {
            if (selectedOptions == null)
            {
                artistToUpdate.MoviesArtists = new List<MoviesArtist>();
                return;
            }

            var selectedOptionsHS = new HashSet<string>(selectedOptions);
            var artistOptionsHS = new HashSet<int>(artistToUpdate.MoviesArtists
                .Select(m => m.MovieId));
            foreach (var option in _context.Movies)
            {
                if (selectedOptionsHS.Contains(option.Id.ToString())) // чекбокс выделен
                {
                    if (!artistOptionsHS.Contains(option.Id)) // но не отображено в таблице многие-ко-многим
                    {
                        artistToUpdate.MoviesArtists.Add(new MoviesArtist
                        {
                            ArtistId = artistToUpdate.Id,
                            MovieId = option.Id
                        });
                    }
                }
                else
                {
                    // чекбокс не выделен
                    if (artistOptionsHS.Contains(option.Id)) // но в таблице многие-ко-многим такое отношение было
                    {
                        MoviesArtist movieToRemove = artistToUpdate.MoviesArtists
                            .SingleOrDefault(m => m.MovieId == option.Id);
                        _context.MoviesArtists.Remove(movieToRemove ?? throw new InvalidOperationException());
                    }
                }
            }
        }

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
        
            var deleteModel = _context.Artists.Where(m => m.Id == id).Select(m => new DeleteArtistViewModel
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday
            }).FirstOrDefault();
            
            if (deleteModel == null)
            {
                return NotFound();
            }
        
            return View(deleteModel);
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var artist = _context.Artists.Find(id);
            var сommunications = _context.MoviesArtists.Where(ma => ma.ArtistId == id)
                .Select(ma => ma).ToList();
            foreach (var elem in сommunications)
            {
                _context.MoviesArtists.Remove(elem);
            }
            _context.Artists.Remove(artist);
            _context.SaveChanges();
            _logger.LogError($"Artist with id {artist.Id} has been deleted!");
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}