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
public class DalInvalidNumberException : Exception
{
    public DalInvalidNumberException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidIntegerException : Exception
{
    public DalInvalidIntegerException(string? message) : base(message) { }
}

[Serializable]
public class DalInvalidDateException : Exception
{
    public DalInvalidDateException(string? message) : base(message) { }
}


