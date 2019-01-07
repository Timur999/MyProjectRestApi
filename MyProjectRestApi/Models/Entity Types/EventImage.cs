﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class EventImage
    {
        [ForeignKey("Event")]
        public int Id { get; set; }
        [Required]
        public string ImagePath { get; set; }
        [Required]
        [StringLength(150)]
        public string ImageName { get; set; }

        public virtual Event Event { get; set; }
    }
}