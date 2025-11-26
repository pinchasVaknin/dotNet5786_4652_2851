namespace DalApi;
public static class Factory
{
    /// <summary>
    /// Gets the singleton instance of the data access layer (DAL) implementation.
    /// </summary>
    public static IDal Get
    {
        // Load the DAL implementation based on the configuration
        get
        {
            // Extract DAL type from configuration
            string dalType = DalApi.DalConfig.s_dalName ?? 
                throw new DalConfigException($"DAL name is not extracted from the configuration"); // if missing
            // Get DAL implementation details from configuration
            DalApi.DalConfig.DalImplementation dal = DalApi.DalConfig.s_dalPackages[dalType] ?? throw new DalConfigException($"Package for {dalType} is not found in packages list in dal-config.xml");

            // Load the DAL assembly
            try
            { System.Reflection.Assembly.Load(dal.Package ?? throw new DalConfigException($"Package {dal.Package} is null")); }
            // catch loading exceptions and rethrow as DalConfigException
            catch (Exception ex)
            { throw new DalConfigException($"Failed to load {dal.Package}.dll package", ex); } 

            // Get the DAL type
            Type type = Type.GetType($"{dal.Namespace}.{dal.Class}, {dal.Package}") ??
                throw new DalConfigException($"Class {dal.Namespace}.{dal.Class} was not found in {dal.Package}.dll"); // if missing

            // Get the singleton instance of the DAL
            return type.GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null) as IDal ??
                throw new DalConfigException($"Class {dal.Class} is not a singleton or wrong property name for Instance"); // if not singleton
        }
    }
}

