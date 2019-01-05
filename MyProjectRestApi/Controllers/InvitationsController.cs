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
using MyProjectRestApi.Models.Entity_Types;

namespace MyProjectRestApi.Controllers
{
    public class InvitationsController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: api/Invitations
        public IQueryable<InvitationDTO> GetInvitations()
        {
            return db.Invitations
                .Select(invite => new InvitationDTO()
                {
                    Id = invite.Id,
                    UserIdSender = invite.UserIdSender,
                    GroupsName = db.Groups.Where(group => group.Id == invite.GroupId).FirstOrDefault().GroupsName
                });

        }

        // GET: api/Invitations?userId=b3913d15-f95f-412f-b395-a11d281df016(this is userid)
        public IQueryable<InvitationDTO> GetInvitationsByUserId()
        {
            string currentUserId = GetCurrentUserId();
            return db.Invitations.Where(userid => userid.ApplicationUser.Id == currentUserId)
                .Select(invite => new InvitationDTO()
                {
                    Id = invite.Id,
                    UserIdSender = invite.UserIdSender,
                    GroupsName = db.Groups.Where(group => group.Id == invite.GroupId).FirstOrDefault().GroupsName
                });

        }

        // GET: api/Invitations/5
        [ResponseType(typeof(Invitation))]
        public async Task<IHttpActionResult> GetInvitation(int id)
        {
            Invitation invitation = await db.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }

            return Ok(invitation);
        }

        // PUT: api/Invitations/5
        [ResponseType(typeof(void))]
        //Adding or not user to group and remove invitation
        public async Task<IHttpActionResult> PutInvitation(int id, InvitationDTO invitationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //TODO: Important compare invitationDto.UserIdReceiver with currentUserId
            //TODO: Get all current User invite and check is this invitation belong to him, and check every invitation properties are corect with receive invitationDTO
            Invitation invite = await db.Invitations.FindAsync(id);
            if (invite == null|| id != invitationDto.Id)
            {
                return NotFound();
                //return BadRequest();
            }

            if (invitationDto.IsAccepted)
            {
                ApplicationUser userReceiverInvitaton = db.Users.Find(invitationDto.UserIdReceiver);
                Group group = await db.Groups.FindAsync(invitationDto.GroupId);
                group.Users = new List<ApplicationUser>() { userReceiverInvitaton };
                db.Invitations.Remove(invite);
                try
                {
                    await db.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvitationExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                    // return InternalServerError();
                }
            }
            else
            {
                db.Invitations.Remove(invite);
                await db.SaveChangesAsync();
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Invitations
        //[ResponseType(typeof(List<InvitationDTO>))]
        public async Task<IHttpActionResult> PostInvitation(List<InvitationDTO> invitationsDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            List<Invitation> invitations = new List<Invitation>();
            foreach(InvitationDTO invitationDto in invitationsDTO)
            {
                ApplicationUser userReceiverInvitaton = db.Users.Find(invitationDto.UserIdReceiver);
                if (userReceiverInvitaton == null)
                    return BadRequest();
                try
                {
                    Invitation hasInvitationAlreadyExist = db.Invitations
                                                        .Where(invite => invite.ApplicationUser.Id == invitationDto.UserIdReceiver
                                                        && invite.GroupId == invitationDto.GroupId)
                                                        .FirstOrDefault();

                    int hasUserAlreadyExistInGroup = (from gr in db.Groups
                                                      where (gr.Id == invitationDto.GroupId)
                                                      let userInGroup = gr.Users.Where(m => m.Id == invitationDto.UserIdReceiver)
                                                      where userInGroup.Any()
                                                      select userInGroup.Count()).FirstOrDefault();


                    if (hasInvitationAlreadyExist != null || hasUserAlreadyExistInGroup != 0)
                        return Content(HttpStatusCode.PreconditionFailed, "User already exist in the Group");
                    
                }catch(Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, ex.Message);
                }
            

                Invitation inviteToGroup = new Invitation(invitationDto, userReceiverInvitaton);
                invitations.Add(inviteToGroup);
            }


            invitations.ForEach(invite => db.Invitations.Add(invite));
            await db.SaveChangesAsync();

            //I can not return any value because Invitation is send to frends and I have not to get any invitation back
            return StatusCode(HttpStatusCode.NoContent);
            //return Ok();
           // return CreatedAtRoute("DefaultApi", new { id = invitation.Id }, new InvitationDTO() { GroupId = invitation.GroupId, Id = invitation.Id });
        }

        // DELETE: api/Invitations/5
        [ResponseType(typeof(Invitation))]
        public async Task<IHttpActionResult> DeleteInvitation(int id)
        {
            Invitation invitation = await db.Invitations.FindAsync(id);
            if (invitation == null)
            {
                return NotFound();
            }
            string currentUserId = GetCurrentUserId();
            if (!IsUserHasThisInvite(id, currentUserId))
                return BadRequest();

            db.Invitations.Remove(invitation);
            await db.SaveChangesAsync();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool InvitationExists(int id)
        {
            return db.Invitations.Count(e => e.Id == id) > 0;
        }

        private string GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            return user.Id;
        }

        private bool IsUserHasThisInvite(int GroupId, string UserIdReceiver)
        {
            Invitation isUserHasThisInvite = db.Invitations
                                               .Where(invite => invite.ApplicationUser.Id == UserIdReceiver
                                               && invite.GroupId == GroupId)
                                               .FirstOrDefault();
            return (isUserHasThisInvite != null);

        }

    }
}