using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NPoco;

namespace RTWin.Entities
{
    [TableName("user")]
    [PrimaryKey("UserId")]
    public class User
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public DateTime? LastLogin { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
        public bool SyncData { get; set; }

        public static User NewUser()
        {
            return new User()
            {
                AccessKey = "",
                AccessSecret = "",
                LastLogin = null,
                SyncData = false,
                Username = "User 1"
            };
        }
    }
}
