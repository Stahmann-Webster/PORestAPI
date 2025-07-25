
using POManagerAPI.Models;

namespace POManagerAPI.Services
{
    public interface IProductionOrderService
    {
        ProductionOrder GetProductionOrder(string poNumber);
        bool SubmitGoodsReceipt(string poNumber, string sscc, int quantity);
    }
}
