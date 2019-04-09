﻿using Interop.QBFC13;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Model
{
    public class QBResponceList
    {
       
        public IAccountRetList AccountList { get; set; }

        public IPreferencesRet PreferencesRet { get; set; }

        public IList<QBResponceItem> ItemInventoryRetList { get; set; }
        
    }
}
