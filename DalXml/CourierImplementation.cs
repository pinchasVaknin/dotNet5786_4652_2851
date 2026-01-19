namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

//==================== Courier CRUD Implementation (XML) ===================\\

/// <summary>
/// Implementation of the ICourier interface for the DalXml layer.
/// Manages Courier data using XML serialization for persistent storage.
/// </summary>
internal class CourierImplementation : ICourier
{
    //==================== Create & Update ===================\\

    #region CreateUpdate

    /// <summary>
    /// Creates a new courier record in the XML store.
    /// </summary>
    /// <param name="item">The courier entity to add.</param>
    /// <exception cref="DalAlreadyExistsException">Thrown if a courier with the same ID already exists.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Create(Courier item)
    {
        // Load all couriers from the XML file into a list
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Check if a courier with the same ID already exists
        if (Couriers.Any(c => c.CourierId == item.CourierId))
            throw new DalAlreadyExistsException($"Courier with ID={item.CourierId} already exists");

        // Add new courier to the list
        Couriers.Add(item);

        // Save updated list back to XML
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Updates an existing courier record in the XML store.
    /// </summary>
    /// <param name="item">The courier entity with updated values.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the courier to update does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Update(Courier item)
    {
        // Load current couriers from XML
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Remove the old courier entry (must exist)
        // RemoveAll returns how many items were removed
        if (Couriers.RemoveAll(c => c.CourierId == item.CourierId) == 0)
            throw new DalDoesNotExistException($"Courier with ID={item.CourierId} does not exist");

        // Add the updated courier version
        Couriers.Add(item);

        // Save updated list back to XML
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    #endregion CreateUpdate

    //==================== Read Operations ===================\\

    #region ReadOperations

    /// <summary>
    /// Retrieves a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the courier to find.</param>
    /// <returns>The courier entity if found, otherwise null.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(int id)
    {
        // Load list of couriers from XML
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Find courier with matching ID (or null)
        return Couriers.FirstOrDefault(c => c.CourierId == id);
    }

    /// <summary>
    /// Retrieves the first courier that matches the specified filter condition.
    /// </summary>
    /// <param name="filter">A predicate function to test each element.</param>
    /// <returns>The first matching courier, or null if no match is found.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public Courier? Read(Func<Courier, bool> filter)
    {
        // Load list of couriers from XML
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Return first courier that satisfies the filter
        return Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Retrieves all couriers, optionally filtered by a condition.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of courier entities.</returns>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // Load list of couriers from XML
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // If no filter is provided, return all couriers
        // Otherwise, return only couriers matching the filter
        return filter is null ? Couriers : Couriers.Where(filter);
    }

    #endregion ReadOperations

    //==================== Delete Operations ===================\\

    #region DeleteOperations

    /// <summary>
    /// Deletes a courier by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the courier to delete.</param>
    /// <exception cref="DalDoesNotExistException">Thrown if the courier does not exist.</exception>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void Delete(int id)
    {
        // Load couriers from XML
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Attempt to remove matching courier
        if (Couriers.RemoveAll(c => c.CourierId == id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={id} does not exist");

        // Save updated list
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes all courier records from the XML store.
    /// </summary>
    [MethodImpl(MethodImplOptions.Synchronized)] //stage 7
    public void DeleteAll()
    {
        // Overwrite file with an empty list to clear all couriers
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }

    #endregion DeleteOperations

}