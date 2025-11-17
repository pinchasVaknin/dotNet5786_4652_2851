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
        throw new NotImplementedException();
    }

    public void Delete(int id)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (Orders.RemoveAll(it => it.orderId == id) == 0)
            throw new DalDoesNotExistException($"Course with ID={id} does Not exist");
        XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
    }

    public void DeleteAll()
    {
        XMLTools.SaveListToXMLSerializer(new List<Order>(), Config.s_orders_xml);
    }

    public Order? Read(int id)
    {
        XElement? orderElem =
    XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements().FirstOrDefault(st => (int?)st.Element("Id") == id);
        return orderElem is null ? null : getOrder(orderElem);
    }

    public Order? Read(Func<Order, bool> filter)
    {
        return XMLTools.LoadListFromXMLElement(Config.s_orders_xml).Elements().Select(s => getOrder(s)).FirstOrDefault(filter);
    }


    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null)
    {
        throw new NotImplementedException();
    }

    public void Update(Order item)
    {
        List<Order> Orders = XMLTools.LoadListFromXMLSerializer<Order>(Config.s_orders_xml);
        if (Orders.RemoveAll(it => it.orderId == item.orderId) == 0)
            throw new DalDoesNotExistException($"Course with ID={item.orderId} does Not exist");
        Orders.Add(item);
        XMLTools.SaveListToXMLSerializer(Orders, Config.s_orders_xml);
    }

}
