using Microsoft.EntityFrameworkCore;
using Northwind.Services.EntityFramework.Entities;
using Northwind.Services.Repositories;
using RepositoryOrder = Northwind.Services.Repositories.Order;

namespace Northwind.Services.EntityFramework.Repositories;

public sealed class OrderRepository : IOrderRepository
{
    private readonly NorthwindContext context;

    public OrderRepository(NorthwindContext context)
    {
        this.context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<RepositoryOrder> GetOrderAsync(long orderId)
    {
        var order = await this.context.Orders
            .Include(o => o.OrderDetails)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
        {
            throw new OrderNotFoundException(nameof(orderId));
        }

        var resultOrder = new RepositoryOrder(orderId)
        {
            Customer = new Services.Repositories.Customer(new CustomerCode(order.CustomerId ?? throw new InvalidOperationException("CustomerId is null")))
            {
                CompanyName = order.Customer?.CompanyName ?? string.Empty,
            },
            Employee = new Services.Repositories.Employee(order.EmployeeId)
            {
                FirstName = order.Employee?.FirstName ?? string.Empty,
                LastName = order.Employee?.LastName ?? string.Empty,
                Country = order.Employee?.Country ?? string.Empty,
            },
            OrderDate = order.OrderDate,
            ShippedDate = order.ShippedDate,
            RequiredDate = order.RequiredDate,
            Shipper = new Services.Repositories.Shipper(order.Shipper?.ShipperId ?? 0)
            {
                CompanyName = order.Shipper?.CompanyName ?? string.Empty,
            },
            Freight = order.Freight,
            ShipName = order.ShipName ?? string.Empty,
            ShippingAddress = new ShippingAddress(
                order.ShipAddress ?? string.Empty,
                order.ShipCity ?? string.Empty,
                order.ShipRegion ?? string.Empty,
                order.ShipPostalCode ?? string.Empty,
                order.ShipCountry ?? string.Empty),
        };

        return resultOrder;
    }

    public async Task<IList<RepositoryOrder>> GetOrdersAsync(int skip, int count)
    {
        ValidateSkipAndCount(skip, count);
        return await this.GetOrdersInternalAsync(skip, count);
    }

    public async Task<long> AddOrderAsync(RepositoryOrder order)
    {
        ValidateOrder(order);
        return await this.AddOrderInternalAsync(order);
    }

    public async Task UpdateOrderAsync(RepositoryOrder order)
    {
        ValidateOrder(order);
        await this.UpdateOrderInternalAsync(order);
    }

    public async Task RemoveOrderAsync(long orderId)
    {
        var order = await this.context.Orders.FindAsync(orderId);
        if (order == null)
        {
            throw new OrderNotFoundException(nameof(orderId));
        }

        try
        {
            this.context.Orders.Remove(order);
            await this.context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Error removing order", ex);
        }
    }

    private static void ValidateSkipAndCount(int skip, int count)
    {
        if (skip < 0 || count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(skip));
        }
    }

    private static void ValidateOrder(RepositoryOrder order)
    {
        ArgumentNullException.ThrowIfNull(order);
    }

    private async Task<IList<RepositoryOrder>> GetOrdersInternalAsync(int skip, int count)
    {
        var entityOrder = await this.context.Orders
            .OrderBy(x => x.OrderId)
            .Skip(skip)
            .Take(count)
            .ToListAsync();

        var repositoryOrder = entityOrder.Select(oe => new RepositoryOrder(oe.OrderId)
        {
            Customer = new Services.Repositories.Customer(new CustomerCode(oe.CustomerId ?? throw new InvalidOperationException("CustomerId is null")))
            {
                CompanyName = oe.Customer?.CompanyName ?? string.Empty,
            },
            Employee = new Services.Repositories.Employee(oe.EmployeeId)
            {
                FirstName = oe.Employee?.FirstName ?? string.Empty,
                LastName = oe.Employee?.LastName ?? string.Empty,
                Country = oe.Employee?.Country ?? string.Empty,
            },
            OrderDate = oe.OrderDate,
            RequiredDate = oe.RequiredDate,
            ShippedDate = oe.ShippedDate,
            Shipper = new Services.Repositories.Shipper(oe.Shipper?.ShipperId ?? 0)
            {
                CompanyName = oe.Shipper?.CompanyName ?? string.Empty,
            },
            Freight = oe.Freight,
            ShipName = oe.ShipName ?? string.Empty,
            ShippingAddress = new ShippingAddress(
                oe.ShipAddress ?? string.Empty,
                oe.ShipCity ?? string.Empty,
                oe.ShipRegion ?? string.Empty,
                oe.ShipPostalCode ?? string.Empty,
                oe.ShipCountry ?? string.Empty),
        }).ToList();

        return repositoryOrder;
    }

