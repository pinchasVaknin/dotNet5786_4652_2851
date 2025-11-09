namespace DalApi;
using DO;
public interface ICourier
{
    void Create(Courier item); //Creates new Courier object in DAL
    Courier? Read(int id); //Reads Courier object by its ID
    List<Courier> ReadAll(); //stage 1 only, Reads all Courier objects
    void Update(Courier item); //Updates Courier object
    void Delete(int id); //Deletes an object by its Id
    void DeleteAll(); //Delete all Courier objects
}
