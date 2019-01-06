using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class CommentPost
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public int PostId { get; set; }
        public ApplicationUser User { get; set; }
        [StringLength(250)]
        public string Text { get; set; }
    }
}