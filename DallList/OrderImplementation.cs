namespace Dal;
using DalApi;
using DO;

internal class OrderImplementation : IOrder
{
    public void Create(Order item)
    {
        // Generate a new running ID from the DAL config
        int newId = Config.NextOrderId;

        // Create a copy of the object with the new ID
        Order copy = item with { orderId = newId };

        // Add the copy to the data source
        DataSource.Orders.Add(copy);
    }
    public void Delete(int id)
    {
        // Retrieve the order with the specified ID (returns null if not found)
        Order? temp = Read(id);

        // If no matching order exists, throw an exception
        if (temp == null)
            throw new DalDoesNotExistException($"Order with ID={id} does not exist");

        // Otherwise, remove the order from the data source
        else DataSource.Orders.Remove(temp);
    }
    public void DeleteAll()
    {
        // Remove all orders from the data source
        DataSource.Orders.Clear();
    }
    public Order? Read(int id)
    {
        // Return the first order with the given ID (or null if not found)
        //return DataSource.Orders.Find(same => same.orderId == id); //stage 1
        return DataSource.Orders.FirstOrDefault(item => item.orderId == id); //stage 2
    }
    public Order? Read(Func<Order, bool> filter)
    {
        foreach (var item in DataSource.Orders) { if (filter(item)) return item; } return null;
    }
    public IEnumerable<Order> ReadAll(Func<Order, bool>? filter = null) //stage 2
        => filter == null
            ? DataSource.Orders.Select(item => item)
            : DataSource.Orders.Where(filter);
    public void Update(Order item)
    {
        int newId = item.orderId;
        // Remove the existing Order with the same ID (throws if it does not exist)
        Delete(item.orderId);

        // Add the updated Order back to the collection (throws if the ID already exists)
        // Create a copy of the object with the new ID
        Order copy = item with { orderId = newId };

        // Add the copy to the data source
        DataSource.Orders.Add(copy);
    }

    /*public List<Order> ReadAll()
    {
        // Return a copy of the orders list
        return new List<Order>(DataSource.Orders);
    }*/
}
