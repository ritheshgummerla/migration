using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Model
{
    public class QBBaseModel
    {
        public int StatusCode { get; set; }
        public string StatusSeverity { get; set; }
        public string StatusMessage { get; set; }
        public QBAction Action { get; set; }

    }

    public enum QBAction
    {
        Add = 1,
        Modify = 2,
        Delete = 3
    }
}
