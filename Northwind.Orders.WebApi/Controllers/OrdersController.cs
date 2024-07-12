using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Northwind.Orders.WebApi.Models;
using Northwind.Services.Repositories;
using ModelsAddOrder = Northwind.Orders.WebApi.Models.AddOrder;
using ModelsBriefOrder = Northwind.Orders.WebApi.Models.BriefOrder;
using ModelsBriefOrderDetail = Northwind.Orders.WebApi.Models.BriefOrderDetail;
using ModelsCustomer = Northwind.Orders.WebApi.Models.Customer;
using ModelsEmployee = Northwind.Orders.WebApi.Models.Employee;
using ModelsFullOrder = Northwind.Orders.WebApi.Models.FullOrder;
using ModelsShipper = Northwind.Orders.WebApi.Models.Shipper;
using RepositoryCustomer = Northwind.Services.Repositories.Customer;
using RepositoryCustomerCode = Northwind.Services.Repositories.CustomerCode;
using RepositoryEmployee = Northwind.Services.Repositories.Employee;
using RepositoryOrder = Northwind.Services.Repositories.Order;
using RepositoryOrderDetail = Northwind.Services.Repositories.OrderDetail;
using RepositoryProduct = Northwind.Services.Repositories.Product;
using RepositoryShipper = Northwind.Services.Repositories.Shipper;
using ShippingAddress = Northwind.Services.Repositories.ShippingAddress;

