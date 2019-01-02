using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models
{
    public class MessageDTO
    {
        public int ChatId { get; set; }
        [StringLength(250)]
        public string MessageText { get; set; }
        public string SenderName { get; set; }
        public List<string> ListUserReceiver { get; set; }
    }
}