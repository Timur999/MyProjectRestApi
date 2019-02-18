using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class EventDTO
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string EventName { get; set; }
        public string UserName { get; set; }
        public string AdminId { get; set; }
        [Required]
        [StringLength(250)]
        public string Text { get; set; }
        public DateTime EventDate { get; set; }
        public List<ApplicationUserDTO> Users { get; set; }
        public string ImagePath { get; set; }
        public string ImageName { get; set; }
        public string Base64StringImage { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsSubscriber { get; set; }
        public int ArrayLength { get; set; }
    }
}