using Microsoft.EntityFrameworkCore;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderContext _orderContext;

        public OrderRepository(OrderContext orderContext)
        {
            _orderContext = orderContext;
        }

        public async Task<IEnumerable<Entities.Order>> GetOrdersAsync()
        {
            return await _orderContext.Order
                .OrderByDescending(x => x.CreatedDate).ToListAsync();
        }

        public async Task<Entities.Order> GetOrderByIdAsync(Guid orderId)
        {
            var orderIdBytes = orderId.ToByteArray();

            return await _orderContext.Order.SingleOrDefaultAsync(x => _orderContext.Database.IsInMemory() ? x.Id.SequenceEqual(orderIdBytes) : x.Id == orderIdBytes );
        }

        public async Task<IEnumerable<Entities.Order>> GetOrdersByStatusIdAsync(Guid statusId)
        {
            var statusIdBytes = statusId.ToByteArray();

            return await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.StatusId.SequenceEqual(statusIdBytes) : x.StatusId == statusIdBytes)
                .OrderByDescending(x => x.CreatedDate).ToListAsync();
        }

        public async Task UpdateOrder()
        {
            await _orderContext.SaveChangesAsync();
        }

        public async Task AddOrder(Data.Entities.Order order)
        {
            _orderContext.Order.Add(order);
            await _orderContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Entities.Order>> GetOrdersByYearStatusIdAsync(int year, byte[] statusId)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = startDate.AddYears(1);

            return await _orderContext.Order
                .Where(x => _orderContext.Database.IsInMemory() ? x.StatusId.SequenceEqual(statusId) : x.StatusId == statusId
                        && x.CreatedDate >= startDate && x.CreatedDate < endDate)
                .OrderByDescending(x => x.CreatedDate).ToListAsync();
        }
    }
}
