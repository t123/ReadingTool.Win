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

        [Ignore]
        public LanguageSettings Settings
        {
            get { return JsonConvert.DeserializeObject<LanguageSettings>(JsonSettings); }
            set { JsonSettings = JsonConvert.SerializeObject(value); }
        }

        public string JsonSettings { get; set; }
    }
}
