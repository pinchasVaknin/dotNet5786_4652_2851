namespace Dal;

using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

internal class DeliveryImplementation : IDelivery
{
    private static readonly string filePath = Config.s_deliverys_xml;

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
    public Delivery? Read(int id)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        XElement? elem =
            root.Elements("Delivery")
                .FirstOrDefault(d => (int?)d.Element("DeliveryId") == id);

        return elem is null ? null : FromXElement(elem);
    }
    public Delivery? Read(Func<Delivery, bool> filter)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        return root.Elements("Delivery")
                   .Select(FromXElement)
                   .FirstOrDefault(filter);
    }
    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        XElement root = XMLTools.LoadListFromXMLElement(filePath);

        IEnumerable<Delivery> all =
            root.Elements("Delivery")
                .Select(FromXElement);

        return filter is null ? all : all.Where(filter);
    }
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
    public void DeleteAll()
    {
        // איפוס גמור של קובץ ה-Delivery
        XElement root = new XElement("Deliveries");
        XMLTools.SaveListToXMLElement(root, filePath);
    }
}
