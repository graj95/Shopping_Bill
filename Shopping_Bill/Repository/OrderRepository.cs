using Dapper;
using Shopping_Bill.Context;
using Shopping_Bill.Models;
using Shopping_Bill.Repository.Interface;
using System.Data;

namespace Shopping_Bill.Repository
{
    public class OrderRepository : IOrderRepository
    {
         private readonly DapperContext _context;

        public OrderRepository(DapperContext context)
        {
            _context = context;
        }
        public async Task<int> Delete(int id)
        {
            int result = 0;
            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@orderId", id);
                result = connection.Execute("SP_Delete_Order", dynamicParameters, commandType: CommandType.StoredProcedure);
            }
            return result;
        }

        public async Task<Order> GetOrderById(int orderId)
        {
            Order order = new Order();

            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@orderId", orderId);
                order = connection.Query<Order>("SP_SelectById_Order", dynamicParameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                order.OrderDetails = connection.Query<OrderDetails>("SP_SelectbyId_OrderDetails", dynamicParameters, commandType: CommandType.StoredProcedure).ToList();

            }
            return order;
        }

        public async Task<IEnumerable<Order>> GetOrders()
        {
            List<Order> orders = new List<Order>();
            Order order = new Order();
            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();

                orders = connection.Query<Order>("SP_SelectAll_Order", commandType: CommandType.StoredProcedure).ToList();
                foreach (var item in orders)
                {
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("@orderId", item.orderId);
                    item.OrderDetails = connection.Query<OrderDetails>("SP_SelectById_OrderDetails", dynamicParameters, commandType: CommandType.StoredProcedure).ToList();
                }
            }
            return orders;
        }
        public async Task<int> PlaceOrder(Order order)
        {
            int result = 0;
            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@orderCode", order.orderCode);
                dynamicParameters.Add("@custName", order.custName);
                dynamicParameters.Add("@mobileNumber", order.mobileNumber);
                dynamicParameters.Add("@shippingAddress", order.shippingAddress);
                dynamicParameters.Add("@billingAddress", order.billingAddress);
                List<OrderDetails> odlist = new List<OrderDetails>();
                odlist = order.OrderDetails.ToList();
                result = await connection.QuerySingleAsync<int>("SP_Place_Order", dynamicParameters, commandType: CommandType.StoredProcedure);
                double result1 = await AddProduct(odlist, result);
                DynamicParameters dynamicParameters1 = new DynamicParameters();
                dynamicParameters1.Add("@orderId", result);
                dynamicParameters1.Add("@totalAmount", result1);
                var result2 = await connection.ExecuteAsync("SP_UP_Order", dynamicParameters1, commandType: CommandType.StoredProcedure);

            }
            return result;
        }

        private async Task<double> AddProduct(List<OrderDetails> orders, int orderId)
        {
            double grandtotal = 0;
            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                foreach (OrderDetails order in orders)
                {
                    order.orderId = orderId;
                    DynamicParameters dynamicParameters = new DynamicParameters();
                    dynamicParameters.Add("@productId", order.productId);
                    dynamicParameters.Add("@productName", order.productName);
                    dynamicParameters.Add("@rate", order.rate);
                    dynamicParameters.Add("@quentity", order.quentity);
                    dynamicParameters.Add("@orderId", order.orderId);
                    int result = await connection.QuerySingleAsync<int>("SP_Add_Product", dynamicParameters, commandType: CommandType.StoredProcedure);
                    order.totalAmount = order.rate * order.quentity;
                    grandtotal = grandtotal + order.totalAmount;
                    DynamicParameters dynamicParameters1 = new DynamicParameters();
                    dynamicParameters1.Add("@detailsId", result);
                    dynamicParameters1.Add("@totalAmount", order.totalAmount);
                    result = await connection.ExecuteAsync("SP_UP_Product", dynamicParameters1, commandType: CommandType.StoredProcedure);

                }
            }
            return grandtotal;
        }

        public async Task<int> UpdateOrder(Order order)
        {
            int result = 0;
            using (var connection = _context.CreateConnection())
            {
                if (connection.State == ConnectionState.Closed)
                    connection.Open();
                DynamicParameters dynamicParameters = new DynamicParameters();
                dynamicParameters.Add("@orderId", order.orderId);
                dynamicParameters.Add("@orderCode", order.orderCode);
                dynamicParameters.Add("@custName", order.custName);
                dynamicParameters.Add("@mobileNumber", order.mobileNumber);
                dynamicParameters.Add("@shippingAddress", order.shippingAddress);
                dynamicParameters.Add("@billingAddress", order.billingAddress);
                List<OrderDetails> odlist = new List<OrderDetails>();
                odlist = order.OrderDetails.ToList();
                result = connection.Execute("SP_Update_Order", dynamicParameters, commandType: CommandType.StoredProcedure);
                double result1 = await AddProduct(odlist, order.orderId);
                DynamicParameters dynamicParameters1 = new DynamicParameters();
                dynamicParameters1.Add("@orderId", order.orderId);
                dynamicParameters1.Add("@totalAmount", result1);
                var result2 = await connection.ExecuteAsync("SP_UP_Order", dynamicParameters1, commandType: CommandType.StoredProcedure);
                result = order.orderId;
            }
            return result;
        }
    }
}
