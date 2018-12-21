using MyProjectRestApi.Models.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class GroupPost
    {
        public int Id { get; set; }
        [Required]
        [StringLength(250)]
        public string Text { get; set; }
        public DateTime DateOfPublication { get; set; }
        //[Required]
        public virtual ApplicationUser User { get; set; }
        //[Required]
        public virtual Blog Blog { get; set; }
        public virtual ApplicationImage Image { get; set; }
        

        public GroupPost() { }

        public GroupPost(string text, Blog blog, ApplicationUser User)
        {
            this.Text = text;
            this.DateOfPublication = DateTime.Now;
            this.Blog = blog;
            this.User = User;
        }

        public GroupPost(string text, ApplicationImage image, Blog blog, ApplicationUser User)
        {
            this.Text = text;
            this.DateOfPublication = DateTime.Now;
            this.Blog = blog;
            this.User = User;
            this.Image = image;
        }

    }
}