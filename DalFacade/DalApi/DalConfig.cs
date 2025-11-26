namespace DalApi;
using System.Xml.Linq;

static class DalConfig
{
    /// <summary>
    /// internal PDS class
    /// </summary>
    internal record DalImplementation
    (string Package,   // package/dll name
     string Namespace, // namespace where DAL implementation class is contained in
     string Class   // DAL implementation class name
    );

    internal static string s_dalName;
    internal static Dictionary<string, DalImplementation> s_dalPackages;

    /// <summary>
    /// Initializes the static configuration for the Data Access Layer (DAL).
    /// </summary>
    /// <remarks>
    /// This static constructor loads the DAL configuration from an XML file and initializes the DAL
    /// name and package implementations. It throws a <see cref="DalConfigException"/> if the configuration file or
    /// required elements are missing.
    /// </remarks>
    /// <exception cref="DalConfigException">
    /// Thrown if the configuration file "dal-config.xml" is not found, or elements are missing.
    /// </exception>
    static DalConfig()
    {
        // Load DAL configuration from XML file
        XElement dalConfig = XElement.Load(@"..\xml\dal-config.xml") ?? 
            throw new DalConfigException("dal-config.xml file is not found"); // throw if missing

        // Read DAL name
        s_dalName =
           dalConfig.Element("dal")?.Value ?? throw new DalConfigException("<dal> element is missing");

        // Read DAL package implementations
        var packages = dalConfig.Element("dal-packages")?.Elements() ?? 
            throw new DalConfigException("<dal-packages> element is missing"); // throw if missing
        // Create dictionary of DAL implementations
        s_dalPackages = (from item in packages
                         let pkg = item.Value
                         let ns = item.Attribute("namespace")?.Value ?? "Dal"
                         let cls = item.Attribute("class")?.Value ?? pkg
                         select (item.Name, new DalImplementation(pkg, ns, cls))
                        ).ToDictionary(p => "" + p.Name, p => p.Item2); // convert to dictionary like json
    }
}

/// <summary>
/// Exception
/// </summary>
[Serializable]
public class DalConfigException : Exception
{
    public DalConfigException(string msg) : base(msg) { }
    public DalConfigException(string msg, Exception ex) : base(msg, ex) { }
}
