using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderStatusRepository : IOrderStatusRepository
    {
        private readonly OrderContext _orderContext;

        public OrderStatusRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<bool> DoesOrderStatusIdExistAsync(Guid statusId)
        {
            var statusIdBytes = statusId.ToByteArray();
            return await _orderContext.OrderStatus.AnyAsync(x => x.Id == statusIdBytes);
        }

        public async Task<OrderStatus> GetStatusByName(string name)
        {
            return await _orderContext.OrderStatus.FirstOrDefaultAsync(x => x.Name == name);
        }
    }
}
