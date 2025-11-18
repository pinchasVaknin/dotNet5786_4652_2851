namespace Dal;
using DalApi;
using DO;
using System;
using System.Collections.Generic;
using System.Xml.Linq;


internal class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        Order newOrder = item with { orderId = Config.NextOrderId };

        if (orders.Any(o => o.orderId == newOrder.orderId))
            throw new DalAlreadyExistsException($"Order with ID={newOrder.orderId} already exists");

        orders.Add(newOrder);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
    public Order? Read(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(o => o.orderId == id);
    }
    public Order? Read(Func<Order, bool> filter)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return orders.FirstOrDefault(filter);
    }
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        return filter is null ? orders : orders.Where(filter);
    }
    public void Update(Order item)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        if (orders.RemoveAll(o => o.orderId == item.orderId) == 0)
            throw new DalDoesNotExistException($"Order with ID={item.orderId} does not exist");

        orders.Add(item);
        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
    public void Delete(int id)
    {
        List<Order> orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);

        if (orders.RemoveAll(o => o.orderId == id) == 0)
            throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        XMLTools.SaveListToXMLSerializer(orders, Config.s_orders_xml);
    }
    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }
}


