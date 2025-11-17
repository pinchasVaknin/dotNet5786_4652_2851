namespace DalApi;
public interface ICrud<T> where T : class
{
    void Create(T item); // Creates new T object in DAL
    T? Read(int id); // Reads T object by its ID
    T? Read(Func<T, bool> filter); // stage 2 
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null); // stage 2 only, Reads all T objects
    void Update(T item); // Updates T object
    void Delete(int id); // Deletes an T by its Id
    void DeleteAll(); // Delete all T objects

    /*List<T> ReadAll(); // stage 1 only, Reads all T objects*/
}
