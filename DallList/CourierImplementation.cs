namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;
using System.Linq;

//==================== Courier CRUD Implementation (List) ===================\\

/// <summary>
/// Implementation of the ICourier interface for the DalList layer.
/// Manages Courier data using an in-memory list storage.
/// </summary>
internal class CourierImplementation : ICourier
{
    //==================== Create & Update ===================\\

    #region CreateUpdate

    /// <summary>
    /// Adds a new courier to the data source.
    /// </summary>
    /// <param name="item">The courier entity to add.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if a courier with the same ID already exists.</exception>
    public void Create(Courier item)
    {
        // Check if a courier with the same ID already exists
        if (Read(item.CourierId) is not null)
            throw new DalAlreadyExistsException($"Courier with ID={item.CourierId} already exists");

        // Add the new courier to the list
        DataSource.Couriers.Add(item);
    }

    /// <summary>
    /// Updates an existing courier in the data source.
    /// </summary>
    /// <param name="item">The courier entity with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the courier to update does not exist.</exception>
    public void Update(Courier item)
    {
        // Delete the existing courier (throws exception if not found)
        Delete(item.CourierId);

        // Add the updated courier as a new entry
        Create(item);
    }

    #endregion CreateUpdate

    //==================== Read Operations ===================\\

    #region ReadOperations

    /// <summary>
    /// Retrieves a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the courier to find.</param>
    /// <returns>The courier entity if found, otherwise null.</returns>
    public Courier? Read(int id)
    {
        // Return the first courier matching the ID, or null if not found
        return DataSource.Couriers.FirstOrDefault(item => item.CourierId == id);
    }

    /// <summary>
    /// Retrieves the first courier that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A predicate function to test each element.</param>
    /// <returns>The first matching courier, or null if no match is found.</returns>
    public Courier? Read(Func<Courier, bool> filter)
    {
        // Iterate and find the first match based on the filter
        return DataSource.Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all couriers, optionally filtered by a condition.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of courier entities.</returns>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // If no filter is provided, return all items
        if (filter == null)
            return DataSource.Couriers.Select(item => item);

        // Otherwise, return only items matching the filter
        return DataSource.Couriers.Where(filter);
    }

    #endregion ReadOperations

    //==================== Delete Operations ===================\\

    #region DeleteOperations

    /// <summary>
    /// Deletes a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the courier does not exist.</exception>
    public void Delete(int id)
    {
        // Attempt to find the courier
        Courier? temp = Read(id);

        // If not found, throw exception
        if (temp is null)
            throw new DalDoesNotExistException($"Courier with ID={id} does not exist");

        // Remove the courier from the list
        DataSource.Couriers.Remove(temp);
    }

    /// <summary>
    /// Deletes all couriers from the data source.
    /// </summary>
    public void DeleteAll()
    {
        // Clear the entire list
        DataSource.Couriers.Clear();
    }

    #endregion DeleteOperations

}