using Order.Data.Entities;
using System;
using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderStatusRepository
    {
        /// <summary>
        /// Checks whether the provided order status id exists
        /// </summary>
        /// <param name="statusId">Guid of the Order Status</param>
        /// <returns>The Order Status or NULL is status doesn't exist</returns>
        Task<bool> DoesOrderStatusIdExistAsync(Guid statusId);

        /// <summary>
        /// Gets the status details for the named status
        /// </summary>
        /// <param name="name">The name of the status to retrieve</param>
        /// <returns>The Status</returns>
        Task<OrderStatus> GetStatusByName(string name);
    }
}
