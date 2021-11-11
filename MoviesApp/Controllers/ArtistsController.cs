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
        
        [HttpGet]
        public IActionResult Films(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            IEnumerable<FilmsViewModel> viewModel = (_context.MoviesArtists.Where(ma => ma.ArtistId == id)
                .Select(m => new FilmsViewModel
                {
                    Title = m.Movie.Title,
                    ReleaseDate = m.Movie.ReleaseDate,
                    Genre = m.Movie.Genre,
                    Price = m.Movie.Price
                })).ToList();

            return View(viewModel);
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
                Birthday = m.Birthday
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
            return View();
        }

        // POST: Movies/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("FirstName,LastName,Birthday")] InputArtistViewModel inputModel)
        {
            if (ModelState.IsValid)
            {
                _context.Artists.Add(new Artist
                {
                    FirstName = inputModel.FirstName,
                    LastName = inputModel.LastName,
                    Birthday = inputModel.Birthday
                });
                _context.SaveChanges();

                return RedirectToAction(nameof(Index));
            }
            return View(inputModel);
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
                Birthday = m.Birthday
            }).FirstOrDefault();
            
            if (editModel == null)
            {
                return NotFound();
            }
            
            return View(editModel);
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