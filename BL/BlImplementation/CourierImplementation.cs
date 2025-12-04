namespace BlImplementation;

using BlApi;
using BO;


internal class CourierImplementation : ICourier
{
    public void AddCourier(int requesterId, Courier courier)
    {
        
    }

    public void DeleteCourier(int requesterId, int courierId)
    {
        
    }

    public Courier GetCourier(int requesterId, int courierId)
    {
        
    }

    public IEnumerable<CourierInList> GetCouriers(
        int requesterId, 
        bool? isActiveFilter = null, 
        CourierListSortBy? sortBy = null)
    {
        
    }

    public UserRole Login(int userId, string password)
    {
        
    }

    public void UpdateCourier(int requesterId, Courier courier)
    {
        
    }
}
