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

        // GET: Movies
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

        // GET: Movies/Details/5
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
        
        // GET: Movies/Create
        [HttpGet]
        public IActionResult Create()
        {
            var artist = new Artist();
            PopulateAssignedMovieData(artist);
            return View();
        }


        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
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
        
        // [HttpGet]
        // // GET: Movies/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
        
            var editModel = _context.Artists.Where(m => m.Id == id).Select(m => new EditArtistViewModel
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday,
            }).FirstOrDefault();
            
            if (editModel == null)
            {
                return NotFound();
            }
            
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
                        _context.SaveChanges();
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
                        _context.SaveChanges();
                    }
                }
            }
        }

        // POST: Movies/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("FirstName,LastName,Birthday")] EditArtistViewModel editModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var artist = new Artist
                    {
                        Id = id,
                        FirstName = editModel.FirstName,
                        LastName = editModel.LastName,
                        Birthday = editModel.Birthday
                    };
                    
                    _context.Update(artist);
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
        
        // POST: Movies/Delete/5
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