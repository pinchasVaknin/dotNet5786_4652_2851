namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class DeliveryImplementation : IDelivery
{
    public void Create(Delivery item)
    {
        // Generate a new running ID from the DAL config
        int newId = Config.NextDeliveryId;

        // Create a copy of the object with the new ID
        Delivery copy = item with { deliveryId = newId };

        // Add the copy to the data source
        DataSource.Deliverys.Add(copy);
    }
    public void Delete(int id)
    {
        // Retrieve the Delivery with the specified ID (returns null if not found)
        Delivery? temp = Read(id);

        // If no matching Delivery exists, throw an exception
        if (temp == null)
            throw new Exception($"Delivery with ID={id} does not exist");

        // Otherwise, remove the Delivery from the data source
        else DataSource.Deliverys.Remove(temp);
    }
    public void DeleteAll()
    {
        // Remove all Deliverys from the data source
        DataSource.Deliverys.Clear();
    }
    public Delivery? Read(int id)
    {
        // if Exists deliveryId return.
        return DataSource.Deliverys.Find(same => same.deliveryId == id);
    }
    public List<Delivery> ReadAll()
    {
        // Return a copy of the Delivery list
        return new List<Delivery>(DataSource.Deliverys);
    }
    public void Update(Delivery item)
    {
        int newId = item.deliveryId;
        // Remove the existing Delivery with the same ID (throws if it does not exist)
        Delete(item.deliveryId);

        // Add the updated Delivery back to the collection (throws if the ID already exists)
        // Create a copy of the object with the new ID
        Delivery copy = item with { deliveryId = newId };

        // Add the copy to the data source
        DataSource.Deliverys.Add(copy);
    }
}
