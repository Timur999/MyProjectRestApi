using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;

namespace MyProjectRestApi.Controllers
{
    public class UsersController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Users
        public IQueryable<ApplicationUser> GetUsers()
        {
            return db.Users;
        }

        // GET: api/GetUserId
        [Route("api/GetUserId")]
        public async Task<IHttpActionResult> GetUserId()
        {
            string currentUserId = GetCurrentUserId();
            return Ok(currentUserId);
        }

        // GET: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        public async Task<IHttpActionResult> GetUserById(string id)
        {
            ApplicationUser applicationUser = db.Users.Find(id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            return Ok(applicationUser);
        }

        // GET: api/getshortlistusers
        [Route("api/getshortlistusers")]
        [ResponseType(typeof(List<ApplicationUserDTO>))]
        public async Task<IHttpActionResult> GetShortListUsers()
        {
            string currentUserId = GetCurrentUserId();
            List<ApplicationUserDTO> applicationUserList = await db.Users.Where(m => m.Id != currentUserId).
                Select(m => new ApplicationUserDTO() { Id = m.Id, UserName = m.UserName }).Take(10).ToListAsync();
            if (applicationUserList == null || applicationUserList.Count < 1)
            {
                return NotFound();
            }

            return Ok(applicationUserList);
        }

        [Route("api/getusersbyname/{userName}")]
        [ResponseType(typeof(ApplicationUser))]
        public async Task<IHttpActionResult> GetUserByName(string userName)
        {
            string currentUserId = GetCurrentUserId();
            List<ApplicationUserDTO> applicationUserList = await db.Users.Where(m => m.UserName.Contains(userName)
            && m.Id != currentUserId).
                Select(m => new ApplicationUserDTO() { Id = m.Id, UserName = m.UserName }).
                Take(10).ToListAsync();
            if (applicationUserList == null || applicationUserList.Count < 1)
            {
                return NotFound();
            }

            return Ok(applicationUserList);
        }

        // PUT: api/Users/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutApplicationUser(string id, ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != applicationUser.Id)
            {
                return BadRequest();
            }

            db.Entry(applicationUser).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApplicationUserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Users
        [ResponseType(typeof(ApplicationUser))]
        public async Task<IHttpActionResult> PostApplicationUser(ApplicationUser applicationUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Users.Add(applicationUser);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ApplicationUserExists(applicationUser.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtRoute("DefaultApi", new { id = applicationUser.Id }, applicationUser);
        }

        // DELETE: api/Users/5
        [ResponseType(typeof(ApplicationUser))]
        public async Task<IHttpActionResult> DeleteApplicationUser(string id)
        {
            ApplicationUser applicationUser = db.Users.Find (id);
            if (applicationUser == null)
            {
                return NotFound();
            }

            db.Users.Remove(applicationUser);
            await db.SaveChangesAsync();

            return Ok(applicationUser);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ApplicationUserExists(string id)
        {
            return db.Users.Count(e => e.Id == id) > 0;
        }

        private string GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            return user.Id;
        }
    }
}