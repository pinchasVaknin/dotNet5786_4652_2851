namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;

internal class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);
        if (Deliverys.RemoveAll(it => it.deliveryId == id) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Deliverys, Config.s_deliverys_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Delivery>(), Config.s_deliverys_xml);
    }

    public Delivery? Read(int id)
    {
        throw new NotImplementedException();
    }

    public Delivery? Read(Func<Delivery, bool> filter)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Delivery> ReadAll(Func<Delivery, bool>? filter = null)
    {
        throw new NotImplementedException();
    }

    public void Update(Delivery item)
    {
        List<Delivery> Deliverys = XMLTools.LoadListFromXMLSerializer<Delivery>(Config.s_deliverys_xml);
        if (Deliverys.RemoveAll(it => it.deliveryId == item.deliveryId) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.deliveryId} does Not exist");
        Deliverys.Add(item);
        XMLTools.SaveListToXMLSerializer(Deliverys, Config.s_deliverys_xml);
    }
}
