using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace RTWin.Entities
{
    [TableName("settings")]
    [PrimaryKey("Id")]
    public class DbSetting
    {
        public class Keys
        {
            public const string BaseWebAPIAddress = @"basewebapiaddress";
            public const string BaseWebSignalRAddress = @"basewebsignalraddress";
        }

        public int Id { get; set; }
        [Column(Name = "SKey")]
        public string Key { get; set; }
        [Column(Name = "SValue")]
        public string Value { get; set; }
    }
}
