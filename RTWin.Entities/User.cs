using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace RTWin.Entities
{
    [TableName("user")]
    [PrimaryKey("UserId")]
    public class User
    {
        public long UserId { get; set; }
        public string Username { get; set; }
    }
}
