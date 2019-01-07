using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class Event
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string AdminId { get; set; }
        [Required]
        [StringLength(250)]
        public string Text { get; set; }
        public DateTime EventDate { get; set; }
        public virtual ICollection <ApplicationUser> Users { get; set; }
        public virtual EventImage Image { get; set; }
    }
}