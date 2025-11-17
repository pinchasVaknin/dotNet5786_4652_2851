namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Linq;

internal class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        if (Couriers.Any(c => c.courierId == item.courierId))
            throw new DalAlreadyExistsException(
                $"Courier with ID={item.courierId} already exists");

        Couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    public Courier? Read(int id)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        return Couriers.FirstOrDefault(c => c.courierId == id);
    }

    public Courier? Read(Func<Courier, bool> filter)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        return Couriers.FirstOrDefault(filter);
    }

    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        return filter is null ? Couriers : Couriers.Where(filter);
    }

    public void Update(Courier item)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        if (Couriers.RemoveAll(c => c.courierId == item.courierId) == 0)
            throw new DalDoesNotExistException(
                $"Courier with ID={item.courierId} does not exist");

        Couriers.Add(item);
        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    public void Delete(int id)
    {
        List<Courier> Couriers =
            XMLTools.LoadListFromXMLSerializer<Courier>(Config.s_couriers_xml);

        if (Couriers.RemoveAll(c => c.courierId == id) == 0)
            throw new DalDoesNotExistException(
                $"Courier with ID={id} does not exist");

        XMLTools.SaveListToXMLSerializer(Couriers, Config.s_couriers_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Courier>(), Config.s_couriers_xml);
    }
}
