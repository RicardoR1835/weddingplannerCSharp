using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using weddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace weddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private HomeContext dbContext;
     
        // here we can "inject" our context service into the constructorcopy
        public HomeController(HomeContext context)
        {
            dbContext = context;
        }

        DateTime Now = DateTime.Now;

        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("create")]
        public IActionResult Create(User newUser)
        {
            if(ModelState.IsValid)
            {
                if(dbContext.Users.Any(user => user.Email == newUser.Email))
                {
                    ModelState.AddModelError("NewUser.Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    Console.WriteLine(newUser.UserId);
                    HttpContext.Session.SetInt32("Id", newUser.UserId);
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password); 
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    Console.WriteLine(newUser.UserId);
                    return Redirect($"dash/{HttpContext.Session.GetInt32("Id")}");
                }
            }
            return View("Index");
        }

        [HttpPost("login")]
        public IActionResult Login(LogUser LoggedUser)
        {

            if(ModelState.IsValid)
            {
                var confirmUser = dbContext.Users.FirstOrDefault(user => user.Email == LoggedUser.Email);
                if(confirmUser == null)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                
                var hasher = new PasswordHasher<LogUser>();
                
                var result = hasher.VerifyHashedPassword(LoggedUser, confirmUser.Password, LoggedUser.Password);
                
                if(result == 0)
                {
                    ModelState.AddModelError("Email", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    HttpContext.Session.SetInt32("Id", confirmUser.UserId);
                    return Redirect($"dash/{confirmUser.UserId}");
                }
            }
            return View("Index");

        }

        [HttpGet("dash/{id}")]
        public IActionResult Dash(int id)
        {
            if (HttpContext.Session.GetInt32("Id") == null)
            {
                ModelState.AddModelError("Email", "Please log in or register!");
                return View("Index");
            }
            if (HttpContext.Session.GetInt32("Id") != id)
            {
                return Redirect($"/account/{HttpContext.Session.GetInt32("Id")}");
            }
            User loggedUser = dbContext.Users
            .FirstOrDefault(user => user.UserId == id);

            var WeddingsAndGuests = dbContext.Weddings
            .Include(wedding => wedding.GuestList)
            .ThenInclude(g => g.Guest)
            .ToList();

            ViewBag.LoggedIn = loggedUser;

            return View("Dashboard", WeddingsAndGuests);

        }

        [HttpGet("wedding/new")]
        public IActionResult NewWedding()
        {
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View("NewWedding");
        }

        [HttpPost("create/wedding")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            if(ModelState.IsValid)
            {
                if (newWedding.Date < Now)
                {
                    ModelState.AddModelError("Date", "You cant go back in time!?");
                    ViewBag.Id = HttpContext.Session.GetInt32("Id");
                    return View("NewWedding");
                }
                Console.WriteLine("************************");
                User Creator = dbContext.Users.FirstOrDefault(user => user.UserId == newWedding.UserId);
                newWedding.UserId = Creator.UserId;
                newWedding.Creator = Creator;
                dbContext.Add(newWedding);
                dbContext.SaveChanges();
                return Redirect($"/dash/{newWedding.UserId}");
            }
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View("NewWedding");
        }

        [HttpGet("{id}")]
        public IActionResult Wedding(int id)
        {
            var ThisWedding = dbContext.Weddings
            .FirstOrDefault(wed => wed.WeddingId == id);
            
            List<RSVP> GuestList = dbContext.Rsvps
            .Where(thiswed => thiswed.WeddingId == id)
            .Include(guest => guest.Guest)
            .ToList();
            ViewBag.Guestlist = GuestList;
            ViewBag.Id = HttpContext.Session.GetInt32("Id");
            return View("Wedding", ThisWedding);
        }

        [HttpGet("rsvp/{id}")]
        public IActionResult Rsvp(int id)
        {
            Wedding WedToRsvp = dbContext.Weddings
            .FirstOrDefault(thiswed => thiswed.WeddingId == id);

            User UserRsvp = dbContext.Users
            .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("Id"));

            RSVP NewRsvp = new RSVP();
            NewRsvp.UserId = UserRsvp.UserId;
            NewRsvp.WeddingId = WedToRsvp.WeddingId;
            NewRsvp.Guest = UserRsvp;
            NewRsvp.UpcomingWedding = WedToRsvp;
            dbContext.Add(NewRsvp);
            dbContext.SaveChanges();
            return Redirect($"/dash/{HttpContext.Session.GetInt32("Id")}");

        }

        [HttpGet("unrsvp/{id}")]
        public IActionResult UnRsvp(int id)
        {
            Wedding WedToUnRsvp = dbContext.Weddings
            .FirstOrDefault(thiswed => thiswed.WeddingId == id);

            User UserUnRsvp = dbContext.Users
            .FirstOrDefault(user => user.UserId == HttpContext.Session.GetInt32("Id"));

            List<RSVP> Rsvps = dbContext.Rsvps
            .Where(rs => rs.WeddingId == WedToUnRsvp.WeddingId)
            .ToList();
            RSVP Unrsvp = Rsvps.FirstOrDefault(user => user.UserId == UserUnRsvp.UserId);
            dbContext.Remove(Unrsvp);
            dbContext.SaveChanges();
            return Redirect($"/dash/{HttpContext.Session.GetInt32("Id")}");
        }

        [HttpGet("delete/{id}")]
        public IActionResult Destroy(int id)
        {
            Wedding thisWedding = dbContext.Weddings
            .FirstOrDefault(wed => wed.WeddingId == id);
            dbContext.Remove(thisWedding);
            dbContext.SaveChanges();
            return Redirect($"/dash/{HttpContext.Session.GetInt32("Id")}");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

    }
}
