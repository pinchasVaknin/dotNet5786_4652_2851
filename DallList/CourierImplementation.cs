namespace Dal;
using DalApi;
using DO;
using System.Collections.Generic;

/// <summary>
/// CRUD on Courier
/// </summary>
internal class CourierImplementation : ICourier
{
    public void Create(Courier item)
    {
        // if already Exists.
        if (Read(item.CourierId) != null)
            throw new DalAlreadyExistsException($"Courier with ID={item.CourierId} already exists");

        // else add this Courier.
        DataSource.Couriers.Add(item);
    }
    public void Delete(int id)
    {
        // Retrieve the courier with the specified ID (returns null if not found)
        Courier? temp = Read(id);

        // If no matching courier exists, throw an exception
        if (temp == null)
            throw new DalDoesNotExistException($"Courier with ID={id} does not exist");

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
        //return DataSource.Couriers.Find(same => same.courierId == id); //stage 1
        return DataSource.Couriers.FirstOrDefault(item => item.CourierId == id); //stage 2
    }
    public Courier? Read(Func<Courier, bool> filter)
    {
        foreach (var item in DataSource.Couriers) { if (filter(item)) return item; } return null;
    }
    public IEnumerable<Courier> ReadAll(Func<Courier, bool>? filter = null) //stage 2
        => filter == null
            ? DataSource.Couriers.Select(item => item)
            : DataSource.Couriers.Where(filter);
    public void Update(Courier item)
    {
        // Remove the existing courier with the same ID (throws if it does not exist)
        Delete(item.CourierId);

        // Add the updated courier back to the collection (throws if the ID already exists)
        Create(item);
    }

/*public List<Courier> ReadAll()
    {
        // Return a copy of the courier list
        return new List<Courier>(DataSource.Couriers);
    }*/
}
