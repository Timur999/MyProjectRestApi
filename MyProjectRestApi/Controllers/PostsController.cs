using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;
using MyProjectRestApi.Models.Entity_Types;

namespace MyProjectRestApi.Controllers
{
    [Authorize]
    public class PostsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Posts
        [Route("api/getall")]
        [Authorize(Roles = "Admin")]
        public IQueryable<PostDTO> GetGroupPosts()
        {
            return db.GroupPosts
                .Select(p => new PostDTO()
                {
                    Text = p.Text,
                    DateOfPublication = p.DateOfPublication
                });
        }

        // GET: api/Posts/5
        [ResponseType(typeof(GroupPost))]
        public async Task<IHttpActionResult> GetGroupPost(int id)
        {
            GroupPost groupPost = await db.GroupPosts.FindAsync(id);
            if (groupPost == null)
            {
                return NotFound();
            }

            return Ok(groupPost);
        }

        // GET: api/PostByGroup/5
        [ResponseType(typeof(PostDTO))]
        [Route("api/PostByGroup/{id:int}")]
        public async Task<IHttpActionResult> GetGroupPostByGroup(int id)
        {
            List<PostDTO> posts = null;
            string currentUser = GetCurrentUserId();
            if (IsUserBelongToGroup(id, currentUser))
            {
                posts = await (from post in db.GroupPosts
                               where (post.Blog.BLogId == id)
                               select new PostDTO()
                               {
                                   Id = post.Id,
                                   Text = post.Text,
                                   DateOfPublication = post.DateOfPublication,
                                   UserName = post.User.UserName,
                                   IsPostOwner = currentUser == post.User.Id
                               }).ToListAsync();
            }

            if (posts == null)
            {
                return NotFound();
            }

            return Ok(posts);
        }
        // GET: api/Posts/5/10
        [ResponseType(typeof(PostDTO))]
        //[Route("api/PostByGroup/{id:int}")]
        public async Task<IHttpActionResult> GetGroupPost(int id, int numberPost)
        {
            const int numberOfPostsToDownloaded = 10;
            numberPost = numberPost * numberOfPostsToDownloaded;
            List<PostDTO> posts = null;
            int postsListLength = 0;
            string currentUser = GetCurrentUserId();
            if (IsUserBelongToGroup(id, GetCurrentUserId()))
            {
                posts = await (from post in db.GroupPosts
                               where (post.Blog.BLogId == id)
                               select new PostDTO()
                               {
                                   Id = post.Id,
                                   Text = post.Text,
                                   DateOfPublication = post.DateOfPublication,
                                   UserName = post.User.UserName,
                                   IsPostOwner = currentUser == post.User.Id,
                                   ImagePath = post.Image.ImagePath,
                                   ImageName = post.Image.ImageName
                               })
                               .OrderByDescending(post => post.DateOfPublication)
                               .Skip(numberPost)
                               .Take(numberOfPostsToDownloaded).ToListAsync();

                postsListLength = await (from post in db.GroupPosts
                                         where (post.Blog.BLogId == id)
                                         select new PostDTO()).CountAsync();
            }
            else
            {
                return Content(HttpStatusCode.Forbidden, "An error occurred, please try again or contact the administrator.");
            }

            if (posts == null)
            {
                return Content(HttpStatusCode.NotFound, "there aren't any post in this group");
            }

            return Ok(posts);
        }

        // PUT: api/Posts/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutGroupPost(int id, GroupPost groupPost)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //TODO: check userId from GroupPost and compare with current User (Only post owner can update your post)

            if (id != groupPost.Id)
            {
                return BadRequest();
            }
            GroupPost post = await db.GroupPosts.FindAsync(id);
            post.DateOfPublication = DateTime.Now;
            post.Text = groupPost.Text;

