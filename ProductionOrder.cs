
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
}
