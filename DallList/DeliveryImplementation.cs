namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;

//==================== Delivery CRUD Implementation (List) ===================\\

/// <summary>
/// Implementation of the IDelivery interface for the DalList layer.
/// Manages Delivery data using an in-memory list storage.
/// </summary>
internal class DeliveryImplementation : IDelivery
{
    //==================== Create & Update ===================\\

    #region CreateUpdate

    /// <summary>
    /// Adds a new delivery to the data source.
    /// Assigns a new unique ID automatically.
    /// </summary>
    /// <param name="item">The delivery entity to add.</param>
    public void Create(Delivery item)
    {
        // Generate a new running ID from the DAL config
        int newId = Config.NextDeliveryId;

        // Create a copy of the object with the new ID
        Delivery copy = item with { DeliveryId = newId };

        // Add the copy to the data source
        DataSource.Deliverys.Add(copy);
    }

    /// <summary>
    /// Updates an existing delivery in the data source.
    /// </summary>
    /// <param name="item">The delivery entity with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the delivery to update does not exist.</exception>
    public void Update(Delivery item)
    {
        // Delete the existing delivery (throws exception if not found)
        Delete(item.DeliveryId);

        // Add the updated delivery back to the list
        // Note: We add 'item' directly as it already contains the correct ID
        DataSource.Deliverys.Add(item);
    }

    #endregion CreateUpdate

    //==================== Read Operations ===================\\

    #region ReadOperations

    /// <summary>
    /// Retrieves a delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the delivery to find.</param>
    /// <returns>The delivery entity if found, otherwise null.</returns>
    public Delivery? Read(int id)
    {
        // Return the first delivery matching the ID, or null if not found
        return DataSource.Deliverys.FirstOrDefault(item => item.DeliveryId == id);
    }

    /// <summary>
    /// Retrieves the first delivery that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A predicate function to test each element.</param>
    /// <returns>The first matching delivery, or null if no match is found.</returns>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        // Iterate and find the first match based on the filter
        return DataSource.Deliverys.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all deliveries, optionally filtered by a condition.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of delivery entities.</returns>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        // If no filter is provided, return all items
        if (filter == null)
            return DataSource.Deliverys.Select(item => item);

        // Otherwise, return only items matching the filter
        return DataSource.Deliverys.Where(filter);
    }

    #endregion ReadOperations

    //==================== Delete Operations ===================\\

    #region DeleteOperations

    /// <summary>
    /// Deletes a delivery by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the delivery to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the delivery does not exist.</exception>
    public void Delete(int id)
    {
        // Attempt to find the delivery
        Delivery? temp = Read(id);

        // If not found, throw exception
        if (temp is null)
            throw new DalDoesNotExistException($"Delivery with ID={id} does not exist");

        // Remove the delivery from the list
        DataSource.Deliverys.Remove(temp);
    }

    /// <summary>
    /// Deletes all deliveries from the data source.
    /// </summary>
    public void DeleteAll()
    {
        // Clear the entire list
        DataSource.Deliverys.Clear();
    }

    #endregion DeleteOperations

}