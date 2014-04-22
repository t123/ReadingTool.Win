using System;

namespace RTWin.Models
{
    public class UserModel
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string AccessKey { get; set; }
        public string AccessSecret { get; set; }
        public bool SyncData { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}