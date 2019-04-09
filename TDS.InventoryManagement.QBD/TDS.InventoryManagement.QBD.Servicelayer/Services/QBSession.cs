using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interop.QBFC13;
using TDS.InventoryManagement.QBD.Servicelayer.Interface;
using System.Configuration;


namespace TDS.InventoryManagement.QBD.Servicelayer.Services
{
    public class QBSession : IQBSessionInterface
    {
        public bool CreateQBSession(out QBSessionManager sessionManager)
        {
            // We want to know if we begun a session so we can end it if an
            // error happens
            bool booSessionBegun = false;
            sessionManager = null;
            string QBDLicenceCertificateName = ConfigurationManager.AppSettings["QBDLicenceCertificateName"].ToString();
           // Create the session manager object using QBFC                
                try
                {
                    sessionManager = new QBSessionManager();
                    // Open the connection and begin a session to QuickBooks                    
                    sessionManager.OpenConnection("", QBDLicenceCertificateName);
                    sessionManager.BeginSession("", ENOpenMode.omDontCare);
                    booSessionBegun = true;
                }
                catch (Exception ex)
                {
                    booSessionBegun = false;
                }
                return booSessionBegun;            
        }

        public IMsgSetRequest getLatestMsgSetRequest(QBSessionManager sessionManager)
        {
            // Find and adapt to supported version of QuickBooks
            double supportedVersion = QBFCLatestVersion(sessionManager);

            short qbXMLMajorVer = 0;
            short qbXMLMinorVer = 0;

            if (supportedVersion >= 13.0)
            {
                qbXMLMajorVer = 13;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 12.0)
            {
                qbXMLMajorVer = 12;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 11.0)
            {
                qbXMLMajorVer = 11;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 10.0)
            {
                qbXMLMajorVer = 10;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 9.0)
            {
                qbXMLMajorVer = 9;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 8.0)
            {
                qbXMLMajorVer = 8;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 7.0)
            {
                qbXMLMajorVer = 7;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 6.0)
            {
                qbXMLMajorVer = 6;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 5.0)
            {
                qbXMLMajorVer = 5;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 4.0)
            {
                qbXMLMajorVer = 4;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 3.0)
            {
                qbXMLMajorVer = 3;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 2.0)
            {
                qbXMLMajorVer = 2;
                qbXMLMinorVer = 0;
            }
            else if (supportedVersion >= 1.1)
            {
                qbXMLMajorVer = 1;
                qbXMLMinorVer = 1;
            }
            else
            {
                qbXMLMajorVer = 1;
                qbXMLMinorVer = 0;                
            }

            // Create the message set request object
            IMsgSetRequest requestMsgSet = sessionManager.CreateMsgSetRequest("US", qbXMLMajorVer, qbXMLMinorVer);
            return requestMsgSet;
        }

        public void CloseQBConnection(QBSessionManager sessionManager)
        {
            if (sessionManager != null)
            {
                sessionManager.EndSession();
                sessionManager.CloseConnection();
            }
        }

        #region private Methods
        // Code for handling different versions of QuickBooks
        private double QBFCLatestVersion(QBSessionManager SessionManager)
        {
            short qbXMLMajorVersion = 0;
            short qbXMLMinorVersion = 0;
            string Country = ConfigurationManager.AppSettings["QBCountry"].ToString();
            short.TryParse(ConfigurationManager.AppSettings["QBXMLMajorVersion"].ToString(), out qbXMLMajorVersion);
            short.TryParse(ConfigurationManager.AppSettings["QBXMLMinorVersion"].ToString(), out qbXMLMinorVersion);

            // Use oldest version to ensure that this application work with any QuickBooks (US)
            IMsgSetRequest msgset = SessionManager.CreateMsgSetRequest(Country, qbXMLMajorVersion, qbXMLMinorVersion);    //("US", 13, 0);
            msgset.AppendHostQueryRq();
            IMsgSetResponse QueryResponse = SessionManager.DoRequests(msgset);    

            // The response list contains only one response,
            // which corresponds to our single HostQuery request
            IResponse response = QueryResponse.ResponseList.GetAt(0);

            // Please refer to QBFC Developers Guide for details on why 
            // "as" clause was used to link this derrived class to its base class
            IHostRet HostResponse = response.Detail as IHostRet;
            IBSTRList supportedVersions = HostResponse.SupportedQBXMLVersionList as IBSTRList;

            int i;
            double vers;
            double LastVers = 0;
            string svers = null;

            for (i = 0; i <= supportedVersions.Count - 1; i++)
            {
                svers = supportedVersions.GetAt(i);
                vers = Convert.ToDouble(svers);
                if (vers > LastVers)
                {
                    LastVers = vers;
                }
            }
            return LastVers;
        }
        #endregion
    }
}
