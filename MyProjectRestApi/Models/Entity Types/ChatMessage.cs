using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class ChatMessage
    {
        [Key]
        public int MessageId { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }
        public virtual ChatRoom ChatRoom { get; set; }
        public string Text { get; set; }
        public DateTime DateSendMessage { get; set; }
        public EnumMessageState MessageState { get; set; }
        public enum EnumMessageState
        {
            Sent,
            Delivered
        }
        public ChatMessage()
        {
            MessageState = EnumMessageState.Sent;
        }
    
    }
}