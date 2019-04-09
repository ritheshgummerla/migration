using Interop.QBFC13;
using TDS.InventoryManagement.QBD.Servicelayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Interface
{
    public interface IQBItemInventoryInterface
    {
        IResponseList LoadQBItemInventoryList(int maxRecords, QBSessionManager sessionManager);
        IResponseList GetItemInventor(string fullName, QBSessionManager sessionManager);
        QBResponceItem Inventoryadjustment(QBSessionManager sessionManager, IAccountRetList accountInfo, IPreferencesRet PreferencesRet, IItemInventoryRet inventoryItem);
        QBResponceList InventoryadjustmentBulkOperation(QBSessionManager sessionManager, QBRequestItemSet qBRequestItemSet);
    }
}
