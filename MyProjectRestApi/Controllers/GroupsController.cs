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
    [Authorize]
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
            GroupDTO group = await db.Groups
                .Where(g => g.Id == id)
                .Select(g => new GroupDTO()
                {
                    Id = g.Id,
                    GroupsName = g.GroupsName,
                    AdminGroupId = g.AdminGroupId,
                    DateOfCreatedGroup = g.DateOfCreatedGroup
                }).FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound();
            }

            return Ok(group);
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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(ModelState);
            }
            Group group = new Group(groupDto.GroupsName);
            group.CreateBlog();
            db.Groups.Add(group);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = group.Id }, new GroupDTO() { Id = group.Id, GroupsName = group.GroupsName });
        }

        // DELETE: api/Groups/5
        //Remove group and releated post
        [ResponseType(typeof(Group))]
        public async Task<IHttpActionResult> DeleteGroup(int id)
        {
            Blog blog = db.Blogs.Find(id);
            Group group = await db.Groups.FindAsync(id);
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
        public IQueryable<ApplicationUserDTO> GetUsersBelongToGroup(int id)
        {
            return db.Groups
                .Where(m => m.Id == id)
                .SelectMany(m => m.Users)
                .Select(m => new ApplicationUserDTO() { UserName = m.UserName });
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
        public IQueryable<GroupDTO> GetFiveGroupsBelongToUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            if (user == null)
            {
                var msg = new HttpResponseMessage(HttpStatusCode.Unauthorized) { ReasonPhrase = "Oops!!!" };
                throw new HttpResponseException(msg);
            }

            return db.Users
                .Where(m => m.Id == user.Id)
                .SelectMany(m => m.Groups)
                .Select(m => new GroupDTO() { Id = m.Id, GroupsName = m.GroupsName })
                .OrderBy(g => g.GroupsName).Take(5);
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
    }
}