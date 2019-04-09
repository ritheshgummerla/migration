using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Text;
using System.Threading.Tasks;
using TDS.InventoryManagement.QBD.EF;
using TDS.InventoryManagement.QBD.Model;
using TDS.InventoryManagement.QBD.Log;
using System.IO;

namespace TDS.InventoryManagement.QBD
{
    public class DataManipulation
    {
         static string BodyMessage = string.Empty;
       /// LogMessageService Logmsg;
        public static void SendEmail(String ToEmail, string cc, string bcc, String Subj, string Message)
        {
            //Reading sender Email credential from web.config file  

            string HostAdd = ConfigurationManager.AppSettings["Host"].ToString();
            string FromEmailid = ConfigurationManager.AppSettings["FromMail"].ToString();
            string Pass = ConfigurationManager.AppSettings["Password"].ToString();

            //creating the object of MailMessage  
            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress(FromEmailid); //From Email Id  
            mailMessage.Subject = Subj; //Subject of Email  
            mailMessage.Body = Message; //body or message of Email  
                                     mailMessage.IsBodyHtml = true;
            // mailMessage.Attachments.Add(new Attachment(@"C:\\myapp.log20190401"));
            //string name = Path.GetFileName("C:/test");
            string path = "C:\\test.txt";

            string extension = Path.GetExtension(path);
            string filename = Path.GetFileName(path);
            string filenameNoExtension = Path.GetFileNameWithoutExtension(path);
            string root = Path.GetPathRoot(path);

            System.Net.Mail.Attachment attachment;
            attachment = new Attachment(root+filename);
            mailMessage.Attachments.Add(attachment);

            string[] ToMuliId = ToEmail.Split(',');
            foreach (string ToEMailId in ToMuliId)
            {
                mailMessage.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id  
            }


            if (!string.IsNullOrEmpty(cc))
            {
                string[] CCId = cc.Split(',');

                foreach (string CCEmail in CCId)
                {
                    mailMessage.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id  
                }
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                string[] bccid = bcc.Split(',');

                foreach (string bccEmailId in bccid)
                {
                    mailMessage.Bcc.Add(new MailAddress(bccEmailId)); //Adding Multiple BCC email Id  
                }
            }
            SmtpClient smtp = new SmtpClient();  // creating object of smptpclient  
            smtp.Host = HostAdd;              //host of emailaddress for example smtp.gmail.com etc  

            //network and security related credentials  


            NetworkCredential NetworkCred = new NetworkCredential();
            NetworkCred.UserName = mailMessage.From.Address;
            NetworkCred.Password = Pass;
            smtp.UseDefaultCredentials = true;
            smtp.Credentials = NetworkCred;
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.Send(mailMessage); //sending Email  
        }
        public static void Email(string Message)
        {
            string Subject = ConfigurationManager.AppSettings["Subject"].ToString();
            //string Msg = "Hi," + Environment.NewLine + Message;//whatever msg u want to send write here.  
                                                                                                        // Here you can write the   
            SendEmail("subbarao.k@camelotis.com", "","", Subject + DateTime.Now.ToString("dd-MMM-yyyy"), Message);


        }

        //IList<TubularDataTRCQB> SetMetrialData(IEnumerable<vw_INVSTK_TRCQB> users)
        //{
        //    return users.Select(s => new TubularDataTRCQB()
        //    {               
        //        Quantity = s.Quantity,
        //        Length = s.Length,
        //        TRC_CODE=s.
        //        TRC_Description = s.TRC_Description
        //    }).OrderBy(c => c.MaterialID).ToList();
        //}
         static string dataMsg = string.Empty;
        public static List<vw_INVSTK_TRCQB> GetMetrialData()
        {
            BodyMessage = string.Empty;
            BodyMessage += "<html><body><div>";
            BodyMessage += "<h4>Hi,</h4>";
            BodyMessage += "\n<p>Date: " + DateTime.Now.ToString("dd-MMM-yyyy")+"</p>";

            List<vw_INVSTK_TRCQB> ListTdDSdata = new List<vw_INVSTK_TRCQB>();
            try
            {
                dataMsg = string.Empty;
                LogMessageService.log.Fatal("TDS Job Started" + DateTime.Now.ToLongDateString());
                using (TubularDataSystemsEntities hhh = new TubularDataSystemsEntities())
                {
                    //ListTdDSdata = hhh.vw_INVSTK_TRCQB.ToList();
                    BodyMessage += "\n<p>Total TDS Records: " + ListTdDSdata.Count() +"</p>";

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
                BodyMessage += "</div></body></html>";
              //  Email(BodyMessage);
            }
            catch (Exception ex)
            {
                LogMessageService.log.Error(ex.ToString());
                Email("Error Message: " + ex.Message.ToString());
            }
            return ListTdDSdata;

        }
        public static void testparrell(vw_INVSTK_TRCQB item)
        {
            dataMsg += item.InvStkID + " " + item.TRC_Description + Environment.NewLine;
        }
    }
}
