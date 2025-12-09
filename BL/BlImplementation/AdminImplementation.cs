namespace BlImplementation;

using BO;
using BlApi;
using Helpers;


internal class AdminImplementation : IAdmin
{

    //======== Clock =========\\

    #region Clock

    public DateTime GetClock()
    {
        return AdminManager.Now;
    }

    public void ForwardClock(TimeUnit unit)
    {
        AdminManager.ForwardClock(unit);
    }

    #endregion Clock

    //======== Configuration Variables =========\\

    #region Configuration Variables

    public Config GetConfig()
    {
        return AdminManager.GetConfig();
    }

    public void SetConfig(Config config)
    {
        AdminManager.SetConfig(config);
    }

    #endregion Configuration Variables

    //======== Database Initialization / Reset =========\\

    #region Database Initialization / Reset

    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    #endregion Database Initialization / Reset

}
