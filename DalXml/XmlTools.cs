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
    /// Serializes a given list of objects into an XML file using <see cref="XmlSerializer"/>.
    /// Overwrites the file if it already exists.
    /// </summary>
    /// <typeparam name="T">Type of the objects contained in the list (must be a reference type).</typeparam>
    /// <param name="list">The list of objects to serialize and save into the XML file.</param>
    /// <param name="xmlFileName">The target XML file name (including its extension).</param>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the XML file cannot be created or when serialization fails.
    /// Wraps the underlying I/O or serialization exception.
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
    /// Loads and deserializes a list of objects from an XML file using <see cref="XmlSerializer"/>.
    /// If the file does not exist, an empty list is returned (this is expected on first program run).
    /// </summary>
    /// <typeparam name="T">Type of the objects contained in the list (must be a reference type).</typeparam>
    /// <param name="xmlFileName">The name of the XML file to load and deserialize.</param>
    /// <returns>
    /// A <see cref="List{T}"/> containing the deserialized objects.
    /// Returns an empty list if the file is missing or the XML contains an empty list.
    /// </returns>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the XML file exists but cannot be opened or deserialized.
    /// Wraps the underlying I/O or serialization exception.
    /// </exception>
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
    /// Saves an <see cref="XElement"/> document into an XML file.
    /// Overwrites the file if it already exists.
    /// </summary>
    /// <param name="rootElem">
    /// The root <see cref="XElement"/> that represents the XML document to save.
    /// </param>
    /// <param name="xmlFileName">
    /// The file name (including extension) to which the XML should be saved.
    /// </param>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the file cannot be created or written to.
    /// Wraps the underlying file I/O exception.
    /// </exception>
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
    /// Loads an <see cref="XElement"/> document from an XML file.
    /// If the file does not exist, a new root element is created using the file name
    /// (without path) as the element name, and the new file is saved to disk.
    /// </summary>
    /// <param name="xmlFileName">
    /// The file name (including extension) from which to load the XML document.
    /// </param>
    /// <returns>
    /// The loaded <see cref="XElement"/> representing the root element of the XML document.
    /// If the file does not exist, returns a new empty <see cref="XElement"/> with the
    /// given file name as its tag name.
    /// </returns>
    /// <exception cref="DalXMLFileLoadCreateException">
    /// Thrown when the file exists but cannot be opened or parsed,
    /// or when writing a new file fails.
    /// Wraps the underlying file I/O or XML parsing exception.
    /// </exception>
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
    /// Retrieves an integer configuration value from the XML file,
    /// increments it by one, writes the updated value back to the file,
    /// and returns the original (pre-increment) value.
    /// This method is typically used for generating auto-incrementing IDs.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file that contains the requested element.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value should be read and increased.
    /// </param>
    /// <returns>
    /// The original integer value stored in the configuration before it was incremented.
    /// </returns>
    /// <exception cref="DalInvalidIntegerException">
    /// Thrown when the configuration value cannot be parsed as an integer.
    /// </exception>
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
    /// Retrieves an integer configuration value from the XML file
    /// without modifying it.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The element name containing the integer value.
    /// </param>
    /// <returns>
    /// The integer value stored in the configuration.
    /// </returns>
    /// <exception cref="DalInvalidIntegerException">
    /// Thrown if the element exists but cannot be converted to an integer.
    /// </exception>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        int num = root.ToIntNullable(elemName)
            ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Retrieves a double configuration value from the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The element name containing the double value.
    /// </param>
    /// <returns>
    /// The double precision floating-point value stored in the configuration.
    /// </returns>
    /// <exception cref="DalInvalidDoubleException">
    /// Thrown if the element exists but cannot be converted to a double.
    /// </exception>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        double num = root.ToDoubleNullable(elemName)
            ?? throw new DalInvalidDoubleException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a nullable double configuration value from the XML file.
    /// Returns <c>null</c> if the element is missing or empty.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element that contains the double value.
    /// </param>
    /// <returns>
    /// A nullable <see cref="double"/> representing the configuration value,
    /// or <c>null</c> if the value is not present or cannot be parsed.
    /// </returns>
    public static double? GetConfigDoubleNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return root.ToDoubleNullable(elemName);
    }

    /// <summary>
    /// Reads a required <see cref="DateTime"/> configuration value from the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element that contains the date/time value.
    /// </param>
    /// <returns>
    /// The <see cref="DateTime"/> value stored in the configuration.
    /// </returns>
    /// <exception cref="DalInvalidDateException">
    /// Thrown if the element exists but cannot be converted to a valid <see cref="DateTime"/>.
    /// </exception>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        DateTime dt = root.ToDateTimeNullable(elemName)
            ?? throw new DalInvalidDateException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Reads a required string configuration value from the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element that contains the string value.
    /// </param>
    /// <returns>
    /// The string value stored in the configuration.
    /// </returns>
    /// <exception cref="DalInvalidStringException">
    /// Thrown if the element is missing or its value is <c>null</c>.
    /// </exception>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        string val = (string?)root.Element(elemName)
            ?? throw new DalInvalidStringException($"can't convert: {xmlFileName}, {elemName}");
        return val;
    }

    /// <summary>
    /// Reads a nullable string configuration value from the XML file.
    /// Returns <c>null</c> if the element is missing or empty.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element that contains the string value.
    /// </param>
    /// <returns>
    /// A nullable <see cref="string"/> representing the configuration value,
    /// or <c>null</c> if the element does not exist or is empty.
    /// </returns>
    public static string? GetConfigStringNullableVal(string xmlFileName, string elemName)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        return (string?)root.Element(elemName);
    }

    /// <summary>
    /// Reads a required <see cref="TimeSpan"/> configuration value from the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to read from.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element that contains the time-span value.
    /// </param>
    /// <returns>
    /// The <see cref="TimeSpan"/> value stored in the configuration.
    /// </returns>
    /// <exception cref="DalInvalidTimeSpanException">
    /// Thrown if the element exists but cannot be parsed into a valid <see cref="TimeSpan"/>.
    /// </exception>
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
    /// Writes (or overwrites) an integer configuration value inside the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value should be updated.
    /// </param>
    /// <param name="elemVal">
    /// The integer value to write into the configuration.
    /// </param>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes (or overwrites) a double-precision configuration value inside the XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value should be updated.
    /// </param>
    /// <param name="elemVal">
    /// The double value to write into the configuration.
    /// </param>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable double configuration value to the XML file.
    /// If the element does not exist, it is created automatically.
    /// If <c>null</c> is provided, the element value is stored as an empty string.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value should be created or updated.
    /// </param>
    /// <param name="elemVal">
    /// The nullable double value to store.
    /// When <c>null</c>, an empty string is written to the element.
    /// </param>
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
    /// Writes (or overwrites) a <see cref="DateTime"/> configuration value
    /// into the specified XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value will be updated.
    /// </param>
    /// <param name="elemVal">
    /// The <see cref="DateTime"/> value to write into the configuration.
    /// </param>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal.ToString());
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes (or overwrites) a string configuration value
    /// into the specified XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value will be updated.
    /// </param>
    /// <param name="elemVal">
    /// The string value to store.
    /// </param>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal);
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable string configuration value into the specified XML file.
    /// If <c>null</c> is supplied, an empty string is written instead.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value will be updated.
    /// </param>
    /// <param name="elemVal">
    /// A nullable string to write. When <c>null</c>, an empty string is stored.
    /// </param>
    public static void SetConfigStringNullableVal(string xmlFileName, string elemName, string? elemVal)
    {
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);
        root.Element(elemName)?.SetValue(elemVal ?? "");
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes (or overwrites) a <see cref="TimeSpan"/> configuration value
    /// into the specified XML file.
    /// </summary>
    /// <param name="xmlFileName">
    /// The XML configuration file to update.
    /// </param>
    /// <param name="elemName">
    /// The name of the configuration element whose value will be updated.
    /// </param>
    /// <param name="elemVal">
    /// The <see cref="TimeSpan"/> value to write into the configuration.
    /// </param>
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
