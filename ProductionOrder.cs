
namespace POManagerAPI.Models
{
    public class ProductionOrder
    {
        public string PONumber { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
    }

     class GoodsReceiptData
    {
        public int DocNum { get; set; }
        public DateTime DocDate { get; set; }
        public DateTime DocDueDate { get; set; }
        public string Reference1 { get; set; }
        public string Reference2 { get; set; }
        public string Comments { get; set; }
        public string JournalMemo { get; set; }
        public List<GoodReceiptLineData> Lines { get; set; }
        public List<GoodReceiptBatchData> Batch { get; set; }

    }

    public class GoodReceiptLineData
    {
        public int LineNum { get; set; }
        public double Quantity { get; set; }
        public DateTime ShipDate { get; set; }
        public string WarehouseCode { get; set; }
        public int BaseType { get; set; }
        public int BaseEntry { get; set; }
        public int BaseLine { get; set; }
        public string CostingCode { get; set; }
        public string CostingCode2 { get; set; }
        public string CostingCode3 { get; set; }
        public string CostingCode4 { get; set; }
        public string U_PMX_LOCO { get; set; }
        public string U_PMX_QYSC { get; set; }
        public double U_PMX_QUAN { get; set; }
        public string U_PMX_BATC { get; set; }
        public DateTime U_PMX_BBDT { get; set; }
        public string U_PMX_SSCC { get; set; }
    }

    public class GoodReceiptBatchData
    {
        public int LineNum { get; set; }
        public double Quantity { get; set; }
        public string BatchNumber { get; set; }
        public DateTime ExpiryDate { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime AddmisionDate { get; set; }
    }
}
