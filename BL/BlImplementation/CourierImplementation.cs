namespace BlImplementation;

using BlApi;
using Helpers;

internal class CourierImplementation : ICourier
{
    //======== Login and List Retrieval ========\\

    #region Login and List Retrieval

    public BO.UserRole Login(int userId, string password)
    {
        return CourierManager.GetUserRole(userId, password);
    }


    public IEnumerable<BO.CourierInList> GetCouriers(
        int requesterId,
        bool? isActiveFilter = null,
        BO.CourierListSortBy? sortBy = null)
    {
        return CourierManager.GetListOfCouriers(requesterId, isActiveFilter, sortBy);
    }

    #endregion

    //======== Courier Operations ========\\

    #region CRUD Operations

    public void AddCourier(int requesterId, BO.Courier courier)
    {
        CourierManager.AddCourier(courier);
    }

    public BO.Courier GetCourier(int requesterId, int courierId)
    {
        return CourierManager.GetCourier(courierId);
    }

    public void UpdateCourier(int requesterId, BO.Courier courier)
    {
        CourierManager.UpdateCourier(courier);
    }

    public void DeleteCourier(int requesterId, int courierId)
    {
        CourierManager.DeleteCourier(courierId);
    }

    #endregion
    
}
