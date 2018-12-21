using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MyProjectRestApi.Models.Entity_Types
{
    public class Blog
    {
        [ForeignKey("Group")]
        public int BLogId { get; set; }
        public string BLogsName { get; set; }
        public virtual Group Group { get; set; }
        public virtual ICollection<GroupPost> GroupPost { get; set; }

        public Blog() { }

        public Blog(string groupName)
        {
            this.BLogsName = groupName;
        }
    }
}