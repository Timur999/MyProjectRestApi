using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class PostDTO
    {
        public int Id { get; set; }
        public string Text { get; set; }
        public DateTime DateOfPublication { get; set; }
        public int BlogId { get; set; }
        public string UserName { get; set; }
        public bool IsPostOwner { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string Base64StringImage { get; set; }
        public int PostCount { get; set; }
    }
}