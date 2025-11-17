namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;

internal class CourierImplementation : ICourier
{
    public void Create(Courier item)// change
    {
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (Couriers.RemoveAll(it => it.courierId == id) == 0)
            throw new DalDoesNotExistException($"Courier with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }

    public Courier? Read(int id)//change
    {
        throw new NotImplementedException();
    }

    public Courier? Read(Func<Courier, bool> filter)//change
    {
        throw new NotImplementedException();
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        throw new NotImplementedException();
    }

    public void Update(Courier item)
    {
        List<Courier> Couriers = XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);
        if (Couriers.RemoveAll(it => it.courierId == item.courierId) == 0)
            throw new DalDoesNotExistException($"Delivery with ID={item.courierId} does Not exist");
        Couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }
}
