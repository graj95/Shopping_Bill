using Shopping_Bill.Models;

namespace Shopping_Bill.Repository.Interface
{
    public interface IOrderRepository
    {
        public Task<IEnumerable<Order>> GetOrders();
        public Task<Order> GetOrderById(int id);
        public Task<int> PlaceOrder(Order order);
        public Task<int> UpdateOrder(Order order);

        public Task<int> Delete(int id);
    }
}
