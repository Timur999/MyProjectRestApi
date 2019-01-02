using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class ChatRoom
    {
        public int ChatRoomId { get; set; }
        public string ChatRoomName { get; set; }
        public string ChatRoomAdminId { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; }

        public ChatRoom(){}

        public ChatRoom(List<ApplicationUser> userList)
        {
            this.Users = userList;
        }
    }
}