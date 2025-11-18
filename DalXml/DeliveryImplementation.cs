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
/// <remarks>This class interacts with an XML data store to persist delivery information. It ensures that each
/// delivery has a unique identifier and provides methods to query deliveries by ID or custom filters. The class also
/// handles exceptions related to data integrity, such as duplicate entries or missing records.</remarks>
internal class DeliveryImplementation : IDelivery
{
    private static readonly string filePath = Config.s_deliverys_xml;

    /// <summary>
    /// Parses a <see cref="Delivery"/> instance from an <see cref="XElement"/>, validating required fields.
    /// Throws specific Dal* exceptions when required values are missing or invalid.
    /// </summary>
    private static Delivery FromXElement(XElement d)
    {
        int deliveryId = d.ToIntNullable("DeliveryId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing DeliveryId in {filePath}");

        int orderId = d.ToIntNullable("OrderId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing OrderId in {filePath}");

        int courierId = d.ToIntNullable("CourierId")
            ?? throw new DalInvalidIntegerException($"Invalid or missing CourierId in {filePath}");

        double? maxDistance = d.ToDoubleNullable("DeliveryMaxDistance");

        DateTime deliveryDate = d.ToDateTimeNullable("DeliveryDate")
            ?? throw new DalInvalidDateException($"Invalid or missing DeliveryDate in {filePath}");

        DateTime deliveryFinishDate = d.ToDateTimeNullable("DeliveryFinishDate")
            ?? throw new DalInvalidDateException($"Invalid or missing DeliveryFinishDate in {filePath}");

        ShipmentType shipmentType = d.ToEnumNullable<ShipmentType>("ShipmentType")
            ?? throw new DalInvalidShipmentTypeException($"Invalid ShipmentType in {filePath}");

        DeliveryFinishType finishType = d.ToEnumNullable<DeliveryFinishType>("DeliveryFinishType")
            ?? throw new DalInvalidDeliveryStatusException($"Invalid DeliveryFinishType in {filePath}");

        return new Delivery(
            deliveryId: deliveryId,
            orderId: orderId,
            courierId: courierId,
            deliveryMaxDistance: maxDistance,
            deliveryDate: deliveryDate,
            deliveryFinishDate: deliveryFinishDate,
            shipmentType: shipmentType,
            deliveryFinishType: finishType
        );
    }

    /// <summary>
    /// Converts a <see cref="Delivery"/> instance into an <see cref="XElement"/> for storage.
    /// </summary>
    private static XElement ToXElement(Delivery d) =>
        new XElement("Delivery",
            new XElement("DeliveryId", d.deliveryId),
            new XElement("OrderId", d.orderId),
            new XElement("CourierId", d.courierId),
            new XElement("DeliveryMaxDistance", d.deliveryMaxDistance),
            new XElement("DeliveryDate", d.deliveryDate),
            new XElement("DeliveryFinishDate", d.deliveryFinishDate),
            new XElement("ShipmentType", d.shipmentType),
            new XElement("DeliveryFinishType", d.deliveryFinishType)
        );

    /// <summary>
    /// Creates a new delivery record in the XML store and assigns it a unique identifier.
    /// Throws <see cref="DalAlreadyExistsException"/> if a delivery with the assigned ID already exists.
    /// </summary>
    public void Create(Delivery item)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        Delivery newDelivery = item with { deliveryId = Config.NextDeliveryId };

        if (root.Elements("Delivery")
                .Any(d => (int?)d.Element("DeliveryId") == newDelivery.deliveryId))
            throw new DalAlreadyExistsException(
                $"Delivery with ID={newDelivery.deliveryId} already exists");

        root.Add(ToXElement(newDelivery));
        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Reads and returns the delivery with the specified identifier, or null if not found.
    /// </summary>
    public Delivery? Read(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == id);

        return elem is null ? null : FromXElement(elem);
    }

    /// <summary>
    /// Reads and returns the first delivery that matches the provided predicate, or null if none match.
    /// </summary>
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        return root.Elements("Delivery")
                   .Select(FromXElement)
                   .FirstOrDefault(filter);
    }

    /// <summary>
    /// Returns all deliveries or those that match the optional filter predicate.
    /// </summary>
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        IEnumerable<Delivery> all =
            root.Elements("Delivery")
                .Select(FromXElement);

        return filter is null ? all : all.Where(filter);
    }

    /// <summary>
    /// Updates an existing delivery record. Throws <see cref="DalDoesNotExistException"/> if the delivery does not exist.
    /// </summary>
    public void Update(Delivery item)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == item.deliveryId);

        if (elem is null)
            throw new DalDoesNotExistException(
                $"Delivery with ID={item.deliveryId} does not exist");

        elem.ReplaceWith(ToXElement(item));

        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Deletes the delivery with the specified identifier. Throws <see cref="DalDoesNotExistException"/> if not found.
    /// </summary>
    public void Delete(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == id);

        if (elem is null)
            throw new DalDoesNotExistException(
                $"Delivery with ID={id} does not exist");

        elem.Remove();

        XMLTools.SaveListToXMLElement(root, filePath);
    }

    /// <summary>
    /// Removes all delivery records from the XML store.
    /// </summary>
    public void DeleteAll()
    {
        XElement root = new XElement("Deliveries");
        XMLTools.SaveListToXMLElement(root, filePath);
    }
}
