using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NPoco;

namespace RTWin.Entities
{
    [TableName("dbversion")]
    [PrimaryKey("DbVersionId")]
    public class DbVersion
    {
        public long DbVersionId { get; set; }
        public int Version { get; set; }
    }
}
