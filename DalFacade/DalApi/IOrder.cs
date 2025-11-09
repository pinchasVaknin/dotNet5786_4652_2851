namespace DalApi;
using DO;
internal interface IOrder
{
    void Create(Order item); //Creates new Order object in DAL
    Order? Read(int id); //Reads Order object by its ID
    List<Order> ReadAll(); //stage 1 only, Reads all Order objects
    void Update(Order item); //Updates Order object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all Order objects
}
