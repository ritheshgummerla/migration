using Interop.QBFC13;
using TDS.InventoryManagement.QBD.Servicelayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer
{
    public class QBRequestItem : QBBaseModel
    {
        public IItemInventoryRet ItemInventoryRet { get; set; }

    }
}
