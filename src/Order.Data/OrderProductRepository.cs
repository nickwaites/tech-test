using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using System;
using System.Threading.Tasks;
namespace Order.Data
{
    public class OrderProductRepository : IOrderProductRepository
    {
        private readonly OrderContext _orderContext;

        public OrderProductRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<OrderProduct> GetProductById(Guid productId)
        {
            var productIdBytes = productId.ToByteArray();
            return await _orderContext.OrderProduct.FirstOrDefaultAsync(x => x.Id == productIdBytes);
        }
    }
}
