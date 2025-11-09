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
            throw new Exception("An object of type Courier with the same ID already exists");

        // else add this Courier.
        DataSource.Couriers.Add(item);
    }
    public void Delete(int id)
    {
        if(Read(id) == null)
            throw new Exception("Courier with this ID does not exist");
        else
            DataSource.Couriers.Remove;
    }
    public void DeleteAll()
    {
        throw new NotImplementedException();
    }
    public Courier? Read(int id)
    {
        // if Exists courierId return.
        return DataSource.Couriers.Find(same => same.courierId == id);
    }
    public List<Courier> ReadAll()
    {
        return new List<Courier>(DataSource.Couriers);
    }
    public void Update(Courier item)
    {
        
    }
    

}
