namespace Domain.Exceptions;

public class DuplicateUserException:Exception
{
    public DuplicateUserException(string message) :base(message)
    {
    }
}