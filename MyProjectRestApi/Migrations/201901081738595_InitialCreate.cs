namespace MyProjectRestApi.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blogs",
                c => new
                    {
                        BLogId = c.Int(nullable: false),
                        BLogsName = c.String(),
                    })
                .PrimaryKey(t => t.BLogId)
                .ForeignKey("dbo.Groups", t => t.BLogId)
                .Index(t => t.BLogId);
            
            CreateTable(
                "dbo.Groups",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupsName = c.String(),
                        AdminGroupId = c.String(),
                        DateOfCreatedGroup = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.ChatMessages",
                c => new
                    {
                        MessageId = c.Int(nullable: false, identity: true),
                        Text = c.String(maxLength: 250),
                        DateSendMessage = c.DateTime(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                        ChatRoom_ChatRoomId = c.Int(),
                    })
                .PrimaryKey(t => t.MessageId)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoom_ChatRoomId)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.ChatRoom_ChatRoomId);
            
            CreateTable(
                "dbo.ChatRooms",
                c => new
                    {
                        ChatRoomId = c.Int(nullable: false, identity: true),
                        ChatRoomName = c.String(),
                        ChatRoomAdminId = c.String(),
                        UsersIdBelongToGroupChat = c.String(),
                    })
                .PrimaryKey(t => t.ChatRoomId);
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.Events",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        AdminId = c.String(),
                        Text = c.String(nullable: false, maxLength: 250),
                        EventDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EventImages",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ImagePath = c.String(nullable: false),
                        ImageName = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Events", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Invitations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GroupId = c.Int(nullable: false),
                        UserIdSender = c.String(),
                        IsAccepted = c.Boolean(nullable: false),
                        ApplicationUser_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.GroupPosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Text = c.String(nullable: false, maxLength: 250),
                        DateOfPublication = c.DateTime(nullable: false),
                        Blog_BLogId = c.Int(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blogs", t => t.Blog_BLogId)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.Blog_BLogId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.PostImages",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        ImagePath = c.String(nullable: false),
                        ImageName = c.String(nullable: false, maxLength: 150),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.GroupPosts", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.CommentPosts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        BlogId = c.Int(nullable: false),
                        PostId = c.Int(nullable: false),
                        Text = c.String(maxLength: 250),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
            CreateTable(
                "dbo.ChatRoomApplicationUsers",
                c => new
                    {
                        ChatRoom_ChatRoomId = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.ChatRoom_ChatRoomId, t.ApplicationUser_Id })
                .ForeignKey("dbo.ChatRooms", t => t.ChatRoom_ChatRoomId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.ChatRoom_ChatRoomId)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.EventApplicationUsers",
                c => new
                    {
                        Event_Id = c.Int(nullable: false),
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.Event_Id, t.ApplicationUser_Id })
                .ForeignKey("dbo.Events", t => t.Event_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .Index(t => t.Event_Id)
                .Index(t => t.ApplicationUser_Id);
            
            CreateTable(
                "dbo.ApplicationUserGroups",
                c => new
                    {
                        ApplicationUser_Id = c.String(nullable: false, maxLength: 128),
                        Group_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.ApplicationUser_Id, t.Group_Id })
                .ForeignKey("dbo.AspNetUsers", t => t.ApplicationUser_Id, cascadeDelete: true)
                .ForeignKey("dbo.Groups", t => t.Group_Id, cascadeDelete: true)
                .Index(t => t.ApplicationUser_Id)
                .Index(t => t.Group_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.CommentPosts", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.GroupPosts", "User_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.PostImages", "Id", "dbo.GroupPosts");
            DropForeignKey("dbo.GroupPosts", "Blog_BLogId", "dbo.Blogs");
            DropForeignKey("dbo.Blogs", "BLogId", "dbo.Groups");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Invitations", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ApplicationUserGroups", "Group_Id", "dbo.Groups");
            DropForeignKey("dbo.ApplicationUserGroups", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.EventApplicationUsers", "Event_Id", "dbo.Events");
            DropForeignKey("dbo.EventImages", "Id", "dbo.Events");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatRoomApplicationUsers", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropForeignKey("dbo.ChatRoomApplicationUsers", "ChatRoom_ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatMessages", "ChatRoom_ChatRoomId", "dbo.ChatRooms");
            DropForeignKey("dbo.ChatMessages", "ApplicationUser_Id", "dbo.AspNetUsers");
            DropIndex("dbo.ApplicationUserGroups", new[] { "Group_Id" });
            DropIndex("dbo.ApplicationUserGroups", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.EventApplicationUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.EventApplicationUsers", new[] { "Event_Id" });
            DropIndex("dbo.ChatRoomApplicationUsers", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.ChatRoomApplicationUsers", new[] { "ChatRoom_ChatRoomId" });
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.CommentPosts", new[] { "User_Id" });
            DropIndex("dbo.PostImages", new[] { "Id" });
            DropIndex("dbo.GroupPosts", new[] { "User_Id" });
            DropIndex("dbo.GroupPosts", new[] { "Blog_BLogId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.Invitations", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.EventImages", new[] { "Id" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.ChatMessages", new[] { "ChatRoom_ChatRoomId" });
            DropIndex("dbo.ChatMessages", new[] { "ApplicationUser_Id" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.Blogs", new[] { "BLogId" });
            DropTable("dbo.ApplicationUserGroups");
            DropTable("dbo.EventApplicationUsers");
            DropTable("dbo.ChatRoomApplicationUsers");
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.CommentPosts");
            DropTable("dbo.PostImages");
            DropTable("dbo.GroupPosts");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.Invitations");
            DropTable("dbo.EventImages");
            DropTable("dbo.Events");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.ChatRooms");
            DropTable("dbo.ChatMessages");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Groups");
            DropTable("dbo.Blogs");
        }
    }
}
