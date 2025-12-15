namespace DalApi;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

//==================== DAL Configuration ===================\\

/// <summary>
/// Static class responsible for loading and parsing the DAL configuration from "dal-config.xml".
/// It determines which implementation of the DAL (List, XML, SQL, etc.) should be loaded at runtime.
/// </summary>
static class DalConfig
{
    //==================== Inner Types ===================\\

    #region InnerTypes

    /// <summary>
    /// Represents a specific DAL implementation configuration.
    /// </summary>
    /// <param name="Package">The name of the package/DLL (e.g., "DalXml").</param>
    /// <param name="Namespace">The namespace containing the class (e.g., "Dal").</param>
    /// <param name="Class">The specific class name to instantiate.</param>
    internal record DalImplementation
    (
        string Package,
        string Namespace,
        string Class
    );

    #endregion InnerTypes

    //==================== Fields ===================\\

    #region Fields

    // The name of the currently active DAL implementation (e.g., "xml", "list").
    internal static string s_dalName;

    // A dictionary mapping implementation names (keys) to their details (values).
    internal static Dictionary<string, DalImplementation> s_dalPackages;

    #endregion Fields

    //==================== Constructor ===================\\

    #region Constructor

    /// <summary>
    /// Static constructor.
    /// Loads the configuration file and parses the DAL settings.
    /// </summary>
    /// <exception cref="DalConfigException">Thrown if the file is missing or invalid.</exception>
    static DalConfig()
    {
        // 1. Load the XML file
        // Note: Assumes execution from bin folder, looking for xml folder two levels up.
        XElement dalConfig = XElement.Load(@"..\xml\dal-config.xml")
            ?? throw new DalConfigException("dal-config.xml file is not found");

        // 2. Read the <dal> element to determine the active implementation
        s_dalName = dalConfig.Element("dal")?.Value
            ?? throw new DalConfigException("<dal> element is missing");

        // 3. Read the <dal-packages> element
        var packages = dalConfig.Element("dal-packages")?.Elements()
            ?? throw new DalConfigException("<dal-packages> element is missing");

        // 4. Parse packages into the dictionary
        s_dalPackages = (from item in packages
                         let pkg = item.Value
                         let ns = item.Attribute("namespace")?.Value ?? "Dal"
                         let cls = item.Attribute("class")?.Value ?? pkg
                         select (Name: item.Name.LocalName, Implementation: new DalImplementation(pkg, ns, cls))
                        ).ToDictionary(p => p.Name, p => p.Implementation);
    }

    #endregion Constructor

}

//==================== Configuration Exception ===================\\

#region Exception

// Represents errors that occur during the loading or parsing of the DAL configuration.
[Serializable]
public class DalConfigException : Exception
{
    public DalConfigException(string msg) : base(msg) { }
    public DalConfigException(string msg, Exception ex) : base(msg, ex) { }
}

#endregion Exception
