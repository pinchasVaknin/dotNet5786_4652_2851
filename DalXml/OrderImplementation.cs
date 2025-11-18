namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// Provides methods to manage orders, including creating, reading, updating, and deleting orders.
/// </summary>
/// <remarks>This class interacts with an XML data store to persist order information. It ensures that each order
/// has a unique identifier and provides functionality to filter and retrieve orders based on specific
/// criteria.</remarks>
internal class OrderImplementation : IOrder
{
    /// <summary>
    /// Creates a new order in the XML store and assigns it a unique identifier.
    /// Throws <see cref="DalAlreadyExistsException"/> if an order with the same ID already exists.
    /// </summary>
    public void Create(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        Order newOrder = item with { orderId = Config.NextOrderId };

        if (orders.Any(o => o.orderId == newOrder.orderId))
            throw new DalAlreadyExistsException($"Order with ID={newOrder.orderId} already exists");

        orders.Add(newOrder);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Reads an order by its identifier.
    /// Returns the order or null if not found.
    /// </summary>
    public Order? Read(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(o => o.orderId == id);
    }

    /// <summary>
    /// Reads the first order that matches the provided predicate.
    /// Returns the order or null if no match is found.
    /// </summary>
    public Order? Read(Func<Order, bool> filter)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all orders, optionally filtered by the provided predicate.
    /// Returns an enumerable of orders.
    /// </summary>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return filter is null ? orders : orders.Where(filter);
    }

    /// <summary>
    /// Updates an existing order record.
    /// Throws <see cref="DalDoesNotExistException"/> if the order does not exist.
    /// </summary>
    public void Update(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        if (orders.RemoveAll(o => o.orderId == item.orderId) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.orderId} does not exist");

        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes an order by its identifier.
    /// Throws <see cref="DalDoesNotExistException"/> if the order does not exist.
    /// </summary>
    public void Delete(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        if (orders.RemoveAll(o => o.orderId == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes all orders from the XML store.
    /// </summary>
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }
}


