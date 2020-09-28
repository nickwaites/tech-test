using Order.Data;
using Order.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderStatusRepository _orderStatusRepository;
        private readonly IOrderProductRepository _orderProductRepository;


        public OrderService(IOrderRepository orderRepository, 
            IOrderStatusRepository orderStatusRepository,
            IOrderProductRepository orderProductRepository)
        {
            _orderRepository = orderRepository;
            _orderStatusRepository = orderStatusRepository;
            _orderProductRepository = orderProductRepository;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersAsync()
        {
            var orderEntities = await _orderRepository.GetOrdersAsync();

            return ConvertToOrderSummaries(orderEntities);
        }

        public async Task<OrderDetail> GetOrderByIdAsync(Guid orderId)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);

            if (order == null)
                return null;

            var orderDetail = new OrderDetail
            {
                Id = new Guid(order.Id),
                ResellerId = new Guid(order.ResellerId),
                CustomerId = new Guid(order.CustomerId),
                StatusId = new Guid(order.StatusId),
                StatusName = order.Status.Name,
                CreatedDate = order.CreatedDate,
                TotalCost = order.Items.Sum(x => x.Quantity * x.Product.UnitCost).Value,
                TotalPrice = order.Items.Sum(x => x.Quantity * x.Product.UnitPrice).Value,
                Items = order.Items.Select(x => new Model.OrderItem
                {
                    Id = new Guid(x.Id),
                    OrderId = new Guid(x.OrderId),
                    ServiceId = new Guid(x.ServiceId),
                    ServiceName = x.Service.Name,
                    ProductId = new Guid(x.ProductId),
                    ProductName = x.Product.Name,
                    UnitCost = x.Product.UnitCost,
                    UnitPrice = x.Product.UnitPrice,
                    TotalCost = x.Product.UnitCost * x.Quantity.Value,
                    TotalPrice = x.Product.UnitPrice * x.Quantity.Value,
                    Quantity = x.Quantity.Value
                })
            };

            return orderDetail;
        }

        public async Task<IEnumerable<OrderSummary>> GetOrdersByStatusIdAsync(Guid statusId)
        {
            var orderEntities = await _orderRepository.GetOrdersByStatusIdAsync(statusId);

            return ConvertToOrderSummaries(orderEntities);
        }

        public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatusUpdate orderStatusUpdateModel)
        {
            var order = await _orderRepository.GetOrderByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException("Order Id does not exist");

            if(!orderStatusUpdateModel.StatusId.HasValue || !await _orderStatusRepository.DoesOrderStatusIdExistAsync(orderStatusUpdateModel.StatusId.Value))
                throw new ArgumentException("Status Id is invalid", "statusId");

            order.StatusId = orderStatusUpdateModel.StatusId.Value.ToByteArray();
            await _orderRepository.UpdateOrder();
        }

        public async Task<Guid> CreateOrderAsync(NewOrder newOrder)
        {
            if (!newOrder.CustomerId.HasValue)
                throw new ArgumentException("Customer Id is invalid", "Order.CustomerId");

            if (!newOrder.ResellerId.HasValue)
                throw new ArgumentException("Reseller Id is invalid", "Order.ResellerId");

            if(newOrder.Items == null || !newOrder.Items.Any())
                throw new ArgumentException("An order must have at least one order line", "Order.Items");

            var orderId = Guid.NewGuid();

            //Get the "Created" status
            var status = await _orderStatusRepository.GetStatusByName("Created");

            var order = new Data.Entities.Order
            {
                Id = orderId.ToByteArray(),
                CreatedDate = DateTime.Now,
                CustomerId = newOrder.CustomerId.Value.ToByteArray(),
                ResellerId = newOrder.ResellerId.Value.ToByteArray(),
                StatusId = status.Id,
                Items = new List<Data.Entities.OrderItem>()
            };

            foreach (var item in newOrder.Items)
            {
                if (!item.ProductId.HasValue)
                    throw new ArgumentException($"Product Id must be supplied ", "Order.Item.ProductId");
                else
                {
                    var product = await _orderProductRepository.GetProductById(item.ProductId.Value);
                    if(product == null)
                        throw new ArgumentException($"Invalid Order Product Id : {item.ProductId}", "Order.Item.ProductId");

                    order.Items.Add(new Data.Entities.OrderItem
                    {
                        Id = Guid.NewGuid().ToByteArray(),
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        ServiceId = product.ServiceId
                    });
                }
            }

            await _orderRepository.AddOrder(order);
            return orderId;
        }

        public async Task<IEnumerable<ProfitForMonth>> GetMonthlyProfitForYear(int year)
        {
            var completedStatus = await _orderStatusRepository.GetStatusByName("Completed");
            var orderEntities = await _orderRepository.GetOrdersByYearStatusIdAsync(year, completedStatus.Id);

            List<ProfitForMonth> report = new List<ProfitForMonth>();

            for (int n = 1; n <= 12; n++)
            {
                var startDate = new DateTime(year, n, 1);
                var endDate = startDate.AddMonths(1);
                var monthProfit = orderEntities.Where(x => x.CreatedDate > startDate && x.CreatedDate < endDate)
                        .Sum(x => x.Items.Sum(s => s.Quantity * (s.Product.UnitPrice - s.Product.UnitCost)));

                report.Add(new ProfitForMonth
                {
                    Month = n,
                    MonthName = startDate.ToString("MMMM"),
                    Profit = monthProfit ?? 0
                });
            }
            return report;
        }

        /// <summary>
        /// Private method to do the mapping from Order Entities to OrderSummary models
        /// Refactored as was being used multiple times
        /// Could maybe be replaced by AutoMapper if architecting full solution
        /// </summary>
        /// <param name="orders"></param>
        /// <returns>List of OrderSummaries</returns>
        private IEnumerable<OrderSummary> ConvertToOrderSummaries(IEnumerable<Data.Entities.Order> orders)
        {
            return orders.Select(x => new OrderSummary
            {
                Id = new Guid(x.Id),
                ResellerId = new Guid(x.ResellerId),
                CustomerId = new Guid(x.CustomerId),
                StatusId = new Guid(x.StatusId),
                StatusName = x.Status.Name,
                ItemCount = x.Items.Count,
                TotalCost = x.Items.Sum(i => i.Quantity * i.Product.UnitCost).Value,
                TotalPrice = x.Items.Sum(i => i.Quantity * i.Product.UnitPrice).Value,
                CreatedDate = x.CreatedDate
            });
        }
    }
}
