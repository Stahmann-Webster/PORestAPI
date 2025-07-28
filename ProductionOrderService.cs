using POManagerAPI.Models;
using SAP_Business_One;
using SAPbobsCOM;
using System.Runtime.InteropServices;

namespace POManagerAPI.Services
{
    public class ProductionOrderService : IProductionOrderService
    {
        private readonly ServerConnection _serverConnection;

        public ProductionOrderService(ServerConnection serverConnection)
        {
            _serverConnection = serverConnection;
        }


        public ProductionOrder GetProductionOrder(string poNumber)
        {
            if (_serverConnection.Connect() != 0)
            {
                throw new Exception($"SAP Connection failed: {_serverConnection.GetErrorCode()} - {_serverConnection.GetErrorMessage()}");
            }

            Company oCompany = _serverConnection.GetCompany();
            Recordset ors = (Recordset)oCompany.GetBusinessObject(BoObjectTypes.BoRecordset);

            string query = $@"
                SELECT DISTINCT 
                    T0.[Prod Ord],                    
                    IGN1.DocDate AS Date, 
                    [Created Date], 
                    U_PMX_PLCD AS ProdLine, 
                    Item,
                    T0.[Batch Qty],
                    T0.[Status],
                FROM [MCS_QLIK_PRODBATCH_CREATED_UNION_V3] T0 
                JOIN IGN1 ON [Prod Ord] = BaseRef  
                JOIN OIGN ON IGN1.DocDate = OIGN.DocDate 
                JOIN OWOR ON OWOR.DocNum = [Prod Ord]  
                WHERE IGN1.DocDate >= '2018-10-23'   
                  AND IGN1.DocDate <= '2019-10-23'   
                  AND IGN1.BaseType = 202   
                  AND [Trans Type] IS NOT NULL   
                  AND [Receipt Type] != 'Virtual'   
                  AND IGN1.DocDate = [Created Date]
                  AND T0.[Prod Ord] = '{poNumber}'
                ORDER BY IGN1.DocDate, T0.[Prod Ord]";

            ors.DoQuery(query);

            ProductionOrder po = null;

            if (!ors.EoF)
            {
                po = new ProductionOrder
                {
                    PONumber = ors.Fields.Item(0).Value.ToString(),
                    DueDate = Convert.ToDateTime(ors.Fields.Item(1).Value),
                    Description = ors.Fields.Item(4).Value.ToString(),
                    Quantity = ors.Fields.Item(5).Value.ToString(),   //100, // Placeholder
                    Status = ors.Fields.Item(6).Value.ToString(), //"Released" // Placeholder
                };
            }

            Marshal.ReleaseComObject(ors);
            _serverConnection.Disconnect();

            return po;
        }

        public bool SubmitGoodsReceipt(string poNumber, string sscc, int quantity)
        {
            if (_serverConnection.Connect() != 0)
            {
                throw new Exception($"SAP Connection failed: {_serverConnection.GetErrorCode()} - {_serverConnection.GetErrorMessage()}");
            }

            Company company = _serverConnection.GetCompany();

            try
            {
                // Get PO DocEntry
                Recordset rs = (Recordset)company.GetBusinessObject(BoObjectTypes.BoRecordset);
                rs.DoQuery($"SELECT DocEntry, ItemCode FROM OWOR WHERE DocNum = '{poNumber}'");

                if (rs.EoF)
                {
                    Marshal.ReleaseComObject(rs);
                    return false;
                }

                int baseEntry = int.Parse(rs.Fields.Item("DocEntry").Value.ToString());
                string itemCode = rs.Fields.Item("ItemCode").Value.ToString();
                Marshal.ReleaseComObject(rs);

                // Create Goods Receipt
                Documents goodsReceipt = (Documents)company.GetBusinessObject(BoObjectTypes.oInventoryGenEntry);

                goodsReceipt.DocDate = DateTime.Now;
                goodsReceipt.DocDueDate = DateTime.Now.AddDays(1);
                goodsReceipt.Reference1 = poNumber;
                goodsReceipt.Comments = "Submitted via Web API";
                goodsReceipt.JournalMemo = "Goods Receipt for Production Order";
                goodsReceipt.BPL_IDAssignedToInvoice = 19; // Default branch ID

                // Add line
                goodsReceipt.Lines.ItemCode = itemCode;
                goodsReceipt.Lines.Quantity = quantity;
                goodsReceipt.Lines.WarehouseCode = "81LEE"; // Default warehouse
                goodsReceipt.Lines.BaseType = 202;
                goodsReceipt.Lines.BaseEntry = baseEntry;
                goodsReceipt.Lines.BaseLine = 0;

                // Optional user fields (fill with defaults if required)
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_LOCO").Value = "DEFAULT_LOCO";
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_QYSC").Value = "RELEASED";
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_QUAN").Value = quantity.ToString();
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_BATC").Value = sscc;
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_BBDT").Value = DateTime.Now.AddYears(1).ToString("yyyyMMdd");
                goodsReceipt.Lines.UserFields.Fields.Item("U_PMX_SSCC").Value = sscc;

                // Add batch info
                goodsReceipt.Lines.BatchNumbers.BatchNumber = sscc;
                goodsReceipt.Lines.BatchNumbers.Quantity = quantity;
                goodsReceipt.Lines.BatchNumbers.ManufacturingDate = DateTime.Now.AddMonths(-1);
                goodsReceipt.Lines.BatchNumbers.ExpiryDate = DateTime.Now.AddYears(1);
                goodsReceipt.Lines.BatchNumbers.AddmisionDate = DateTime.Now;

                int result = goodsReceipt.Add();
                if (result != 0)
                {
                    string error = company.GetLastErrorDescription();
                    Console.WriteLine("Error: " + error);
                    return false;
                }

                return true;
            }
            finally
            {
                _serverConnection.Disconnect();
            }
        }
    }
}


