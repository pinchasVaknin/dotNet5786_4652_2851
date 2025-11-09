namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

public class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        // if already Exists.
        if (Read(item.courierId) != null)
            throw new Exception($"Courier with ID={item.courierId} already exists");

        // else add this Courier.
        DataSource.Couriers.Add(item);
    }
    public void Delete(int id)
    {
        // Retrieve the courier with the specified ID (returns null if not found)
        Courier? temp = Read(id);

        // If no matching courier exists, throw an exception
        if (temp == null)
            throw new Exception($"Courier with ID={id} does not exist");

        // Otherwise, remove the courier from the data source
        else DataSource.Couriers.Remove(temp);
    }
    public void DeleteAll()
    {
        // Remove all couriers from the data source
        DataSource.Couriers.Clear();
    }
    public Courier? Read(int id)
    {
        // if Exists courierId return.
        return DataSource.Couriers.Find(same => same.courierId == id);
    }
    public List<Courier> ReadAll()
    {
        // Return a copy of the courier list
        return new List<Courier>(DataSource.Couriers);
    }
    public void Update(Courier item)
    {
        // Remove the existing courier with the same ID (throws if it does not exist)
        Delete(item.courierId);

        // Add the updated courier back to the collection (throws if the ID already exists)
        Create(item);
    }

}
