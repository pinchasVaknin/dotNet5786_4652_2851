namespace BO;

[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    //
    public BlDoesNotExistException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string? message) : base(message) { }
    //
    public BlAlreadyExistsException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidIntegerException : Exception
{
    public BlInvalidIntegerException(string? message) : base(message) { }
    //
    public BlInvalidIntegerException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDoubleException : Exception
{
    public BlInvalidDoubleException(string? message) : base(message) { }
    //
    public BlInvalidDoubleException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidStringException : Exception
{
    public BlInvalidStringException(string? message) : base(message) { }
    //
    public BlInvalidStringException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidTimeSpanException : Exception
{
    public BlInvalidTimeSpanException(string? message) : base(message) { }
    //
    public BlInvalidTimeSpanException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDateException : Exception
{
    public BlInvalidDateException(string? message) : base(message) { }
    //
    public BlInvalidDateException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidVehicleTypeException : Exception
{
    public BlInvalidVehicleTypeException(string? message) : base(message) { }
    //
    public BlInvalidVehicleTypeException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidShipmentTypeException : Exception
{
    public BlInvalidShipmentTypeException(string? message) : base(message) { }
    //
    public BlInvalidShipmentTypeException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlInvalidDeliveryStatusException : Exception
{
    public BlInvalidDeliveryStatusException(string? message) : base(message) { }
    //
    public BlInvalidDeliveryStatusException(string message, Exception DalException) : base(message) { }
}

[Serializable]
public class BlXMLFileLoadCreateException : Exception
{
    public BlXMLFileLoadCreateException(string? message) : base(message) { }
    //
    public BlXMLFileLoadCreateException(string message, Exception DalException) : base(message) { }
}
