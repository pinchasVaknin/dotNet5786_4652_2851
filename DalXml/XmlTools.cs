namespace Dal;

using DO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

/// <summary>
/// Provides utility methods for saving and loading data to and from XML files using both XML serialization and XElement
/// manipulation.
/// </summary>
/// <remarks>This static class offers methods to serialize and deserialize lists of objects to XML files, as well
/// as to manipulate XML configuration values. It supports operations with both XML serialization and direct XElement
/// handling, allowing for flexible XML data management.</remarks>   
static class XMLTools
{
    const string s_xmlDir = @"..\xml\";
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #region SaveLoadWithXMLSerializer
    /// <summary>
    /// Serializes a list of objects to an XML file using <see cref="XmlSerializer"/>.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the XML file cannot be created or serialization fails (wraps underlying IO or serialization errors).
    /// </exception>
    public static void SaveListToXMLSerializer<T>(List<T> list, string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            using FileStream file = new(xmlFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            new XmlSerializer(typeof(List<T>)).Serialize(file, list);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {xmlFilePath}, {ex.Message}");
        }
    }
    /// <summary>
    /// Deserializes a list of objects from an XML file using <see cref="XmlSerializer"/>. Returns an empty list if the file does not exist.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the XML file cannot be opened or deserialization fails (wraps underlying IO or serialization errors).
    /// </exception>
    public static List<T> LoadListFromXMLSerializer<T>(string xmlFileName) where T : class
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            if (!File.Exists(xmlFilePath)) return new();
            using FileStream file = new(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlSerializer x = new(typeof(List<T>));
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
    /// Saves an <see cref="XElement"/> root element to an XML file.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when saving the XElement to disk fails (wraps underlying IO errors).
    /// </exception>
    public static void SaveListToXMLElement(XElement rootElem, string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;

        try
        {
            rootElem.Save(xmlFilePath);
        }
        catch (Exception ex)
        {
            throw new DalXMLFileLoadCreateException($"fail to create xml file: {s_xmlDir + xmlFilePath}, {ex.Message}");
        }
    }
    /// <summary>
    /// Loads an <see cref="XElement"/> root from an XML file. If the file does not exist, creates and returns a new root element.
    /// </summary>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when loading or creating the XML file fails (wraps underlying IO or parsing errors).
    /// </exception>
    public static XElement LoadListFromXMLElement(string xmlFileName)
    {
        string xmlFilePath = s_xmlDir + xmlFileName;
        try
        {
            if (File.Exists(xmlFilePath))
                return XElement.Load(xmlFilePath);
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
    /// <summary>
    /// Reads an integer config value, increments it, saves the new value, and returns the original value.
    /// </summary>
    public static int GetAndIncreaseConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int nextId = root.ToIntNullable(elemName) ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");
        root.Element(elemName)?.SetValue((nextId + 1).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
        return nextId;
    }
    /// <summary>
    /// Reads an integer configuration value from the XML and returns it.
    /// </summary>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName) ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    /// <summary>
    /// Reads a double configuration value from the XML and returns it.
    /// </summary>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToIntNullable(elemName) ?? throw new DalInvalidDoubleException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }
    /// <summary>
    /// Reads a nullable double configuration value from the XML and returns it (or null).
    /// </summary>
    public static double? GetConfigDoubleNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return root.ToDoubleNullable(elemName);
    }
    /// <summary>
    /// Reads a DateTime configuration value from the XML and returns it.
    /// </summary>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName) ?? throw new DalInvalidDateException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }
    /// <summary>
    /// Reads a non-nullable string configuration value from the XML and returns it.
    /// </summary>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string val = (string?)root.Element(elemName) ?? throw new DalInvalidStringException($"can't convert: {xmlFileName}, {elemName}");
        return val;
    }
    /// <summary>
    /// Reads a nullable string configuration value from the XML and returns it (or null).
    /// </summary>
    public static string? GetConfigStringNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return (string?)root.Element(elemName);
    }
    /// <summary>
    /// Reads a TimeSpan configuration value from the XML and returns it.
    /// </summary>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        if (TimeSpan.TryParse((string?)root.Element(elemName), out TimeSpan result))
            return result;

        throw new DalInvalidTimeSpanException($"can't convert: {xmlFileName}, {elemName}");
    }


    /// <summary>
    /// Sets an integer configuration element value and saves the XML.
    /// </summary>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a double configuration element value and saves the XML.
    /// </summary>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a nullable double configuration element value (creates element if missing) and saves the XML.
    /// </summary>
    public static void SetConfigDoubleNullableVal(string xmlFileName, string elemName, double? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        var el = root.Element(elemName);
        if (el == null)
        {
            el = new XElement(elemName);
            root.Add(el);
        }

        el.SetValue(elemVal.HasValue ? elemVal.Value.ToString() : "");

        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a DateTime configuration element value and saves the XML.
    /// </summary>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue((elemVal).ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a string configuration element value and saves the XML.
    /// </summary>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a nullable string configuration element value (empty string when null) and saves the XML.
    /// </summary>
    public static void SetConfigStringNullableVal(string xmlFileName, string elemName, string? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal ?? "");
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    /// <summary>
    /// Sets a TimeSpan configuration element value and saves the XML.
    /// </summary>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }
    #endregion

    #region ExtensionFuctions
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;
    #endregion

}