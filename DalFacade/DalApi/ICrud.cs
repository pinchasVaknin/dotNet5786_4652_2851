namespace DalApi;

//==================== Generic CRUD Interface ===================\\

/// <summary>
/// Generic interface for basic CRUD (Create, Read, Update, Delete) operations.
/// </summary>
/// <typeparam name="T">The entity type to manage.</typeparam>
public interface ICrud<T> where T : class
{

    /// <summary>
    /// Creates a new entity object in the Data Access Layer.
    /// </summary>
    /// <param name="item">The entity to create.</param>
    void Create(T item);

    /// <summary>
    /// Reads an entity object by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the entity to retrieve.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    T? Read(int id);

    /// <summary>
    /// Reads the first entity object that matches the provided filter condition.
    /// </summary>
    /// <param name="filter">The predicate to filter by.</param>
    /// <returns>The matching entity if found, otherwise null.</returns>
    T? Read(Func<T, bool> filter);

    /// <summary>
    /// Reads all entity objects, optionally filtered by a condition.
    /// </summary>
    /// <param name="filter">Optional predicate to filter the results.</param>
    /// <returns>A collection of entities.</returns>
    IEnumerable<T> ReadAll(Func<T, bool>? filter = null);

    /// <summary>
    /// Updates an existing entity object.
    /// </summary>
    /// <param name="item">The entity with updated values.</param>
    void Update(T item);

    /// <summary>
    /// Deletes an entity object by its unique identifier.
    /// </summary>
    /// <param name="id">The ID of the entity to delete.</param>
    void Delete(int id);

    /// <summary>
    /// Deletes all entity objects from the storage.
    /// </summary>
    void DeleteAll();

}