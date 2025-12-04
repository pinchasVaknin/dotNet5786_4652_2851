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
        Tools.EnsureAdmin(requesterId, nameof(GetCouriers));
        return CourierManager.GetListOfCouriers(requesterId, isActiveFilter, sortBy);
    }

    #endregion

    //======== Courier Operations ========\\

    #region CRUD Operations

    public void AddCourier(int requesterId, BO.Courier courier)
    {
        Tools.EnsureAdmin(requesterId, nameof(AddCourier));
        CourierManager.AddCourier(courier);
    }

    public BO.Courier GetCourier(int requesterId, int courierId)
    {
        Tools.EnsureAdmin(requesterId, nameof(GetCourier));
        return CourierManager.GetCourier(courierId);
    }

    public void UpdateCourier(int requesterId, BO.Courier courier)
    {
        Tools.EnsureAdmin(requesterId, nameof(UpdateCourier));
        CourierManager.UpdateCourier(courier);
    }

    public void DeleteCourier(int requesterId, int courierId)
    {
        Tools.EnsureAdmin(requesterId, nameof(DeleteCourier));
        CourierManager.DeleteCourier(courierId);
    }

    #endregion
    
}