namespace Northwind.Orders.WebApi.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public sealed class OrdersController : ControllerBase
    {
        private readonly IOrderRepository orderRepository;
        private readonly ILogger<OrdersController> logger;

        public OrdersController(IOrderRepository orderRepository, ILogger<OrdersController> logger)
        {
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("{orderId}")]
        public async Task<ActionResult<ModelsFullOrder>> GetOrderAsync(long orderId)
        {
            try
            {
                var order = await this.orderRepository.GetOrderAsync((int)orderId);
                if (order == null)
                {
                    this.logger.LogWarning("OrderID not found.");
                    return this.NotFound();
                }

                var fullOrder = new ModelsFullOrder
                {
                    Id = order.Id,
                    Customer = new ModelsCustomer
                    {
                        Code = order.Customer.Code.Code,
                        CompanyName = order.Customer.CompanyName,
                    },
                    Employee = new ModelsEmployee
                    {
                        Id = order.Employee.Id,
                        FirstName = order.Employee.FirstName,
                        LastName = order.Employee.LastName,
                        Country = order.Employee.Country,
                    },
                    OrderDate = order.OrderDate,
                    RequiredDate = order.RequiredDate,
                    ShippedDate = order.ShippedDate,
                    Shipper = new ModelsShipper
                    {
                        Id = order.Shipper.Id,
                        CompanyName = order.Shipper.CompanyName,
                    },
                    Freight = order.Freight,
                    ShipName = order.ShipName,
                    ShippingAddress = new Models.ShippingAddress
                    {
                        Address = order.ShippingAddress.Address,
                        City = order.ShippingAddress.City,
                        Region = order.ShippingAddress.Region,
                        PostalCode = order.ShippingAddress.PostalCode,
                        Country = order.ShippingAddress.Country,
                    },
                    OrderDetails = new List<FullOrderDetail>
                    {
                        new FullOrderDetail
                        {
                            ProductId = order.OrderDetails[0].Product.Id,
                            ProductName = order.OrderDetails[0].Product.ProductName,
                            CategoryId = order.OrderDetails[0].Product.CategoryId,
                            CategoryName = order.OrderDetails[0].Product.Category,
                            SupplierId = order.OrderDetails[0].Product.SupplierId,
                            SupplierCompanyName = order.OrderDetails[0].Product.Supplier,
                            UnitPrice = order.OrderDetails[0].UnitPrice,
                            Quantity = order.OrderDetails[0].Quantity,
                            Discount = order.OrderDetails[0].Discount,
                        },
                    },
                };

                return this.Ok(fullOrder);
            }
            catch (OrderNotFoundException e)
            {
                this.logger.LogWarning(e, "OrderID not found.");
                return this.NotFound();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving order.");
                return this.StatusCode(500);
            }
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ModelsBriefOrder>>> GetOrdersAsync(int? skip = 0, int? count = 10)
        {
            try
            {
                var orders = await this.orderRepository.GetOrdersAsync(skip ?? 0, count ?? 10);
                var briefOrders = new List<ModelsBriefOrder>();

                foreach (var order in orders)
                {
                    briefOrders.Add(new ModelsBriefOrder
                    {
                        Id = order.Id,
                        CustomerId = order.Customer.Code.Code,
                        EmployeeId = order.Employee.Id,
                        OrderDate = order.OrderDate,
                        RequiredDate = order.RequiredDate,
                        ShippedDate = order.ShippedDate,
                        ShipperId = order.Shipper.Id,
                        Freight = order.Freight,
                        ShipName = order.ShipName,
                        ShipAddress = order.ShippingAddress.Address,
                        ShipCity = order.ShippingAddress.City,
                        ShipRegion = order.ShippingAddress.Region,
                        ShipPostalCode = order.ShippingAddress.PostalCode,
                        ShipCountry = order.ShippingAddress.Country,
                    });
                }

                return this.Ok(briefOrders);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                this.logger.LogWarning(ex, "Invalid skip or count value.");
                return this.BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error retrieving orders.");
                return this.StatusCode(500);
            }
        }

        [HttpPost]
        public async Task<ActionResult<ModelsAddOrder>> AddOrderAsync(ModelsBriefOrder order)
        {
            if (order == null)
            {
                return this.BadRequest("Order cannot be null.");
            }

            try
            {
                var newOrder = new RepositoryOrder(order.Id)
                {
                    Customer = new RepositoryCustomer(new RepositoryCustomerCode(order.CustomerId)),
                    Employee = new RepositoryEmployee(order.EmployeeId),
                    OrderDate = order.OrderDate,
                    RequiredDate = order.RequiredDate,
                    ShippedDate = order.ShippedDate,
                    Shipper = new RepositoryShipper(order.ShipperId),
                    Freight = order.Freight,
                    ShipName = order.ShipName,
                    ShippingAddress = new ShippingAddress(order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry),
                };

                await this.orderRepository.AddOrderAsync(newOrder);

                var addOrderResult = new AddOrder
                {
                    OrderId = newOrder.Id,
                };

                return this.CreatedAtAction(nameof(this.GetOrderAsync), new { orderId = newOrder.Id }, addOrderResult);
            }
            catch (RepositoryException e)
            {
                this.logger.LogError(e, "Error adding order.");
                return this.StatusCode(500);
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Unexpected error adding order.");
                return this.StatusCode(500);
            }
        }

        [HttpDelete("{orderId}")]
        public async Task<ActionResult> RemoveOrderAsync(long orderId)
        {
            try
            {
                await this.orderRepository.RemoveOrderAsync((int)orderId);
                return this.NoContent();
            }
            catch (OrderNotFoundException ex)
            {
                this.logger.LogWarning(ex, "OrderID not found.");
                return this.NotFound();
            }
            catch (RepositoryException ex)
            {
                this.logger.LogError(ex, "Error removing order.");
                return this.StatusCode(500);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error removing order.");
                return this.StatusCode(500);
            }
        }

        [HttpPut("{orderId}")]
        public async Task<ActionResult> UpdateOrderAsync(long orderId, ModelsBriefOrder order)
        {
            if (order == null || orderId != order.Id)
            {
                return this.BadRequest("Order data is invalid.");
            }

            try
            {
                var existingOrder = await this.orderRepository.GetOrderAsync((int)orderId);

                existingOrder.Customer = new RepositoryCustomer(new RepositoryCustomerCode(order.CustomerId));
                existingOrder.Employee = new RepositoryEmployee(order.EmployeeId);
                existingOrder.OrderDate = order.OrderDate;
                existingOrder.RequiredDate = order.RequiredDate;
                existingOrder.ShippedDate = order.ShippedDate;
                existingOrder.Shipper = new RepositoryShipper(order.ShipperId);
                existingOrder.Freight = order.Freight;
                existingOrder.ShipName = order.ShipName;
                existingOrder.ShippingAddress = new ShippingAddress(order.ShipAddress, order.ShipCity, order.ShipRegion, order.ShipPostalCode, order.ShipCountry);

                await this.orderRepository.UpdateOrderAsync(existingOrder);
                return this.NoContent();
            }
            catch (OrderNotFoundException e)
            {
                this.logger.LogWarning(e, "OrderID not found.");
                return this.NotFound();
            }
            catch (RepositoryException ex)
            {
                this.logger.LogError(ex, "Error updating order.");
                return this.StatusCode(500);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Unexpected error updating order.");
                return this.StatusCode(500);
            }
        }
    }
}
