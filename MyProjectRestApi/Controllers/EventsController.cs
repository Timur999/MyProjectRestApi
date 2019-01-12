using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;
using MyProjectRestApi.Models.Entity_Types;
using MyProjectRestApi.Models.Helper;

namespace MyProjectRestApi.Controllers
{
    public class EventsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // Create storagecredentials object by reading the values from the configuration (appsettings.json)
        private static StorageCredentials storageCredentials = new StorageCredentials(StorageAzureHelper._storageConfig.AccountName, StorageAzureHelper._storageConfig.AccountKey);

        // Create cloudstorage account by passing the storagecredentials
        private static CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, true);


        [ResponseType(typeof(List<EventDTO>))]
        public async Task<IHttpActionResult> GetEvents()
        {
            string currentUserId = GetCurrentUserId();
            List<EventDTO> result = await (from e in db.Events
                                           let eventDto = new EventDTO
                                           {
                                               Id = e.Id,
                                               EventName = e.Name,
                                               EventDate = e.EventDate,
                                               Users = e.Users.Select(u => new ApplicationUserDTO() { UserName = u.UserName }).ToList(),
                                               IsAdmin = currentUserId == e.AdminId,
                                               IsSubscriber = (e.Users.Where(u => u.Id == currentUserId).FirstOrDefault() != null)
                                           }
                                           select (eventDto)).ToListAsync();

            return Ok(result);
        }



        // GET: api/Events
        [ResponseType(typeof(EventDTO))]
        [Route("api/Events/{skipEventPost:int}")]
        public async Task<IHttpActionResult> GetEvents(int skipEventPost)
        {
            int eventsPostListLength = db.Events.Count();

            string currentUserId = GetCurrentUserId();
            List<EventDTO> eventDTOs = new List<EventDTO>() { };
            try
            {
                string adminUserName = db.Users.Find(currentUserId).UserName;

                eventDTOs = await db.Events
                 .Select(eventDto => new EventDTO()
                 {
                     Id = eventDto.Id,
                     UserName = adminUserName,
                     IsAdmin = currentUserId == eventDto.AdminId,
                     EventDate = eventDto.EventDate,
                     EventName = eventDto.Name,
                     Text = eventDto.Text,
                     ImageName = eventDto.Image.ImageName,
                     ImagePath = eventDto.Image.ImagePath,
                     Users = eventDto.Users.Select(user => new ApplicationUserDTO()
                     { Id = user.Id, UserName = user.UserName }).ToList(),
                     ArrayLength = eventsPostListLength
                 }).OrderByDescending(e => e.Id)
                  .Skip(skipEventPost * 10)
                  .Take(10).ToListAsync();
            }
            catch (Exception ex) { }
            //Adding image to Event if exist
            foreach (var item in eventDTOs)
            {
                item.Base64StringImage = !String.IsNullOrEmpty(item.ImagePath) ? FromAzureToBase64(item.ImagePath, storageAccount) : null;
            }

            return Ok(eventDTOs);
        }

        // GET: api/Events/5
        [Route("api/SubscriptionEvents/{id:int}")]
        [ResponseType(typeof(Event))]
        public async Task<IHttpActionResult> PutEvent(int id)
        {
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            string currentUserId = GetCurrentUserId();
            ApplicationUser user = db.Users.Find(currentUserId);

            int isUserSubsribe = (from e in db.Events
                                  where (e.Id == id)
                                  let subscription = e.Users.Where(u => u.Id == currentUserId)
                                  .Select(us => new ApplicationUserDTO() { Id = us.Id, UserName = us.UserName })
                                  select (subscription)).FirstOrDefault().Count();


            ApplicationUserDTO userDTO = new ApplicationUserDTO()
            {
                Id = user.Id,
                UserName = user.UserName
            };
            if (isUserSubsribe != 0)
            {
                @event.Users.Remove(user);
                userDTO.IsSubscribe = false;
            }
            else
            {
                @event.Users.Add(user);
                userDTO.IsSubscribe = true;
            }
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Ok(userDTO);
        }

        // PUT: api/Events/5
        [Route("api/Events/{id:int}")]
        [ResponseType(typeof(EventDTO))]
        public async Task<IHttpActionResult> PutEditEvent(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string currentUserId = GetCurrentUserId();
            EventImage image = null;
            var httpRequest = HttpContext.Current.Request;

            var postedEventFile = httpRequest.Files["Image"];
            string postedEventText = httpRequest["Text"];
            string sPostedEventDate = httpRequest["EventDate"];
            string postedEventName = httpRequest["EventName"];
            DateTime postedEventDate = DateTime.Now;
            try
            {
                int index = sPostedEventDate.IndexOf("GMT+");
                sPostedEventDate = sPostedEventDate.Remove(index);
                postedEventDate = DateTime.Parse(sPostedEventDate);
            }
            catch (Exception ex) { }

            Event editEvent = db.Events.Find(id);
            if (editEvent.AdminId != currentUserId)
            {
                return Content(HttpStatusCode.Unauthorized, "An error occurred, please try again or contact the administrator.");
            }


            if (postedEventFile != null)
            {
                //EventImages and Event is relathionship 1 - 1, so eventId equals ImageId
                EventImage oldImage = await db.EventImages.FindAsync(id);
                if (oldImage != null)
                {
                    try
                    {
                        string filePath = @"D:\project folder\MyProjectRestApi\MyProjectRestApi\Image\";  //TRy directly add path from oldImage
                        string fileName = oldImage.ImagePath;
                        string fullPath = Path.Combine(filePath, fileName);
                        await StorageAzureHelper.DeleteFileToStorage(oldImage.ImageName, StorageAzureHelper._storageConfig);
                        //File.Delete(oldImage.ImagePath);
                        db.EventImages.Remove(oldImage);
                        await db.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return Content(HttpStatusCode.InternalServerError, ex.Message);
                    }
                }


                image = await saveFileToStorageAzure(postedEventFile);
                editEvent.Image = image;
            }

            editEvent.Text = postedEventText;
            editEvent.Name = postedEventName;
            editEvent.EventDate = postedEventDate;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            string userName = db.Users.Find(currentUserId).UserName;
            string Base64StringImage = "";
            try
            {
                Base64StringImage = !String.IsNullOrEmpty(editEvent.Image.ImagePath) ? FromAzureToBase64(editEvent.Image.ImagePath, storageAccount) : null;
            }
            catch (Exception ex) { }

            EventDTO eventDto = new EventDTO()
            {
                Id = editEvent.Id,
                EventName = postedEventName,
                UserName = userName,
                AdminId = currentUserId,
                EventDate = postedEventDate,
                Text = postedEventText,
                ImageName = editEvent.Image.ImageName,
                Base64StringImage = Base64StringImage,
                Users = editEvent.Users.Select(user => new ApplicationUserDTO()
                { Id = user.Id, UserName = user.UserName }).ToList(),
                IsAdmin = true
            };

            return Ok(eventDto);
        }

        // POST: api/Events
        [ResponseType(typeof(EventDTO))]
        public async Task<IHttpActionResult> PostEvent()
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string currentUserId = GetCurrentUserId();
            EventImage image = null;
            var httpRequest = HttpContext.Current.Request;

            var postedEventFile = httpRequest.Files["Image"];
            string postedEventText = httpRequest["Text"];
            string sPostedEventDate = httpRequest["EventDate"];
            string postedEventName = httpRequest["EventName"];
            DateTime postedEventDate = DateTime.Now;
            try
            {
                int index = sPostedEventDate.IndexOf("GMT+");
                sPostedEventDate = sPostedEventDate.Remove(index);
                postedEventDate = DateTime.Parse(sPostedEventDate);
            }
            catch (Exception ex) { }


            if (postedEventFile != null)
            {
                image = await saveFileToStorageAzure(postedEventFile);
            }

            Event objEvent = new Event()
            {
                Name = postedEventName,
                AdminId = currentUserId,
                EventDate = postedEventDate,
                Image = image,
                Text = postedEventText
            };

            try
            {
                db.Events.Add(objEvent);
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }

            string userName = db.Users.Find(currentUserId).UserName;

            EventDTO eventDto = null;
            if (objEvent.Image == null)
            {
                eventDto = new EventDTO()
                {
                    Id = objEvent.Id,
                    EventName = postedEventName,
                    UserName = userName,
                    AdminId = currentUserId,
                    EventDate = postedEventDate,
                    Text = postedEventText,
                    IsAdmin = true
                };
            }
            else
            {
                string Base64StringImage = FromAzureToBase64(objEvent.Image.ImagePath, storageAccount);
                eventDto = new EventDTO()
                {
                    Id = objEvent.Id,
                    EventName = postedEventName,
                    UserName = userName,
                    AdminId = currentUserId,
                    EventDate = postedEventDate,
                    Text = postedEventText,
                    ImageName = objEvent.Image.ImageName,
                    Base64StringImage = Base64StringImage,
                    IsAdmin = true
                };
            }

            return Ok(eventDto);
        }

        // DELETE: api/Events/5
        [Route("api/DeleteEvent/{id:int}")]
        [ResponseType(typeof(EventDTO))]
        public async Task<IHttpActionResult> DeleteEvent(int id)
        {
            string currentUserId = GetCurrentUserId();
            Event @event = await db.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }

            if (@event.AdminId != currentUserId)
            {
                return Content(HttpStatusCode.Unauthorized, "An error occurred, please try again or contact the administrator.");
            }
            EventImage img = await db.EventImages.FindAsync(id);
            if (img != null)
            {
                await StorageAzureHelper.DeleteFileToStorage(img.ImageName, StorageAzureHelper._storageConfig);
            }
            @event.Image = null;
            @event.Users = null;
            db.Events.Remove(@event);
            await db.SaveChangesAsync();

            EventDTO eventDto = new EventDTO() { Id = @event.Id };
            return Ok(eventDto);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool EventExists(int id)
        {
            return db.Events.Count(e => e.Id == id) > 0;
        }

        private string GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            return user.Id;
        }


        private static string FromAzureToBase64(string azureUri, CloudStorageAccount StorageAccount)
        {
            Uri blobUri = new Uri(azureUri);
            CloudBlockBlob blob = new CloudBlockBlob(blobUri, StorageAccount.Credentials);
            blob.FetchAttributes();//Fetch blob's properties
            byte[] arr = new byte[blob.Properties.Length];
            blob.DownloadToByteArray(arr, 0);
            var azureBase64 = Convert.ToBase64String(arr);
            return azureBase64;
        }

        private async Task<EventImage> saveFileToStorageAzure(HttpPostedFile postedFile)
        {
            string imageUrl = "";
            string imageName = null;
            EventImage image = null;
            //create custome filename
            imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
            imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);

            try
            {
                if (postedFile != null)
                {
                    using (Stream stream = postedFile.InputStream)
                    {
                        imageUrl = await StorageAzureHelper.UploadFileToStorage(stream, imageName, StorageAzureHelper._storageConfig);
                    }
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        image = new EventImage()
                        {
                            ImageName = imageName,
                            ImagePath = imageUrl
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }

            return image;
        }

        //private string getBase64String(string ImagePath)
        //{
        //    string base64String = "";

        //    using (Image img = Image.FromFile(ImagePath))
        //    {
        //        using (MemoryStream m = new MemoryStream())
        //        {
        //            img.Save(m, img.RawFormat);
        //            byte[] imageBytes = m.ToArray();

        //            // Convert byte[] to Base64 String
        //            base64String = Convert.ToBase64String(imageBytes);
        //        }

        //    }
        //    return base64String;
        //}


        //private EventImage SaveImage(HttpPostedFile postedFile)
        //{
        //    string imageName = null;
        //    //create custome filename
        //    imageName = new string(Path.GetFileNameWithoutExtension(postedFile.FileName).Take(10).ToArray()).Replace(" ", "-");
        //    imageName = imageName + DateTime.Now.ToString("yymmssfff") + Path.GetExtension(postedFile.FileName);
        //    var filePath = HttpContext.Current.Server.MapPath("~/Image/" + imageName);
        //    try
        //    {
        //        postedFile.SaveAs(filePath);
        //        postedFile.InputStream.Flush();
        //    }
        //    catch (Exception ex) { }
        //    finally
        //    {
        //        postedFile.InputStream.Close();
        //    }
        //    EventImage image = new EventImage()
        //    {
        //        ImageName = imageName,
        //        ImagePath = filePath
        //    };

        //    return image;
        //}
    }
}