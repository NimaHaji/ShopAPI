namespace Domain.Entities;

public enum TransactionType
{
    StockIn = 1,
    Reservation = 2,
    Confirmation = 3,
    Cancellation = 4,
    Adjustment = 5,
    Transfer = 6
}