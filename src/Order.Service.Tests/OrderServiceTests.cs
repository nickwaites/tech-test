using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Order.Data;
using Order.Data.Entities;
using Order.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Service.Tests
{
    public class OrderServiceTests
    {
        private IOrderService _orderService;
        private IOrderRepository _orderRepository;
        private IOrderStatusRepository _orderStatusRepository;
        private IOrderProductRepository _orderProductRepository;
        private OrderContext _orderContext;


        private readonly byte[] _orderStatusCreatedId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderStatusCompletedId = Guid.NewGuid().ToByteArray();

        private readonly byte[] _orderServiceEmailId = Guid.NewGuid().ToByteArray();
        private readonly byte[] _orderProductEmailId = Guid.NewGuid().ToByteArray();

        [SetUp]
        public async Task Setup()
        {
            var options = new DbContextOptionsBuilder<OrderContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .EnableDetailedErrors(true)
                .EnableSensitiveDataLogging(true)
                .Options;

            _orderContext = new OrderContext(options);
            _orderRepository = new OrderRepository(_orderContext);
            _orderStatusRepository = new OrderStatusRepository(_orderContext);
            _orderProductRepository = new OrderProductRepository(_orderContext);
            _orderService = new OrderService(_orderRepository, _orderStatusRepository, _orderProductRepository);

            await AddReferenceDataAsync(_orderContext);
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsCorrectNumberOfOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            Assert.AreEqual(3, orders.Count());
        }

        [Test]
        public async Task GetOrdersAsync_ReturnsOrdersWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3);

            // Act
            var orders = await _orderService.GetOrdersAsync();

            // Assert
            var order1 = orders.SingleOrDefault(x => x.Id == orderId1);
            var order2 = orders.SingleOrDefault(x => x.Id == orderId2);
            var order3 = orders.SingleOrDefault(x => x.Id == orderId3);

            Assert.AreEqual(0.8m, order1.TotalCost);
            Assert.AreEqual(0.9m, order1.TotalPrice);

            Assert.AreEqual(1.6m, order2.TotalCost);
            Assert.AreEqual(1.8m, order2.TotalPrice);

            Assert.AreEqual(2.4m, order3.TotalCost);
            Assert.AreEqual(2.7m, order3.TotalPrice);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrder()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(orderId1, order.Id);
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsCorrectOrderItemCount()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1, order.Items.Count());
        }

        [Test]
        public async Task GetOrderByIdAsync_ReturnsOrderWithCorrectTotals()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 2);

            // Act
            var order = await _orderService.GetOrderByIdAsync(orderId1);

            // Assert
            Assert.AreEqual(1.6m, order.TotalCost);
            Assert.AreEqual(1.8m, order.TotalPrice);
        }

        [Test]
        public async Task GetOrdersByStatusIdAsync_ReturnsCorrectOrders()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1, _orderStatusCreatedId);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2, _orderStatusCompletedId);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3, _orderStatusCreatedId);

            var orderStatusIdGuid = new Guid(_orderStatusCreatedId);

            // Act
            var orders = await _orderService.GetOrdersByStatusIdAsync(orderStatusIdGuid);

            // Assert
            Assert.AreEqual(2, orders.Count());

            Assert.IsTrue(orders.Any(x => x.Id == orderId1));
            Assert.IsFalse(orders.Any(x => x.Id == orderId2));
            Assert.IsTrue(orders.Any(x => x.Id == orderId3));
        }

        [Test]
        public async Task GetOrdersByStatusIdAsync_ReturnsCorrectOrders_InvalidStatusId()
        {
            // Arrange
            var orderId1 = Guid.NewGuid();
            await AddOrder(orderId1, 1, _orderStatusCreatedId);

            var orderId2 = Guid.NewGuid();
            await AddOrder(orderId2, 2, _orderStatusCompletedId);

            var orderId3 = Guid.NewGuid();
            await AddOrder(orderId3, 3, _orderStatusCreatedId);

            var orderStatusIdGuid = Guid.NewGuid();

            // Act
            var orders = await _orderService.GetOrdersByStatusIdAsync(orderStatusIdGuid);

            // Assert
            Assert.AreEqual(0, orders.Count());
        }


        private async Task AddOrder(Guid orderId, int quantity, byte[] orderStatusIdBytes = null)
        {
            var orderIdBytes = orderId.ToByteArray();
            var statusIdBytes = orderStatusIdBytes ?? _orderStatusCreatedId;

            _orderContext.Order.Add(new Data.Entities.Order
            {
                Id = orderIdBytes,
                ResellerId = Guid.NewGuid().ToByteArray(),
                CustomerId = Guid.NewGuid().ToByteArray(),
                CreatedDate = DateTime.Now,
                StatusId = statusIdBytes,
            });

            _orderContext.OrderItem.Add(new Data.Entities.OrderItem
            {
                Id = Guid.NewGuid().ToByteArray(),
                OrderId = orderIdBytes,
                ServiceId = _orderServiceEmailId,
                ProductId = _orderProductEmailId,
                Quantity = quantity
            });

            await _orderContext.SaveChangesAsync();
        }

        private async Task AddReferenceDataAsync(OrderContext orderContext)
        {
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCreatedId,
                Name = "Created",
            });
            orderContext.OrderStatus.Add(new OrderStatus
            {
                Id = _orderStatusCompletedId,
                Name = "Completed",
            });

            orderContext.OrderService.Add(new Data.Entities.OrderService
            {
                Id = _orderServiceEmailId,
                Name = "Email"
            });

            orderContext.OrderProduct.Add(new OrderProduct
            {
                Id = _orderProductEmailId,
                Name = "100GB Mailbox",
                UnitCost = 0.8m,
                UnitPrice = 0.9m,
                ServiceId = _orderServiceEmailId
            });

            await orderContext.SaveChangesAsync();
        }
    }
}
