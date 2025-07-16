using MovieTicketMVC.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using MovieTicketMVC.Services;


namespace MovieTicketMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private ApplicationDbContext _context;

        public TicketsController()
        {
            _context = new ApplicationDbContext();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpGet]
        public JsonResult GetUnavailableSeats(int movieId, DateTime day, string time)
        {
            var existingTickets = _context.Tickets
                .Where(t => t.MovieId == movieId
                    && DbFunctions.TruncateTime(t.SelectedDay) == DbFunctions.TruncateTime(day)
                    && t.SelectedTime == time)
                .ToList();

            var purchasedSeats = new List<string>();
            foreach (var t in existingTickets)
            {
                if (!string.IsNullOrEmpty(t.SelectedSeats))
                    purchasedSeats.AddRange(t.SelectedSeats.Split(','));
            }

            var distinctSeats = purchasedSeats.Distinct().ToList();

            return Json(distinctSeats, JsonRequestBehavior.AllowGet);
        }

        // GET: Tickets
        [Authorize]
        public ActionResult Index()
        {
            var query = _context.Tickets
                                .Include(t => t.Movie)
                                .OrderByDescending(t => t.Id)
                                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                var myEmail = User.Identity.Name;
                query = query.Where(t => t.Email == myEmail);
            }

            var model = query.ToList();
            return View(model);
        }

        // GET: Tickets/Buy
        [AllowAnonymous]
        [HttpGet]
        public ActionResult Buy()
        {
            if (!User.Identity.IsAuthenticated)
            {
                ViewBag.ReturnUrl = Url.Action("Buy", "Tickets");
                return View("LoginPrompt");
            }

            var ticket = new Ticket { Email = User.Identity.Name };
            var all = _context.Movies.ToList();
            ViewBag.CurrentMovies = all.Where(m => m.ReleaseDate <= DateTime.Today).ToList();
            ViewBag.ComingSoonMovies = all.Where(m => m.ReleaseDate > DateTime.Today).ToList();
            return View(ticket);
        }

        // POST: Tickets/Buy
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Buy(Ticket ticket, string[] selectedSeats)
        {
            var allMovies = _context.Movies.ToList();
            ViewBag.CurrentMovies = allMovies.Where(m => m.ReleaseDate <= DateTime.Today).ToList();
            ViewBag.ComingSoonMovies = allMovies.Where(m => m.ReleaseDate > DateTime.Today).ToList();

            ticket.SelectedSeats = (selectedSeats != null && selectedSeats.Any())
                                      ? string.Join(",", selectedSeats)
                                      : "";
            ticket.NumberOfSeats = (selectedSeats != null) ? selectedSeats.Length : 0;

            if (ticket.MovieId == 0)
                ModelState.AddModelError("MovieId", "Ве молиме одберете филм.");

            if (string.IsNullOrWhiteSpace(ticket.SelectedSeats))
                ModelState.AddModelError("SelectedSeats", "Мора да одберете барем едно седиште.");

            var movie = _context.Movies.Find(ticket.MovieId);
            if (movie == null)
            {
                ModelState.AddModelError("MovieId", "Невалиден филм.");
            }
            else
            {
                if (ticket.SelectedDay < DateTime.Today)
                    ModelState.AddModelError("SelectedDay", "Не можете да одберете ден во минатото.");

                if (movie.ReleaseDate > DateTime.Today &&
                    ticket.SelectedDay < movie.ReleaseDate)
                {
                    ModelState.AddModelError("SelectedDay",
                        "Не можете да одберете ден пред почетокот на филмот.");
                }

                if (movie.IsForAdults == true &&
                    ticket.AgeCategory == "Under18")
                {
                    ModelState.AddModelError("",
                        "Не можете да купите билет за овој филм бидејќи е 18+.");
                }
            }

            if (ticket.SelectedDay == DateTime.Today &&
                !string.IsNullOrWhiteSpace(ticket.SelectedTime) &&
                DateTime.TryParse(ticket.SelectedTime, out var timePart))
            {
                var now = DateTime.Now;
                var chosen = new DateTime(now.Year, now.Month, now.Day,
                                          timePart.Hour, timePart.Minute, 0);
                if (chosen < now)
                    ModelState.AddModelError("SelectedTime",
                        "Избраната проекција е веќе помината денес.");
            }

            if (!string.IsNullOrWhiteSpace(ticket.SelectedSeats))
            {
                var existingTickets = _context.Tickets.Where(t =>
                    t.MovieId == ticket.MovieId &&
                    DbFunctions.TruncateTime(t.SelectedDay) == DbFunctions.TruncateTime(ticket.SelectedDay) &&
                    t.SelectedTime == ticket.SelectedTime
                ).ToList();

                var taken = existingTickets
                    .SelectMany(t => (t.SelectedSeats ?? "").Split(','))
                    .ToList();

                var overlaps = ticket.SelectedSeats
                    .Split(',')
                    .Intersect(taken)
                    .ToList();

                if (overlaps.Any())
                    ModelState.AddModelError("",
                        "Следните седишта се веќе зафатени: " + string.Join(", ", overlaps));
            }

            if (!ModelState.IsValid)
            {
                return View(ticket);
            }

            ticket.TotalPrice = ticket.SelectedSeats
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Sum(seatId =>
                {
                    var rowStr = seatId.Split('C')[0].Substring(1);
                    return int.TryParse(rowStr, out var row) && row <= 2
                        ? 500 : 250;
                });

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var pdfBytes = new TicketPdfGenerator().GeneratePdf(ticket);

            await TicketEmailService.SendTicketConfirmationAsync(
                to: ticket.Email,
                subject: "ВАШИТЕ КИНО БИЛЕТИ",
                body: "Ви благодариме за купувањето! Во прилог ги испраќаме вашите билети и инструкциите за плаќање.",
                attachmentBytes: pdfBytes,
                attachmentName: "MovieTickets.pdf"
            );

            TempData["SuccessMessage"] = "Ви благодариме! Билети се испратени на вашата e-маил адреса.";

            return RedirectToAction("Summary", new { id = ticket.Id });
        }

        // GET: Tickets/Summary
        public ActionResult Summary(int id)
        {
            var ticket = _context.Tickets
                .Include(t => t.Movie)
                .SingleOrDefault(t => t.Id == id);

            if (ticket == null) return HttpNotFound();

            return View(ticket);
        }
    }
}