namespace Shared.Exceptions;

public class DuplicateUserException:Exception
{
    public DuplicateUserException(string message) :base(message)
    {
    }
}