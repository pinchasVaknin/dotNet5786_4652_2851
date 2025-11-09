namespace DalApi;
using DO;
public interface IDelivery
{
    void Create(Delivery item); //Creates new Delivery object in DAL
    Delivery? Read(int id); //Reads Delivery object by its ID
    List<Delivery> ReadAll(); //stage 1 only, Reads all Delivery objects
    void Update(Delivery item); //Updates Delivery object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all Delivery objects
}
