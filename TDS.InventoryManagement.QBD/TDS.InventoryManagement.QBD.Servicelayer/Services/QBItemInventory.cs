using Interop.QBFC13;
using TDS.InventoryManagement.QBD.Servicelayer.Interface;
using TDS.InventoryManagement.QBD.Servicelayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TDS.InventoryManagement.QBD.Servicelayer.Services
{
    public class QBItemInventory : IQBItemInventoryInterface
    {
        public void CreatInventoryItem(QBSessionManager sessionManager, IAccountRetList accountInfo, IPreferencesRet PreferencesRet, IItemInventoryRet inventoryItem)
        {                          
                IMsgSetRequest requestMsgSet = null;
                QBSession QBMgr = null;
                try
                {
                    QBMgr = new QBSession();
                    if(sessionManager == null)
                       QBMgr.CreateQBSession(out sessionManager);

                    if (sessionManager != null)
                    {
                        // Get the RequestMsgSet based on the correct QB Version
                        IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                        if (requestSet != null)
                        {
                            // Initialize the message set request object
                            requestSet.Attributes.OnError = ENRqOnError.roeStop;

                            BuildItemInventoryAddRq(requestSet, accountInfo, PreferencesRet, inventoryItem);                            

                            // Do the request and get the response message set object
                            IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                            //WalkItemInventoryAddRs(responseSet);                            
                        }
                    }
                }
                catch (Exception ex)
                {                    
                }  
        }

        public IResponseList GetItemInventor(string fullName, QBSessionManager sessionManager)
        {            
            QBSession QBMgr = null;
            IResponseList responseList = null;
            try
            {
                QBMgr = new QBSession();
                if(sessionManager == null)
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
                        itemInventory.ORListQueryWithOwnerIDAndClass.FullNameList.Add(fullName);



                        // Optionally, you can put filter on it.
                        //itemInventory.ORListQueryWithOwnerIDAndClass.ListWithClassFilter.MaxReturned.SetValue(50);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseMsgSet = sessionManager.DoRequests(requestSet);


                        if (responseMsgSet == null) return null;
                        responseList = responseMsgSet.ResponseList;
                        if (responseList == null) return null;

                    }
                }
            }
            catch (Exception ex)
            {               
            }
            return responseList;
        }

        public QBResponceItem Inventoryadjustment(QBSessionManager sessionManager, IAccountRetList accountInfo, IPreferencesRet PreferencesRet, IItemInventoryRet inventoryItem)
        {
            QBResponceList qbResponceData = null;
            QBSession QBMgr = null;
            QBResponceItem _qbResponceItem = null;
            try
            {
                QBMgr = new QBSession();
                if(sessionManager == null)
                    QBMgr.CreateQBSession(out sessionManager);

                if (sessionManager != null && inventoryItem != null && accountInfo != null)
                {
                    // Get the RequestMsgSet based on the correct QB Version
                    IMsgSetRequest requestSet = QBMgr.getLatestMsgSetRequest(sessionManager);

                    if (requestSet != null)
                    {
                        // Initialize the message set request object
                        requestSet.Attributes.OnError = ENRqOnError.roeStop;

                        BuildInventoryAdjustmentAddRq(requestSet, accountInfo, PreferencesRet, inventoryItem);

                        // Uncomment the following to view and save the request and response XML
                        //string requestXML = requestSet.ToXMLString();
                        //MessageBox.Show(requestXML);

                        // Do the request and get the response message set object
                        IMsgSetResponse responseSet = sessionManager.DoRequests(requestSet);

                        //string responseXML = responseSet.ToXMLString();
                        //MessageBox.Show(responseXML);

                        _qbResponceItem = WalkInventoryAdjustmentAddRs(responseSet);

                        //Close the session and connection with QuickBooks
                        //QBMgr.CloseQBConnection(sessionManager);
                    }
                }
            }
            catch (Exception ex)
            {                
            }
            return _qbResponceItem;
        }

        public QBResponceList InventoryadjustmentBulkOperation(QBSessionManager sessionManager, QBRequestItemSet qBRequestItemSet)
        {
            QBResponceList qbResponceData = null;
            QBSession QBMgr = null;
            QBResponceItem _qbResponceItem = null;
            QBMgr = new QBSession();
            if (sessionManager == null)
                QBMgr.CreateQBSession(out sessionManager);

            if (sessionManager != null && qBRequestItemSet != null && qBRequestItemSet.QBRequestItemList != null && qBRequestItemSet.QBRequestItemList.Count> 0)
            {
                //Parallel.ForEach(qBRequestItemSet.QBRequestItemList, item => Process(item));
                foreach (var data in qBRequestItemSet.QBRequestItemList)
                {
                    if (data != null)
                    {

                        if (data.Action == QBAction.Modify)
                        {
                            //Update quntity.
                           var responceItem = Inventoryadjustment(sessionManager, qBRequestItemSet.AccountList, qBRequestItemSet.PreferencesRet, data.ItemInventoryRet);
                        }
                        else
                        {
                            //Add Item
                            CreatInventoryItem(sessionManager, qBRequestItemSet.AccountList, qBRequestItemSet.PreferencesRet, data.ItemInventoryRet);
                        }
                    }
                }
            
            }
            return qbResponceData;
        }



        public IResponseList LoadQBItemInventoryList(int maxRecords, QBSessionManager sessionManager)
        {
            // IMsgSetRequest requestMsgSet = null;
            QBSession QBMgr = null;
            IResponseList responseList = null;

            try
            {
                QBMgr = new QBSession();
                if (sessionManager == null)
                {
                    QBMgr.CreateQBSession(out sessionManager);
                }

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
                        IMsgSetResponse responseMsgSet = sessionManager.DoRequests(requestSet);

                        //IItemInventoryRetList itemInventoryRetList = null;
                        if (responseMsgSet == null) return null;
                        responseList = responseMsgSet.ResponseList;
                        if (responseList == null) return null;

                       // string responseXML = responseMsgSet.ToXMLString();
                    }
                }
            }
            catch (Exception ex)
            {
            }            
            return responseList;
        }

        private  QBResponceItem WalkInventoryAdjustmentAddRs(IMsgSetResponse responseMsgSet)
        {
            QBResponceItem _qbResponceItem = null;
            if (responseMsgSet == null) return null;
            IResponseList responseList = responseMsgSet.ResponseList;
            if (responseList == null) return null;
            //if we sent only one request, there is only one response, we'll walk the list for this sample
            for (int i = 0; i < responseList.Count; i++)
            {
                _qbResponceItem = new QBResponceItem();
                IResponse response = responseList.GetAt(i);

                _qbResponceItem.StatusCode = response.StatusCode;
                _qbResponceItem.StatusMessage = response.StatusMessage;
                _qbResponceItem.StatusSeverity = response.StatusSeverity;


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

                            _qbResponceItem.InventoryItemAdjustment = InventoryAdjustmentRet;                           
                        }
                    }
                }
            }
            return _qbResponceItem;
        }

        private void BuildInventoryAdjustmentAddRq(IMsgSetRequest requestMsgSet, IAccountRetList accountInfo, IPreferencesRet PreferencesRet, IItemInventoryRet inventoryItem)
        {
            IInventoryAdjustmentAdd InventoryAdjustmentAddRq = requestMsgSet.AppendInventoryAdjustmentAddRq();
            //Set attributes
            //Set field value for defMacro
            InventoryAdjustmentAddRq.defMacro.SetValue("IQBStringType");
            //Set field value for ListID
            InventoryAdjustmentAddRq.AccountRef.ListID.SetValue(inventoryItem.AssetAccountRef.ListID.GetValue()); ; //"80000022-1552565294"
            //Set field value for FullName
            InventoryAdjustmentAddRq.AccountRef.FullName.SetValue(inventoryItem.AssetAccountRef.FullName.GetValue()); //"Inventory Asset"

            //Set field value for TxnDate
            InventoryAdjustmentAddRq.TxnDate.SetValue(DateTime.Now); //"20-03-2019"

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
            InventoryAdjustmentLineAdd10753.ItemRef.ListID.SetValue(inventoryItem.ListID.GetValue()); //"8000000B-1553066645" Item List Id
            //Set field value for FullName
            InventoryAdjustmentLineAdd10753.ItemRef.FullName.SetValue(inventoryItem.FullName.GetValue()); //"venky04" Item Name
            string ORTypeAdjustmentElementType10754 = "QuantityAdjustment";
            if (ORTypeAdjustmentElementType10754 == "QuantityAdjustment")
            {
                string ORQuantityAdjustmentElementType10755 = "NewQuantity";
                if (ORQuantityAdjustmentElementType10755 == "NewQuantity")
                {
                    //Set field value for NewQuantity
                    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.NewQuantity.SetValue(inventoryItem.QuantityOnHand.GetValue());
                }
                //if (ORQuantityAdjustmentElementType10755 == "QuantityDifference")
                //{
                //    //Set field value for QuantityDifference
                //    InventoryAdjustmentLineAdd10753.ORTypeAdjustment.QuantityAdjustment.ORQuantityAdjustment.QuantityDifference.SetValue(10);
                //}

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

        private void BuildItemInventoryAddRq(IMsgSetRequest requestMsgSet, IAccountRetList accountInfo, IPreferencesRet PreferencesRet, IItemInventoryRet inventoryItem)
        {
            //IItemInventoryAdd[] AdditemList = new IItemInventoryAdd[2];
            IItemInventoryAdd ItemInventoryAddRq = requestMsgSet.AppendItemInventoryAddRq();
            //Set field value for Name
            ItemInventoryAddRq.Name.SetValue(inventoryItem.Name.GetValue()); //"venky01"

            //Set field value for BarCodeValue
            //ItemInventoryAddRq.BarCode.BarCodeValue.SetValue("ab");
            //Set field value for AssignEvenIfUsed
            //ItemInventoryAddRq.BarCode.AssignEvenIfUsed.SetValue(true);
            //Set field value for AllowOverride
            //ItemInventoryAddRq.BarCode.AllowOverride.SetValue(true);

            //Set field value for IsActive
            ItemInventoryAddRq.IsActive.SetValue(true);

            if (inventoryItem.ClassRef != null)
            {
                ////Set field value for ListID
                //ItemInventoryAddRq.ClassRef.ListID.SetValue("200000-1011023419");
                ////Set field value for FullName
                //ItemInventoryAddRq.ClassRef.FullName.SetValue("ab");
            }

                if (inventoryItem.ParentRef != null)
            {
                ////Set field value for ListID
                //ItemInventoryAddRq.ParentRef.ListID.SetValue("200000-1011023419");
                ////Set field value for FullName
                //ItemInventoryAddRq.ParentRef.FullName.SetValue("ab");
            }

            //Set field value for ManufacturerPartNumber
            ItemInventoryAddRq.ManufacturerPartNumber.SetValue(inventoryItem.ManufacturerPartNumber.GetValue());
            if (inventoryItem.UnitOfMeasureSetRef != null)
            {
                //Set field value for ListID
                // ItemInventoryAddRq.UnitOfMeasureSetRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                //ItemInventoryAddRq.UnitOfMeasureSetRef.FullName.SetValue("ab");
            }

            //Set field value for IsTaxIncluded
            //ItemInventoryAddRq.IsTaxIncluded.SetValue(true);

            if (inventoryItem.SalesTaxCodeRef != null)
            {
                //Set field value for ListID
                //ItemInventoryAddRq.SalesTaxCodeRef.ListID.SetValue("200000-1011023419");
                //Set field value for FullName
                // ItemInventoryAddRq.SalesTaxCodeRef.FullName.SetValue("ab");
            }

            //Set field value for SalesDesc
            ItemInventoryAddRq.SalesDesc.SetValue(inventoryItem.SalesDesc.GetValue());
            //Set field value for SalesPrice
            ItemInventoryAddRq.SalesPrice.SetValue(inventoryItem.SalesPrice.GetValue());

            if (inventoryItem.IncomeAccountRef != null)
            {
                //Set field value for ListID
                ItemInventoryAddRq.IncomeAccountRef.ListID.SetValue(inventoryItem.IncomeAccountRef.ListID.GetValue());//"80000008-1552559357"
                                                                                                                      //Set field value for FullName
                ItemInventoryAddRq.IncomeAccountRef.FullName.SetValue(inventoryItem.IncomeAccountRef.FullName.GetValue());
            }

            //Set field value for PurchaseDesc
            ItemInventoryAddRq.PurchaseDesc.SetValue(inventoryItem.PurchaseDesc.GetValue());
            //Set field value for PurchaseCost
            ItemInventoryAddRq.PurchaseCost.SetValue(inventoryItem.PurchaseCost.GetValue());

            //Set field value for ListID
            // ItemInventoryAddRq.PurchaseTaxCodeRef.ListID.SetValue("200000-1011023419");
            //Set field value for FullName
            // ItemInventoryAddRq.PurchaseTaxCodeRef.FullName.SetValue("ab");

            if (inventoryItem.COGSAccountRef != null)
            {
                //Set field value for ListID
                ItemInventoryAddRq.COGSAccountRef.ListID.SetValue(inventoryItem.COGSAccountRef.ListID.GetValue());
                //Set field value for FullName
                ItemInventoryAddRq.COGSAccountRef.FullName.SetValue(inventoryItem.COGSAccountRef.FullName.GetValue());
            }

            if (inventoryItem.PrefVendorRef != null)
            {
                //Set field value for ListID
                ItemInventoryAddRq.PrefVendorRef.ListID.SetValue(inventoryItem.PrefVendorRef.ListID.GetValue());
                //Set field value for FullName
                ItemInventoryAddRq.PrefVendorRef.FullName.SetValue(inventoryItem.PrefVendorRef.FullName.GetValue());
            }

            if (inventoryItem.AssetAccountRef != null)
            {
                //Set field value for ListID
                ItemInventoryAddRq.AssetAccountRef.ListID.SetValue(inventoryItem.AssetAccountRef.ListID.GetValue());
                //Set field value for FullName
                ItemInventoryAddRq.AssetAccountRef.FullName.SetValue(inventoryItem.AssetAccountRef.FullName.GetValue());
            }

            //Set field value for ReorderPoint
            // ItemInventoryAddRq.ReorderPoint.SetValue(2);
            //Set field value for Max
            // ItemInventoryAddRq.Max.SetValue(2);
            //Set field value for QuantityOnHand
            ItemInventoryAddRq.QuantityOnHand.SetValue(inventoryItem.QuantityOnHand.GetValue());
            //Set field value for TotalValue
            //ItemInventoryAddRq.TotalValue.SetValue(1500);
            //Set field value for InventoryDate
            //ItemInventoryAddRq.InventoryDate.SetValue(DateTime.Now);
            //ItemInventoryAddRq.InventoryDate.SetValue(DateTime.Parse("19-03-2019"));

            //Set field value for ExternalGUID
            //ItemInventoryAddRq.ExternalGUID.SetValue(Guid.NewGuid().ToString());

            //Set field value for IncludeRetElementList
            //May create more than one of these if needed
            //ItemInventoryAddRq.IncludeRetElementList.Add("ab");
        }

    }
}
