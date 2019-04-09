using Quartz;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TDS.InventoryManagement.QBD.EF;
using TDS.InventoryManagement.QBD.Log;
using Interop.QBFC13;
using TDS.InventoryManagement.QBD.Model;
using TDS.InventoryManagement.QBD.Servicelayer.Services;
using TDS.InventoryManagement.QBD.Servicelayer.Model;
using TDS.InventoryManagement.QBD.Servicelayer;

namespace TDS.InventoryManagement.QBD.Jobs
{

    public class TDSJobService : IJob
    {
        public List<vw_INVSTK_TRCQB> TDSItemInventoryList = null;
        public QBSession _QBSession = null;
        QBSessionManager sessionManager;
        QBResponceList qbResponceData = null;
        QBRequestItemSet qbRequestItemSet = null;


        public void Execute(IJobExecutionContext context)
        {
            IAccountRetList AccountList = null;
            IPreferencesRet PreferencesRet = null;
            IItemInventoryRetList itemInventoryRetList = null;
            int maxRecords = 0;
            try
            {

                //** 1. To Get TDS Data Using Entity Frame Work.
                //TDSService _tdsService = new TDSService();

                TDSItemInventoryList = GetMetrialData();

                //** 2. If TDS data is grater than zero. than need to creat QBD session.
                if (TDSItemInventoryList != null && TDSItemInventoryList.Any())
                {

                    qbRequestItemSet = new QBRequestItemSet();
                    qbRequestItemSet.QBRequestItemList = new List<QBRequestItem>();
                    var bSessionResult = CreateQBsession();
                    if (bSessionResult)
                    {
                        //** 3. Using the QBD session to get AccountInfo.
                        AccountList = GetQBAccountDetails();

                        if (AccountList != null && AccountList.Count > 0)
                        {
                            qbRequestItemSet.AccountList = AccountList;

                            //** 4. Using the QBD session to get GetPreferences.
                            PreferencesRet = GetQBPreferencesDetails();
                            if (PreferencesRet != null)
                                qbRequestItemSet.PreferencesRet = PreferencesRet;


                            //** 5. Using the QBD session to get qbd invertory item list.
                            itemInventoryRetList = GetItemInventoryRetList(maxRecords);
                            if (itemInventoryRetList != null && itemInventoryRetList.Count > 0)
                            {
                                //Parallel.ForEach(itemInventoryRetList,item=>)
                                for (int i = 0; i < itemInventoryRetList.Count; i++)
                                {
                                    var reqdata = itemInventoryRetList.GetAt(i);
                                    if(TDSItemInventoryList.Where(x => x.TRC_CODE.Trim() == reqdata.FullName.GetValue()).SingleOrDefault() !=null)
                                    {
                                         var reqitem = TDSItemInventoryList.Where(x => x.TRC_CODE.Trim() == reqdata.FullName.GetValue()).SingleOrDefault();
                                        //var reqitem=itemInventoryRetList.GetAt(i);
                                        double hh = reqdata.QuantityOnHand.GetValue();
                                        if (reqitem.Quantity != reqdata.QuantityOnHand.GetValue())
                                        {
                                            QBRequestItem requestItem = new QBRequestItem();
                                            requestItem.Action = QBAction.Modify;
                                            reqdata.QuantityOnHand.SetValue(Convert.ToDouble(reqitem.Quantity));
                                            requestItem.ItemInventoryRet = reqdata;
                                            qbRequestItemSet.QBRequestItemList.Add(requestItem);
                                        }                                      
                                    }
                                    else
                                    {


                                    }
                                }
                                    
                                   
                                
                                
                                
                                //** 4. To compare TDS and QBD item list using TRC_CODE code to asume as Name/Number in QBD item inventory list.
                                //** 5. If TRC_CODE is not match in QBD. than need to add to QBD as a new inventory item.
                                //** 6. If TRC_CODE is match in QBD. than need to ajust the quantity in QBD.
                                //** 7. final status need to send to client using email.
                            }
                            else
                            {
                                //** 5. If TRC_CODE is not match in QBD. than need to add to QBD as a new inventory item.
                            }
                        }
                    }
                }
                else
                {
                    //ServiceLog.log.Fatal("TDS Data is empty " + DateTime.Now.ToLongDateString());
                }

                //ServiceLog.GetUserInfo();
                //Email("Execution Execution");
            }
            catch (Exception ex)
            {
                // ServiceLog.Email(ex.ToString());
            }
            finally
            {
                _QBSession.CloseQBConnection(sessionManager);
            }

        }

