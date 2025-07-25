using Microsoft.AspNetCore.Mvc;
using POManagerAPI.Models;
using POManagerAPI.Services;

namespace POManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductionOrderController : ControllerBase
    {
        private readonly IProductionOrderService _service;

        public ProductionOrderController(IProductionOrderService service)
        {
            _service = service;
        }

        [HttpGet("{poNumber}")]
        public ActionResult<ProductionOrder> GetProductionOrder(string poNumber)
        {
            var po = _service.GetProductionOrder(poNumber);
            if (po == null) return NotFound();
            return Ok(po);
        }

        [HttpPost("goodsreceipt")]
        public IActionResult SubmitGoodsReceipt([FromBody] GoodsReceiptRequest request)
        {
            var success = _service.SubmitGoodsReceipt(request.PONumber, request.SSCC, request.Quantity);
            if (!success) return BadRequest("Failed to submit goods receipt.");
            return Ok("Goods receipt submitted successfully.");
        }
    }

    public class GoodsReceiptRequest
    {
        public string PONumber { get; set; }
        public string SSCC { get; set; }
        public int Quantity { get; set; }
    }
}
