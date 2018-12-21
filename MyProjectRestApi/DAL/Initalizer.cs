using MyProjectRestApi.Models;
using MyProjectRestApi.Models.Entity_Types;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.DAL
{
    public class Initalizer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {

            ApplicationUser user = new ApplicationUser() { UserName = "UserSpamer" };

            List<GroupPost> listGroupPosts = new List<GroupPost>(){
                new GroupPost() { Text = "PiwoBlogtest1", DateOfPublication = DateTime.Now, User = user},
                new GroupPost() { Text = "PiwoBlogtest2", DateOfPublication = DateTime.Now, User = user},
                new GroupPost() { Text = "PiwoBlogtest3", DateOfPublication = DateTime.Now, User = user},
                new GroupPost() { Text = "Winotest1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest3", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo3", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "10", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "PiwoBlogtest1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "PiwoBlogtest2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "PiwoBlogtest3", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest3", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo3", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "20", DateOfPublication = DateTime.Now},
            };
            List<GroupPost> listGroupPosts2 = new List<GroupPost>(){
                new GroupPost() { Text = "Winotest1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Winotest3", DateOfPublication = DateTime.Now},
            };
            List<GroupPost> listGroupPosts3 = new List<GroupPost>(){
                new GroupPost() { Text = "Paliwo1", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo2", DateOfPublication = DateTime.Now},
                new GroupPost() { Text = "Paliwo3", DateOfPublication = DateTime.Now},
            };

            Blog blog = new Blog() { BLogsName = "PiwoBlog", GroupPost = listGroupPosts };


            List<Invitation> listInvitation = new List<Invitation>()
            {
                new Invitation(){GroupId = 1, UserIdSender = "currentUser"},
                new Invitation(){GroupId = 2, UserIdSender = "currentUser"},
            };

            List<Invitation> listInvitation2 = new List<Invitation>()
            {
                new Invitation(){GroupId = 3, UserIdSender = "someUser"},
                new Invitation(){GroupId = 4, UserIdSender = "someUser"},
            };

            List<ApplicationUser> userList = new List<ApplicationUser>()
            {
                new ApplicationUser() {UserName = "Jackson", Invitations = listInvitation},
                new ApplicationUser() {UserName = "Robocop"},
                new ApplicationUser() {UserName = "Terminator", Invitations = listInvitation2}
            };

            List<Group> listGroup = new List<Group>()
            {
               new Group(){ GroupsName = "Killer run", Users = userList, Blog = blog },
               new Group("Run run"),
               new Group("Brum brum"),
               new Group("Biegam bo lubie")//new Group { GroupsName = "Piwo",*/ DateOfCreatedGroup = DateTime.Now, Blog =  blog},
                //new Group { GroupsName = "Wino", DateOfCreatedGroup = DateTime.Now},//, Blog = new Blog(){BLogsName= "WinoBlog", GroupPost = listGroupPosts2 } },
                //new Group { GroupsName = "i", DateOfCreatedGroup = DateTime.Now},//, Blog = new Blog(){BLogsName= "iBlog", GroupPost = listGroupPosts } },
                //new Group { GroupsName = "Paliwo", DateOfCreatedGroup = DateTime.Now}//, Blog = new Blog(){BLogsName= "PaliwoBlog", GroupPost = listGroupPosts3 } }
            };

            //context.Blogs.Add(blog);

            listGroup.ForEach(p => context.Groups.Add(p));
            context.SaveChanges();
        }
    }
}