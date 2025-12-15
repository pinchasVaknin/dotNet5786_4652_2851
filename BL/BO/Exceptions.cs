namespace BO;
using System;

//==================== BL Exceptions ===================\\

#region General & DAL Wrappers

/// <summary>
/// Thrown when an entity is expected to exist but is not found.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when attempting to create an entity that already exists.
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string? message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when an integer value is invalid (e.g. negative ID).
/// </summary>
[Serializable]
public class BlInvalidIntegerException : Exception
{
    public BlInvalidIntegerException(string? message) : base(message) { }
    public BlInvalidIntegerException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a double value is invalid (e.g. negative weight/distance).
/// </summary>
[Serializable]
public class BlInvalidDoubleException : Exception
{
    public BlInvalidDoubleException(string? message) : base(message) { }
    public BlInvalidDoubleException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a string value is invalid (e.g. empty name, invalid phone format).
/// </summary>
[Serializable]
public class BlInvalidStringException : Exception
{
    public BlInvalidStringException(string? message) : base(message) { }
    public BlInvalidStringException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a TimeSpan configuration is invalid.
/// </summary>
[Serializable]
public class BlInvalidTimeSpanException : Exception
{
    public BlInvalidTimeSpanException(string? message) : base(message) { }
    public BlInvalidTimeSpanException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a date is logically invalid (e.g. future date where past is expected).
/// </summary>
[Serializable]
public class BlInvalidDateException : Exception
{
    public BlInvalidDateException(string? message) : base(message) { }
    public BlInvalidDateException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when an unknown vehicle type is encountered.
/// </summary>
[Serializable]
public class BlInvalidVehicleTypeException : Exception
{
    public BlInvalidVehicleTypeException(string? message) : base(message) { }
    public BlInvalidVehicleTypeException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when an unknown shipment type is encountered.
/// </summary>
[Serializable]
public class BlInvalidShipmentTypeException : Exception
{
    public BlInvalidShipmentTypeException(string? message) : base(message) { }
    public BlInvalidShipmentTypeException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a delivery status transition is invalid.
/// </summary>
[Serializable]
public class BlInvalidDeliveryStatusException : Exception
{
    public BlInvalidDeliveryStatusException(string? message) : base(message) { }
    public BlInvalidDeliveryStatusException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when a critical XML file operation fails.
/// </summary>
[Serializable]
public class BlXMLFileLoadCreateException : Exception
{
    public BlXMLFileLoadCreateException(string? message) : base(message) { }
    public BlXMLFileLoadCreateException(string message, Exception DalException) : base(message, DalException) { }
}

/// <summary>
/// Thrown when an object property is null unexpectedly.
/// </summary>
[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when an unknown time unit is passed to clock operations.
/// </summary>
[Serializable]
public class BlUnknownTimeUnitException : Exception
{
    public BlUnknownTimeUnitException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when a requested service is temporarily unavailable.
/// </summary>
[Serializable]
public class BlTemporaryNotAvailableException : Exception
{
    public BlTemporaryNotAvailableException(string? message) : base(message) { }
}

#endregion General & DAL Wrappers

//==================== Business Logic Rules ===================\\

#region Business Logic Rules

/// <summary>
/// Thrown when a user attempts an action requiring Admin privileges.
/// </summary>
[Serializable]
public class BlAdminPermissionException : Exception
{
    public BlAdminPermissionException(string? message) : base(message) { }
}

/// <summary>
/// Thrown during login when the password is incorrect.
/// </summary>
[Serializable]
public class BlInvalidPasswordException : Exception
{
    public BlInvalidPasswordException(string? message) : base(message) { }
}

/// <summary>
/// Thrown during login when the User ID is not found.
/// </summary>
[Serializable]
public class BlUserNotFoundException : Exception
{
    public BlUserNotFoundException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when attempting to delete a courier who has an active delivery.
/// </summary>
[Serializable]
public class BlCourierHasActiveDeliveryException : Exception
{
    public BlCourierHasActiveDeliveryException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when attempting to perform an action on an order that conflicts with its active delivery.
/// </summary>
[Serializable]
public class BlOrderHasActiveDeliveryException : Exception
{
    public BlOrderHasActiveDeliveryException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when attempting to cancel an order that is already canceled or closed.
/// </summary>
[Serializable]
public class BlOrderAlreadyCanceledException : Exception
{
    public BlOrderAlreadyCanceledException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when a courier tries to complete a delivery assigned to someone else.
/// </summary>
[Serializable]
public class BlCourierNotAssignedToDeliveryException : Exception
{
    public BlCourierNotAssignedToDeliveryException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when attempting to assign a courier to an order that is not in 'Open' status.
/// </summary>
[Serializable]
public class BlOrderNotOpenForAssignmentException : Exception
{
    public BlOrderNotOpenForAssignmentException(string? message) : base(message) { }
}

/// <summary>
/// Thrown when attempting to assign an order to a disabled courier.
/// </summary>
[Serializable]
public class BlCourierDisabledException : Exception
{
    public BlCourierDisabledException(string? message) : base(message) { }
}

#endregion Business Logic Rules