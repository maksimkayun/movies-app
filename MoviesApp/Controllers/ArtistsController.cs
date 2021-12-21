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
        [Authorize]
        public IActionResult Index()
        {
            var artists = _mapper.Map<IEnumerable<ArtistDto>, IEnumerable<ArtistViewModel>>(_service.GetAllArtists().ToList());

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
        [Authorize]
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var viewModel = _mapper.Map<ArtistViewModel>(_service.GetArtist((int) id));

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
            var artist = new InputArtistViewModel();
            PopulateAssignedMovieData(artist);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AgeOfTheArtist]
        [Authorize(Roles = "Admin")]
        public IActionResult Create([Bind("FirstName,LastName,Birthday")] InputArtistViewModel inputModel,
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

            if (ModelState.IsValid)
            {
                ArtistDto newArtist = _mapper.Map<ArtistDto>(inputModel);
                newArtist.SelectOptions = selectedOptions;
                _service.AddArtist(newArtist);
                return RedirectToAction(nameof(Index));
            }

            PopulateAssignedMovieData(inputModel);
            return View(inputModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            EditArtistViewModel editModel = _mapper.Map<EditArtistViewModel>(_service.GetArtist((int) id));
            

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
        [Authorize(Roles = "Admin")]
        public IActionResult Edit(int id, [Bind("FirstName,LastName,Birthday")] EditArtistViewModel editModel,
            string[] selectedOptions)
        {
            var artistToUpdate = _mapper.Map<EditArtistViewModel>(editModel);
            if (ModelState.IsValid)
            {
                try
                {
                    var artistDto = _mapper.Map<ArtistDto>(editModel);
                    artistDto.SelectOptions = selectedOptions.ToList();
                    artistDto.Id = id;
                    artistToUpdate = _mapper.Map<EditArtistViewModel>(_service.UpdateArtist(artistDto));
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
            
            PopulateAssignedMovieData(artistToUpdate);
            return View(artistToUpdate);
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

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            DeleteArtistViewModel deleteModel =
                _mapper.Map<ArtistDto, DeleteArtistViewModel>(_service.GetArtist((int) id));

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
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var artist = _mapper.Map<ArtistDto>(_service.DeleteArtist(id));
            _logger.LogInformation($"Artist with id {id} has been deleted!");
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}