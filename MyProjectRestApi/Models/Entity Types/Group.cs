using MyProjectRestApi.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.ModelBinding;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class Group
    {
        public int Id { get; set; }
        public string GroupsName  { get; set; }
        public string AdminGroupId { get; set; }
        public DateTime DateOfCreatedGroup { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual Blog Blog { get; set; }
        // public virtual ICollection<UserPost> UserPosts { get; set; }

        public Group()
        {
            this.DateOfCreatedGroup = DateTime.Now;
        }

        public Group(string groupsName)
        {
            this.GroupsName = groupsName;
            this.DateOfCreatedGroup = DateTime.Now;
            this.Blog = new Blog(groupsName);
        }

        public Group(string groupsName, List<ApplicationUser> users)
        {
            this.GroupsName = groupsName;
            this.DateOfCreatedGroup = DateTime.Now;
            this.Blog = new Blog(groupsName);
            this.Users = users;
            //users list has always one element 
            this.AdminGroupId = users[0].Id;
        }

        public void CreateBlog()
        {
            this.Blog = new Blog(this.GroupsName);
        }

    }
}