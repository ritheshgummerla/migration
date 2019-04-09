using Interop.QBFC13;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Interface
{
    public interface IQBSessionInterface
    {
        bool CreateQBSession(out QBSessionManager sessionManager);
        IMsgSetRequest getLatestMsgSetRequest(QBSessionManager sessionManager);
        void CloseQBConnection(QBSessionManager sessionManager);
    }
}
