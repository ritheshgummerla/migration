using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.QBFC13;

namespace TDS.InventoryManagement.QBD.Servicelayer.Interface
{
    public interface IQBAccountInterface
    {
        IResponseList GetQBAccountInfo(QBSessionManager sessionManager);
        IResponseList GetPreferences(QBSessionManager sessionManager);
    }
}
