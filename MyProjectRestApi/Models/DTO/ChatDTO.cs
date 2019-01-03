using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class ChatDTO
    {
        public int Id { get; set; }
        public string ChatName { get; set; }
        public string ChatAdminId { get; set; }
        public List<string> UsersInChat { get; set; }
    }
}