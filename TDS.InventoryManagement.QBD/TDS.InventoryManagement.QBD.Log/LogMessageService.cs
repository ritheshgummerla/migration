using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Log
{
   public static class LogMessageService
    {
        public static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        //public static void WarnMg(string msg)
        //{
        //    log.Fatal("Get TDS data sucessfully" + DateTime.Now.ToLongDateString() + " " + Environment.NewLine + msg);
        //}
        //public static void ErrorMg(string msg)
        //{
        //    log.Error("Error" + DateTime.Now.ToLongDateString() + " " + Environment.NewLine + msg);
        //}
    }
}