            db.Entry(post).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GroupPostExists(id))
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

        [Route("api/PostImage")]
        [ResponseType(typeof(PostDTO))]
        public async Task<IHttpActionResult> PostImage()
        {

            string imageName = null;
            ApplicationImage image = null;
            var httpRequest = HttpContext.Current.Request;

            var postedFile = httpRequest.Files["Image"];
            var postedText = httpRequest["Text"];
            int postedBlogId = int.Parse(httpRequest["BlogId"]);

            if (postedFile != null)
            {
                image = SaveImage(postedFile);
            }
            else
            {
                //postedFile.InputStream.Close();
            }

            string result = await SaveImageToDatabase(image, postedText, postedBlogId);

            return StatusCode(HttpStatusCode.NoContent);

        }

        // POST: api/Posts
        [ResponseType(typeof(PostDTO))]
        public async Task<IHttpActionResult> PostGroupPost(PostDTO postDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Blog blog = await db.Blogs.FindAsync(postDTO.BlogId);
            if (blog == null)
            {
                return BadRequest();
            }
            string userId = GetCurrentUserId();

            bool isBelongToGroup = IsUserBelongToGroup(blog.BLogId,userId);
            if (!isBelongToGroup)
            {
                return Content(HttpStatusCode.Unauthorized, "Unauthorized");
            }

            ApplicationUser currentUser = db.Users.Find(userId);
            GroupPost groupPost = new GroupPost(postDTO.Text, blog, currentUser);

            db.GroupPosts.Add(groupPost);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = groupPost.Id }, postDTO);
        }

        // DELETE: api/Posts/5
        [ResponseType(typeof(GroupPost))]
        public async Task<IHttpActionResult> DeleteGroupPost(int id)
        {
            GroupPost groupPost = await db.GroupPosts.FindAsync(id);
            if (groupPost == null)
            {
                return NotFound();
            }

            groupPost.Blog = null;

            db.GroupPosts.Remove(groupPost);
            await db.SaveChangesAsync();

            return Ok(groupPost);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GroupPostExists(int id)
        {
            return db.GroupPosts.Count(e => e.Id == id) > 0;
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

        private ApplicationImage SaveImage (HttpPostedFile postedFile)
        {

            string imageName = null;
            //create custome filename
            imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
            var filePath = HttpContext.Current.Server.MapPath("~/Images/" + imageName);
            try
            {
                postedFile.SaveAs(filePath);
                postedFile.InputStream.Flush();
            }
            catch (Exception ex)
            {

            }
            finally{
                postedFile.InputStream.Close();
                
            }
            ApplicationImage image = new ApplicationImage()
            {
                ImageName = imageName,
                ImagePath = filePath
            };

            return image;
        }

        private async Task<string> SaveImageToDatabase(ApplicationImage image, string postedText, int blogId)
        {
     
            using (ApplicationDbContext con = db)
            {
                Blog blog = await con.Blogs.FindAsync(blogId);
                if (blog == null)
                {
                    return "BadRequest";
                }
                string userId = GetCurrentUserId();

                bool isBelongToGroup = IsUserBelongToGroup(blog.BLogId, userId);
                if (!isBelongToGroup)
                {
                    return "Unauthorized";
                }

                ApplicationUser currentUser = con.Users.Find(userId);
                GroupPost groupPost = null;
                PostDTO postDTO = null;
                if (image != null)
                {
                    groupPost = new GroupPost(postedText, image, blog, currentUser);
                    postDTO = new PostDTO()
                    {
                        Text = groupPost.Text,
                        DateOfPublication = groupPost.DateOfPublication,
                        BlogId = blog.BLogId,
                        UserName = currentUser.UserName,
                        ImagePath = image.ImagePath,
                        ImageName = image.ImageName,
                        IsPostOwner = true
                    };
                }
                else
                {
                    groupPost = new GroupPost(postedText, blog, currentUser);
                    postDTO = new PostDTO()
                    {
                        BlogId = blog.BLogId,
                        DateOfPublication = groupPost.DateOfPublication,
                        Text = groupPost.Text,
                        UserName = currentUser.UserName,
                        IsPostOwner = true
                    };
                }

                try
                {
                    con.GroupPosts.Add(groupPost);
                    await db.SaveChangesAsync();
                }catch(Exception ex)
                {
                    return ex.Message;
                }
       

                return "Success";
            }
        }
  
    }
}