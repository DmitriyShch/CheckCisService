namespace CheckCisService.Exceptions;

public class ServiceException : Exception
{
    public ServiceException(string? message) : base(message) { }

    public ServiceException(string? message, Exception? innerException) :
        base(message, innerException)
    { }
}

public class ObjectNotExistsException : ServiceException
{
    public ObjectNotExistsException(string? message) : base(message) { }

    public ObjectNotExistsException(string? message, Exception? innerException) :
        base(message, innerException)
    { }
}

