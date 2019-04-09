using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Interop.QBFC13;

namespace TDS.InventoryManagement.QBD.Servicelayer
{
    public class QBAccount1
    {

        public void DoAccountQuery()
        {
            bool sessionBegun = false;
            bool connectionOpen = false;
            QBSessionManager sessionManager = null;
            QBSessionMgr QBMgr = null;

            try
            {
                //Create the session Manager object
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);

                // Get the RequestMsgSet based on the correct QB Version
                IMsgSetRequest requestMsgSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                requestMsgSet.Attributes.OnError = ENRqOnError.roeContinue;

                // BuildAccountQueryRq(requestMsgSet);

                IAccountQuery AccountQueryRq = requestMsgSet.AppendAccountQueryRq();

                // Uncomment the following to view and save the request and response XML
                // string requestXML = requestSet.ToXMLString();
                // MessageBox.Show(requestXML);

                //Send the request and get the response from QuickBooks
                IMsgSetResponse responseMsgSet = sessionManager.DoRequests(requestMsgSet);

                string responseXML = responseMsgSet.ToXMLString();
                //MessageBox.Show(responseXML);

                WalkAccountQueryRs(responseMsgSet);

                //End the session and close the connection to QuickBooks
                //Close the session and connection with QuickBooks
                QBMgr.CloseQBConnection(sessionManager);

                
            }
            catch (Exception ex)
            {
               // MessageBox.Show(e.Message, "Error");
               // MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
                if (sessionBegun)
                {
                    sessionManager.EndSession();
                }
                if (connectionOpen)
                {
                    sessionManager.CloseConnection();
                }
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }
        }
        void BuildAccountQueryRq(IMsgSetRequest requestMsgSet)
        {
            IAccountQuery AccountQueryRq = requestMsgSet.AppendAccountQueryRq();
            //Set attributes
            //Set field value for metaData
            AccountQueryRq.metaData.SetValue(ENmetaData.mdMetaDataAndResponseData); //"IQBENmetaDataType"
            string ORAccountListQueryElementType433 = "ListIDList";
            if (ORAccountListQueryElementType433 == "ListIDList")
            {
                //Set field value for ListIDList
                //May create more than one of these if needed
                // AccountQueryRq.ORAccountListQuery.ListIDList.Add("80000002-1552559344");
            }
            if (ORAccountListQueryElementType433 == "FullNameList")
            {
                //Set field value for FullNameList
                //May create more than one of these if needed
               // AccountQueryRq.ORAccountListQuery.FullNameList.Add("CML01");
            }
            if (ORAccountListQueryElementType433 == "AccountListFilter")
            {
                //Set field value for MaxReturned
                AccountQueryRq.ORAccountListQuery.AccountListFilter.MaxReturned.SetValue(6);
                //Set field value for ActiveStatus
                AccountQueryRq.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
                //Set field value for FromModifiedDate
                AccountQueryRq.ORAccountListQuery.AccountListFilter.FromModifiedDate.SetValue(DateTime.Now, false);
                //Set field value for ToModifiedDate
                AccountQueryRq.ORAccountListQuery.AccountListFilter.ToModifiedDate.SetValue(DateTime.Now, false);
                string ORNameFilterElementType434 = "NameFilter";
                if (ORNameFilterElementType434 == "NameFilter")
                {
                    //Set field value for MatchCriterion
                   // AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith);
                    //Set field value for Name
                    //AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameFilter.Name.SetValue("CML01");
                }
                if (ORNameFilterElementType434 == "NameRangeFilter")
                {
                    //Set field value for FromName
                   // AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue("CML01");
                    //Set field value for ToName
                    //AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.ToName.SetValue("CML01");
                }
                //Set field value for AccountTypeList
                //May create more than one of these if needed
                AccountQueryRq.ORAccountListQuery.AccountListFilter.AccountTypeList.Add(ENAccountType.atAccountsReceivable); //ENAccountTypeList.atlAccountsPayable
                string ORCurrencyFilterElementType435 = "ListIDList";
                if (ORCurrencyFilterElementType435 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                   // AccountQueryRq.ORAccountListQuery.AccountListFilter.CurrencyFilter.ORCurrencyFilter.ListIDList.Add("200000-1011023419");
                }
                if (ORCurrencyFilterElementType435 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    //AccountQueryRq.ORAccountListQuery.AccountListFilter.CurrencyFilter.ORCurrencyFilter.FullNameList.Add("CML01");
                }
            }
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            //AccountQueryRq.IncludeRetElementList.Add("CML01");
            //Set field value for OwnerIDList
            //May create more than one of these if needed
            //AccountQueryRq.OwnerIDList.Add(Guid.NewGuid().ToString("B"));
        }