        static string dataMsg = string.Empty;
        public static List<vw_INVSTK_TRCQB> GetMetrialData()
        {
            //BodyMessage = string.Empty;
            //BodyMessage += "<html><body><div>";
            //BodyMessage += "<h4>Hi,</h4>";
            //BodyMessage += "\n<p>Date: " + DateTime.Now.ToString("dd-MMM-yyyy") + "</p>";

            List<vw_INVSTK_TRCQB> ListTdDSdata = new List<vw_INVSTK_TRCQB>();
            try
            {
                dataMsg = string.Empty;
                LogMessageService.log.Fatal("TDS Job Started" + DateTime.Now.ToLongDateString());
                using (TubularDataSystemsEntities hhh = new TubularDataSystemsEntities())
                {
                    ListTdDSdata = hhh.vw_INVSTK_TRCQB.ToList();
                  //  BodyMessage += "\n<p>Total TDS Records: " + ListTdDSdata.Count() + "</p>";

                    //var TDSInfo = hhh.vw_INVSTK_TRCQB.Select(s => new TubularDataTRCQB()
                    //{
                    //    InvStkID = s.InvStkID,
                    //    TRC_CODE = s.TRC_CODE,
                    //    Quantity = s.Quantity,
                    //    Length = s.Length,
                    //    TRC_Description = s.TRC_Description
                    //}).OrderBy(c => c.InvStkID).ToList();
                    if (ListTdDSdata != null)
                    {
                        Parallel.ForEach(ListTdDSdata, item => testparrell(item));
                        //{
                        //    dataMsg += item.InvStkID + " " + item.TRC_Description + Environment.NewLine;
                        //}
                    }
                    //LogMessageService.WarnMg(dataMsg);
                    LogMessageService.log.Fatal(dataMsg + Environment.NewLine);
                    LogMessageService.log.Fatal("TDS Job Ended" + DateTime.Now.ToLongDateString());
                }
               // BodyMessage += "</div></body></html>";
                //  Email(BodyMessage);
            }
            catch (Exception ex)
            {
                LogMessageService.log.Error(ex.ToString());
               // Email("Error Message: " + ex.Message.ToString());
            }
            return ListTdDSdata;

        }
        public static void testparrell(vw_INVSTK_TRCQB item)
        {
            dataMsg += item.InvStkID + " " + item.TRC_Description + Environment.NewLine;
        }

        private bool CreateQBsession()
        {
            var bSessionResult = false;
            try
            {
                _QBSession = new QBSession();
                bSessionResult = _QBSession.CreateQBSession(out sessionManager);
            }
            catch (Exception e)
            {
            }
            return bSessionResult;
        }

        private IItemInventoryRetList GetItemInventoryRetList(int maxRecords)
        {
            IItemInventoryRetList itemInventoryRetList = null;
            try
            {
                QBItemInventory _QBItemInventory = new QBItemInventory();
                IResponseList responseList = _QBItemInventory.LoadQBItemInventoryList(maxRecords, sessionManager);

                //if we sent only one request, there is only one response.
                for (int i = 0; i < responseList.Count; i++)
                {
                    IResponse response = responseList.GetAt(i);
                    //check the status code of the response, 0=ok, >0 is warning
                    if (response.StatusCode >= 0)
                    {
                        //the request-specific response is in the details, make sure we have some
                        if (response.Detail != null)
                        {
                            //make sure the response is the type we're expecting
                            ENResponseType responseType = (ENResponseType)response.Type.GetValue();
                            if (responseType == ENResponseType.rtItemInventoryQueryRs)
                            {
                                //upcast to more specific type here, this is safe because we checked with response.Type check above
                                itemInventoryRetList = (IItemInventoryRetList)response.Detail;
                            }
                        }
                    }
                    else
                    {
                        int statusCode = response.StatusCode;
                        string statusMessage = response.StatusMessage;
                        string statusSeverity = response.StatusSeverity;
                    }
                }
            }
            catch (Exception e)
            {
            }

            return itemInventoryRetList;
        }

