using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class InvitationDTO
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public string GroupsName{ get; set; }
        public string UserIdSender { get; set; }        //User id who send you invitation to Friends group
        public string UserNameSender { get; set; }
        public string UserIdReceiver { get; set; }
        public bool IsAccepted { get; set; }
    }


}