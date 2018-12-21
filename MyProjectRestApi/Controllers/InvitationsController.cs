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
        public IQueryable<InvitationDTO> GetInvitationsByUserId(string userId)
        {
            return db.Invitations.Where(userid => userid.ApplicationUser.Id == userId)
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
        [ResponseType(typeof(InvitationDTO))]
        public async Task<IHttpActionResult> PostInvitation(Invitation invitation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApplicationUser userReceiverInvitaton = db.Users.Find(invitation.ApplicationUser.Id);
            if (userReceiverInvitaton == null)
                return BadRequest();

            Invitation hasInvitationAlreadyExist = await db.Invitations
                                                        .Where(invite => invite.ApplicationUser.Id == invitation.ApplicationUser.Id
                                                        && invite.GroupId == invitation.GroupId)
                                                        .FirstOrDefaultAsync();

            int hasUserAlreadyExistInGroup = (from gr in db.Groups
                                              where (gr.Id == invitation.GroupId)
                                              let userInGroup = gr.Users.Where(m => m.Id == invitation.ApplicationUser.Id)
                                              where userInGroup.Any()
                                              select userInGroup.Count()).FirstOrDefault();


            if (hasInvitationAlreadyExist != null || hasUserAlreadyExistInGroup != 0)
                return BadRequest();

            invitation.ApplicationUser = userReceiverInvitaton;
            db.Invitations.Add(invitation);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = invitation.Id }, new InvitationDTO() { GroupId = invitation.GroupId, Id = invitation.Id });
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

            db.Invitations.Remove(invitation);
            await db.SaveChangesAsync();

            return Ok(invitation);
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
    }
}