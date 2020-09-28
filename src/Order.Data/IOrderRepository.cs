using Order.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Entities.Order>> GetOrdersAsync();

        Task<Entities.Order> GetOrderByIdAsync(Guid orderId);

        /// <summary>
        /// Retrieves all orders with the specified status Id
        /// </summary>
        /// <param name="statusId">Guid of the status to filter the orders by</param>
        /// <returns>Enumeralble list of orders withthe specified status Id</returns>
        Task<IEnumerable<Entities.Order>> GetOrdersByStatusIdAsync(Guid statusId);

        /// <summary>
        /// Commits any changes to the database
        /// </summary>
        Task UpdateOrder();

        /// <summary>
        /// Adds a new order
        /// </summary>
        /// <param name="order">The order to add</param>
        Task AddOrder(Data.Entities.Order order);

        /// <summary>
        /// Retrieves all orders within the specified year with the specified status Id
        /// </summary>
        /// <param name="year">The year to pull the orders for</param>
        /// <param name="statusId">Guid of the status to filter the orders by</param>
        /// <returns>Enumeralble list of orders withthe specified status Id</returns>
        Task<IEnumerable<Entities.Order>> GetOrdersByYearStatusIdAsync(int year, byte[] statusId);
    }
}
