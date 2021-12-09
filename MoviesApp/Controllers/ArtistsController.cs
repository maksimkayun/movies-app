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
using MoviesApp.Validation;
using MoviesApp.ViewModels;

namespace MoviesApp.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly MoviesContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private readonly IArtistService _service;

        public ArtistsController(MoviesContext context, ILogger<HomeController> logger, IMapper mapper, IArtistService service)
        {
            _context = context;
            _logger = logger;
            _mapper = mapper;
            _service = service;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var artists = _mapper.Map<IEnumerable<Artist>, IEnumerable<ArtistViewModel>>(_context.Artists.ToList());

            #region without mapper

            /*return View(_context.Artists.Select(m => new ArtistViewModel
            {
                Id = m.Id,
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday
            }).ToList());*/

            #endregion

            return View(artists);
        }

        [HttpGet]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<Artist, ArtistViewModel>(
                _context.Artists.Include(ng => ng.MoviesArtists)
                    .ThenInclude(ng => ng.Movie)
                    .SingleOrDefault(art => art.Id == id)
            );

            if (viewModel == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var artist = new InputArtistViewModel();
            PopulateAssignedMovieData(artist);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AgeOfTheArtist]
        public IActionResult Create([Bind("FirstName,LastName,Birthday")] InputArtistViewModel artist,
            string[] selectedOptions)
        {
            #region without mapper

            /*var newArtist = new Artist
            {
                FirstName = artist.FirstName,
                LastName = artist.LastName,
                Birthday = artist.Birthday,
                MoviesArtists = artist.MoviesArtists
            };*/

            #endregion

            Artist newArtist = _mapper.Map<InputArtistViewModel, Artist>(artist);
            if (ModelState.IsValid)
            {
                _context.Artists.Add(newArtist);
                _context.SaveChanges();
                if (selectedOptions != null)
                {
                    foreach (var movie in selectedOptions)
                    {
                        var movieToAdd = new MoviesArtist
                        {
                            ArtistId = newArtist.Id,
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

        [HttpGet]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            /*var editModel = _context.Artists.Include(a => a.MoviesArtists)
                .ThenInclude(ma => ma.Movie).AsNoTracking().SingleOrDefault(a => a.Id == id);*/

            EditArtistViewModel editModel = _mapper.Map<Artist, EditArtistViewModel>(
                _context.Artists
                    .Include(ng => ng.MoviesArtists)
                    .ThenInclude(ng => ng.Movie)
                    .FirstOrDefault(m => m.Id == id)
            );

            #region without mapper

            /*var editModel = _context.Artists.Where(m => m.Id == id).AsNoTracking()
                .Select(m => new EditArtistViewModel
                {
                    FirstName = m.FirstName,
                    LastName = m.LastName,
                    Birthday = m.Birthday,
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
        [AgeOfTheArtist]
        public IActionResult Edit(int id, [Bind("FirstName,LastName,Birthday")] EditArtistViewModel editModel,
            string[] selectedOptions)
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

        private void PopulateAssignedMovieData(InputArtistViewModel artist)
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

            DeleteArtistViewModel deleteModel = _mapper.Map<Artist, DeleteArtistViewModel>(
                _context.Artists
                    .Include(ng => ng.MoviesArtists)
                    .ThenInclude(ng => ng.Movie)
                    .FirstOrDefault(art => art.Id == id)
            );

            #region without mapper

            /*var deleteModel = _context.Artists.Where(m => m.Id == id).Select(m => new DeleteArtistViewModel
            {
                FirstName = m.FirstName,
                LastName = m.LastName,
                Birthday = m.Birthday
            }).FirstOrDefault();*/

            #endregion

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