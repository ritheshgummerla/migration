using Interop.QBFC13;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Model
{
    public class QBResponceItem : QBBaseModel
    {
        public IInventoryAdjustmentRet InventoryItemAdjustment { get; set; }
    }
}
