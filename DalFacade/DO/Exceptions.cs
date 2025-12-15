namespace DO;

//==================== Existence Exceptions ===================\\

#region ExistenceExceptions

/// <summary>
/// Exception thrown when an entity is not found in the data layer.
/// </summary>
[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when an attempt is made to create an entity that already exists.
/// </summary>
[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}

#endregion ExistenceExceptions

//==================== Data Validation Exceptions ===================\\

#region ValidationExceptions

/// <summary>
/// Exception thrown when an integer value is invalid.
/// </summary>
[Serializable]
public class DalInvalidIntegerException : Exception
{
    public DalInvalidIntegerException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a double-precision floating-point value is invalid.
/// </summary>
[Serializable]
public class DalInvalidDoubleException : Exception
{
    public DalInvalidDoubleException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a string value is invalid or empty.
/// </summary>
[Serializable]
public class DalInvalidStringException : Exception
{
    public DalInvalidStringException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a TimeSpan value is invalid.
/// </summary>
[Serializable]
public class DalInvalidTimeSpanException : Exception
{
    public DalInvalidTimeSpanException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a DateTime value is invalid.
/// </summary>
[Serializable]
public class DalInvalidDateException : Exception
{
    public DalInvalidDateException(string? message) : base(message) { }
}

#endregion ValidationExceptions

//==================== Enum Validation Exceptions ===================\\

#region EnumExceptions

/// <summary>
/// Exception thrown when a vehicle type definition is invalid.
/// </summary>
[Serializable]
public class DalInvalidVehicleTypeException : Exception
{
    public DalInvalidVehicleTypeException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a shipment type definition is invalid.
/// </summary>
[Serializable]
public class DalInvalidShipmentTypeException : Exception
{
    public DalInvalidShipmentTypeException(string? message) : base(message) { }
}

/// <summary>
/// Exception thrown when a delivery status definition is invalid.
/// </summary>
[Serializable]
public class DalInvalidDeliveryStatusException : Exception
{
    public DalInvalidDeliveryStatusException(string? message) : base(message) { }
}

#endregion EnumExceptions

//==================== System & IO Exceptions ===================\\

#region SystemExceptions

/// <summary>
/// Exception thrown when an error occurs while loading or creating an XML file.
/// </summary>
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}

#endregion SystemExceptions