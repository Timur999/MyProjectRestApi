using MyProjectRestApi.Models;
using MyProjectRestApi.Models.DTO;
using MyProjectRestApi.Models.Entity_Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace MyProjectRestApi.Controllers
{
    //[Authorize]
    public class ChatController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public IHttpActionResult GetAllChats()
        {

            return Ok();
        }

        [ResponseType(typeof(MessageDTO))]
        public async Task<IHttpActionResult> GetChatById(int id)
        {
            string currentUserId = GetCurrentUserId();
            int chatroomId = 0;
            //if chatroomId is not null or 0 then currentUser belong to chat
            chatroomId = CheckIfUserBelongToChat(currentUserId, id);

            if (chatroomId == 0)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }

            List<MessageDTO> messageDTO = await (from msg in db.ChatMessages
                                           where (msg.ChatRoom.ChatRoomId == chatroomId)
                                           select (new MessageDTO() {
                                               ChatId = msg.ChatRoom.ChatRoomId,
                                               MessageText = msg.Text,
                                               SenderName = msg.ApplicationUser.UserName
                                           })).ToListAsync();
            return Ok(messageDTO);
        }

        [Route("api/GetChatByMember/{chatMemberId}")]
        public IHttpActionResult GetChatByMember(string chatMemberId)
        {
            //TODO: find chat by members
            string currentUserId = GetCurrentUserId();

            int chatroomId = (from chat in db.ChatRooms
                                        let usersInChat = chat.Users.Where(user => user.Id == chatMemberId)
                                        where usersInChat.Any()
                                        let userInChat = chat.Users.Where(user => user.Id == currentUserId)
                                        where userInChat.Any()
                                        select (chat.ChatRoomId)).FirstOrDefault();
            if(chatroomId != 0)
                return Ok(chatroomId);

            List<string> ChatMembersIdList = new List<string>() { chatMemberId };
            ChatMembersIdList.Add(GetCurrentUserId());
            chatroomId = CreateChat(ChatMembersIdList);
            return Ok(chatroomId);
        }

        [ResponseType(typeof(string[]))]
        [Route("api/GetChatsMembers/{chatId:int}")]
        public async Task<IHttpActionResult> GetChatsMembers(int chatId)
        {
            string currentUserId = GetCurrentUserId();
            if (CheckIfUserBelongToChat(currentUserId, chatId) == 0)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }
                List<string> MemberOfChatId = await (from user in db.Users
                                                     where(user.Id != currentUserId)
                                                     let usersChat = user.ChatRooms.Where(chat => chat.ChatRoomId == chatId)
                                                     where usersChat.Any()
                                                     select (user.Id)).ToListAsync();

                return Ok(MemberOfChatId);

        }

        [ResponseType(typeof(ChatDTO[]))]
        [Route("api/GetChatsCreatedByUser")]
        public async Task<IHttpActionResult> GetChatsCreatedByUser()
        {
            string currentUserId = GetCurrentUserId();

            List<ChatDTO> chatDtoList = await db.ChatRooms.Where(chat => chat.ChatRoomAdminId == currentUserId)
                .Select(chat => new ChatDTO() {
                    Id = chat.ChatRoomId,
                    ChatAdminId = chat.ChatRoomAdminId,
                    ChatName = chat.ChatRoomName }).ToListAsync();

            return Ok(chatDtoList);
        }

        public IHttpActionResult PutChatById(int id)
        {
            return Ok();
        }

        public IHttpActionResult PostChat(ChatDTO chatDto)
        {
            string currentUserId = GetCurrentUserId();
            chatDto.UsersInChat.Add(currentUserId);
            int chatId = CreateChat(chatDto.UsersInChat, currentUserId, chatDto.ChatName);
            if (chatId == 0)
            {
                return BadRequest("User or chat is not exist");
            }

            return Ok(chatId);
        }

        [Route("api/PostMessage")]
        public async Task<IHttpActionResult> PostMessage(MessageDTO msg)
        {
            //TODO: save message to db
            ApplicationUser currentUser = db.Users.Find(GetCurrentUserId());
            ChatRoom chat = await db.ChatRooms.FindAsync(msg.ChatId);  
            if(currentUser == null || chat == null)
            {
                return Content(HttpStatusCode.NotFound, "User or chat is not exist");
            }
           
            ChatMessage chatMessage = new ChatMessage()
            {
                ChatRoom = chat,
                Text = msg.MessageText,
                DateSendMessage = DateTime.Now,
                ApplicationUser = currentUser
            };
            db.ChatMessages.Add(chatMessage);
            try
            {
                await db.SaveChangesAsync();
            }catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message );
            }
            return Ok();
        }

    
        private int CreateChat(List<string> chatMembersId, string chatAdmin = "", string chatName = "")
        {
            List<ApplicationUser> applicationUsersList = new List<ApplicationUser>();
            for (int i = 0; i < chatMembersId.Count; i++)
            {
                applicationUsersList.Add(db.Users.Find(chatMembersId[i]));
                if (applicationUsersList[i] == null)
                    return 0;
            }
            ChatRoom chat = null;
            if (!string.IsNullOrEmpty(chatAdmin))
            {
                chat = new ChatRoom(applicationUsersList, chatAdmin, chatName);
            }
            else
            {
                chat = new ChatRoom(applicationUsersList);
            }
             
            db.ChatRooms.Add(chat);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return 0;
            }

            return chat.ChatRoomId; 
        }

        private string GetCurrentUserId()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            var user = db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);

            return user.Id;
        }


        private int CheckIfUserBelongToChat(string currentUserId, int chatId)
        {
            //if chatroomId is not null or 0 then currentUser belong to chat
            int chatroomId = (from chat in db.ChatRooms
                          where (chat.ChatRoomId == chatId)
                          let usersInChat = chat.Users.Where(user => user.Id == currentUserId)
                          where usersInChat.Any()
                          select (chat.ChatRoomId)).FirstOrDefault();

            return chatroomId;
        }
    }
}
