using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;
using MyProjectRestApi.Models.Entity_Types;
using Microsoft.AspNet.Identity.Owin;
using System.Security.Claims;

namespace MyProjectRestApi.Controllers
{
    //[Authorize]
    public class GroupsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Groups
        public IQueryable<GroupDTO> GetGroups()
        {
            return db.Groups
                .Select(g => new GroupDTO()
                {
                    GroupsName = g.GroupsName,
                    AdminGroupId = g.AdminGroupId,
                    DateOfCreatedGroup = g.DateOfCreatedGroup
                });
        }

        // GET: api/Groups/5
        [ResponseType(typeof(GroupDTO))]
        public async Task<IHttpActionResult> GetGroup(int id)
        {
            string currentUser = GetCurrentUserId();
            if (!IsUserBelongToGroup(id, currentUser))
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission");
            }
            GroupDTO group = await db.Groups
                .Where(g => g.Id == id)
                .Select(g => new GroupDTO()
                {
                    Id = g.Id,
                    GroupsName = g.GroupsName,
                    AdminGroupId = g.AdminGroupId,
                    DateOfCreatedGroup = g.DateOfCreatedGroup,
                    IsAdmin = g.AdminGroupId == currentUser ? true : false
                }).FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
        }


        [ResponseType(typeof(void))]
        [Route("api/LeaveGroup/{id:int}")]
        public async Task<IHttpActionResult> PutLeaveGroup(int id)
        {
            string currentUserId = GetCurrentUserId();

            if (!IsUserBelongToGroup(id, currentUserId))
                return Content(HttpStatusCode.Forbidden, "You have not permission");

            Group group = await db.Groups.FindAsync(id);

            if (group == null)
                return Content(HttpStatusCode.NotFound, "Group not exist");

            if (group.AdminGroupId == currentUserId)
            {
                List<GroupPost> listPost = db.Blogs.Where(m => m.BLogId == id).SelectMany(m => m.GroupPost).ToList();
                listPost.ForEach(p => db.GroupPosts.Remove(p));
                Blog blog = db.Blogs.Find(id);
                db.Blogs.Remove(blog);
                db.Groups.Remove(group);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }

                return Ok();
            }
            else
            {
                ApplicationUser user = db.Users.Find(currentUserId);
                group.Users.Remove(user);

                try
                {
                    await db.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return InternalServerError(ex);
                }
                return Ok();
            }

        }

        // PUT: api/Groups/5
        //Update group and if list of user from groupWithUserDTO is not null, then remove user from group and related posts
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGroup(int id, GroupWithUserDTO groupWithUserDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //TODO: if not admin group return NotFound() or you have permision to do that
            Group group = await db.Groups.FindAsync(id);
            Blog groupBlog = db.Blogs.Find(id);
            if (group == null || id != groupWithUserDTO.Id)
            {
                return BadRequest();
            }

            if (groupWithUserDTO.UsersIdToRemoveFromGroup.Count > 0)
            {
                foreach (string userId in groupWithUserDTO.UsersIdToRemoveFromGroup)
                {
                    ApplicationUser userBelongToGroup = (from groups in db.Groups
                                                         where (groups.Id == id)
                                                         let userInGroup = groups.Users.Where(user => user.Id == userId)
                                                         where userInGroup.Any()
                                                         select userInGroup.FirstOrDefault()).FirstOrDefault();

                    if (userBelongToGroup == null)
                        return BadRequest();

                    group.Users.Remove(userBelongToGroup);

                    List<GroupPost> posts = (from blog in db.Blogs
                                             where (blog.BLogId == id)
                                             let usersPostsByGroup = blog.GroupPost.Where(post => post.User.Id == userId)
                                             where usersPostsByGroup.Any()
                                             select usersPostsByGroup.ToList()).FirstOrDefault();

                    posts.ForEach(item => db.GroupPosts.Remove(item));
                }
            }

            if (!string.IsNullOrEmpty(groupWithUserDTO.GroupsName))
                group.GroupsName = groupWithUserDTO.GroupsName;

            if (!string.IsNullOrEmpty(groupWithUserDTO.AdminGroupId))
                group.AdminGroupId = groupWithUserDTO.AdminGroupId;

            db.Entry(group).State = EntityState.Modified;
            db.Entry(groupBlog).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(id))
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

        // POST: api/Groups
        [ResponseType(typeof(GroupDTO))]
        public async Task<IHttpActionResult> PostGroup(GroupDTO groupDto)
        {
            string currentUserId = GetCurrentUserId();
            ApplicationUser user = db.Users.Find(currentUserId);
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(ModelState);
            }
            List<ApplicationUser> users = new List<ApplicationUser>() { user };
            Group group = new Group(groupDto.GroupsName, users);
            db.Groups.Add(group);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = group.Id },
                new GroupDTO() { Id = group.Id, GroupsName = group.GroupsName, AdminGroupId = group.AdminGroupId, IsAdmin = true });
        }

        // DELETE: api/Groups/5
        //Remove group and releated post
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> DeleteGroup(int id)
        {
            string currentUserId = GetCurrentUserId();
            Blog blog = db.Blogs.Find(id);
            Group group = await db.Groups.FindAsync(id);

            if (group.AdminGroupId == currentUserId)
                return Content(HttpStatusCode.Forbidden, "You have not permission");

            if (blog == null || group == null)
            {
                return NotFound();
            }

            //TODO: if current user is not admin group return NotFound()
            List<GroupPost> listPost = db.Blogs.Where(m => m.BLogId == blog.BLogId).SelectMany(m => m.GroupPost).ToList();

            listPost.ForEach(p => db.GroupPosts.Remove(p));
            db.Blogs.Remove(blog);
            db.Groups.Remove(group);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
            return Ok(group);
        }

        // GET: api/Groups/5/Users
        [Route("api/Groups/{id:int}/Users")]
        [ResponseType(typeof(List<ApplicationUserDTO>))]
        public async Task<IHttpActionResult> GetUsersBelongToGroup(int id)
        {
            string currentUserId = GetCurrentUserId();

            if (!IsUserBelongToGroup(id, currentUserId))
                return Content(HttpStatusCode.Forbidden, "You have not permission");

            string groupAdminId = db.Groups.Where(g => g.Id == id).Select(m => m.AdminGroupId).FirstOrDefault();

            List<ApplicationUserDTO> usersDto = await db.Groups
                .Where(m => m.Id == id)
                .SelectMany(m => m.Users)
                .Select(m => new ApplicationUserDTO() {
                    UserName = m.UserName ,
                    Role = groupAdminId == currentUserId ? "Admin" : "User"}).ToListAsync();

            return Ok(usersDto);
        }

        // GET: api/Groups/Users?userId=iasjdoajdiasdlji3o2j4
        public IQueryable<GroupDTO> GetGroupsBelongToUser(string userId)
        {
            return db.Users
                .Where(m => m.Id == userId)
                .SelectMany(m => m.Groups)
                .Select(m => new GroupDTO() { Id = m.Id, GroupsName = m.GroupsName });
        }

        // GET: api/Groups/Users?userId=iasjdoajdiasdlji3o2j4
        [Route("api/fivegroups")]
        [ResponseType(typeof(GroupDTO))]
        public async Task<IHttpActionResult> GetFiveGroupsBelongToUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            if (user == null)
            {
                return Content(HttpStatusCode.Forbidden, "An error occurred, please try again or contact the administrator.");
                //var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Oops!!!" };
                //throw new HttpResponseException(msg);
            }

            List<GroupDTO> shortListOfGroup = await db.Users
                .Where(m => m.Id == user.Id)
                .SelectMany(m => m.Groups)
                .Select(m => new GroupDTO() { Id = m.Id, GroupsName = m.GroupsName })
                .OrderBy(g => g.GroupsName).Take(5).ToListAsync();

            return Ok(shortListOfGroup);
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GroupExists(int id)
        {
            return db.Groups.Count(e => e.Id == id) > 0;
        }

        private bool IsUserBelongToGroup(int groupId, string userId)
        {
            int hasUserAlreadyExistInGroup = (from g in db.Groups
                                              where (g.Id == groupId)
                                              let userInGroup = g.Users.Where(u => u.Id == userId)
                                              where userInGroup.Any()
                                              select userInGroup.Count()).FirstOrDefault();

            return hasUserAlreadyExistInGroup > 0 ? true : false;

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