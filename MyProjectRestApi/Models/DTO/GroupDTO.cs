using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.DTO
{
    public class GroupDTO
    {
        public int Id { get; set; }
        public string GroupsName { get; set; }
        public string AdminGroupId { get; set; }
        public DateTime DateOfCreatedGroup { get; set; }
    }

    public class GroupWithUserDTO : GroupDTO
    {
        public List<string> UsersIdToRemoveFromGroup;
    }

}