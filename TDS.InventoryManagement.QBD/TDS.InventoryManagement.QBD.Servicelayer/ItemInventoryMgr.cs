using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Interop.QBFC13;

namespace TDS.InventoryManagement.QBD.Servicelayer
{
    public class ItemInventoryMgr
    {

        #region QB
        public IItemInventoryRetList LoadQBItemInventoryList(int maxRecords)
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            IItemInventoryRetList itemInventoryRetList = null;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        IItemInventoryQuery itemInventory = requestSet.AppendItemInventoryQueryRq();

                        //Set field value for metaData
                        itemInventory.metaData.SetValue(ENmetaData.mdMetaDataAndResponseData); //"IQBENmetaDataType"

                        // Optionally, you can put filter on it.
                        if (maxRecords != 0)
                            itemInventory.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.MaxReturned.SetValue(maxRecords);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                        // Uncomment the following to view and save the request and response XML
                        //string requestXML = requestSet.ToXMLString();
                        //MessageBox.Show(requestXML);

                        //string responseXML = responseSet.ToXMLString();
                        //MessageBox.Show(responseXML);

                        //WalkItemInventoryQueryRs(responseSet);
                        itemInventoryRetList = WalkItemInventoryQueryList(responseSet);

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }
            return itemInventoryRetList;
        }

        private IItemInventoryRetList WalkItemInventoryQueryList(IMsgSetResponse responseMsgSet)
        {
            IItemInventoryRetList itemInventoryRetList = null;
            if (responseMsgSet == null) return null;
            IResponseList responseList = responseMsgSet.ResponseList;
            if (responseList == null) return null;
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
                    //MessageBox.Show("Status:\nCode = " + statusCode + "\nMessage = " + statusMessage + "\nSeverity = " + statusSeverity);
                }
            }

            return itemInventoryRetList;
        }

        public bool BulkItemInventoryQuantityAdjustment(IItemInventoryRetList itemRetlist)
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            bool bResult = false;
            int intNewQuantity = 0;
            int diffQuantity = 0;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        if (itemRetlist != null && itemRetlist.Count > 0)
                        {
                            IItemInventoryRet itemRet = null;
                            for (int i = 0; i <= itemRetlist.Count - 1; i++)
                            {
                                itemRet = itemRetlist.GetAt(i);
                                
                                if (itemRet != null)
                                {
                                    BuildInventoryAdjustmentAdd(itemRet, intNewQuantity, diffQuantity, requestSet);

                                    // Uncomment the following to view and save the request and response XML
                                    // string requestXML = requestSet.ToXMLString();
                                    // MessageBox.Show(requestXML);

                                    // Do the request and get the response message set object
                                    IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);



                                    string responseXML = responseSet.ToXMLString();
                                    // MessageBox.Show(responseXML);

                                    bResult = WalkInventoryAdjustmentAdd(responseSet);
                                }
                            }
                        }

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }

            return bResult;
        }

        public bool AdjustmentItemInventoryQuantity(IItemInventoryRet itemRet, int intNewQuantity, int diffQuantity)
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            bool bResult = false;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        BuildInventoryAdjustmentAdd(itemRet, intNewQuantity, diffQuantity, requestSet);

                        // Uncomment the following to view and save the request and response XML
                       // string requestXML = requestSet.ToXMLString();
                       // MessageBox.Show(requestXML);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);



                        string responseXML = responseSet.ToXMLString();
                       // MessageBox.Show(responseXML);

                        bResult = WalkInventoryAdjustmentAdd(responseSet);

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }

            return bResult;
        }

        private void BuildInventoryAdjustmentAdd(IItemInventoryRet itemRet, int intNewQuantity, int diffQuantity, IMsgSetRequest requestMsgSet)
        {
            IInventoryAdjustmentAdd InventoryAdjustmentAddRq = requestMsgSet.AppendInventoryAdjustmentAddRq();
            //Set attributes
            //Set field value for defMacro
            InventoryAdjustmentAddRq.defMacro.SetValue("IQBStringType");
            //Set field value for ListID
            InventoryAdjustmentAddRq.AccountRef.ListID.SetValue(itemRet.AssetAccountRef.ListID.GetValue()); //"80000022-1552565294"
            //Set field value for FullName
            InventoryAdjustmentAddRq.AccountRef.FullName.SetValue(itemRet.AssetAccountRef.FullName.GetValue()); //"Inventory Asset"

            //Set field value for TxnDate
            InventoryAdjustmentAddRq.TxnDate.SetValue(DateTime.Now);

            //Set field value for RefNumber
            //InventoryAdjustmentAddRq.RefNumber.SetValue("ab");
            //Set field value for ListID
            //InventoryAdjustmentAddRq.InventorySiteRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.InventorySiteRef.FullName.SetValue("ab");
            //Set field value for ListID
            //InventoryAdjustmentAddRq.CustomerRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.CustomerRef.FullName.SetValue("ab");
            //Set field value for ListID
            // InventoryAdjustmentAddRq.ClassRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.ClassRef.FullName.SetValue("ab");
            //Set field value for Memo
            // InventoryAdjustmentAddRq.Memo.SetValue("ab");
            //Set field value for ExternalGUID
            //InventoryAdjustmentAddRq.ExternalGUID.SetValue(Guid.NewGuid().ToString());
            IInventoryAdjustmentLineAdd InventoryAdjustmentLineAdd10753 = InventoryAdjustmentAddRq.InventoryAdjustmentLineAddList.Append();
            //Set field value for ListID
            InventoryAdjustmentLineAdd10753.ItemRef.ListID.SetValue(itemRet.ListID.GetValue());
            //Set field value for FullName
            InventoryAdjustmentLineAdd10753.ItemRef.FullName.SetValue(itemRet.FullName.GetValue());
            string ORTypeAdjustmentElementType10754 = "QuantityAdjustment";
            if (ORTypeAdjustmentElementType10754 == "QuantityAdjustment")
            {
                string ORQuantityAdjustmentElementType10755 = "NewQuantity";
                if (ORQuantityAdjustmentElementType10755 == "NewQuantity")
                {
                    //Set field value for NewQuantity
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.NewQuantity.SetValue(intNewQuantity);
                }
                if (ORQuantityAdjustmentElementType10755 == "QuantityDifference")
                {
                    //Set field value for QuantityDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.QuantityDifference.SetValue(diffQuantity);
                }

                //string ORSerialLotNumberElementType10756 = "SerialNumber";
                //if (ORSerialLotNumberElementType10756 == "SerialNumber")
                //{
                //    //Set field value for SerialNumber
                //    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORSerialLotNumber.SerialNumber.SetValue("12345");
                //}
                //if (ORSerialLotNumberElementType10756 == "LotNumber")
                //{
                //    //Set field value for LotNumber
                //    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORSerialLotNumber.LotNumber.SetValue("ab");
                //}

                ////Set field value for ListID
                //InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                ////Set field value for FullName
                //InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }

            /*if (ORTypeAdjustmentElementType10754 == "ValueAdjustment")
            {
                string ORQuantityAdjustmentElementType10757 = "NewQuantity";
                if (ORQuantityAdjustmentElementType10757 == "NewQuantity")
                {
                    //Set field value for NewQuantity
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORQuantityAdjustment.NewQuantity.SetValue(intNewQuantity);
                }
                if (ORQuantityAdjustmentElementType10757 == "QuantityDifference")
                {
                    //Set field value for QuantityDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORQuantityAdjustment.QuantityDifference.SetValue(diffQuantity);
                }
                string ORValueAdjustmentElementType10758 = "NewValue";
                if (ORValueAdjustmentElementType10758 == "NewValue")
                {
                    //Set field value for NewValue
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORValueAdjustment.NewValue.SetValue(10.01);
                }
                if (ORValueAdjustmentElementType10758 == "ValueDifference")
                {
                    //Set field value for ValueDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORValueAdjustment.ValueDifference.SetValue(10.01);
                }
            }
            if (ORTypeAdjustmentElementType10754 == "SerialNumberAdjustment")
            {
                string ORSerialNumberAdjustmentElementType10759 = "AddSerialNumber";
                if (ORSerialNumberAdjustmentElementType10759 == "AddSerialNumber")
                {
                    //Set field value for AddSerialNumber
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.ORSerialNumberAdjustment.AddSerialNumber.SetValue("ab");
                }
                if (ORSerialNumberAdjustmentElementType10759 == "RemoveSerialNumber")
                {
                    //Set field value for RemoveSerialNumber
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.ORSerialNumberAdjustment.RemoveSerialNumber.SetValue("ab");
                }
                //Set field value for ListID
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }
            if (ORTypeAdjustmentElementType10754 == "LotNumberAdjustment")
            {
                //Set field value for LotNumber
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.LotNumber.SetValue("ab");
                //Set field value for CountAdjustment
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.CountAdjustment.SetValue(6);
                //Set field value for ListID
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }
            if (ORTypeAdjustmentElementType10754 == "")
            {
            }*/

            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            // InventoryAdjustmentAddRq.IncludeRetElementList.Add("ab");
        }

        private  bool WalkInventoryAdjustmentAdd(IMsgSetResponse responseMsgSet)
        {
            bool bResult = false;
            if (responseMsgSet == null) return false;
            IResponseList responseList = responseMsgSet.ResponseList;
            if (responseList == null) return false;
            //if we sent only one request, there is only one response, we'll walk the list for this sample
            for (int i = 0; i < responseList.Count; i++)
            {
                IResponse response = responseList.GetAt(i);
                //check the status code of the response, 0=ok, >0 is warning
                if (response.StatusCode == 0)
                {
                    bResult = true;
                    //the request-specific response is in the details, make sure we have some
                    if (response.Detail != null)
                    {
                        //make sure the response is the type we're expecting
                        ENResponseType responseType = (ENResponseType)response.Type.GetValue();
                        if (responseType == ENResponseType.rtInventoryAdjustmentAddRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IInventoryAdjustmentRet InventoryAdjustmentRet = (IInventoryAdjustmentRet)response.Detail;                            
                        }
                    }
                }
            }
            return bResult;
        }

        #endregion


        public void GetItemInventoryList()
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        IItemInventoryQuery itemInventory = requestSet.AppendItemInventoryQueryRq();

                        //Set field value for metaData
                        itemInventory.metaData.SetValue(ENmetaData.mdMetaDataAndResponseData); //"IQBENmetaDataType"

                        // Optionally, you can put filter on it.
                        itemInventory.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.MaxReturned.SetValue(50);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                        // Uncomment the following to view and save the request and response XML
                        string requestXML = requestSet.ToXMLString();
                        //MessageBox.Show(requestXML);

                        string responseXML = responseSet.ToXMLString();
                        //MessageBox.Show(responseXML);

                        WalkItemInventoryQueryRs(responseSet);

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
               // MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }
        }

        public void GetItemInventor()
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        IItemInventoryQuery itemInventory = requestSet.AppendItemInventoryQueryRq();

                        //Set field value for metaData
                        itemInventory.metaData.SetValue(ENmetaData.mdMetaDataAndResponseData);

                        //Set field value for ActiveStatus
                        //itemInventory.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);

                        //Set field value for iterator
                        //itemInventory.iterator.SetValue(ENiterator.itContinue); //"IQBENiteratorType"

                        //May create more than one of these if needed
                        //itemInventory.ORListQueryWithOwnerIDAndClass.ListIDList.Add("80000004-1552565462");

                        //May create more than one of these if needed
                        itemInventory.ORListQueryWithOwnerIDAndClass.FullNameList.Add("103");



                        // Optionally, you can put filter on it.
                        //itemInventory.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.MaxReturned.SetValue(50);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                        // Uncomment the following to view and save the request and response XML
                        string requestXML = requestSet.ToXMLString();
                        //MessageBox.Show(requestXML);

                        string responseXML = responseSet.ToXMLString();
                        //MessageBox.Show(responseXML);

                        WalkItemInventoryQueryRs(responseSet);

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }
        }

        public void GetPreferences()
        {           
                QBSessionManager sessionManager = null;
                IMsgSetRequest requestMsgSet = null;
                QBSessionMgr QBMgr = null;
                try
                {
                    QBMgr = new QBSessionMgr();
                    QBMgr.CreateQBSession(out sessionManager);
                    if (sessionManager != null)
                    {
                        // Get the RequestMsgSet based on the correct QB Version
                        IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                        if (requestSet != null)
                        {
                            // Initialize the message set request object
                            requestSet.Attributes.OnError = ENRqOnError.roeStop;

                            BuildPreferencesQueryRq(requestSet);

                            // Uncomment the following to view and save the request and response XML
                            string requestXML = requestSet.ToXMLString();
                            //MessageBox.Show(requestXML);

                            // Do the request and get the response message set object
                            IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);



                            string responseXML = responseSet.ToXMLString();
                            //MessageBox.Show(responseXML);

                            WalkPreferencesQueryRs(responseSet);

                            //Close the session and connection with QuickBooks
                            QBMgr.CloseQBConnection(sessionManager);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
                }
                finally
                {
                    QBMgr.CloseQBConnection(sessionManager);
                }            
        }

        void BuildPreferencesQueryRq(IMsgSetRequest requestMsgSet)
        {
            IPreferencesQuery PreferencesQueryRq = requestMsgSet.AppendPreferencesQueryRq();
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            //PreferencesQueryRq.IncludeRetElementList.Add("ab");
        }

        void WalkPreferencesQueryRs(IMsgSetResponse responseMsgSet)
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
                        if (responseType == ENResponseType.rtPreferencesQueryRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IPreferencesRet PreferencesRet = (IPreferencesRet)response.Detail;
                            WalkPreferencesRet(PreferencesRet);
                        }
                    }
                }
            }
        }

        void WalkPreferencesRet(IPreferencesRet PreferencesRet)
        {
            if (PreferencesRet == null) return;
            //Go through all the elements of IPreferencesRet
            //Get value of IsUsingAccountNumbers
            bool IsUsingAccountNumbers17327 = (bool)PreferencesRet.AccountingPreferences.IsUsingAccountNumbers.GetValue();
            //Get value of IsRequiringAccounts
            bool IsRequiringAccounts17328 = (bool)PreferencesRet.AccountingPreferences.IsRequiringAccounts.GetValue();
            //Get value of IsUsingClassTracking
            bool IsUsingClassTracking17329 = (bool)PreferencesRet.AccountingPreferences.IsUsingClassTracking.GetValue();
            //Get value of AssignClassesTo
            if (PreferencesRet.AccountingPreferences.AssignClassesTo != null)
            {
                ENAssignClassesTo AssignClassesTo17330 = (ENAssignClassesTo)PreferencesRet.AccountingPreferences.AssignClassesTo.GetValue();
            }
            //Get value of IsUsingAuditTrail
            bool IsUsingAuditTrail17331 = (bool)PreferencesRet.AccountingPreferences.IsUsingAuditTrail.GetValue();
            //Get value of IsAssigningJournalEntryNumbers
            bool IsAssigningJournalEntryNumbers17332 = (bool)PreferencesRet.AccountingPreferences.IsAssigningJournalEntryNumbers.GetValue();
            //Get value of ClosingDate
            if (PreferencesRet.AccountingPreferences.ClosingDate != null)
            {
                DateTime ClosingDate17333 = (DateTime)PreferencesRet.AccountingPreferences.ClosingDate.GetValue();
            }
            //Get value of AnnualInterestRate
            if (PreferencesRet.FinanceChargePreferences.AnnualInterestRate != null)
            {
                double AnnualInterestRate17334 = (double)PreferencesRet.FinanceChargePreferences.AnnualInterestRate.GetValue();
            }
            //Get value of MinFinanceCharge
            if (PreferencesRet.FinanceChargePreferences.MinFinanceCharge != null)
            {
                double MinFinanceCharge17335 = (double)PreferencesRet.FinanceChargePreferences.MinFinanceCharge.GetValue();
            }
            //Get value of GracePeriod
            int GracePeriod17336 = (int)PreferencesRet.FinanceChargePreferences.GracePeriod.GetValue();
            if (PreferencesRet.FinanceChargePreferences.FinanceChargeAccountRef != null)
            {
                //Get value of ListID
                if (PreferencesRet.FinanceChargePreferences.FinanceChargeAccountRef.ListID != null)
                {
                    string ListID17337 = (string)PreferencesRet.FinanceChargePreferences.FinanceChargeAccountRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.FinanceChargePreferences.FinanceChargeAccountRef.FullName != null)
                {
                    string FullName17338 = (string)PreferencesRet.FinanceChargePreferences.FinanceChargeAccountRef.FullName.GetValue();
                }
            }
            //Get value of IsAssessingForOverdueCharges
            bool IsAssessingForOverdueCharges17339 = (bool)PreferencesRet.FinanceChargePreferences.IsAssessingForOverdueCharges.GetValue();
            //Get value of CalculateChargesFrom
            ENCalculateChargesFrom CalculateChargesFrom17340 = (ENCalculateChargesFrom)PreferencesRet.FinanceChargePreferences.CalculateChargesFrom.GetValue();
            //Get value of IsMarkedToBePrinted
            bool IsMarkedToBePrinted17341 = (bool)PreferencesRet.FinanceChargePreferences.IsMarkedToBePrinted.GetValue();
            //Get value of IsUsingEstimates
            bool IsUsingEstimates17342 = (bool)PreferencesRet.JobsAndEstimatesPreferences.IsUsingEstimates.GetValue();
            //Get value of IsUsingProgressInvoicing
            bool IsUsingProgressInvoicing17343 = (bool)PreferencesRet.JobsAndEstimatesPreferences.IsUsingProgressInvoicing.GetValue();
            //Get value of IsPrintingItemsWithZeroAmounts
            bool IsPrintingItemsWithZeroAmounts17344 = (bool)PreferencesRet.JobsAndEstimatesPreferences.IsPrintingItemsWithZeroAmounts.GetValue();
            if (PreferencesRet.MultiCurrencyPreferences != null)
            {
                //Get value of IsMultiCurrencyOn
                if (PreferencesRet.MultiCurrencyPreferences.IsMultiCurrencyOn != null)
                {
                    bool IsMultiCurrencyOn17345 = (bool)PreferencesRet.MultiCurrencyPreferences.IsMultiCurrencyOn.GetValue();
                }
                if (PreferencesRet.MultiCurrencyPreferences.HomeCurrencyRef != null)
                {
                    //Get value of ListID
                    if (PreferencesRet.MultiCurrencyPreferences.HomeCurrencyRef.ListID != null)
                    {
                        string ListID17346 = (string)PreferencesRet.MultiCurrencyPreferences.HomeCurrencyRef.ListID.GetValue();
                    }
                    //Get value of FullName
                    if (PreferencesRet.MultiCurrencyPreferences.HomeCurrencyRef.FullName != null)
                    {
                        string FullName17347 = (string)PreferencesRet.MultiCurrencyPreferences.HomeCurrencyRef.FullName.GetValue();
                    }
                }
            }
            if (PreferencesRet.MultiLocationInventoryPreferences != null)
            {
                //Get value of IsMultiLocationInventoryAvailable
                if (PreferencesRet.MultiLocationInventoryPreferences.IsMultiLocationInventoryAvailable != null)
                {
                    bool IsMultiLocationInventoryAvailable17348 = (bool)PreferencesRet.MultiLocationInventoryPreferences.IsMultiLocationInventoryAvailable.GetValue();
                }
                //Get value of IsMultiLocationInventoryEnabled
                if (PreferencesRet.MultiLocationInventoryPreferences.IsMultiLocationInventoryEnabled != null)
                {
                    bool IsMultiLocationInventoryEnabled17349 = (bool)PreferencesRet.MultiLocationInventoryPreferences.IsMultiLocationInventoryEnabled.GetValue();
                }
            }
            //Get value of IsUsingInventory
            bool IsUsingInventory17350 = (bool)PreferencesRet.PurchasesAndVendorsPreferences.IsUsingInventory.GetValue();
            //Get value of DaysBillsAreDue
            int DaysBillsAreDue17351 = (int)PreferencesRet.PurchasesAndVendorsPreferences.DaysBillsAreDue.GetValue();
            //Get value of IsAutomaticallyUsingDiscounts
            bool IsAutomaticallyUsingDiscounts17352 = (bool)PreferencesRet.PurchasesAndVendorsPreferences.IsAutomaticallyUsingDiscounts.GetValue();
            if (PreferencesRet.PurchasesAndVendorsPreferences.DefaultDiscountAccountRef != null)
            {
                //Get value of ListID
                if (PreferencesRet.PurchasesAndVendorsPreferences.DefaultDiscountAccountRef.ListID != null)
                {
                    string ListID17353 = (string)PreferencesRet.PurchasesAndVendorsPreferences.DefaultDiscountAccountRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.PurchasesAndVendorsPreferences.DefaultDiscountAccountRef.FullName != null)
                {
                    string FullName17354 = (string)PreferencesRet.PurchasesAndVendorsPreferences.DefaultDiscountAccountRef.FullName.GetValue();
                }
            }
            //Get value of AgingReportBasis
            ENAgingReportBasis AgingReportBasis17355 = (ENAgingReportBasis)PreferencesRet.ReportsPreferences.AgingReportBasis.GetValue();
            //Get value of SummaryReportBasis
            ENSummaryReportBasis SummaryReportBasis17356 = (ENSummaryReportBasis)PreferencesRet.ReportsPreferences.SummaryReportBasis.GetValue();
            if (PreferencesRet.SalesAndCustomersPreferences.DefaultShipMethodRef != null)
            {
                //Get value of ListID
                if (PreferencesRet.SalesAndCustomersPreferences.DefaultShipMethodRef.ListID != null)
                {
                    string ListID17357 = (string)PreferencesRet.SalesAndCustomersPreferences.DefaultShipMethodRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.SalesAndCustomersPreferences.DefaultShipMethodRef.FullName != null)
                {
                    string FullName17358 = (string)PreferencesRet.SalesAndCustomersPreferences.DefaultShipMethodRef.FullName.GetValue();
                }
            }
            //Get value of DefaultFOB
            if (PreferencesRet.SalesAndCustomersPreferences.DefaultFOB != null)
            {
                string DefaultFOB17359 = (string)PreferencesRet.SalesAndCustomersPreferences.DefaultFOB.GetValue();
            }
            //Get value of DefaultMarkup
            if (PreferencesRet.SalesAndCustomersPreferences.DefaultMarkup != null)
            {
                double DefaultMarkup17360 = (double)PreferencesRet.SalesAndCustomersPreferences.DefaultMarkup.GetValue();
            }
            //Get value of IsTrackingReimbursedExpensesAsIncome
            bool IsTrackingReimbursedExpensesAsIncome17361 = (bool)PreferencesRet.SalesAndCustomersPreferences.IsTrackingReimbursedExpensesAsIncome.GetValue();
            //Get value of IsAutoApplyingPayments
            bool IsAutoApplyingPayments17362 = (bool)PreferencesRet.SalesAndCustomersPreferences.IsAutoApplyingPayments.GetValue();
            if (PreferencesRet.SalesAndCustomersPreferences.PriceLevels != null)
            {
                //Get value of IsUsingPriceLevels
                bool IsUsingPriceLevels17363 = (bool)PreferencesRet.SalesAndCustomersPreferences.PriceLevels.IsUsingPriceLevels.GetValue();
                //Get value of IsRoundingSalesPriceUp
                if (PreferencesRet.SalesAndCustomersPreferences.PriceLevels.IsRoundingSalesPriceUp != null)
                {
                    bool IsRoundingSalesPriceUp17364 = (bool)PreferencesRet.SalesAndCustomersPreferences.PriceLevels.IsRoundingSalesPriceUp.GetValue();
                }
            }
            if (PreferencesRet.SalesTaxPreferences != null)
            {
                //Get value of ListID
                if (PreferencesRet.SalesTaxPreferences.DefaultItemSalesTaxRef.ListID != null)
                {
                    string ListID17365 = (string)PreferencesRet.SalesTaxPreferences.DefaultItemSalesTaxRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.SalesTaxPreferences.DefaultItemSalesTaxRef.FullName != null)
                {
                    string FullName17366 = (string)PreferencesRet.SalesTaxPreferences.DefaultItemSalesTaxRef.FullName.GetValue();
                }
                //Get value of PaySalesTax
                ENPaySalesTax PaySalesTax17367 = (ENPaySalesTax)PreferencesRet.SalesTaxPreferences.PaySalesTax.GetValue();
                //Get value of ListID
                if (PreferencesRet.SalesTaxPreferences.DefaultTaxableSalesTaxCodeRef.ListID != null)
                {
                    string ListID17368 = (string)PreferencesRet.SalesTaxPreferences.DefaultTaxableSalesTaxCodeRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.SalesTaxPreferences.DefaultTaxableSalesTaxCodeRef.FullName != null)
                {
                    string FullName17369 = (string)PreferencesRet.SalesTaxPreferences.DefaultTaxableSalesTaxCodeRef.FullName.GetValue();
                }
                //Get value of ListID
                if (PreferencesRet.SalesTaxPreferences.DefaultNonTaxableSalesTaxCodeRef.ListID != null)
                {
                    string ListID17370 = (string)PreferencesRet.SalesTaxPreferences.DefaultNonTaxableSalesTaxCodeRef.ListID.GetValue();
                }
                //Get value of FullName
                if (PreferencesRet.SalesTaxPreferences.DefaultNonTaxableSalesTaxCodeRef.FullName != null)
                {
                    string FullName17371 = (string)PreferencesRet.SalesTaxPreferences.DefaultNonTaxableSalesTaxCodeRef.FullName.GetValue();
                }
                //Get value of IsUsingVendorTaxCode
                if (PreferencesRet.SalesTaxPreferences.IsUsingVendorTaxCode != null)
                {
                    bool IsUsingVendorTaxCode17372 = (bool)PreferencesRet.SalesTaxPreferences.IsUsingVendorTaxCode.GetValue();
                }
                //Get value of IsUsingCustomerTaxCode
                if (PreferencesRet.SalesTaxPreferences.IsUsingCustomerTaxCode != null)
                {
                    bool IsUsingCustomerTaxCode17373 = (bool)PreferencesRet.SalesTaxPreferences.IsUsingCustomerTaxCode.GetValue();
                }
                //Get value of IsUsingAmountsIncludeTax
                if (PreferencesRet.SalesTaxPreferences.IsUsingAmountsIncludeTax != null)
                {
                    bool IsUsingAmountsIncludeTax17374 = (bool)PreferencesRet.SalesTaxPreferences.IsUsingAmountsIncludeTax.GetValue();
                }
            }
            if (PreferencesRet.TimeTrackingPreferences != null)
            {
                //Get value of FirstDayOfWeek
                ENFirstDayOfWeek FirstDayOfWeek17375 = (ENFirstDayOfWeek)PreferencesRet.TimeTrackingPreferences.FirstDayOfWeek.GetValue();
            }
            //Get value of IsAutomaticLoginAllowed
            bool IsAutomaticLoginAllowed17376 = (bool)PreferencesRet.CurrentAppAccessRights.IsAutomaticLoginAllowed.GetValue();
            //Get value of AutomaticLoginUserName
            if (PreferencesRet.CurrentAppAccessRights.AutomaticLoginUserName != null)
            {
                string AutomaticLoginUserName17377 = (string)PreferencesRet.CurrentAppAccessRights.AutomaticLoginUserName.GetValue();
            }
            //Get value of IsPersonalDataAccessAllowed
            bool IsPersonalDataAccessAllowed17378 = (bool)PreferencesRet.CurrentAppAccessRights.IsPersonalDataAccessAllowed.GetValue();
            if (PreferencesRet.ItemsAndInventoryPreferences != null)
            {
                //Get value of EnhancedInventoryReceivingEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.EnhancedInventoryReceivingEnabled != null)
                {
                    bool EnhancedInventoryReceivingEnabled17379 = (bool)PreferencesRet.ItemsAndInventoryPreferences.EnhancedInventoryReceivingEnabled.GetValue();
                }
                //Get value of IsTrackingSerialOrLotNumber
                if (PreferencesRet.ItemsAndInventoryPreferences.IsTrackingSerialOrLotNumber != null)
                {
                    ENIsTrackingSerialOrLotNumber IsTrackingSerialOrLotNumber17380 = (ENIsTrackingSerialOrLotNumber)PreferencesRet.ItemsAndInventoryPreferences.IsTrackingSerialOrLotNumber.GetValue();
                }
                //Get value of isTrackingOnSalesTransactionsEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnSalesTransactionsEnabled != null)
                {
                    bool isTrackingOnSalesTransactionsEnabled17381 = (bool)PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnSalesTransactionsEnabled.GetValue();
                }
                //Get value of isTrackingOnPurchaseTransactionsEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnPurchaseTransactionsEnabled != null)
                {
                    bool isTrackingOnPurchaseTransactionsEnabled17382 = (bool)PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnPurchaseTransactionsEnabled.GetValue();
                }
                //Get value of isTrackingOnInventoryAdjustmentEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnInventoryAdjustmentEnabled != null)
                {
                    bool isTrackingOnInventoryAdjustmentEnabled17383 = (bool)PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnInventoryAdjustmentEnabled.GetValue();
                }
                //Get value of isTrackingOnBuildAssemblyEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnBuildAssemblyEnabled != null)
                {
                    bool isTrackingOnBuildAssemblyEnabled17384 = (bool)PreferencesRet.ItemsAndInventoryPreferences.isTrackingOnBuildAssemblyEnabled.GetValue();
                }
                //Get value of FIFOEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.FIFOEnabled != null)
                {
                    bool FIFOEnabled17385 = (bool)PreferencesRet.ItemsAndInventoryPreferences.FIFOEnabled.GetValue();
                }
                //Get value of FIFOEffectiveDate
                if (PreferencesRet.ItemsAndInventoryPreferences.FIFOEffectiveDate != null)
                {
                    DateTime FIFOEffectiveDate17386 = (DateTime)PreferencesRet.ItemsAndInventoryPreferences.FIFOEffectiveDate.GetValue();
                }
                //Get value of IsRSBEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.IsRSBEnabled != null)
                {
                    bool IsRSBEnabled17387 = (bool)PreferencesRet.ItemsAndInventoryPreferences.IsRSBEnabled.GetValue();
                }
                //Get value of IsBarcodeEnabled
                if (PreferencesRet.ItemsAndInventoryPreferences.IsBarcodeEnabled != null)
                {
                    bool IsBarcodeEnabled17388 = (bool)PreferencesRet.ItemsAndInventoryPreferences.IsBarcodeEnabled.GetValue();
                }
            }
        }


        public void Inventoryadjustment()
        {
            QBSessionManager sessionManager = null;
            IMsgSetRequest requestMsgSet = null;
            QBSessionMgr QBMgr = null;
            try
            {
                QBMgr = new QBSessionMgr();
                QBMgr.CreateQBSession(out sessionManager);
                if (sessionManager != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        BuildInventoryAdjustmentAddRq(requestSet);

                        // Uncomment the following to view and save the request and response XML
                        string requestXML = requestSet.ToXMLString();
                        //MessageBox.Show(requestXML);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);



                        string responseXML = responseSet.ToXMLString();
                        //MessageBox.Show(responseXML);

                        WalkInventoryAdjustmentAddRs(responseSet);

                        //Close the session and connection with QuickBooks
                        QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
            }
            finally
            {
                QBMgr.CloseQBConnection(sessionManager);
            }

        }


        void BuildInventoryAdjustmentAddRq(IMsgSetRequest requestMsgSet)
        {
            IInventoryAdjustmentAdd InventoryAdjustmentAddRq = requestMsgSet.AppendInventoryAdjustmentAddRq();
            //Set attributes
            //Set field value for defMacro
            InventoryAdjustmentAddRq.defMacro.SetValue("IQBStringType");
            //Set field value for ListID
            InventoryAdjustmentAddRq.AccountRef.ListID.SetValue("80000022-1552565294");
            //Set field value for FullName
            InventoryAdjustmentAddRq.AccountRef.FullName.SetValue("Inventory Asset");

            //Set field value for TxnDate
            InventoryAdjustmentAddRq.TxnDate.SetValue(DateTime.Parse("20-03-2019"));

            //Set field value for RefNumber
            //InventoryAdjustmentAddRq.RefNumber.SetValue("ab");
            //Set field value for ListID
            //InventoryAdjustmentAddRq.InventorySiteRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.InventorySiteRef.FullName.SetValue("ab");
            //Set field value for ListID
            //InventoryAdjustmentAddRq.CustomerRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.CustomerRef.FullName.SetValue("ab");
            //Set field value for ListID
           // InventoryAdjustmentAddRq.ClassRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //InventoryAdjustmentAddRq.ClassRef.FullName.SetValue("ab");
            //Set field value for Memo
           // InventoryAdjustmentAddRq.Memo.SetValue("ab");
            //Set field value for ExternalGUID
            //InventoryAdjustmentAddRq.ExternalGUID.SetValue(Guid.NewGuid().ToString());
            IInventoryAdjustmentLineAdd InventoryAdjustmentLineAdd10753 = InventoryAdjustmentAddRq.InventoryAdjustmentLineAddList.Append();
            //Set field value for ListID
            InventoryAdjustmentLineAdd10753.ItemRef.ListID.SetValue("8000000B-1553066645");
            //Set field value for FullName
            InventoryAdjustmentLineAdd10753.ItemRef.FullName.SetValue("venky04");
            string ORTypeAdjustmentElementType10754 = "QuantityAdjustment";
            if (ORTypeAdjustmentElementType10754 == "QuantityAdjustment")
            {
                string ORQuantityAdjustmentElementType10755 = "NewQuantity";
                if (ORQuantityAdjustmentElementType10755 == "NewQuantity")
                {
                    //Set field value for NewQuantity
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.NewQuantity.SetValue(99);
                }
                if (ORQuantityAdjustmentElementType10755 == "QuantityDifference")
                {
                    //Set field value for QuantityDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.QuantityDifference.SetValue(10);
                }

                //string ORSerialLotNumberElementType10756 = "SerialNumber";
                //if (ORSerialLotNumberElementType10756 == "SerialNumber")
                //{
                //    //Set field value for SerialNumber
                //    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORSerialLotNumber.SerialNumber.SetValue("12345");
                //}
                //if (ORSerialLotNumberElementType10756 == "LotNumber")
                //{
                //    //Set field value for LotNumber
                //    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORSerialLotNumber.LotNumber.SetValue("ab");
                //}

                ////Set field value for ListID
                //InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                ////Set field value for FullName
                //InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }
            if (ORTypeAdjustmentElementType10754 == "ValueAdjustment")
            {
                string ORQuantityAdjustmentElementType10757 = "NewQuantity";
                if (ORQuantityAdjustmentElementType10757 == "NewQuantity")
                {
                    //Set field value for NewQuantity
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORQuantityAdjustment.NewQuantity.SetValue(2);
                }
                if (ORQuantityAdjustmentElementType10757 == "QuantityDifference")
                {
                    //Set field value for QuantityDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORQuantityAdjustment.QuantityDifference.SetValue(2);
                }
                string ORValueAdjustmentElementType10758 = "NewValue";
                if (ORValueAdjustmentElementType10758 == "NewValue")
                {
                    //Set field value for NewValue
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORValueAdjustment.NewValue.SetValue(10.01);
                }
                if (ORValueAdjustmentElementType10758 == "ValueDifference")
                {
                    //Set field value for ValueDifference
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.ValueAdjustment.ORValueAdjustment.ValueDifference.SetValue(10.01);
                }
            }
            if (ORTypeAdjustmentElementType10754 == "SerialNumberAdjustment")
            {
                string ORSerialNumberAdjustmentElementType10759 = "AddSerialNumber";
                if (ORSerialNumberAdjustmentElementType10759 == "AddSerialNumber")
                {
                    //Set field value for AddSerialNumber
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.ORSerialNumberAdjustment.AddSerialNumber.SetValue("ab");
                }
                if (ORSerialNumberAdjustmentElementType10759 == "RemoveSerialNumber")
                {
                    //Set field value for RemoveSerialNumber
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.ORSerialNumberAdjustment.RemoveSerialNumber.SetValue("ab");
                }
                //Set field value for ListID
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.SerialNumberAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }
            if (ORTypeAdjustmentElementType10754 == "LotNumberAdjustment")
            {
                //Set field value for LotNumber
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.LotNumber.SetValue("ab");
                //Set field value for CountAdjustment
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.CountAdjustment.SetValue(6);
                //Set field value for ListID
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.InventorySiteLocationRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                InventoryAdjustmentLineAdd10753.ORTypeAdjustment.LotNumberAdjustment.InventorySiteLocationRef.FullName.SetValue("ab");
            }
            if (ORTypeAdjustmentElementType10754 == "")
            {
            }
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
           // InventoryAdjustmentAddRq.IncludeRetElementList.Add("ab");
        }


        void WalkInventoryAdjustmentAddRs(IMsgSetResponse responseMsgSet)
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
                        if (responseType == ENResponseType.rtInventoryAdjustmentAddRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IInventoryAdjustmentRet InventoryAdjustmentRet = (IInventoryAdjustmentRet)response.Detail;
                            WalkInventoryAdjustmentRet(InventoryAdjustmentRet);
                        }
                    }
                }
            }
        }
       
        void BuildInventoryAdjustmentQueryRq(IMsgSetRequest requestMsgSet)
        {
            IInventoryAdjustmentQuery InventoryAdjustmentQueryRq = requestMsgSet.AppendInventoryAdjustmentQueryRq();
            //Set attributes
            //Set field value for metaData
            InventoryAdjustmentQueryRq.metaData.SetValue(ENmetaData.mdMetaDataAndResponseData); //"IQBENmetaDataType"

            //Set field value for iterator
            // InventoryAdjustmentQueryRq.iterator.SetValue(ENiterator.itContinue); //"IQBENiteratorType"

            //Set field value for iteratorID
            //InventoryAdjustmentQueryRq.iteratorID.SetValue("IQBUUIDType");


            string ORInventoryAdjustmentQueryElementType10903 = "IItemFilter";

            //string ORInventoryAdjustmentQueryElementType10903 = "TxnIDList";
            if (ORInventoryAdjustmentQueryElementType10903 == "TxnIDList")
            {
                //Set field value for TxnIDList
                //May create more than one of these if needed
                InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnIDList.Add("200000-1011023419");
            }

            if (ORInventoryAdjustmentQueryElementType10903 == "RefNumberList")
            {
                //Set field value for RefNumberList
                //May create more than one of these if needed
                InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.RefNumberList.Add("ab");
            }
            if (ORInventoryAdjustmentQueryElementType10903 == "RefNumberCaseSensitiveList")
            {
                //Set field value for RefNumberCaseSensitiveList
                //May create more than one of these if needed
                InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.RefNumberCaseSensitiveList.Add("ab");
            }

            //IItemFilter
            if (ORInventoryAdjustmentQueryElementType10903 == "IItemFilter")
            {

                //Set field value for MaxReturned
                InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.MaxReturned.SetValue(10);

                string ORItemFilterElementType10908 = "ListIDList";
                if (ORItemFilterElementType10908 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.ListIDList.Add("80000005-1552565646");
                }
                if (ORItemFilterElementType10908 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.FullNameList.Add("104");
                }
            }


            if (ORInventoryAdjustmentQueryElementType10903 == "TxnFilterWithItemFilter")
            {
                //Set field value for MaxReturned
                InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.MaxReturned.SetValue(6);
                string ORDateRangeFilterElementType10904 = "ModifiedDateRangeFilter";
                if (ORDateRangeFilterElementType10904 == "ModifiedDateRangeFilter")
                {
                    //Set field value for FromModifiedDate
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORDateRangeFilter.ModifiedDateRangeFilter.FromModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), false);
                    //Set field value for ToModifiedDate
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORDateRangeFilter.ModifiedDateRangeFilter.ToModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), false);
                }
                if (ORDateRangeFilterElementType10904 == "TxnDateRangeFilter")
                {
                    string ORTxnDateRangeFilterElementType10905 = "TxnDateFilter";
                    if (ORTxnDateRangeFilterElementType10905 == "TxnDateFilter")
                    {
                        //Set field value for FromTxnDate
                        InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.FromTxnDate.SetValue(DateTime.Parse("12/15/2007"));
                        //Set field value for ToTxnDate
                        InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.TxnDateFilter.ToTxnDate.SetValue(DateTime.Parse("12/15/2007"));
                    }
                    if (ORTxnDateRangeFilterElementType10905 == "DateMacro")
                    {
                        //Set field value for DateMacro
                        InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORDateRangeFilter.TxnDateRangeFilter.ORTxnDateRangeFilter.DateMacro.SetValue(ENDateMacro.dmAll);
                    }
                }
                string OREntityFilterElementType10906 = "ListIDList";
                if (OREntityFilterElementType10906 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.EntityFilter.OREntityFilter.ListIDList.Add("200000-1011023419");
                }
                if (OREntityFilterElementType10906 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.EntityFilter.OREntityFilter.FullNameList.Add("ab");
                }
                if (OREntityFilterElementType10906 == "ListIDWithChildren")
                {
                    //Set field value for ListIDWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.EntityFilter.OREntityFilter.ListIDWithChildren.SetValue("200000-1011023419");
                }
                if (OREntityFilterElementType10906 == "FullNameWithChildren")
                {
                    //Set field value for FullNameWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.EntityFilter.OREntityFilter.FullNameWithChildren.SetValue("ab");
                }
                string ORAccountFilterElementType10907 = "ListIDList";
                if (ORAccountFilterElementType10907 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.AccountFilter.ORAccountFilter.ListIDList.Add("200000-1011023419");
                }
                if (ORAccountFilterElementType10907 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.AccountFilter.ORAccountFilter.FullNameList.Add("ab");
                }
                if (ORAccountFilterElementType10907 == "ListIDWithChildren")
                {
                    //Set field value for ListIDWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.AccountFilter.ORAccountFilter.ListIDWithChildren.SetValue("200000-1011023419");
                }
                if (ORAccountFilterElementType10907 == "FullNameWithChildren")
                {
                    //Set field value for FullNameWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.AccountFilter.ORAccountFilter.FullNameWithChildren.SetValue("ab");
                }
                string ORItemFilterElementType10908 = "ListIDList";
                if (ORItemFilterElementType10908 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.ListIDList.Add("200000-1011023419");
                }
                if (ORItemFilterElementType10908 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.FullNameList.Add("ab");
                }
                if (ORItemFilterElementType10908 == "ListIDWithChildren")
                {
                    //Set field value for ListIDWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.ListIDWithChildren.SetValue("200000-1011023419");
                }
                if (ORItemFilterElementType10908 == "FullNameWithChildren")
                {
                    //Set field value for FullNameWithChildren
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ItemFilter.ORItemFilter.FullNameWithChildren.SetValue("ab");
                }
                string ORRefNumberFilterElementType10909 = "RefNumberFilter";
                if (ORRefNumberFilterElementType10909 == "RefNumberFilter")
                {
                    //Set field value for MatchCriterion
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORRefNumberFilter.RefNumberFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith);
                    //Set field value for RefNumber
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORRefNumberFilter.RefNumberFilter.RefNumber.SetValue("ab");
                }
                if (ORRefNumberFilterElementType10909 == "RefNumberRangeFilter")
                {
                    //Set field value for FromRefNumber
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORRefNumberFilter.RefNumberRangeFilter.FromRefNumber.SetValue("ab");
                    //Set field value for ToRefNumber
                    InventoryAdjustmentQueryRq.ORInventoryAdjustmentQuery.TxnFilterWithItemFilter.ORRefNumberFilter.RefNumberRangeFilter.ToRefNumber.SetValue("ab");
                }
            }
            //Set field value for IncludeLineItems
            InventoryAdjustmentQueryRq.IncludeLineItems.SetValue(true);
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            //InventoryAdjustmentQueryRq.IncludeRetElementList.Add("104");
            //Set field value for OwnerIDList
            //May create more than one of these if needed
            //InventoryAdjustmentQueryRq.OwnerIDList.Add(Guid.NewGuid().ToString());
        }

        void WalkInventoryAdjustmentQueryRs(IMsgSetResponse responseMsgSet)
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
                        if (responseType == ENResponseType.rtInventoryAdjustmentQueryRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IInventoryAdjustmentRetList InventoryAdjustmentRet = (IInventoryAdjustmentRetList)response.Detail;
                           

                            if (InventoryAdjustmentRet != null && InventoryAdjustmentRet.Count > 0)
                            {
                                for (int j = 0; j < InventoryAdjustmentRet.Count; j++)
                                {
                                    IInventoryAdjustmentRet iRet = InventoryAdjustmentRet.GetAt(j);
                                    if (iRet != null)
                                    {
                                        WalkInventoryAdjustmentRet(iRet);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        void WalkInventoryAdjustmentRet(IInventoryAdjustmentRet InventoryAdjustmentRet)
        {
            if (InventoryAdjustmentRet == null) return;
            //Go through all the elements of IInventoryAdjustmentRetList
            //Get value of TxnID
            string TxnID10910 = (string)InventoryAdjustmentRet.TxnID.GetValue();
            //Get value of TimeCreated
            DateTime TimeCreated10911 = (DateTime)InventoryAdjustmentRet.TimeCreated.GetValue();
            //Get value of TimeModified
            DateTime TimeModified10912 = (DateTime)InventoryAdjustmentRet.TimeModified.GetValue();
            //Get value of EditSequence
            string EditSequence10913 = (string)InventoryAdjustmentRet.EditSequence.GetValue();
            //Get value of TxnNumber
            if (InventoryAdjustmentRet.TxnNumber != null)
            {
                int TxnNumber10914 = (int)InventoryAdjustmentRet.TxnNumber.GetValue();
            }
            //Get value of ListID
            if (InventoryAdjustmentRet.AccountRef.ListID != null)
            {
                string ListID10915 = (string)InventoryAdjustmentRet.AccountRef.ListID.GetValue();
            }
            //Get value of FullName
            if (InventoryAdjustmentRet.AccountRef.FullName != null)
            {
                string FullName10916 = (string)InventoryAdjustmentRet.AccountRef.FullName.GetValue();
            }
            if (InventoryAdjustmentRet.InventorySiteRef != null)
            {
                //Get value of ListID
                if (InventoryAdjustmentRet.InventorySiteRef.ListID != null)
                {
                    string ListID10917 = (string)InventoryAdjustmentRet.InventorySiteRef.ListID.GetValue();
                }
                //Get value of FullName
                if (InventoryAdjustmentRet.InventorySiteRef.FullName != null)
                {
                    string FullName10918 = (string)InventoryAdjustmentRet.InventorySiteRef.FullName.GetValue();
                }
            }
            //Get value of TxnDate
            DateTime TxnDate10919 = (DateTime)InventoryAdjustmentRet.TxnDate.GetValue();
            //Get value of RefNumber
            if (InventoryAdjustmentRet.RefNumber != null)
            {
                string RefNumber10920 = (string)InventoryAdjustmentRet.RefNumber.GetValue();
            }
            if (InventoryAdjustmentRet.CustomerRef != null)
            {
                //Get value of ListID
                if (InventoryAdjustmentRet.CustomerRef.ListID != null)
                {
                    string ListID10921 = (string)InventoryAdjustmentRet.CustomerRef.ListID.GetValue();
                }
                //Get value of FullName
                if (InventoryAdjustmentRet.CustomerRef.FullName != null)
                {
                    string FullName10922 = (string)InventoryAdjustmentRet.CustomerRef.FullName.GetValue();
                }
            }
            if (InventoryAdjustmentRet.ClassRef != null)
            {
                //Get value of ListID
                if (InventoryAdjustmentRet.ClassRef.ListID != null)
                {
                    string ListID10923 = (string)InventoryAdjustmentRet.ClassRef.ListID.GetValue();
                }
                //Get value of FullName
                if (InventoryAdjustmentRet.ClassRef.FullName != null)
                {
                    string FullName10924 = (string)InventoryAdjustmentRet.ClassRef.FullName.GetValue();
                }
            }
            //Get value of Memo
            if (InventoryAdjustmentRet.Memo != null)
            {
                string Memo10925 = (string)InventoryAdjustmentRet.Memo.GetValue();
            }
            //Get value of ExternalGUID
            if (InventoryAdjustmentRet.ExternalGUID != null)
            {
                string ExternalGUID10926 = (string)InventoryAdjustmentRet.ExternalGUID.GetValue();
            }
            if (InventoryAdjustmentRet.InventoryAdjustmentLineRetList != null)
            {
                for (int i10927 = 0; i10927 < InventoryAdjustmentRet.InventoryAdjustmentLineRetList.Count; i10927++)
                {
                    IInventoryAdjustmentLineRet InventoryAdjustmentLineRet = InventoryAdjustmentRet.InventoryAdjustmentLineRetList.GetAt(i10927);
                    //Get value of TxnLineID
                    string TxnLineID10928 = (string)InventoryAdjustmentLineRet.TxnLineID.GetValue();
                    //Get value of ListID
                    if (InventoryAdjustmentLineRet.ItemRef.ListID != null)
                    {
                        string ListID10929 = (string)InventoryAdjustmentLineRet.ItemRef.ListID.GetValue();
                    }
                    //Get value of FullName
                    if (InventoryAdjustmentLineRet.ItemRef.FullName != null)
                    {
                        string FullName10930 = (string)InventoryAdjustmentLineRet.ItemRef.FullName.GetValue();
                    }
                    if (InventoryAdjustmentLineRet.ORSerialLotNumberPreference != null)
                    {
                        if (InventoryAdjustmentLineRet.ORSerialLotNumberPreference.SerialNumberRet != null)
                        {
                            //Get value of SerialNumberRet
                            if (InventoryAdjustmentLineRet.ORSerialLotNumberPreference.SerialNumberRet != null)
                            {
                                ISerialNumberRet nothing10932 = (ISerialNumberRet)InventoryAdjustmentLineRet.ORSerialLotNumberPreference.SerialNumberRet.SerialNumber;
                            }
                        }
                        if (InventoryAdjustmentLineRet.ORSerialLotNumberPreference.LotNumber != null)
                        {
                            //Get value of LotNumber
                            if (InventoryAdjustmentLineRet.ORSerialLotNumberPreference.LotNumber != null)
                            {
                                string LotNumber10933 = (string)InventoryAdjustmentLineRet.ORSerialLotNumberPreference.LotNumber.GetValue();
                            }
                        }
                    }
                    if (InventoryAdjustmentLineRet.InventorySiteLocationRef != null)
                    {
                        //Get value of ListID
                        if (InventoryAdjustmentLineRet.InventorySiteLocationRef.ListID != null)
                        {
                            string ListID10934 = (string)InventoryAdjustmentLineRet.InventorySiteLocationRef.ListID.GetValue();
                        }
                        //Get value of FullName
                        if (InventoryAdjustmentLineRet.InventorySiteLocationRef.FullName != null)
                        {
                            string FullName10935 = (string)InventoryAdjustmentLineRet.InventorySiteLocationRef.FullName.GetValue();
                        }
                    }
                    //Get value of QuantityDifference
                    int QuantityDifference10936 = (int)InventoryAdjustmentLineRet.QuantityDifference.GetValue();
                    //Get value of ValueDifference
                    double ValueDifference10937 = (double)InventoryAdjustmentLineRet.ValueDifference.GetValue();
                }
            }
            if (InventoryAdjustmentRet.DataExtRetList != null)
            {
                for (int i10938 = 0; i10938 < InventoryAdjustmentRet.DataExtRetList.Count; i10938++)
                {
                    IDataExtRet DataExtRet = InventoryAdjustmentRet.DataExtRetList.GetAt(i10938);
                    //Get value of OwnerID
                    if (DataExtRet.OwnerID != null)
                    {
                        string OwnerID10939 = (string)DataExtRet.OwnerID.GetValue();
                    }
                    //Get value of DataExtName
                    string DataExtName10940 = (string)DataExtRet.DataExtName.GetValue();
                    //Get value of DataExtType
                    ENDataExtType DataExtType10941 = (ENDataExtType)DataExtRet.DataExtType.GetValue();
                    //Get value of DataExtValue
                    string DataExtValue10942 = (string)DataExtRet.DataExtValue.GetValue();
                }
            }
        }


        public void CreatInventoryItem()
        {
            try
            {
                QBSessionManager sessionManager = null;
                IMsgSetRequest requestMsgSet = null;
                QBSessionMgr QBMgr = null;
                try
                {
                    QBMgr = new QBSessionMgr();
                    QBMgr.CreateQBSession(out sessionManager);
                    if (sessionManager != null)
                    {
                        // Get the RequestMsgSet based on the correct QB Version
                        IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                        if (requestSet != null)
                        {
                            // Initialize the message set request object
                            requestSet.Attributes.OnError = ENRqOnError.roeStop;

                            BuildItemInventoryAddRq(requestSet);

                            // Uncomment the following to view and save the request and response XML
                            string requestXML = requestSet.ToXMLString();
                            //MessageBox.Show(requestXML);

                            // Do the request and get the response message set object
                            IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                        

                            string responseXML = responseSet.ToXMLString();
                            //MessageBox.Show(responseXML);

                            WalkItemInventoryAddRs(responseSet);

                            //Close the session and connection with QuickBooks
                            QBMgr.CloseQBConnection(sessionManager);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(ex.Message.ToString() + "\nStack Trace: \n" + ex.StackTrace + "\nExiting the application");
                }
                finally
                {
                    QBMgr.CloseQBConnection(sessionManager);
                }
            }
            catch (Exception ex)
            {

            }
        }

        void BuildItemInventoryAddRq(IMsgSetRequest requestMsgSet)
        {
            //IItemInventoryAdd[] AdditemList = new IItemInventoryAdd[2];
            IItemInventoryAdd ItemInventoryAddRq = requestMsgSet.AppendItemInventoryAddRq();
            //Set field value for Name
            ItemInventoryAddRq.Name.SetValue("venky01");
            //Set field value for BarCodeValue
            //ItemInventoryAddRq.BarCode.BarCodeValue.SetValue("ab");
            //Set field value for AssignEvenIfUsed
            //ItemInventoryAddRq.BarCode.AssignEvenIfUsed.SetValue(true);
            //Set field value for AllowOverride
            //ItemInventoryAddRq.BarCode.AllowOverride.SetValue(true);
            //Set field value for IsActive
            ItemInventoryAddRq.IsActive.SetValue(true);

            ////Set field value for ListID
            //ItemInventoryAddRq.ClassRef.ListID.SetValue("200000-1011023419");
            ////Set field value for FullName
            //ItemInventoryAddRq.ClassRef.FullName.SetValue("ab");
            ////Set field value for ListID
            //ItemInventoryAddRq.ParentRef.ListID.SetValue("200000-1011023419");
            ////Set field value for FullName
            //ItemInventoryAddRq.ParentRef.FullName.SetValue("ab");

            //Set field value for ManufacturerPartNumber
            ItemInventoryAddRq.ManufacturerPartNumber.SetValue("5005");

            //Set field value for ListID
           // ItemInventoryAddRq.UnitOfMeasureSetRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            //ItemInventoryAddRq.UnitOfMeasureSetRef.FullName.SetValue("ab");

            //Set field value for IsTaxIncluded
            //ItemInventoryAddRq.IsTaxIncluded.SetValue(true);
            //Set field value for ListID
            //ItemInventoryAddRq.SalesTaxCodeRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
           // ItemInventoryAddRq.SalesTaxCodeRef.FullName.SetValue("ab");

            //Set field value for SalesDesc
            ItemInventoryAddRq.SalesDesc.SetValue("venky01");
            //Set field value for SalesPrice
            ItemInventoryAddRq.SalesPrice.SetValue(30);
            //Set field value for ListID
            ItemInventoryAddRq.IncomeAccountRef.ListID.SetValue("80000008-1552559357");
            //Set field value for FullName
            ItemInventoryAddRq.IncomeAccountRef.FullName.SetValue("Sales");
            //Set field value for PurchaseDesc
            ItemInventoryAddRq.PurchaseDesc.SetValue("TDS Test");
            //Set field value for PurchaseCost
            ItemInventoryAddRq.PurchaseCost.SetValue(15);

            //Set field value for ListID
           // ItemInventoryAddRq.PurchaseTaxCodeRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
           // ItemInventoryAddRq.PurchaseTaxCodeRef.FullName.SetValue("ab");

            //Set field value for ListID
            ItemInventoryAddRq.COGSAccountRef.ListID.SetValue("80000023-1552565294");
            //Set field value for FullName
            ItemInventoryAddRq.COGSAccountRef.FullName.SetValue("Cost of Goods Sold");
            //Set field value for ListID
            ItemInventoryAddRq.PrefVendorRef.ListID.SetValue("80000001-1552565609");
            //Set field value for FullName
            ItemInventoryAddRq.PrefVendorRef.FullName.SetValue("Vender01");
            //Set field value for ListID
            ItemInventoryAddRq.AssetAccountRef.ListID.SetValue("80000022-1552565294");
            //Set field value for FullName
            ItemInventoryAddRq.AssetAccountRef.FullName.SetValue("Inventory Asset");
            //Set field value for ReorderPoint
           // ItemInventoryAddRq.ReorderPoint.SetValue(2);
            //Set field value for Max
           // ItemInventoryAddRq.Max.SetValue(2);
            //Set field value for QuantityOnHand
            ItemInventoryAddRq.QuantityOnHand.SetValue(50);
            //Set field value for TotalValue
            ItemInventoryAddRq.TotalValue.SetValue(1500);
            //Set field value for InventoryDate
            //ItemInventoryAddRq.InventoryDate.SetValue(DateTime.Now);
            //ItemInventoryAddRq.InventoryDate.SetValue(DateTime.Parse("19-03-2019"));

            //Set field value for ExternalGUID
            //ItemInventoryAddRq.ExternalGUID.SetValue(Guid.NewGuid().ToString());

            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            //ItemInventoryAddRq.IncludeRetElementList.Add("ab");
        }


        void WalkItemInventoryAddRs(IMsgSetResponse responseMsgSet)
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
                        if (responseType == ENResponseType.rtItemInventoryAddRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IItemInventoryRet ItemInventoryRet = (IItemInventoryRet)response.Detail;
                            WalkItemInventoryRet(ItemInventoryRet);
                        }
                    }
                }
            }
        }


        void WalkItemInventoryQueryRs(IMsgSetResponse responseMsgSet)
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
                        if (responseType == ENResponseType.rtItemInventoryQueryRs)
                        {
                            //upcast to more specific type here, this is safe because we checked with response.Type check above
                            IItemInventoryRetList itemInventoryRetList = (IItemInventoryRetList)response.Detail;
                            //ItemInventoryRet
                            if (itemInventoryRetList != null && itemInventoryRetList.Count > 0)
                            {
                                for (int j = 0; j < itemInventoryRetList.Count; j++)
                                {
                                    IItemInventoryRet iRet = itemInventoryRetList.GetAt(j);
                                    if (iRet != null)
                                    {
                                        //iRet.FullName.GetValue()
                                        // WalkItemInventoryRet(iRet);
                                    }
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
                    //MessageBox.Show("Status:\nCode = " + statusCode + "\nMessage = " + statusMessage + "\nSeverity = " + statusSeverity);
                }
            }
        }

        void WalkItemInventoryRet(IItemInventoryRet ItemInventoryRet)
        {
            if (ItemInventoryRet == null) return;
            //Go through all the elements of IItemInventoryRetList
            //Get value of ListID
            string ListID13276 = (string)ItemInventoryRet.ListID.GetValue();
            //Get value of TimeCreated
            DateTime TimeCreated13277 = (DateTime)ItemInventoryRet.TimeCreated.GetValue();
            //Get value of TimeModified
            DateTime TimeModified13278 = (DateTime)ItemInventoryRet.TimeModified.GetValue();
            //Get value of EditSequence
            string EditSequence13279 = (string)ItemInventoryRet.EditSequence.GetValue();
            //Get value of Name
            string Name13280 = (string)ItemInventoryRet.Name.GetValue();
            //Get value of FullName
            string FullName13281 = (string)ItemInventoryRet.FullName.GetValue();
            //Get value of BarCodeValue
            if (ItemInventoryRet.BarCodeValue != null)
            {
                string BarCodeValue13282 = (string)ItemInventoryRet.BarCodeValue.GetValue();
            }
            //Get value of IsActive
            if (ItemInventoryRet.IsActive != null)
            {
                bool IsActive13283 = (bool)ItemInventoryRet.IsActive.GetValue();
            }
            if (ItemInventoryRet.ClassRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.ClassRef.ListID != null)
                {
                    string ListID13284 = (string)ItemInventoryRet.ClassRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.ClassRef.FullName != null)
                {
                    string FullName13285 = (string)ItemInventoryRet.ClassRef.FullName.GetValue();
                }
            }
            if (ItemInventoryRet.ParentRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.ParentRef.ListID != null)
                {
                    string ListID13286 = (string)ItemInventoryRet.ParentRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.ParentRef.FullName != null)
                {
                    string FullName13287 = (string)ItemInventoryRet.ParentRef.FullName.GetValue();
                }
            }
            //Get value of Sublevel
            int Sublevel13288 = (int)ItemInventoryRet.Sublevel.GetValue();
            //Get value of ManufacturerPartNumber
            if (ItemInventoryRet.ManufacturerPartNumber != null)
            {
                string ManufacturerPartNumber13289 = (string)ItemInventoryRet.ManufacturerPartNumber.GetValue();
            }
            if (ItemInventoryRet.UnitOfMeasureSetRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.UnitOfMeasureSetRef.ListID != null)
                {
                    string ListID13290 = (string)ItemInventoryRet.UnitOfMeasureSetRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.UnitOfMeasureSetRef.FullName != null)
                {
                    string FullName13291 = (string)ItemInventoryRet.UnitOfMeasureSetRef.FullName.GetValue();
                }
            }
            //Get value of IsTaxIncluded
            if (ItemInventoryRet.IsTaxIncluded != null)
            {
                bool IsTaxIncluded13292 = (bool)ItemInventoryRet.IsTaxIncluded.GetValue();
            }
            if (ItemInventoryRet.SalesTaxCodeRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.SalesTaxCodeRef.ListID != null)
                {
                    string ListID13293 = (string)ItemInventoryRet.SalesTaxCodeRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.SalesTaxCodeRef.FullName != null)
                {
                    string FullName13294 = (string)ItemInventoryRet.SalesTaxCodeRef.FullName.GetValue();
                }
            }
            //Get value of SalesDesc
            if (ItemInventoryRet.SalesDesc != null)
            {
                string SalesDesc13295 = (string)ItemInventoryRet.SalesDesc.GetValue();
            }
            //Get value of SalesPrice
            if (ItemInventoryRet.SalesPrice != null)
            {
                double SalesPrice13296 = (double)ItemInventoryRet.SalesPrice.GetValue();
            }
            if (ItemInventoryRet.IncomeAccountRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.IncomeAccountRef.ListID != null)
                {
                    string ListID13297 = (string)ItemInventoryRet.IncomeAccountRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.IncomeAccountRef.FullName != null)
                {
                    string FullName13298 = (string)ItemInventoryRet.IncomeAccountRef.FullName.GetValue();
                }
            }
            //Get value of PurchaseDesc
            if (ItemInventoryRet.PurchaseDesc != null)
            {
                string PurchaseDesc13299 = (string)ItemInventoryRet.PurchaseDesc.GetValue();
            }
            //Get value of PurchaseCost
            if (ItemInventoryRet.PurchaseCost != null)
            {
                double PurchaseCost13300 = (double)ItemInventoryRet.PurchaseCost.GetValue();
            }
            if (ItemInventoryRet.PurchaseTaxCodeRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.PurchaseTaxCodeRef.ListID != null)
                {
                    string ListID13301 = (string)ItemInventoryRet.PurchaseTaxCodeRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.PurchaseTaxCodeRef.FullName != null)
                {
                    string FullName13302 = (string)ItemInventoryRet.PurchaseTaxCodeRef.FullName.GetValue();
                }
            }
            if (ItemInventoryRet.COGSAccountRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.COGSAccountRef.ListID != null)
                {
                    string ListID13303 = (string)ItemInventoryRet.COGSAccountRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.COGSAccountRef.FullName != null)
                {
                    string FullName13304 = (string)ItemInventoryRet.COGSAccountRef.FullName.GetValue();
                }
            }
            if (ItemInventoryRet.PrefVendorRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.PrefVendorRef.ListID != null)
                {
                    string ListID13305 = (string)ItemInventoryRet.PrefVendorRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.PrefVendorRef.FullName != null)
                {
                    string FullName13306 = (string)ItemInventoryRet.PrefVendorRef.FullName.GetValue();
                }
            }
            if (ItemInventoryRet.AssetAccountRef != null)
            {
                //Get value of ListID
                if (ItemInventoryRet.AssetAccountRef.ListID != null)
                {
                    string ListID13307 = (string)ItemInventoryRet.AssetAccountRef.ListID.GetValue();
                }
                //Get value of FullName
                if (ItemInventoryRet.AssetAccountRef.FullName != null)
                {
                    string FullName13308 = (string)ItemInventoryRet.AssetAccountRef.FullName.GetValue();
                }
            }
            //Get value of ReorderPoint
            if (ItemInventoryRet.ReorderPoint != null)
            {
                int ReorderPoint13309 = (int)ItemInventoryRet.ReorderPoint.GetValue();
            }
            //Get value of Max
            if (ItemInventoryRet.Max != null)
            {
                int Max13310 = (int)ItemInventoryRet.Max.GetValue();
            }
            //Get value of QuantityOnHand
            if (ItemInventoryRet.QuantityOnHand != null)
            {
                int QuantityOnHand13311 = (int)ItemInventoryRet.QuantityOnHand.GetValue();
            }
            //Get value of AverageCost
            if (ItemInventoryRet.AverageCost != null)
            {
                double AverageCost13312 = (double)ItemInventoryRet.AverageCost.GetValue();
            }
            //Get value of QuantityOnOrder
            if (ItemInventoryRet.QuantityOnOrder != null)
            {
                int QuantityOnOrder13313 = (int)ItemInventoryRet.QuantityOnOrder.GetValue();
            }
            //Get value of QuantityOnSalesOrder
            if (ItemInventoryRet.QuantityOnSalesOrder != null)
            {
                int QuantityOnSalesOrder13314 = (int)ItemInventoryRet.QuantityOnSalesOrder.GetValue();
            }
            //Get value of ExternalGUID
            if (ItemInventoryRet.ExternalGUID != null)
            {
                string ExternalGUID13315 = (string)ItemInventoryRet.ExternalGUID.GetValue();
            }
            if (ItemInventoryRet.DataExtRetList != null)
            {
                for (int i13316 = 0; i13316 < ItemInventoryRet.DataExtRetList.Count; i13316++)
                {
                    IDataExtRet DataExtRet = ItemInventoryRet.DataExtRetList.GetAt(i13316);
                    //Get value of OwnerID
                    if (DataExtRet.OwnerID != null)
                    {
                        string OwnerID13317 = (string)DataExtRet.OwnerID.GetValue();
                    }
                    //Get value of DataExtName
                    string DataExtName13318 = (string)DataExtRet.DataExtName.GetValue();
                    //Get value of DataExtType
                    ENDataExtType DataExtType13319 = (ENDataExtType)DataExtRet.DataExtType.GetValue();
                    //Get value of DataExtValue
                    string DataExtValue13320 = (string)DataExtRet.DataExtValue.GetValue();
                }
            }
        }

        void BuildItemInventoryQueryRq(IMsgSetRequest requestMsgSet)
        {
            IItemInventoryQuery ItemInventoryQueryRq = requestMsgSet.AppendItemInventoryQueryRq();
            //Set attributes
            //Set field value for metaData
           // ItemInventoryQueryRq.metaData.SetValue("IQBENmetaDataType");

            //Set field value for iterator
           // ItemInventoryQueryRq.iterator.SetValue("IQBENiteratorType");

            //Set field value for iteratorID
            ItemInventoryQueryRq.iteratorID.SetValue("IQBUUIDType");
            string ORListQueryWithOwnerIDAndClassElementType13273 = "ListIDList";
            if (ORListQueryWithOwnerIDAndClassElementType13273 == "ListIDList")
            {
                //Set field value for ListIDList
                //May create more than one of these if needed
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListIDList.Add("200000-1011023419");
            }
            if (ORListQueryWithOwnerIDAndClassElementType13273 == "FullNameList")
            {
                //Set field value for FullNameList
                //May create more than one of these if needed
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.FullNameList.Add("ab");
            }
            if (ORListQueryWithOwnerIDAndClassElementType13273 == "ListWithClassFilter")
            {
                //Set field value for MaxReturned
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.MaxReturned.SetValue(6);
                //Set field value for ActiveStatus
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ActiveStatus.SetValue(ENActiveStatus.asActiveOnly);
                //Set field value for FromModifiedDate
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.FromModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), false);
                //Set field value for ToModifiedDate
                ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ToModifiedDate.SetValue(DateTime.Parse("12/15/2007 12:15:12"), false);
                string ORNameFilterElementType13274 = "NameFilter";
                if (ORNameFilterElementType13274 == "NameFilter")
                {
                    //Set field value for MatchCriterion
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ORNameFilter.NameFilter.MatchCriterion.SetValue(ENMatchCriterion.mcStartsWith);
                    //Set field value for Name
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ORNameFilter.NameFilter.Name.SetValue("ab");
                }
                if (ORNameFilterElementType13274 == "NameRangeFilter")
                {
                    //Set field value for FromName
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ORNameFilter.NameRangeFilter.FromName.SetValue("ab");
                    //Set field value for ToName
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ORNameFilter.NameRangeFilter.ToName.SetValue("ab");
                }
                string ORClassFilterElementType13275 = "ListIDList";
                if (ORClassFilterElementType13275 == "ListIDList")
                {
                    //Set field value for ListIDList
                    //May create more than one of these if needed
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ClassFilter.ORClassFilter.ListIDList.Add("200000-1011023419");
                }
                if (ORClassFilterElementType13275 == "FullNameList")
                {
                    //Set field value for FullNameList
                    //May create more than one of these if needed
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ClassFilter.ORClassFilter.FullNameList.Add("ab");
                }
                if (ORClassFilterElementType13275 == "ListIDWithChildren")
                {
                    //Set field value for ListIDWithChildren
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ClassFilter.ORClassFilter.ListIDWithChildren.SetValue("200000-1011023419");
                }
                if (ORClassFilterElementType13275 == "FullNameWithChildren")
                {
                    //Set field value for FullNameWithChildren
                    ItemInventoryQueryRq.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.ClassFilter.ORClassFilter.FullNameWithChildren.SetValue("ab");
                }
            }
            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            ItemInventoryQueryRq.IncludeRetElementList.Add("ab");
            //Set field value for OwnerIDList
            //May create more than one of these if needed
            ItemInventoryQueryRq.OwnerIDList.Add(Guid.NewGuid().ToString());
        }

    }
}
