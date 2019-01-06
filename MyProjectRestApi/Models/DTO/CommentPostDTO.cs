using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class CommentPostDTO
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int PostId { get; set; }
        public string SenderName { get; set; }
        [StringLength(250)]
        public string Text { get; set; }
    }
}