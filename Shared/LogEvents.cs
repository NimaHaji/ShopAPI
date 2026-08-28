using Microsoft.Extensions.Logging;

namespace Shared;

public static class LogEvents
{
    public static class Checkout
    {
        public static readonly EventId Started =
            new(1001, nameof(Started));

        public static readonly EventId Completed =
            new(1002, nameof(Completed));

        public static readonly EventId AddressNotFound =
            new(1003, nameof(AddressNotFound));

        public static readonly EventId CartNotFound =
            new(1004, nameof(CartNotFound));

        public static readonly EventId Rejected =
            new(1005, nameof(Rejected));

        public static readonly EventId IdempotencyCompleted =
            new(1006, nameof(IdempotencyCompleted));

        public static readonly EventId IdempotencyConflict =
            new(1007, nameof(IdempotencyConflict));

        public static readonly EventId ConcurrencyConflict =
            new(1008, nameof(ConcurrencyConflict));

        public static readonly EventId Failed =
            new(1099, nameof(Failed));
    }

    public static class Payment
    {
        public static readonly EventId Created =
            new(2001, nameof(Created));

        public static readonly EventId Failed =
            new(2002, nameof(Failed));

        public static readonly EventId IdempotencyConflict =
            new(2003, nameof(IdempotencyConflict));
    }

    public static class Inventory
    {
        public static readonly EventId StockReserved =
            new(3001, nameof(StockReserved));

        public static readonly EventId InsufficientStock =
            new(3002, nameof(InsufficientStock));

        public static readonly EventId ConcurrencyConflict =
            new(3003, nameof(ConcurrencyConflict));
    }

    public static class Http
    {
        public static readonly EventId RequestStarted =
            new(4001, nameof(RequestStarted));

        public static readonly EventId RequestCompleted =
            new(4002, nameof(RequestCompleted));
    }

    public static class Exception
    {
        public static readonly EventId Handled =
            new(5001, nameof(Handled));

        public static readonly EventId Unhandled =
            new(5002, nameof(Unhandled));
    }

    public static class RefreshToken
    {
        public static readonly EventId Started = new(6001, nameof(Started));
        public static readonly EventId Generating = new(6002, nameof(Generating));
        public static readonly EventId Invalid = new(6003, nameof(Invalid));
        public static readonly EventId Failed = new(6004, nameof(Failed));
        public static readonly EventId Revoked = new(6005, nameof(Revoked));
        public static readonly EventId Expired = new(6006, nameof(Expired));
        public static readonly EventId Rotated = new(6007, nameof(Rotated));
        public static readonly EventId Succeeded = new(6008, nameof(Succeeded));
        public static readonly EventId ConcurrencyConflict = new(6009, nameof(ConcurrencyConflict));
    }

    public static class UserRegistration
    {
        public static readonly EventId Started = new(7001, nameof(Started));
        public static readonly EventId DuplicateEmail = new(7002, nameof(DuplicateEmail));
        public static readonly EventId Succeeded = new(7003, nameof(Succeeded));
    }

    public static class UserLogin
    {
        public static readonly EventId Started = new(8001, nameof(Started));
        public static readonly EventId InvalidCredentials = new(8002, nameof(InvalidCredentials));
        public static readonly EventId Succeeded = new(8003, nameof(Succeeded));
    }
    public static class UserRoleChange
    {
        public static readonly EventId Started = new(9001, nameof(Started));
        public static readonly EventId UserNotFound = new(9002, nameof(UserNotFound));
        public static readonly EventId Succeeded = new(9003, nameof(Succeeded));
    }
    
    public static class UserLogout
    {
        public static readonly EventId Started = new(1101, nameof(Started));
        public static readonly EventId Succeeded = new(1102, nameof(Succeeded));
    }
    
    public static class Profile
    {
        public static readonly EventId UpdateStarted = new(1201, nameof(UpdateStarted));
        public static readonly EventId UpdateSucceeded = new(1202, nameof(UpdateSucceeded));
    }
}