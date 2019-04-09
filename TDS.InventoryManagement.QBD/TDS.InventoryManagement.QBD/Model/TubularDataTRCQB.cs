using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Model
{
   public class TubularDataTRCQB
    {
        public int InvStkID { get; set; }
        public string TRC_CODE { get; set; }
        public string TRC_Description { get; set; }
        public Nullable<int> Quantity { get; set; }
        public Nullable<decimal> Length { get; set; }
    }
}
