namespace DalApi;
using DO;

//==================== Order DAL Interface ===================\\

/// <summary>
/// Interface for Order data access operations.
/// Inherits basic CRUD functionality for the Order entity.
/// </summary>
public interface IOrder : ICrud<Order> { }