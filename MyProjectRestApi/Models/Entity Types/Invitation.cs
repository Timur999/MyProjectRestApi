using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class Invitation
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string UserIdSender { get; set; }        //User id who send you invitation to Friends group
        public virtual ApplicationUser ApplicationUser { get; set; }
        public bool IsAccepted { get; set; }
    }
}