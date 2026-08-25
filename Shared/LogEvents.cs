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
}