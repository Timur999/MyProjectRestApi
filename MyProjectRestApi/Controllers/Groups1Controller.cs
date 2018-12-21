using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using System.Web.Http.OData;
using System.Web.Http.OData.Routing;
using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;
using MyProjectRestApi.Models.Entity_Types;
using MyProjectRestApi.Infrastructure;

namespace MyProjectRestApi.Controllers
{
    /*
    Aby można było dodać trasę dla tego kontrolera, klasa WebApiConfig może wymagać dodatkowych zmian. Dołącz te instrukcje do metody Register klasy WebApiConfig. W adresach URL OData jest uwzględniana wielkość liter.

    using System.Web.Http.OData.Builder;
    using System.Web.Http.OData.Extensions;
    using MyProjectRestApi.Models.Entity_Types;
    ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
    builder.EntitySet<Group>("Groups");
    builder.EntitySet<Blog>("Blogs"); 
    builder.EntitySet<ApplicationUser>("Users"); 
    config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
    */
    public class Groups1Controller : ODataController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: odata/Groups
        [EnableQuery]
        public IQueryable<GroupDTO> GetGroups()
        {
            return db.Groups
                .Select( g => new GroupDTO() {
                    GroupsName = g.GroupsName,
                    AdminGroupId = g.AdminGroupId,
                    DateOfCreatedGroup = g.DateOfCreatedGroup
                });
        }

        // GET: odata/Groups(5)
        [EnableQuery]
        public SingleResult<GroupDTO> GetGroup([FromODataUri] int key)
        {
            return SingleResult.Create(db.Groups
                .Select(g => new GroupDTO() {
                    Id = g.Id,
                    GroupsName = g.GroupsName,
                    AdminGroupId = g.AdminGroupId,
                    DateOfCreatedGroup = g.DateOfCreatedGroup
                }).Where(group => group.Id == key));
        }

        // PUT: odata/Groups(5)
        public IHttpActionResult Put([FromODataUri] int key, Delta<Group> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Group group = db.Groups.Find(key);
            if (group == null)
            {
                return NotFound();
            }

            patch.Put(group);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(group);
        }

        // POST: odata/Groups
        //[ResponseType(typeof(GroupDTO))]
        //[ModelBinder(typeof(GroupModelBinder))] Group group
        public IHttpActionResult Post([FromBody]Group group)
        {
            // TODO: create modelbinder or something similar to bind group using constructor with param
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(ModelState);
            }
            group.CreateBlog();
            db.Groups.Add(group);
            db.SaveChanges();

            return Created(group);
        }

        // PATCH: odata/Groups(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public IHttpActionResult Patch([FromODataUri] int key, Delta<Group> patch)
        {
            Validate(patch.GetEntity());

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Group group = db.Groups.Find(key);
            if (group == null)
            {
                return NotFound();
            }

            patch.Patch(group);

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupExists(key))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Updated(group);
        }

        // DELETE: odata/Groups(5)
        public IHttpActionResult Delete([FromODataUri] int key)
        {
            Group group = db.Groups.Find(key);
            Blog blog = db.Blogs.Find(key);
            if (group == null && blog == null)
            {
                return NotFound();
            }
            db.Blogs.Remove(blog);
            db.Groups.Remove(group);
            db.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/Groups(5)/Blog
        [EnableQuery]
        public SingleResult<Blog> GetBlog([FromODataUri] int key)
        {
            return SingleResult.Create(db.Groups.Where(m => m.Id == key).Select(m => m.Blog));
        }

        // GET: odata/Groups(5)/Users
        [EnableQuery]
        public IQueryable<ApplicationUserDTO> GetUsers([FromODataUri] int key)
        {
            return db.Groups.Where(m => m.Id == key).SelectMany(m => m.Users).Select(m => new ApplicationUserDTO() { UserName = m.UserName });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GroupExists(int key)
        {
            return db.Groups.Count(e => e.Id == key) > 0;
        }
    }
}
