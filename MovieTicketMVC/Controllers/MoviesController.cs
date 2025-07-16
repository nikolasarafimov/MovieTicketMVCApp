using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieTicketMVC.Models;

namespace MovieTicketMVC.Controllers
{
    public class MoviesController : Controller
    {
        private ApplicationDbContext _context = new ApplicationDbContext();
        private static readonly string[] _allowedExts = { ".mp4", ".webm", ".ogg" };

        public MoviesController()
        {
            _context = new ApplicationDbContext();
        }

        // GET: Movies/Current
        public ActionResult Current(string searchString, string sortOrder, string ageFilter)
        {
            IQueryable<Movie> query = _context.Movies
                .Where(m => m.IsCurrentlyShowing);

            if (ageFilter == "Over18")
                query = query.Where(m => m.IsForAdults == true);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lower = searchString.Trim().ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(lower)
                );
            }

            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "title_asc";
            ViewBag.TitleSortParam = (sortOrder == "title_asc")
                ? "title_desc"
                : "title_asc";

            switch (sortOrder)
            {
                case "title_desc":
                    query = query.OrderByDescending(m => m.Title);
                    break;
                default:
                    query = query.OrderBy(m => m.Title);
                    break;
            }

            var model = query.ToList();
            return View(model);
        }

        // GET: Movies/ComingSoon
        public ActionResult ComingSoon(string searchString, string sortOrder, string ageFilter)
        {
            var today = DateTime.Today;
            var toUpdate = _context.Movies
                .Where(m => !m.IsCurrentlyShowing && m.ReleaseDate <= today)
                .ToList();
            toUpdate.ForEach(m => m.IsCurrentlyShowing = true);
            if (toUpdate.Any()) _context.SaveChanges();

            IQueryable<Movie> query = _context.Movies
                .Where(m => !m.IsCurrentlyShowing);

            if (ageFilter == "Over18")
                query = query.Where(m => m.IsForAdults == true);

            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var lower = searchString.Trim().ToLower();
                query = query.Where(m =>
                    m.Title.ToLower().Contains(lower)
                );
            }

            if (string.IsNullOrEmpty(sortOrder))
                sortOrder = "title_asc";
            ViewBag.TitleSortParam = (sortOrder == "title_asc")
                ? "title_desc"
                : "title_asc";

            switch (sortOrder)
            {
                case "title_desc":
                    query = query.OrderByDescending(m => m.Title);
                    break;
                default:
                    query = query.OrderBy(m => m.Title);
                    break;
            }

            var model = query.ToList();
            return View(model);
        }


        // GET: Movies/Details/5
        public ActionResult Details(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return HttpNotFound();

            ViewBag.ActiveMovieSection = movie.IsCurrentlyShowing ? "Current" : "ComingSoon";

            return View(movie);
        }

        // GET: Movies/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.ActiveMovieSection = "ComingSoon";
            var movie = new Movie { IsCurrentlyShowing = false };
            return View(movie);
        }

        // POST: Movies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create(Movie movie, HttpPostedFileBase videoFile)
        {
            if (!ModelState.IsValid)
                return View(movie);

            if (videoFile == null || videoFile.ContentLength == 0)
            {
                ModelState.AddModelError("videoFile", "You must upload a trailer video.");
                return View(movie);
            }

            var allowedExts = new[] { ".mp4", ".webm", ".ogg" };
            var ext = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
            if (!allowedExts.Contains(ext))
            {
                ModelState.AddModelError("videoFile", "Only MP4, WebM or Ogg videos are supported.");
                return View(movie);
            }

            var fileName = Path.GetFileName(videoFile.FileName);
            var videosPath = Server.MapPath("~/Content/Videos/");
            if (!Directory.Exists(videosPath))
                Directory.CreateDirectory(videosPath);
            var fullPath = Path.Combine(videosPath, fileName);
            videoFile.SaveAs(fullPath);

            movie.LocalTrailerPath = "~/Content/Videos/" + fileName;
            movie.IsCurrentlyShowing = false;
            _context.Movies.Add(movie);
            _context.SaveChanges();

            return RedirectToAction("ComingSoon");
        }

        // GET: Movies/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return HttpNotFound();
            ViewBag.ActiveMovieSection = movie.IsCurrentlyShowing ? "Current" : "ComingSoon";
            return View(movie);
        }

        // POST: Movies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(Movie movie, string ReturnSection, HttpPostedFileBase videoFile)
        {
            ViewBag.ReturnSection = ReturnSection;

            if (!ModelState.IsValid)
                return View(movie);

            var movieInDb = _context.Movies.Find(movie.Id);
            if (movieInDb == null) return HttpNotFound();

            movieInDb.Title = movie.Title;
            movieInDb.LengthInMinutes = movie.LengthInMinutes;
            movieInDb.ReleaseDate = movie.ReleaseDate;
            movieInDb.Description = movie.Description;
            movieInDb.Genre = movie.Genre;
            movieInDb.IsForAdults = movie.IsForAdults;
            movieInDb.Rating = movie.Rating;
            movieInDb.Actors = movie.Actors;
            movieInDb.Language = movie.Language;
            movieInDb.IsCurrentlyShowing = (movie.ReleaseDate.Date <= DateTime.Today);

            if (videoFile != null && videoFile.ContentLength > 0)
            {
                var allowedExts = new[] { ".mp4", ".webm", ".ogg" };
                var ext = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
                if (!allowedExts.Contains(ext))
                {
                    ModelState.AddModelError("videoFile", "Only MP4, WebM or Ogg videos are supported.");
                    return View(movie);
                }

                if (!string.IsNullOrEmpty(movieInDb.LocalTrailerPath))
                {
                    var oldPath = Server.MapPath(movieInDb.LocalTrailerPath);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = Path.GetFileName(videoFile.FileName);
                var videosPath = Server.MapPath("~/Content/Videos/");
                if (!Directory.Exists(videosPath))
                    Directory.CreateDirectory(videosPath);
                var fullPath = Path.Combine(videosPath, fileName);
                videoFile.SaveAs(fullPath);

                movieInDb.LocalTrailerPath = "~/Content/Videos/" + fileName;
            }

            _context.SaveChanges();

            return RedirectToAction(ReturnSection == "Current" ? "Current" : "ComingSoon");
        }

        // GET: Movies/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return HttpNotFound();
            ViewBag.ActiveMovieSection = movie.IsCurrentlyShowing ? "Current" : "ComingSoon";
            return View(movie);
        }

        // POST: Movies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            var movie = _context.Movies.Find(id);
            if (movie == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(movie.LocalTrailerPath))
            {
                var filePath = Server.MapPath(movie.LocalTrailerPath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Movies.Remove(movie);
            _context.SaveChanges();

            if (movie.IsCurrentlyShowing)
                return RedirectToAction("Current");
            else
                return RedirectToAction("ComingSoon");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}