namespace Shopping_Bill.Models
{
    public class OrderDetails
    {
        public int detailsId { get; set; }
        public int productId { get; set; }
        public string? productName { get; set; }

        public double rate { get; set; }

        public int quentity { get; set; }


        public double totalAmount { get; set; }

        public int orderId { get; set; }
    }
}
