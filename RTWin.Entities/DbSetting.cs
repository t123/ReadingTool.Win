using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace RTWin.Entities
{
    [TableName("settings")]
    //[PrimaryKey("Key")]
    public class DbSetting
    {
        public class Keys
        {
            public const string BaseWebAPIAddress = @"basewebapiaddress";
            public const string BaseWebSignalRAddress = @"basewebsignalraddress";
            public const string BackupDatabase = "backup_database";
            public const string BackupDatabasePath = "backup_database_path";
            public const string BackupMax = "backup_max";
            public const string ApiServer = "api_server";
            public const string LastVersionCheck = "last_version_check";
            public const string IgnoreUpdateVersion = "ignore_update_version";
            public const string CheckNewVersions = "check_for_new_versions";
        }

        [Column(Name = "Key")]
        public string Key { get; set; }
        [Column(Name = "Value")]
        public string Value { get; set; }
    }
}
