namespace Dal;

using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

/// <summary>
/// Provides an implementation for managing delivery records, including creating, reading, updating, and deleting
/// deliveries.
/// </summary>
/// <remarks>
/// This class interacts with an XML data store to persist delivery information.
/// It ensures unique IDs and provides querying/filtering capabilities.
/// </remarks>
internal class DeliveryImplementation : IDelivery
{
    // Path to the XML file that stores delivery records
    private static readonly string filePath = Config.s_deliverys_xml;

    //------------------ Help functions ------------------\\
    /// <summary>
    /// Parses a <see cref="Delivery"/> instance from an XElement.
    /// Performs validation for required fields and throws specific Dal* exceptions.
    /// </summary>
    private static Delivery FromXElement(XElement d)
    {
        // Extract and validate DeliveryId (required)
        int deliveryId = d.ToIntNullable("DeliveryId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing DeliveryId in {filePath}");

        // Extract and validate OrderId (required)
        int orderId = d.ToIntNullable("OrderId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing OrderId in {filePath}");

        // Extract and validate CourierId (required)
        int courierId = d.ToIntNullable("CourierId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing CourierId in {filePath}");

        // Optional max-distance field
        double? maxDistance = d.ToDoubleNullable("DeliveryMaxDistance");

        // Extract and validate DeliveryDate (required)
        DateTime deliveryDate = d.ToDateTimeNullable("DeliveryDate")
            ?? throw new DalInvalidDateException($"Invalid or missing DeliveryDate in {filePath}");

        // Extract and validate DeliveryFinishDate (required)
        DateTime deliveryFinishDate = d.ToDateTimeNullable("DeliveryFinishDate")
            ?? throw new DalInvalidDateException($"Invalid or missing DeliveryFinishDate in {filePath}");

        // Extract and validate ShipmentType enum
        ShipmentType shipmentType = d.ToEnumNullable<ShipmentType>("ShipmentType")
            ?? throw new DalInvalidShipmentTypeException($"Invalid ShipmentType in {filePath}");

        // Extract and validate finish type enum
        DeliveryFinishType finishType = d.ToEnumNullable<DeliveryFinishType>("DeliveryFinishType")
            ?? throw new DalInvalidDeliveryStatusException($"Invalid DeliveryFinishType in {filePath}");

        // Construct the Delivery object with all validated fields
        return new Delivery(
            DeliveryId: deliveryId,
            OrderId: orderId,
            CourierId: courierId,
            DeliveryMaxDistance: maxDistance,
            DeliveryDate: deliveryDate,
            DeliveryFinishDate: deliveryFinishDate,
            ShipmentType: shipmentType,
            DeliveryFinishType: finishType
        );
    }

    /// <summary>
    /// Converts a Delivery object into an XElement for serialization into the XML file.
    /// </summary>
    private static XElement ToXElement(Delivery d) =>
        new XElement("Delivery",
            new XElement("DeliveryId", d.DeliveryId),
            new XElement("OrderId", d.OrderId),
            new XElement("CourierId", d.CourierId),
            new XElement("DeliveryMaxDistance", d.DeliveryMaxDistance),
            new XElement("DeliveryDate", d.DeliveryDate),
            new XElement("DeliveryFinishDate", d.DeliveryFinishDate),
            new XElement("ShipmentType", d.ShipmentType),
            new XElement("DeliveryFinishType", d.DeliveryFinishType)
        );


    //------------------ CRUD Courier functions ------------------\\
    /// <summary>
    /// Creates a new delivery record in the XML store.
    /// Assigns a new unique ID using Config.NextDeliveryId.
    /// Throws exception if the ID already exists.
    /// </summary>
    public void Create(Delivery item)
    {
        // Load the current XML root element
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Assign a generated unique ID (record with-expression)
        Delivery newDelivery = item with { DeliveryId = Config.NextDeliveryId };

        // Ensure the ID is not already taken
        if (root.Elements("Delivery")
                .Any(d => (int?)d.Element("DeliveryId") == newDelivery.DeliveryId))
            throw new DalAlreadyExistsException($"Delivery with ID={newDelivery.DeliveryId} already exists");

        // Add new record to the XML tree
        root.Add(ToXElement(newDelivery));

        // Save updated XML back to file
        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Reads a delivery by its identifier.
    /// Returns null if not found.
    /// </summary>
    public Delivery? Read(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Find matching element in XML
        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == id);

        // Convert to object if found
        return elem is null ? null : FromXElement(elem);
    }

    /// <summary>
    /// Reads the first delivery matching the provided predicate.
    /// Returns null if none match.
    /// </summary>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Convert all elements to Delivery objects, then apply filter
        return root.Elements("Delivery")
                   .Select(FromXElement)
                   .FirstOrDefault(filter);
    }

    /// <summary>
    /// Returns all deliveries, optionally filtered by the provided predicate.
    /// </summary>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Convert XML to Delivery objects
        IEnumerable<Delivery> all =
            root.Elements("Delivery")
                .Select(FromXElement);

        // Apply filter if provided
        return filter is null ? all : all.Where(filter);
    }

    /// <summary>
    /// Updates an existing delivery.
    /// Throws if the delivery does not exist.
    /// </summary>
    public void Update(Delivery item)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Find the delivery to update
        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == item.DeliveryId);

        if (elem is null)
            throw new DalDoesNotExistException(
                $"Delivery with ID={item.DeliveryId} does not exist");

        // Replace the old element with a new one
        elem.ReplaceWith(ToXElement(item));

        // Save changes
        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Deletes the delivery with the given ID.
    /// Throws if not found.
    /// </summary>
    public void Delete(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        // Find the node to delete
        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == id);

        if (elem is null)
            throw new DalDoesNotExistException(
                $"Delivery with ID={id} does not exist");

        // Remove from XML
        elem.Remove();

        // Persist updated XML
        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Clears all delivery records from the XML file.
    /// </summary>
    public void DeleteAll()
    {
        // Create an empty root element to replace existing content
        XElement root = new XElement("Deliveries");

        // Write empty list to file
        XMLTools.SaveListToXMLElement(root, filePath);
    }
}
