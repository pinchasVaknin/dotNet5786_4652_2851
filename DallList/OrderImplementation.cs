namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

//==================== Order CRUD Implementation (List) ===================\\

/// <summary>
/// Implementation of the IOrder interface for the DalList layer.
/// Manages Order data using an in-memory list storage.
/// </summary>
internal class OrderImplementation : IOrder
{
    //==================== Create & Update ===================\\

    #region CreateUpdate

    /// <summary>
    /// Adds a new order to the data source.
    /// Assigns a new unique ID automatically.
    /// </summary>
    /// <param name="item">The order entity to add.</param>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Order item)
    {
        // Generate a new running ID from the DAL config
        int newId = Config.NextOrderId;

        // Create a copy of the object with the new ID
        Order copy = item with { OrderId = newId };

        // Add the copy to the data source
        DataSource.Orders.Add(copy);
    }

    /// <summary>
    /// Updates an existing order in the data source.
    /// </summary>
    /// <param name="item">The order entity with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the order to update does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Order item)
    {
        // Delete the existing Order (throws exception if not found)
        Delete(item.OrderId);

        // Add the updated Order back to the list
        // Note: We add 'item' directly as it already contains the correct ID
        DataSource.Orders.Add(item);
    }

    #endregion CreateUpdate

    //==================== Read Operations ===================\\

    #region ReadOperations

    /// <summary>
    /// Retrieves an order by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the order to find.</param>
    /// <returns>The order entity if found, otherwise null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(int id)
    {
        // Return the first order with the given ID (or null if not found)
        return DataSource.Orders.FirstOrDefault(item => item.OrderId == id);
    }

    /// <summary>
    /// Retrieves the first order that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A predicate function to test each element.</param>
    /// <returns>The first matching order, or null if no match is found.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Order? Read(Func<Order, bool> filter)
    {
        // Iterate and find the first match based on the filter
        return DataSource.Orders.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all orders, optionally filtered by a condition.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of order entities.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        // If no filter is provided, return all items
        if (filter == null)
            return DataSource.Orders.Select(item => item);

        // Otherwise, return only items matching the filter
        return DataSource.Orders.Where(filter);
    }

    #endregion ReadOperations

    //==================== Delete Operations ===================\\

    #region DeleteOperations

    /// <summary>
    /// Deletes an order by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the order to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the order does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        // Retrieve the order with the specified ID
        Order? temp = Read(id);

        // If no matching order exists, throw an exception
        if (temp is null)
            throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        // Otherwise, remove the order from the data source
        DataSource.Orders.Remove(temp);
    }

    /// <summary>
    /// Deletes all orders from the data source.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        // Clear the entire list
        DataSource.Orders.Clear();
    }

    #endregion DeleteOperations

}