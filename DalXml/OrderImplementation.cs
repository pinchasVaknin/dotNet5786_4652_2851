namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

/// <summary>
/// Provides methods to manage orders, including creating, reading, updating, and deleting orders.
/// </summary>
/// <remarks>
/// This class interacts with an XML data store to persist order information. 
/// It ensures unique IDs and supports filtering and retrieval of orders.
/// </remarks>
internal class OrderImplementation : IOrder
{

    //------------------ CRUD Order functions ------------------\\
    /// <summary>
    /// Creates a new order in the XML store and assigns it a unique identifier.
    /// Throws <see cref="DalAlreadyExistsException"/> if an order with the same ID already exists.
    /// </summary>
    public void Create(Order item)
    {
        // Load all existing orders from the XML file
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Generate a new unique order ID and create a new record using 'with' expression
        Order newOrder = item with { orderId = Config.NextOrderId };

        // Check for duplicate order ID
        if (orders.Any(o => o.orderId == newOrder.orderId))
            throw new DalAlreadyExistsException($"Order with ID={newOrder.orderId} already exists");

        // Add new order to the list
        orders.Add(newOrder);

        // Save updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Reads an order by its identifier.
    /// Returns the order or null if not found.
    /// </summary>
    public Order? Read(int id)
    {
        // Load all orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Return the first match or null if not found
        return orders.FirstOrDefault(o => o.orderId == id);
    }

    /// <summary>
    /// Reads the first order that matches the provided predicate.
    /// Returns the order or null if no match is found.
    /// </summary>
    public Order? Read(Func<Order, bool> filter)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Apply the predicate to find a matching order
        return orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all orders, optionally filtered by the provided predicate.
    /// Returns an enumerable of orders.
    /// </summary>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // If no filter is provided, return all orders; otherwise apply the filter
        return filter is null ? orders : orders.Where(filter);
    }

    /// <summary>
    /// Updates an existing order record.
    /// Throws <see cref="DalDoesNotExistException"/> if the order does not exist.
    /// </summary>
    public void Update(Order item)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Remove old order entry; if nothing was removed, order does not exist
        if (orders.RemoveAll(o => o.orderId == item.orderId) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.orderId} does not exist");

        // Insert updated order object
        orders.Add(item);

        // Save updated list
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes an order by its identifier.
    /// Throws <see cref="DalDoesNotExistException"/> if the order does not exist.
    /// </summary>
    public void Delete(int id)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Remove the order; if none removed, the order does not exist
        if (orders.RemoveAll(o => o.orderId == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        // Save updated list
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Deletes all orders from the XML store.
    /// </summary>
    public void DeleteAll()
    {
        // Overwrite the XML file with an empty list of orders
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }
}
