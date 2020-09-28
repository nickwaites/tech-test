using Order.Data.Entities;
using System;

using System.Threading.Tasks;

namespace Order.Data
{
    public interface IOrderProductRepository
    {

        /// <summary>
        /// Gets the product specified bt the Product Id
        /// </summary>
        /// <param name="productId">Id of Product to retrieve</param>
        /// <returns>Requested product</returns>
        Task<OrderProduct> GetProductById(Guid productId);
    }
}
