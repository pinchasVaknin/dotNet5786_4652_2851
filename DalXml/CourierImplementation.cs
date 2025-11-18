namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Provides methods to manage courier data, including creating, reading, updating, and deleting couriers.
/// </summary>
/// <remarks>
/// This class interacts with an XML data store to perform CRUD operations on courier records.
/// It ensures that each courier has a unique identifier and handles exceptions when operations
/// cannot be completed due to existing or non-existing records.
/// </remarks>
internal class CourierImplementation : ICourier
{

    //------------------ CRUD Courier functions ------------------\\
    /// <summary>
    /// Creates a new courier record in the XML store.
    /// Throws <see cref="DalAlreadyExistsException"/> if a courier with the same ID already exists.
    /// </summary>
    public void Create(Courier item)
    {
        // Load all couriers from the XML file into a list
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Check if a courier with the same ID already exists
        if (Couriers.Any(c => c.courierId == item.courierId))
            throw new DalAlreadyExistsException(
                $"Courier with ID={item.courierId} already exists");

        // Add new courier to the list
        Couriers.Add(item);

        // Save updated list back to XML
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Reads a courier by its identifier.
    /// Returns the courier or null if not found.
    /// </summary>
    public Courier? Read(int id)
    {
        // Load list of couriers from XML
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Find courier with matching ID (or null)
        return Couriers.FirstOrDefault(c => c.courierId == id);
    }

    /// <summary>
    /// Reads the first courier that matches the provided predicate.
    /// Returns the courier or null if no match is found.
    /// </summary>
    public Courier? Read(Func<Courier, bool> filter)
    {
        // Load list of couriers from XML
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Return first courier that satisfies the filter
        return Couriers.FirstOrDefault(filter);
    }

    /// <summary>
    /// Reads all couriers, optionally filtered by the provided predicate.
    /// Returns an enumerable of couriers.
    /// </summary>
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        // Load list of couriers from XML
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // If no filter is provided, return all couriers
        // Otherwise, return only couriers matching the filter
        return filter is null ? Couriers : Couriers.Where(filter);
    }

    /// <summary>
    /// Updates an existing courier record.
    /// Throws <see cref="DalDoesNotExistException"/> if the courier does not exist.
    /// </summary>
    public void Update(Courier item)
    {
        // Load current couriers from XML
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Remove the old courier entry (must exist)
        // RemoveAll returns how many items were removed
        if (Couriers.RemoveAll(c => c.courierId == item.courierId) == 0)
            throw new DalDoesNotExistException(
                $"Courier with ID={item.courierId} does not exist");

        // Add the updated courier version
        Couriers.Add(item);

        // Save updated list back to XML
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes a courier by its identifier.
    /// Throws <see cref="DalDoesNotExistException"/> if the courier does not exist.
    /// </summary>
    public void Delete(int id)
    {
        // Load couriers from XML
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        // Attempt to remove matching courier
        if (Couriers.RemoveAll(c => c.courierId == id) == 0)
            throw new DalDoesNotExistException(
                $"Courier with ID={id} does not exist");

        // Save updated list
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    /// <summary>
    /// Deletes all courier records from the XML store.
    /// </summary>
    public void DeleteAll()
    {
        // Overwrite file with an empty list to clear all couriers
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }
}

