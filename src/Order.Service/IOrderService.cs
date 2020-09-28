using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Service
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderSummary>> GetOrdersAsync();
        
        Task<OrderDetail> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Returns orders filtered by the Status I provided
        /// </summary>
        /// <param name="statusId">The status Id to filter by</param>
        /// <returns>A filtered list of orders</returns>
        Task<IEnumerable<OrderSummary>> GetOrdersByStatusIdAsync(Guid statusId);

        /// <summary>
        /// Updates the Order Status
        /// </summary>
        /// <param name="orderId">Id of the order to update</param>
        /// <param name="orderStatusUpdateModel">Contains the details of the new status</param>
        Task UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdate orderStatusUpdateModel);

        /// <summary>
        /// Validates the params and creates a new order
        /// </summary>
        /// <param name="newOrder">Details of the order to ber created</param>
        /// <returns>Id of the new order</returns>
        Task<Guid> CreateOrderAsync(NewOrder newOrder);

        /// <summary>
        /// Generates a report of the profit by month for the specified year
        /// </summary>
        /// <param name="year">Year to get report for</param>
        /// <returns>List of months where orders were completed and the profit from those orders</returns>
        Task<IEnumerable<ProfitForMonth>> GetMonthlyProfitForYear(int year);
    }
}
