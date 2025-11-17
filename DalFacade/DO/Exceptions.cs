namespace DO;

[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}

[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidIntegerException : Exception
{
    public DalInvalidIntegerException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidDoubleException : Exception
{
    public DalInvalidDoubleException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidStringException : Exception
{
    public DalInvalidStringException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidTimeSpanException : Exception
{
    public DalInvalidTimeSpanException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidDateException : Exception
{
    public DalInvalidDateException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidVehicleTypeException : Exception
{
    public DalInvalidVehicleTypeException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidShipmentTypeException : Exception
{
    public DalInvalidShipmentTypeException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidDeliveryStatusException : Exception
{
    public DalInvalidDeliveryStatusException(string? message) : base(message) { }
}

[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}
