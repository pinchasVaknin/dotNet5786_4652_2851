namespace Dal;

using DO;
using System.Xml.Linq;
using System.Xml.Serialization;

/// <summary>
/// Provides utility methods for saving and loading data to and from XML files using both XML serialization and XElement manipulation.
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

    //==================== Directory Setup ===================\\

    #region DirectorySetup

    // Base directory for all XML files used by the DAL layer
    const string s_xmlDir = @"..\xml\";

    // Static constructor ensures the directory exists before any operations
    static XMLTools()
    {
        if (!Directory.Exists(s_xmlDir))
            Directory.CreateDirectory(s_xmlDir);
    }

    #endregion DirectorySetup

    // ==================== Save/Load with XmlSerializer ===================\\

    #region SaveLoadWithXMLSerializer

    /// <summary>
    /// Saves a list of objects to an XML file using <see cref="XmlSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects contained in the list (must be a reference type).</typeparam>
    /// <param name="list">The list of objects to serialize and save.</param>
    /// <param name="xmlFileName">The name of the XML file to create or overwrite.</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when creating or writing the XML file fails.</exception>
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
    /// Loads a list of objects from an XML file using <see cref="XmlSerializer"/>.
    /// </summary>
    /// <typeparam name="T">Type of the objects contained in the list (must be a reference type).</typeparam>
    /// <param name="xmlFileName">The name of the XML file to load.</param>
    /// <returns>The deserialized list of objects (empty list if file does not exist).</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when opening, reading, or deserializing the XML file fails.</exception>
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

    // ==================== Save/Load with XElement ===================\\

    #region SaveLoadWithXElement

    /// <summary>
    /// Saves an <see cref="XElement"/> document into an XML file (overwrites if exists).
    /// </summary>
    /// <param name="rootElem">The root <see cref="XElement"/> representing the XML document.</param>
    /// <param name="xmlFileName">The file name (including extension) to save into.</param>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when saving the XElement to disk fails (IO/write error).</exception>
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
    /// Loads an <see cref="XElement"/> document from an XML file (creates a new empty root if missing).
    /// </summary>
    /// <param name="xmlFileName">The file name (including extension) to load from.</param>
    /// <returns>The loaded root <see cref="XElement"/> (or a new empty one if file was missing).</returns>
    /// <exception cref="DalXMLFileLoadCreateException">Thrown when loading/parsing the XML fails, or when creating the missing file fails.</exception>
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

    // ==================== XmlConfig Typed Getters/Setters ===================\\

    #region XmlConfig

    //----------- Getters -----------\\

    /// <summary>
    /// Reads an integer config value, increments it in the file, and returns the original value (used for run numbers/IDs).
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The original integer value before increment.</returns>
    /// <exception cref="DalInvalidIntegerException">Thrown when the element value is missing or not a valid integer.</exception>
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
    /// Reads an integer config value without changing it.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The integer value.</returns>
    /// <exception cref="DalInvalidIntegerException">Thrown when the element value is missing or not a valid integer.</exception>
    public static int GetConfigIntVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Parse the integer value; throw if not convertible
        int num = root.ToIntNullable(elemName)
            ?? throw new DalInvalidIntegerException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a double config value.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The double value.</returns>
    /// <exception cref="DalInvalidDoubleException">Thrown when the element value is missing or not a valid double.</exception>
    public static double GetConfigDoubleVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Parse the double value; throw if not convertible
        double num = root.ToDoubleNullable(elemName)
            ?? throw new DalInvalidDoubleException($"can't convert:  {xmlFileName}, {elemName}");
        return num;
    }

    /// <summary>
    /// Reads a nullable double config value (returns null if missing/invalid).
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>Nullable double value or null.</returns>
    public static double? GetConfigDoubleNullableVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Parse the nullable double value
        return root.ToDoubleNullable(elemName);
    }

    /// <summary>
    /// Reads a DateTime config value.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The DateTime value.</returns>
    /// <exception cref="DalInvalidDateException">Thrown when the element value is missing or not a valid DateTime.</exception>
    public static DateTime GetConfigDateVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Parse the DateTime value; throw if not convertible
        DateTime dt = root.ToDateTimeNullable(elemName)
            ?? throw new DalInvalidDateException($"can't convert:  {xmlFileName}, {elemName}");
        return dt;
    }

    /// <summary>
    /// Reads a string config value.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The string value.</returns>
    /// <exception cref="DalInvalidStringException">Thrown when the element is missing or not a valid string.</exception>
    public static string GetConfigStringVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Get the string value; throw if missing
        string val = (string?)root.Element(elemName)
            ?? throw new DalInvalidStringException($"can't convert: {xmlFileName}, {elemName}");
        return val;
    }

    /// <summary>
    /// Reads a nullable string config value (returns null if missing).
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>Nullable string value or null.</returns>
    public static string? GetConfigStringNullableVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Get the nullable string value
        return (string?)root.Element(elemName);
    }

    /// <summary>
    /// Reads a TimeSpan config value.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <returns>The TimeSpan value.</returns>
    /// <exception cref="DalInvalidTimeSpanException">Thrown when the element value is missing or not a valid TimeSpan.</exception>
    public static TimeSpan GetConfigTimeSpanVal(string xmlFileName, string elemName)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // TryParse allows more flexibility for timespan formats
        if (TimeSpan.TryParse((string?)root.Element(elemName), out TimeSpan result))
            return result;

        // Throw if not convertible
        throw new DalInvalidTimeSpanException($"can't convert: {xmlFileName}, {elemName}");
    }


    //----------- Setters -----------\\

    /// <summary>
    /// Writes an integer config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">Integer value to write.</param>
    public static void SetConfigIntVal(string xmlFileName, string elemName, int elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value
        root.Element(elemName)?.SetValue(elemVal.ToString());

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a double config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">Double value to write.</param>
    public static void SetConfigDoubleVal(string xmlFileName, string elemName, double elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value
        root.Element(elemName)?.SetValue(elemVal.ToString());

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable double config value to the XML file (creates the element if missing).
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">Nullable double value (null writes empty string).</param>
    public static void SetConfigDoubleNullableVal(string xmlFileName, string elemName, double? elemVal)
    {
        // Read the XML document
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

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a DateTime config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">DateTime value to write.</param>
    public static void SetConfigDateVal(string xmlFileName, string elemName, DateTime elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value
        root.Element(elemName)?.SetValue(elemVal.ToString());

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a string config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">String value to write.</param>
    public static void SetConfigStringVal(string xmlFileName, string elemName, string elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value
        root.Element(elemName)?.SetValue(elemVal);

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a nullable string config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">Nullable string value (null writes empty string).</param>
    public static void SetConfigStringNullableVal(string xmlFileName, string elemName, string? elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value (empty string if null)
        root.Element(elemName)?.SetValue(elemVal ?? "");

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    /// <summary>
    /// Writes a TimeSpan config value to the XML file.
    /// </summary>
    /// <param name="xmlFileName">Config XML file name.</param>
    /// <param name="elemName">Config element name.</param>
    /// <param name="elemVal">TimeSpan value to write.</param>
    public static void SetConfigTimeSpanVal(string xmlFileName, string elemName, TimeSpan elemVal)
    {
        // Read the XML document
        XElement root = XMLTools.LoadListFromXMLElement(xmlFileName);

        // Update the element value
        root.Element(elemName)?.SetValue(elemVal.ToString());

        // Save back to file
        XMLTools.SaveListToXMLElement(root, xmlFileName);
    }

    #endregion

    // ==================== XElement Extension Methods ===================\\

    #region ExtensionFuctions

    //----------- Convenient extension methods for extracting typed values -----------\\

    /// <summary>
    /// Tries to parse a child element into an enum value (nullable).
    /// </summary>
    /// <typeparam name="T">Enum type.</typeparam>
    /// <param name="element">Source XElement.</param>
    /// <param name="name">Child element name.</param>
    /// <returns>Parsed enum value, or null if missing/invalid.</returns>
    public static T? ToEnumNullable<T>(this XElement element, string name) where T : struct, Enum =>
        Enum.TryParse<T>((string?)element.Element(name), out var result) ? (T?)result : null;

    /// <summary>
    /// Tries to parse a child element into a DateTime value (nullable).
    /// </summary>
    /// <param name="element">Source XElement.</param>
    /// <param name="name">Child element name.</param>
    /// <returns>Parsed DateTime, or null if missing/invalid.</returns>
    public static DateTime? ToDateTimeNullable(this XElement element, string name) =>
        DateTime.TryParse((string?)element.Element(name), out var result) ? (DateTime?)result : null;

    /// <summary>
    /// Tries to parse a child element into a double value (nullable).
    /// </summary>
    /// <param name="element">Source XElement.</param>
    /// <param name="name">Child element name.</param>
    /// <returns>Parsed double, or null if missing/invalid.</returns>
    public static double? ToDoubleNullable(this XElement element, string name) =>
        double.TryParse((string?)element.Element(name), out var result) ? (double?)result : null;

    /// <summary>
    /// Tries to parse a child element into an int value (nullable).
    /// </summary>
    /// <param name="element">Source XElement.</param>
    /// <param name="name">Child element name.</param>
    /// <returns>Parsed int, or null if missing/invalid.</returns>
    public static int? ToIntNullable(this XElement element, string name) =>
        int.TryParse((string?)element.Element(name), out var result) ? (int?)result : null;

    #endregion

}
