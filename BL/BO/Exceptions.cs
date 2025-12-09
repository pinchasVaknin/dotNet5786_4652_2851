namespace BO;

// ========= From DAL Exceptions to BL Exceptions ========= \\

#region DalToBlException

[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlDoesNotExistException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlAlreadyExistsException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidIntegerException : Exception
{
    public BlInvalidIntegerException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidIntegerException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDoubleException : Exception
{
    public BlInvalidDoubleException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidDoubleException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidStringException : Exception
{
    public BlInvalidStringException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidStringException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidTimeSpanException : Exception
{
    public BlInvalidTimeSpanException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidTimeSpanException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDateException : Exception
{
    public BlInvalidDateException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidDateException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidVehicleTypeException : Exception
{
    public BlInvalidVehicleTypeException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidVehicleTypeException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidShipmentTypeException : Exception
{
    public BlInvalidShipmentTypeException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidShipmentTypeException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDeliveryStatusException : Exception
{
    public BlInvalidDeliveryStatusException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlInvalidDeliveryStatusException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlXMLFileLoadCreateException : Exception
{
    public BlXMLFileLoadCreateException(string? message) : base(message) { }
    // Exception chaining constructor to wrap DAL exceptions
    public BlXMLFileLoadCreateException(string message, Exception DalException) : base(message) { }
}

#endregion DalToBlException

[Serializable]
public class BlUnknownTimeUnitException : Exception
{
    public BlUnknownTimeUnitException(string? message) : base(message) { }
}

[Serializable]
public class BlTemporaryNotAvailableException : Exception
{
    public BlTemporaryNotAvailableException(string? message) : base(message) { }
}

[Serializable]
public class BlAdminPermissionException : Exception
{
    public BlAdminPermissionException(string? message) : base(message) { }
}

[Serializable]
public class BlInvalidPasswordException : Exception
{
    public BlInvalidPasswordException(string? message) : base(message) { }
}

[Serializable]
public class BlUserNotFoundException : Exception
{
    public BlUserNotFoundException(string? message) : base(message) { }
}

[Serializable]
public class BlCourierHasActiveDeliveryException : Exception
{
    public BlCourierHasActiveDeliveryException(string? message) : base(message) { }
}

[Serializable]
public class BlOrderHasActiveDeliveryException : Exception
{
    public BlOrderHasActiveDeliveryException(string? message) : base(message) { }
}

[Serializable]
public class BlOrderAlreadyCanceledException : Exception
{
    public BlOrderAlreadyCanceledException(string? message) : base(message) { }
}

[Serializable]
public class BlCourierNotAssignedToDeliveryException : Exception
{
    public BlCourierNotAssignedToDeliveryException(string? message) : base(message) { }
}

[Serializable]
public class BlOrderNotOpenForAssignmentException : Exception
{
    public BlOrderNotOpenForAssignmentException(string? message) : base(message) { }
}

[Serializable]
public class BlCourierDisabledException : Exception
{
    public BlCourierDisabledException(string? message) : base(message) { }
}
