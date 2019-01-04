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

        [ResponseType(typeof(ChatDTO))]
        [Route("api/GetChatInfo/{id:int}")]
        public async Task<IHttpActionResult> GetChatInfoById(int id)
        {
            string currentUserId = GetCurrentUserId();
            int chatroomId = 0;
            //if chatroomId is not null or 0 then currentUser belong to chat
            chatroomId = CheckIfUserBelongToChat(currentUserId, id);
            if (chatroomId == 0)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }

            ChatDTO chatDto = await db.ChatRooms.Where(chat => chat.ChatRoomId == id)
                .Select(chat => new ChatDTO()
                {
                    Id = chat.ChatRoomId,
                    ChatAdminId = chat.ChatRoomAdminId,
                    ChatName = chat.ChatRoomName
                }).FirstOrDefaultAsync();

            return Ok(chatDto);
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
                                                 select (new MessageDTO()
                                                 {
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
            if (chatroomId != 0)
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
                                                 where (user.Id != currentUserId)
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
            List<ChatDTO> chatDtoList = null;
            try
            {
                chatDtoList = await db.ChatRooms.Where(chat => chat.UsersIdBelongToGroupChat.Contains(currentUserId))
                            .Select(chat => new ChatDTO()
                            {
                                Id = chat.ChatRoomId,
                                ChatAdminId = chat.ChatRoomAdminId,
                                ChatName = chat.ChatRoomName
                            }).ToListAsync();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Ok(chatDtoList);
        }

        [Route("api/PutLeaveTheChat/{id:int}")]
        public async Task<IHttpActionResult> PutLeaveTheChat(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bool isChatDeleted = false;
            string currentUserId = GetCurrentUserId();
            if (CheckIfUserBelongToChat(currentUserId, id) == 0)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }

            ApplicationUser user = db.Users.Find(currentUserId);
            ChatRoom chat = await db.ChatRooms.FindAsync(id);
            //when user create new Chat then variable ChatRoomAdminId is initalize
            if (chat.ChatRoomAdminId == null)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }
            if (chat.Users.Count > 1)
            {
                string newListOfMember = "";
                string[] arrUserId = chat.UsersIdBelongToGroupChat.Split(' ');
                for(int i=0; i < arrUserId.Count()-1; i++)
                {
                    if (arrUserId[i] == currentUserId)
                        continue;
                    newListOfMember += arrUserId[i] + ' ';
                }
                chat.UsersIdBelongToGroupChat = newListOfMember;
                chat.Users.Remove(user);
            }
            else
            {
                isChatDeleted = true;
                chat.Users = null;
                List<ChatMessage> listMessage = db.ChatRooms.Where(m => m.ChatRoomId == id).SelectMany(m => m.Messages).ToList();
                listMessage.ForEach(p => db.ChatMessages.Remove(p));
                db.ChatRooms.Remove(chat);
            }
            try
            {
                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }

            return Ok(isChatDeleted);
        }

        [Route("api/PutAddNewMemberToChat/{id:int}")]
        public async Task<IHttpActionResult> PutAddNewMemberToChat(int id, List<string> userIdList)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            string currentUserId = GetCurrentUserId();
            if (CheckIfUserBelongToChat(currentUserId, id) == 0)
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }
            List<ApplicationUser> Users = new List<ApplicationUser>();
            userIdList.ForEach(userId => Users.Add(db.Users.Find(userId)));
            ChatRoom chat = await db.ChatRooms.FindAsync(id);
            foreach(ApplicationUser user in Users)
            {
                if(user != null)
                    chat.Users.Add(user);
                else
                    return BadRequest("This User is not exist");
            }

            try
            {
                await db.SaveChangesAsync();
                return Ok();
            }catch(Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
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
            if (currentUser == null || chat == null)
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
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.InternalServerError, ex.Message);
            }
            return Ok();
        }


        public async Task<IHttpActionResult> DeleteChat(int id)
        {
            string currnetUserId = GetCurrentUserId();

            if (CheckIsUserAdminOfChat(currnetUserId, id))
            {
                ChatRoom chatRoom = await db.ChatRooms.FindAsync(id);
                chatRoom.Users = null;
                List<ChatMessage> listMessage = db.ChatRooms.Where(m => m.ChatRoomId == id).SelectMany(m => m.Messages).ToList();
                listMessage.ForEach(p => db.ChatMessages.Remove(p));
                db.ChatRooms.Remove(chatRoom);
                //DeleteChatAndRelatedMessages(id);
                try
                {
                    await db.SaveChangesAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    return Content(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            else
            {
                return Content(HttpStatusCode.Forbidden, "You have not permission to do this operation");
            }
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

        private bool CheckIsUserAdminOfChat(string currentUserId, int chatId)
        {
            //if chatroomId is null or 0 then user is not admin of this Chat
            int chatroomId = (from chat in db.ChatRooms
                              where (chat.ChatRoomId == chatId && chat.ChatRoomAdminId == currentUserId)
                              select (chat.ChatRoomId)).FirstOrDefault();

            return (chatroomId > 0);
        }

        private async void DeleteChatAndRelatedMessages(int chatId)
        {
            ChatRoom chatRoom = await db.ChatRooms.FindAsync(chatId);
            chatRoom.Users = null;
            List<ChatMessage> listMessage = db.ChatRooms.Where(m => m.ChatRoomId == chatId).SelectMany(m => m.Messages).ToList();
            listMessage.ForEach(p => db.ChatMessages.Remove(p));
            db.ChatRooms.Remove(chatRoom);
        }
    }
}