        private IItemInventoryRet GetInventoryItem(string fullName)
        {
            IItemInventoryRetList itemInventoryRetList = null;
            IItemInventoryRet InventoryItem = null;
            try
            {
                QBItemInventory _QBItemInventory = new QBItemInventory();
                IResponseList responseList = _QBItemInventory.GetItemInventor(fullName, sessionManager);

                //if we sent only one request, there is only one response.
                for (int i = 0; i < responseList.Count; i++)
                {
                    IResponse response = responseList.GetAt(i);
                    //check the status code of the response, 0=ok, >0 is warning
                    if (response.StatusCode >= 0)
                    {
                        //the request-specific response is in the details, make sure we have some
                        if (response.Detail != null)
                        {
                            //make sure the response is the type we're expecting
                            ENResponseType responseType = (ENResponseType)response.Type.GetValue();
                            if (responseType == ENResponseType.rtItemInventoryQueryRs)
                            {
                                //upcast to more specific type here, this is safe because we checked with response.Type check above
                                itemInventoryRetList = (IItemInventoryRetList)response.Detail;
                                if (itemInventoryRetList != null && itemInventoryRetList.Count > 0)
                                {
                                    for (int j = 0; j < itemInventoryRetList.Count; j++)
                                    {
                                        InventoryItem = itemInventoryRetList.GetAt(j);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int statusCode = response.StatusCode;
                        string statusMessage = response.StatusMessage;
                        string statusSeverity = response.StatusSeverity;
                    }
                }
            }
            catch (Exception e)
            {
            }
            return InventoryItem;
        }

        private IPreferencesRet GetQBPreferencesDetails()
        {
            IPreferencesRet PreferencesRet = null;
            try
            {
                if (sessionManager != null)
                {
                    QBAccount _qbAccount = new QBAccount();

                    IResponseList responseList = _qbAccount.GetPreferences(sessionManager);
                    if (responseList == null) return null;

                    //if we sent only one request, there is only one response.
                    for (int i = 0; i < responseList.Count; i++)
                    {
                        IResponse response = responseList.GetAt(i);
                        //check the status code of the response, 0=ok, >0 is warning
                        if (response.StatusCode >= 0)
                        {
                            //the request-specific response is in the details, make sure we have some
                            if (response.Detail != null)
                            {
                                //make sure the response is the type we're expecting
                                ENResponseType responseType = (ENResponseType)response.Type.GetValue();
                                if (responseType == ENResponseType.rtPreferencesQueryRs)
                                {
                                    //upcast to more specific type here, this is safe because we checked with response.Type check above
                                    PreferencesRet = (IPreferencesRet)response.Detail;
                                }
                            }
                        }
                        else
                        {
                            //*** Error Handling.
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            return PreferencesRet;
        }

        private IAccountRetList GetQBAccountDetails()
        {
            IAccountRetList AccountList = null;
            try
            {
                if (sessionManager != null)
                {
                    QBAccount _qbAccount = new QBAccount();

                    IResponseList responseList = _qbAccount.GetQBAccountInfo(sessionManager);
                    if (responseList == null) return null;

                    //if we sent only one request, there is only one response.
                    for (int i = 0; i < responseList.Count; i++)
                    {
                        IResponse response = responseList.GetAt(i);
                        //check the status code of the response, 0=ok, >0 is warning
                        if (response.StatusCode >= 0)
                        {
                            //the request-specific response is in the details, make sure we have some
                            if (response.Detail != null)
                            {
                                //make sure the response is the type we're expecting
                                ENResponseType responseType = (ENResponseType)response.Type.GetValue();
                                if (responseType == ENResponseType.rtAccountQueryRs)
                                {
                                    //upcast to more specific type here, this is safe because we checked with response.Type check above
                                    AccountList = (IAccountRetList)response.Detail;
                                }
                            }
                        }
                        else
                        {
                            //*** Error Handling.
                        }
                    }
                }
            }
            catch (Exception e)
            {
            }
            return AccountList;
        }
    }

    //public class TDSJobService : IJob
    //{
    //    // IEnumerable<TubularDataSystemsEntities> TDSItemInventoryList = null;
    //    List<vw_INVSTK_TRCQB> ListTdDSdata = new List<vw_INVSTK_TRCQB>();
    //    public void Execute(IJobExecutionContext context)
    //    {
    //        try
    //        {

    //            //** 1. To Get TDS Data Using Entity Frame Work.
    //            //TDSService _tdsService = new TDSService();
    //            //TDSItemInventoryList = _tdsService.GetTDSdata();
    //            using (TubularDataSystemsEntities hhh = new TubularDataSystemsEntities())
    //            {
    //                ListTdDSdata = hhh.vw_INVSTK_TRCQB.ToList();
    //            }

    //            //** 2. If TDS data is grater than zero. than need to creat QBD session.
    //            if (ListTdDSdata != null && ListTdDSdata.Any())
    //            {
    //                //** 3. Using the QBD session to get qbd invertory item list & preferencess list.
    //            }
    //            else
    //            {
    //                LogMessageService.log.Warn("TDS Data is empty " + DateTime.Now.ToLongDateString());
    //            }
    //            //** 4. To compare TDS and QBD item list using TRC_CODE code to asume as Name/Number in QBD item inventory list.
    //            //** 5. If TRC_CODE is not match in QBD. than need to add to QBD as a new inventory item.
    //            //** 6. If TRC_CODE is match in QBD. than need to ajust the quantity in QBD.
    //            //** 7. final status need to send to client using email.

    //            //ServiceLog.GetUserInfo();
    //            //Email("Execution Execution");
    //        }
    //        catch (Exception ex)
    //        {
    //            LogMessageService.log.Error(ex.ToString());
    //        }

    //    }

    //    static string BodyMessage = string.Empty;
    //    /// LogMessageService Logmsg;
    //    public static void SendEmail(String ToEmail, string cc, string bcc, String Subj, string Message)
    //    {
    //        //Reading sender Email credential from web.config file  

    //        string HostAdd = ConfigurationManager.AppSettings["Host"].ToString();
    //        string FromEmailid = ConfigurationManager.AppSettings["FromMail"].ToString();
    //        string Pass = ConfigurationManager.AppSettings["Password"].ToString();

    //        //creating the object of MailMessage  
    //        MailMessage mailMessage = new MailMessage();
    //        mailMessage.From = new MailAddress(FromEmailid); //From Email Id  
    //        mailMessage.Subject = Subj; //Subject of Email  
    //        mailMessage.Body = Message; //body or message of Email  
    //        mailMessage.IsBodyHtml = true;
    //        // mailMessage.Attachments.Add(new Attachment(@"C:\\myapp.log20190401"));
    //        //string name = Path.GetFileName("C:/test");
    //        string path = "C:\\test.txt";

    //        string extension = Path.GetExtension(path);
    //        string filename = Path.GetFileName(path);
    //        string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
    //        string root = Path.GetPathRoot(path);

    //        System.Net.Mail.Attachment attachment;
    //        attachment = new Attachment(root + filename);
    //        mailMessage.Attachments.Add(attachment);

    //        string[] ToMuliId = ToEmail.Split(',');
    //        foreach (string ToEMailId in ToMuliId)
    //        {
    //            mailMessage.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id  
    //        }


    //        if (!string.IsNullOrEmpty(cc))
    //        {
    //            string[] CCId = cc.Split(',');

    //            foreach (string CCEmail in CCId)
    //            {
    //                mailMessage.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id  
    //            }
    //        }

    //        if (!string.IsNullOrEmpty(bcc))
    //        {
    //            string[] bccid = bcc.Split(',');

    //            foreach (string bccEmailId in bccid)
    //            {
    //                mailMessage.Bcc.Add(new MailAddress(bccEmailId)); //Adding Multiple BCC email Id  
    //            }
    //        }
    //        SmtpClient smtp = new SmtpClient();  // creating object of smptpclient  
    //        smtp.Host = HostAdd;              //host of emailaddress for example smtp.gmail.com etc  

    //        //network and security related credentials  


    //        NetworkCredential NetworkCred = new NetworkCredential();
    //        NetworkCred.UserName = mailMessage.From.Address;
    //        NetworkCred.Password = Pass;
    //        smtp.UseDefaultCredentials = true;
    //        smtp.Credentials = NetworkCred;
    //        smtp.Port = 587;
    //        smtp.EnableSsl = true;
    //        smtp.Send(mailMessage); //sending Email  
    //    }
    //    public static void Email(string Message)
    //    {
    //        string Subject = ConfigurationManager.AppSettings["Subject"].ToString();
    //        //string Msg = "Hi," + Environment.NewLine + Message;//whatever msg u want to send write here.  
    //        // Here you can write the   
    //        SendEmail("subbarao.k@camelotis.com", "", "", Subject + DateTime.Now.ToString("dd-MMM-yyyy"), Message);


    //    }

    //    //IList<TubularDataTRCQB> SetMetrialData(IEnumerable<vw_INVSTK_TRCQB> users)
    //    //{
    //    //    return users.Select(s => new TubularDataTRCQB()
    //    //    {               
    //    //        Quantity = s.Quantity,
    //    //        Length = s.Length,
    //    //        TRC_CODE=s.
    //    //        TRC_Description = s.TRC_Description
    //    //    }).OrderBy(c => c.MaterialID).ToList();
    //    //}
    //    static string dataMsg = string.Empty;
    //    public static List<vw_INVSTK_TRCQB> GetMetrialData()
    //    {
    //        BodyMessage = string.Empty;
    //        BodyMessage += "<html><body><div>";
    //        BodyMessage += "<h4>Hi,</h4>";
    //        BodyMessage += "\n<p>Date: " + DateTime.Now.ToString("dd-MMM-yyyy") + "</p>";

    //        List<vw_INVSTK_TRCQB> ListTdDSdata = new List<vw_INVSTK_TRCQB>();
    //        try
    //        {
    //            dataMsg = string.Empty;
    //            LogMessageService.log.Fatal("TDS Job Started" + DateTime.Now.ToLongDateString());
    //            using (TubularDataSystemsEntities hhh = new TubularDataSystemsEntities())
    //            {
    //                //ListTdDSdata = hhh.vw_INVSTK_TRCQB.ToList();
    //                BodyMessage += "\n<p>Total TDS Records: " + ListTdDSdata.Count() + "</p>";
    //                if (ListTdDSdata != null)
    //                {
    //                   // Parallel.ForEach(ListTdDSdata, item => testparrell(item));
    //                    //{
    //                    //    dataMsg += item.InvStkID + " " + item.TRC_Description + Environment.NewLine;
    //                    //}
    //                }
    //                //LogMessageService.WarnMg(dataMsg);
    //                LogMessageService.log.Fatal(dataMsg + Environment.NewLine);
    //                LogMessageService.log.Fatal("TDS Job Ended" + DateTime.Now.ToLongDateString());
    //            }
    //            BodyMessage += "</div></body></html>";
    //            //  Email(BodyMessage);
    //        }
    //        catch (Exception ex)
    //        {
    //            LogMessageService.log.Error(ex.ToString());
    //            Email("Error Message: " + ex.Message.ToString());
    //        }
    //        return ListTdDSdata;

    //    }
    //    public static void testparrell(vw_INVSTK_TRCQB item)
    //    {
    //        dataMsg += item.InvStkID + " " + item.TRC_Description + Environment.NewLine;
    //    }
    //}
}
