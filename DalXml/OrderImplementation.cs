namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

//==================== Order CRUD Implementation (XML) ===================\\

/// <summary>
/// Implementation of the IOrder interface for the DalXml layer.
/// Manages Order data using XML serialization for persistent storage.
/// </summary>
internal class OrderImplementation : IOrder
{
    //==================== Create & Update ===================\\

    #region CreateUpdate

    /// <summary>
    /// Creates a new order in the XML store and assigns it a unique identifier.
    /// </summary>
    /// <param name="item">The order entity to add.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if an order with the generated ID already exists.</exception>
    public void Create(Order item)
    {
        // Load all existing orders from the XML file
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Generate a new unique order ID and create a new record using 'with' expression
        Order newOrder = item with { OrderId = Config.NextOrderId };

        // Check for duplicate order ID
        if (orders.Any(o => o.OrderId == newOrder.OrderId))
            throw new DalAlreadyExistsException($"Order with ID={newOrder.OrderId} already exists");

        // Add new order to the list
        orders.Add(newOrder);

        // Save updated list back to the XML file
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    /// <summary>
    /// Updates an existing order record in the XML store.
    /// </summary>
    /// <param name="item">The order entity with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the order to update does not exist.</exception>
    public void Update(Order item)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Remove old order entry; if nothing was removed, order does not exist
        if (orders.RemoveAll(o => o.OrderId == item.OrderId) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.OrderId} does not exist");

        // Insert updated order object
        orders.Add(item);

        // Save updated list
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }

    #endregion CreateUpdate

    //==================== Read Operations ===================\\

    #region ReadOperations

    /// <summary>
    /// Retrieves the order with the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order to retrieve.</param>
    /// <returns>The order entity if found, otherwise null.</returns>
    public Order? Read(int id)
    {
        // Load all orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Return the first match or null if not found
        return orders.FirstOrDefault(o => o.OrderId == id);
    }

    /// <summary>
    /// Reads the first order that matches the provided predicate.
    /// </summary>
    /// <param name="filter">A predicate function to test each element.</param>
    /// <returns>The first matching order, or null if no match is found.</returns>
    public Order? Read(Func<Order, bool> filter)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Apply the predicate to find a matching order
        return orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all orders, optionally filtered by the provided predicate.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of order entities.</returns>
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // If no filter is provided, return all orders; otherwise apply the filter
        return filter is null ? orders : orders.Where(filter);
    }

    #endregion ReadOperations

    //==================== Delete Operations ===================\\

    #region DeleteOperations

    /// <summary>
    /// Deletes an order by its identifier.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the order does not exist.</exception>
    public void Delete(int id)
    {
        // Load orders from XML
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        // Remove the order with the specified ID; if none removed, it does not exist
        if (orders.RemoveAll(o => o.OrderId == id) == 0)
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

    #endregion DeleteOperations

}