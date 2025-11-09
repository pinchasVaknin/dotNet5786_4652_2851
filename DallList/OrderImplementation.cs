namespace Dal;
using DalApi;
using DO;

public class OrderImplementation : IOrder
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
    public Order? Read(int id)
    {
        // Return the first order with the given ID (or null if not found)
        return DataSource.Orders.Find(same => same.orderId == id);
    }
    public List<Order> ReadAll()
    {
        // Return a copy of the orders list
        return new List<Order>(DataSource.Orders);
    }
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
    public void Delete(int id)
    {
        // Retrieve the order with the specified ID (returns null if not found)
        Order? temp = Read(id);

        // If no matching order exists, throw an exception
        if (temp == null)
            throw new Exception($"Order with ID={id} does not exist");

        // Otherwise, remove the order from the data source
        else DataSource.Orders.Remove(temp);
    }
    public void DeleteAll()
    {
        // Remove all orders from the data source
        DataSource.Orders.Clear();
    }
}