        void BuildAccountQueryRq1(IMsgSetRequest requestMsgSet)
        {
            IAccountQuery AccountQueryRq = requestMsgSet.AppendAccountQueryRq();
            //Set attributes
            //Set field value for metaData
            AccountQueryRq.metaData.SetValue(ENmetaData.mdMetaDataOnly); //"IQBENmetaDataType"
            string ORAccountListQueryElementType433 = "ListIDList";
            if (ORAccountListQueryElementType433 == "ListIDList")
            {
                //Set field value for ListIDList
                //May create more than one of these if needed
                // AccountQueryRq.ORAccountListQuery.ListIDList.Add("200000-1011023419");
            }
            if (ORAccountListQueryElementType433 == "FullNameList")
            {
                //Set field value for FullNameList
                //May create more than one of these if needed
                //AccountQueryRq.ORAccountListQuery.FullNameList.Add("CML01");
            }
            if (ORAccountListQueryElementType433 == "AccountListFilter")
            {
                //Set field value for MaxReturned
                AccountQueryRq.ORAccountListQuery.AccountListFilter.MaxReturned.SetValue(6);
                //Set field value for ActiveStatus
                AccountQueryRq.ORAccountListQuery.AccountListFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
                //Set field value for FromModifiedDate
                AccountQueryRq.ORAccountListQuery.AccountListFilter.FromModifiedDate.SetValue(DateTime.Parse("12-3-2019 12:15:12"), false);
                //Set field value for ToModifiedDate
                AccountQueryRq.ORAccountListQuery.AccountListFilter.ToModifiedDate.SetValue(DateTime.Parse("12-3-2019 12:15:12"), false);
                string ORNameFilterElementType434 = "NameFilter";
                if (ORNameFilterElementType434 == "NameFilter")
                {
                    //Set field value for MatchCriterion
                    AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith);
                    //Set field value for Name
                    AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameFilter.Name.SetValue("ab");
                }
                if (ORNameFilterElementType434 == "NameRangeFilter")
                {
                    //Set field value for FromName
                    AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.FromName.SetValue("CML01");
                    //Set field value for ToName
                    AccountQueryRq.ORAccountListQuery.AccountListFilter.ORNameFilter.NameRangeFilter.ToName.SetValue("CML01");
                }
                //Set field value for AccountTypeList
                //May create more than one of these if needed
                AccountQueryRq.ORAccountListQuery.AccountListFilter.AccountTypeList.Add(ENAccountType.atAccountsPayable); //ENAccountTypeList.atlAccountsPayable
                string ORCurrencyFilterElementType435 = "ListIDList";
                if (ORCurrencyFilterElementType435 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    // AccountQueryRq.ORAccountListQuery.AccountListFilter.CurrencyFilter.ORCurrencyFilter.ListIDList.Add("200000-1011023419");
                }
                if (ORCurrencyFilterElementType435 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    AccountQueryRq.ORAccountListQuery.AccountListFilter.CurrencyFilter.ORCurrencyFilter.FullNameList.Add("CML01");
                }
            }
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            AccountQueryRq.IncludeRetElementList.Add("CML01");
            //Set field value for OwnerIDList
            //May create more than one of these if needed
            AccountQueryRq.OwnerIDList.Add(Guid.NewGuid().ToString("B"));
        }




        void WalkAccountQueryRs(IMsgSetResponse responseMsgSet)
        {
            if (responseMsgSet == null) return;
            IResponseList responseList = responseMsgSet.ResponseList;
            if (responseList == null) return;
            //if we sent only one request, there is only one response, we'll walk the list for this sample
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
                          // var vs = response.Detail.GetType();
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IAccountRetList AccountRet = (IAccountRetList)response.Detail;
                            for (int j = 0; j < AccountRet.Count; j++)
                            {
                                IAccountRet accountInfo = AccountRet.GetAt(j);
                                if(accountInfo != null)
                                WalkAccountRet(accountInfo);
                            }
                        }
                    }
                }
            }
        }




        void WalkAccountRet(IAccountRet AccountRet)
        {
            if (AccountRet == null) return;
            //Go through all the elements of IAccountRetList
            //Get value of ListID
             string ListID436 = (string)AccountRet.ListID.GetValue();
            // Get value of TimeCreated
             DateTime TimeCreated437 = (DateTime)AccountRet.TimeCreated.GetValue();
             //Get value of TimeModified
             DateTime TimeModified438 = (DateTime)AccountRet.TimeModified.GetValue();
            //Get value of EditSequence
             string EditSequence439 = (string)AccountRet.EditSequence.GetValue();
             //Get value of Name
             string Name440 = (string)AccountRet.Name.GetValue();
            // Get value of FullName
             string FullName441 = (string)AccountRet.FullName.GetValue();
            // Get value of IsActive
             if (AccountRet.IsActive != null)
             {
                 bool IsActive442 = (bool)AccountRet.IsActive.GetValue();
             }
             if (AccountRet.ParentRef != null)
             {
               //  Get value of ListID
                 if (AccountRet.ParentRef.ListID != null)
                 {
                     string ListID443 = (string)AccountRet.ParentRef.ListID.GetValue();
                 }
               //  Get value of FullName
                 if (AccountRet.ParentRef.FullName != null)
                 {
                     string FullName444 = (string)AccountRet.ParentRef.FullName.GetValue();
                 }
             }
            // Get value of Sublevel
             int Sublevel445 = (int)AccountRet.Sublevel.GetValue();
            // Get value of AccountType
             ENAccountType AccountType446 = (ENAccountType)AccountRet.AccountType.GetValue();
            // Get value of SpecialAccountType
             if (AccountRet.SpecialAccountType != null)
             {
                 ENSpecialAccountType SpecialAccountType447 = (ENSpecialAccountType)AccountRet.SpecialAccountType.GetValue();
             }
            // Get value of IsTaxAccount
             if (AccountRet.IsTaxAccount != null)
             {
                 bool IsTaxAccount448 = (bool)AccountRet.IsTaxAccount.GetValue();
             }
            // Get value of AccountNumber
             if (AccountRet.AccountNumber != null)
             {
                 string AccountNumber449 = (string)AccountRet.AccountNumber.GetValue();
             }
            // Get value of BankNumber
             if (AccountRet.BankNumber != null)
             {
                 string BankNumber450 = (string)AccountRet.BankNumber.GetValue();
             }
            // Get value of Desc
             if (AccountRet.Desc != null)
             {
                 string Desc451 = (string)AccountRet.Desc.GetValue();
             }
            // Get value of Balance
             if (AccountRet.Balance != null)
             {
                 double Balance452 = (double)AccountRet.Balance.GetValue();
             }
            // Get value of TotalBalance
             if (AccountRet.TotalBalance != null)
             {
                 double TotalBalance453 = (double)AccountRet.TotalBalance.GetValue();
             }
             if (AccountRet.SalesTaxCodeRef != null)
             {
                // Get value of ListID
                 if (AccountRet.SalesTaxCodeRef.ListID != null)
                 {
                     string ListID454 = (string)AccountRet.SalesTaxCodeRef.ListID.GetValue();
                 }
                // Get value of FullName
                 if (AccountRet.SalesTaxCodeRef.FullName != null)
                 {
                     string FullName455 = (string)AccountRet.SalesTaxCodeRef.FullName.GetValue();
                 }
             }
             if (AccountRet.TaxLineInfoRet != null)
             {
                // Get value of TaxLineID
                 int TaxLineID456 = (int)AccountRet.TaxLineInfoRet.TaxLineID.GetValue();
                 //Get value of TaxLineName
                 if (AccountRet.TaxLineInfoRet.TaxLineName != null)
                 {
                     string TaxLineName457 = (string)AccountRet.TaxLineInfoRet.TaxLineName.GetValue();
                 }
             }
             //Get value of CashFlowClassification
             if (AccountRet.CashFlowClassification != null)
             {
                 ENCashFlowClassification CashFlowClassification458 = (ENCashFlowClassification)AccountRet.CashFlowClassification.GetValue();
             }
             if (AccountRet.CurrencyRef != null)
             {
                // Get value of ListID
                 if (AccountRet.CurrencyRef.ListID != null)
                 {
                     string ListID459 = (string)AccountRet.CurrencyRef.ListID.GetValue();
                 }
                // Get value of FullName
                 if (AccountRet.CurrencyRef.FullName != null)
                 {
                     string FullName460 = (string)AccountRet.CurrencyRef.FullName.GetValue();
                 }
             }
            if (AccountRet.DataExtRetList != null)
            {
                /*for (int i461 = 0; i461 < AccountRet.DataExtRetList.Count; i461++)
                {
                    IDataExtRet DataExtRet = AccountRet.DataExtRetList.GetAt(i461);
                    Get value of OwnerID
                    if (DataExtRet.OwnerID != null)
                    {
                        string OwnerID462 = (string)DataExtRet.OwnerID.GetValue();
                    }
                    Get value of DataExtName
                    string DataExtName463 = (string)DataExtRet.DataExtName.GetValue();
                    Get value of DataExtType
                    ENDataExtType DataExtType464 = (ENDataExtType)DataExtRet.DataExtType.GetValue();
                    Get value of DataExtValue
                    string DataExtValue465 = (string)DataExtRet.DataExtValue.GetValue();
                }*/
            }
        }
    }
}
