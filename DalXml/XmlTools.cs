namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

/// <summary>
/// Provides utility methods for saving and loading data to and from XML files using both XML serialization
/// and XElement manipulation.
/// </summary>
/// <remarks>
/// This static class centralizes XML operations for the DAL layer, including:
/// - Serializing lists of objects to XML files
/// - Loading lists from XML using XmlSerializer
/// - Saving/loading XElement structures directly
/// - Reading/writing configuration values
/// All methods wrap IO or parsing exceptions into DAL-specific custom exceptions.
/// </remarks>
static class XMLTools
{
    // Base directory for all XML files used by the DAL layer
    const string s_xmlDir = @"..\xml\";

    // Static constructor ensures the directory exists before any operations
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer

    /// <summary>
    /// Serializes a list of objects to an XML file using XmlSerializer.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown if the file cannot be created or serialization fails.
    /// </exception>
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            // Create/overwrite the file and serialize the list to XML
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            // Wrap any IO/serialization exception in a DAL-specific exception
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads and deserializes a list of objects using XmlSerializer.
    /// Returns an empty list if no file exists.
    /// </summary>
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            // If file does not exist, return empty list (expected behavior on first run)
            if (!File.Exists(xmlFilePath)) return new();

            using FileStream file = new(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer x = new(typeof(List<T>));

            // Deserialize the list; return empty list if null
            return x.Deserialize(file) as List<T> ?? new();
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {xmlFilePath}, {ex.Message}");
        }
    }

    #endregion

    #region SaveLoadWithXElement

    /// <summary>
    /// Saves an XElement document to an XML file.
    /// </summary>
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            // Save XElement directly to disk
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    /// <summary>
    /// Loads an XElement document from file.  
    /// If the file does not exist, a new root element is created with the filename as the tag name.
    /// </summary>
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            // Load existing XML document
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);

            // Create new empty root if file is missing
            XElement rootElem = new(xmlFileName);
            rootElem.Save(xmlFilePath);
            return rootElem;
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to load xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }

    #endregion

    #region XmlConfig

    //----------- Configuration (Config XML) Typed Getters/Setters -----------\\

    //----------- Getters -----------\\
    /// <summary>
    /// Retrieves an integer config value, increments it, writes it back to the XML, and returns the original value.
    /// Typically used for auto-incrementing IDs.
    /// </summary>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Read the integer; throw if not convertible
        int nextId = root.ToIntNullable(elemName)
            ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");

        // Update the value in the file
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);

        return nextId;
    }

    /// <summary>
    /// Reads a non-incrementing integer config value.
    /// </summary>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName)
            ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a double config value.
    /// </summary>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToIntNullable(elemName)
            ?? throw new DalInvalidDoubleException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a nullable double config value.
    /// </summary>
    public static double? GetConfigDoubleNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return root.ToDoubleNullable(elemName);
    }

    /// <summary>
    /// Reads a DateTime config value.
    /// </summary>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName)
            ?? throw new DalInvalidDateException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Reads a required string config value.
    /// </summary>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string val = (string?)root.Element(elemName)
            ?? throw new DalInvalidStringException($"can't convert: {xmlFileName}, {elemName}");
        return val;
    }

    /// <summary>
    /// Reads a nullable string config value.
    /// </summary>
    public static string? GetConfigStringNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return (string?)root.Element(elemName);
    }

    /// <summary>
    /// Reads a TimeSpan config value.
    /// </summary>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // TryParse allows more flexibility for timespan formats
        if (TimeSpan.TryParse((string?)root.Element(elemName), out TimeSpan result))
            return result;

        throw new DalInvalidTimeSpanException($"can't convert: {xmlFileName}, {elemName}");
    }


    //----------- Setters -----------\\

    /// <summary>
    /// Writes an integer config value.
    /// </summary>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a double config value.
    /// </summary>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable double config value; creates the element if missing.
    /// </summary>
    public static void SetConfigDoubleNullableVal(string xmlFileName, string elemName, double? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Ensure element exists
        var el = root.Element(elemName);
        if (el == null)
        {
            el = new XElement(elemName);
            root.Add(el);
        }

        // Store empty string if null (consistent with DAL behavior)
        el.SetValue(elemVal.HasValue ? elemVal.Value.ToString() : "");

        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a DateTime config value.
    /// </summary>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a string config value.
    /// </summary>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable string config value. Stores empty string instead of null.
    /// </summary>
    public static void SetConfigStringNullableVal(string xmlFileName, string elemName, string? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal ?? "");
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a TimeSpan config value.
    /// </summary>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    #endregion

    #region ExtensionFuctions

    //----------- Convenient extension methods for extracting typed values -----------\\

    // Attempts to convert an element into an enum of type T
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;

    // Returns DateTime? from XML or null
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;

    // Returns double? from XML or null
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;

    // Returns int? from XML or null
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;

    #endregion

}