    private async Task<long> AddOrderInternalAsync(RepositoryOrder order)
    {
        var entityOrder = new Entities.Order
        {
            OrderId = order.Id,
            CustomerId = order.Customer.Code.Code,
            EmployeeId = order.Employee.Id,
            OrderDate = order.OrderDate,
            RequiredDate = order.RequiredDate,
            ShippedDate = order.ShippedDate,
            ShipVia = order.Shipper.Id,
            Freight = order.Freight,
            ShipName = order.ShipName,
            Employee = new Entities.Employee
            {
                EmployeeId = (int)order.Employee.Id,
                FirstName = order.Employee.FirstName,
                LastName = order.Employee.LastName,
                Country = order.Employee.Country,
            },
            Customer = new Entities.Customer
            {
                Code = order.Customer.Code.Code,
                CompanyName = order.Customer.CompanyName,
            },
            ShipAddress = order.ShippingAddress.Address,
            ShipCity = order.ShippingAddress.City,
            ShipCountry = order.ShippingAddress.Country,
            ShipPostalCode = order.ShippingAddress.PostalCode,
            ShipRegion = order.ShippingAddress.Region,
        };

        try
        {
            await this.context.Orders.AddAsync(entityOrder);
            await this.context.SaveChangesAsync();
            return order.Id;
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Error adding order", ex);
        }
    }

    private async Task UpdateOrderInternalAsync(RepositoryOrder order)
    {
        var existingOrder = await this.context.Orders.FindAsync(order.Id);
        if (existingOrder == null)
        {
            throw new OrderNotFoundException(nameof(order));
        }

        try
        {
            existingOrder.CustomerId = order.Customer?.Code.Code ?? throw new InvalidOperationException("Customer code is null");
            existingOrder.EmployeeId = order.Employee.Id;
            existingOrder.OrderDate = order.OrderDate;
            existingOrder.RequiredDate = order.RequiredDate;
            existingOrder.ShippedDate = order.ShippedDate;
            existingOrder.ShipVia = order.Shipper.Id;
            existingOrder.Freight = order.Freight;
            existingOrder.ShipName = order.ShipName ?? string.Empty;
            existingOrder.Employee = new Entities.Employee
            {
                EmployeeId = (int)order.Employee.Id,
                FirstName = order.Employee?.FirstName ?? string.Empty,
                LastName = order.Employee?.LastName ?? string.Empty,
                Country = order.Employee?.Country ?? string.Empty,
            };
            existingOrder.Customer = new Entities.Customer
            {
                Code = order.Customer.Code.Code ?? throw new InvalidOperationException("Customer code is null"),
                CompanyName = order.Customer?.CompanyName ?? string.Empty,
            };
            existingOrder.ShipAddress = order.ShippingAddress?.Address ?? string.Empty;
            existingOrder.ShipCity = order.ShippingAddress?.City ?? string.Empty;
            existingOrder.ShipCountry = order.ShippingAddress?.Country ?? string.Empty;
            existingOrder.ShipPostalCode = order.ShippingAddress?.PostalCode ?? string.Empty;
            existingOrder.ShipRegion = order.ShippingAddress?.Region ?? string.Empty;

            await this.context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new RepositoryException("Error updating order", ex);
        }
    }
}
