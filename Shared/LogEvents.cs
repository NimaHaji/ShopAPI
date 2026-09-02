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

    public class Address
    {
        public static readonly EventId GetStarted = new(1301, nameof(GetStarted));
        public static readonly EventId GetCompleted = new(1302, nameof(GetCompleted));
        public static readonly EventId GetByIdStarted = new(1303, nameof(GetByIdStarted));
        public static readonly EventId GetByIdCompleted = new(1304, nameof(GetByIdCompleted));
        public static readonly EventId NotFound = new(1305, nameof(NotFound));
        public static readonly EventId CreateStarted = new(1306, nameof(CreateStarted));
        public static readonly EventId CreateCompleted = new(1307, nameof(CreateCompleted));
        public static readonly EventId EditStarted = new(1308, nameof(EditStarted));
        public static readonly EventId EditCompleted = new(1309, nameof(EditCompleted));
        public static readonly EventId DeleteStarted = new(1310, nameof(DeleteStarted));
        public static readonly EventId DeleteCompleted = new(1311, nameof(DeleteCompleted));
        public static readonly EventId SetDefaultStarted = new(1312, nameof(SetDefaultStarted));
        public static readonly EventId SetDefaultCompleted = new(1313, nameof(SetDefaultCompleted));
    }

    public static class Product
    {
        public static readonly EventId GetAllStarted =
            new(1401, nameof(GetAllStarted));

        public static readonly EventId GetAllCompleted =
            new(1402, nameof(GetAllCompleted));

        public static readonly EventId CreateStarted =
            new(1403, nameof(CreateStarted));

        public static readonly EventId CreateCompleted =
            new(1404, nameof(CreateCompleted));

        public static readonly EventId Duplicate =
            new(1405, nameof(Duplicate));

        public static readonly EventId SearchStarted =
            new(1406, nameof(SearchStarted));

        public static readonly EventId SearchCompleted =
            new(1407, nameof(SearchCompleted));

        public static readonly EventId NotFound =
            new(1408, nameof(NotFound));

        public static readonly EventId GetByIdCompleted =
            new(1409, nameof(GetByIdCompleted));

        public static readonly EventId EditStarted =
            new(1410, nameof(EditStarted));

        public static readonly EventId EditCompleted =
            new(1411, nameof(EditCompleted));

        public static readonly EventId EditRejected =
            new(1412, nameof(EditRejected));

        public static readonly EventId DeleteStarted =
            new(1413, nameof(DeleteStarted));

        public static readonly EventId DeleteCompleted =
            new(1414, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted =
            new(1415, nameof(RestoreStarted));

        public static readonly EventId RestoreCompleted =
            new(1416, nameof(RestoreCompleted));

        public static readonly EventId ConcurrencyConflict =
            new(1417, nameof(ConcurrencyConflict));

        public static readonly EventId Failed =
            new(1499, nameof(Failed));
    }

    public static class ProductCategory
    {
        public static readonly EventId GetAllStarted =
            new(1501, nameof(GetAllStarted));

        public static readonly EventId GetAllCompleted =
            new(1502, nameof(GetAllCompleted));

        public static readonly EventId SearchStarted =
            new(1503, nameof(SearchStarted));

        public static readonly EventId SearchCompleted =
            new(1504, nameof(SearchCompleted));

        public static readonly EventId GetByIdCompleted =
            new(1505, nameof(GetByIdCompleted));

        public static readonly EventId NotFound =
            new(1506, nameof(NotFound));

        public static readonly EventId CreateStarted =
            new(1507, nameof(CreateStarted));

        public static readonly EventId CreateCompleted =
            new(1508, nameof(CreateCompleted));

        public static readonly EventId Duplicate =
            new(1509, nameof(Duplicate));

        public static readonly EventId EditStarted =
            new(1510, nameof(EditStarted));

        public static readonly EventId EditCompleted =
            new(1511, nameof(EditCompleted));

        public static readonly EventId DeleteStarted =
            new(1512, nameof(DeleteStarted));

        public static readonly EventId DeleteCompleted =
            new(1513, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted =
            new(1514, nameof(RestoreStarted));

        public static readonly EventId RestoreCompleted =
            new(1515, nameof(RestoreCompleted));

        public static readonly EventId ConcurrencyConflict =
            new(1516, nameof(ConcurrencyConflict));

        public static readonly EventId Failed =
            new(1599, nameof(Failed));
    }

    public static class ProductBrand
    {
        public static readonly EventId GetAllStarted =
            new(1601, nameof(GetAllStarted));

        public static readonly EventId GetAllCompleted =
            new(1602, nameof(GetAllCompleted));

        public static readonly EventId SearchStarted =
            new(1603, nameof(SearchStarted));

        public static readonly EventId SearchCompleted =
            new(1604, nameof(SearchCompleted));

        public static readonly EventId GetByIdCompleted =
            new(1605, nameof(GetByIdCompleted));

        public static readonly EventId NotFound =
            new(1606, nameof(NotFound));

        public static readonly EventId CreateStarted =
            new(1607, nameof(CreateStarted));

        public static readonly EventId CreateCompleted =
            new(1608, nameof(CreateCompleted));

        public static readonly EventId Duplicate =
            new(1609, nameof(Duplicate));

        public static readonly EventId EditStarted =
            new(1610, nameof(EditStarted));

        public static readonly EventId EditCompleted =
            new(1611, nameof(EditCompleted));

        public static readonly EventId DeleteStarted =
            new(1612, nameof(DeleteStarted));

        public static readonly EventId DeleteCompleted =
            new(1613, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted =
            new(1614, nameof(RestoreStarted));

        public static readonly EventId RestoreCompleted =
            new(1615, nameof(RestoreCompleted));

        public static readonly EventId ConcurrencyConflict =
            new(1616, nameof(ConcurrencyConflict));

        public static readonly EventId Failed =
            new(1699, nameof(Failed));
    }

    public static class ProductVariant
    {
        public static readonly EventId EditStarted =
            new(1801, nameof(EditStarted));

        public static readonly EventId EditCompleted =
            new(1802, nameof(EditCompleted));

        public static readonly EventId EditRejected =
            new(1803, nameof(EditRejected));

        public static readonly EventId NotFound =
            new(1804, nameof(NotFound));

        public static readonly EventId ConcurrencyConflict =
            new(1805, nameof(ConcurrencyConflict));

        public static readonly EventId Failed =
            new(1899, nameof(Failed));
    }

    public static class Cart
    {
        public static readonly EventId AddItemStarted =
            new(1901, nameof(AddItemStarted));

        public static readonly EventId AddItemCompleted =
            new(1902, nameof(AddItemCompleted));

        public static readonly EventId UpdateItemStarted =
            new(1903, nameof(UpdateItemStarted));

        public static readonly EventId UpdateItemCompleted =
            new(1904, nameof(UpdateItemCompleted));

        public static readonly EventId DeleteItemStarted =
            new(1905, nameof(DeleteItemStarted));

        public static readonly EventId DeleteItemCompleted =
            new(1906, nameof(DeleteItemCompleted));

        public static readonly EventId ClearStarted =
            new(1907, nameof(ClearStarted));

        public static readonly EventId ClearCompleted =
            new(1908, nameof(ClearCompleted));

        public static readonly EventId InvalidQuantity =
            new(1909, nameof(InvalidQuantity));

        public static readonly EventId ProductNotFound =
            new(1910, nameof(ProductNotFound));

        public static readonly EventId InsufficientStock =
            new(1911, nameof(InsufficientStock));

        public static readonly EventId NotFound =
            new(1912, nameof(NotFound));

        public static readonly EventId ItemNotFound =
            new(1913, nameof(ItemNotFound));
    }

    public static class Coupon
    {
        public static readonly EventId GetAllStarted = new(2001, nameof(GetAllStarted));
        public static readonly EventId GetAllCompleted = new(2002, nameof(GetAllCompleted));

        public static readonly EventId GetByIdCompleted = new(2003, nameof(GetByIdCompleted));
        public static readonly EventId NotFound = new(2004, nameof(NotFound));

        public static readonly EventId CreateStarted = new(2005, nameof(CreateStarted));
        public static readonly EventId CreateCompleted = new(2006, nameof(CreateCompleted));
        public static readonly EventId Duplicate = new(2007, nameof(Duplicate));

        public static readonly EventId EditStarted = new(2008, nameof(EditStarted));
        public static readonly EventId EditCompleted = new(2009, nameof(EditCompleted));

        public static readonly EventId DeleteStarted = new(2010, nameof(DeleteStarted));
        public static readonly EventId DeleteCompleted = new(2011, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted = new(2012, nameof(RestoreStarted));
        public static readonly EventId RestoreCompleted = new(2013, nameof(RestoreCompleted));

        public static readonly EventId ActivateStarted = new(2014, nameof(ActivateStarted));
        public static readonly EventId ActivateCompleted = new(2015, nameof(ActivateCompleted));

        public static readonly EventId DeactivateStarted = new(2016, nameof(DeactivateStarted));
        public static readonly EventId DeactivateCompleted = new(2017, nameof(DeactivateCompleted));

        public static readonly EventId ValidateStarted = new(2018, nameof(ValidateStarted));
        public static readonly EventId ValidateCompleted = new(2019, nameof(ValidateCompleted));
        public static readonly EventId ValidationRejected = new(2020, nameof(ValidationRejected));

        public static readonly EventId Failed = new(2099, nameof(Failed));
    }

    public static class Discount
    {
        public static readonly EventId GetAllStarted = new(2101, nameof(GetAllStarted));
        public static readonly EventId GetAllCompleted = new(2102, nameof(GetAllCompleted));

        public static readonly EventId GetByIdCompleted = new(2103, nameof(GetByIdCompleted));
        public static readonly EventId NotFound = new(2104, nameof(NotFound));

        public static readonly EventId CreateStarted = new(2105, nameof(CreateStarted));
        public static readonly EventId CreateCompleted = new(2106, nameof(CreateCompleted));

        public static readonly EventId EditStarted = new(2107, nameof(EditStarted));
        public static readonly EventId EditCompleted = new(2108, nameof(EditCompleted));

        public static readonly EventId ActivateStarted = new(2109, nameof(ActivateStarted));
        public static readonly EventId ActivateCompleted = new(2110, nameof(ActivateCompleted));

        public static readonly EventId DeactivateStarted = new(2111, nameof(DeactivateStarted));
        public static readonly EventId DeactivateCompleted = new(2112, nameof(DeactivateCompleted));

        public static readonly EventId DeleteStarted = new(2113, nameof(DeleteStarted));
        public static readonly EventId DeleteCompleted = new(2114, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted = new(2115, nameof(RestoreStarted));
        public static readonly EventId RestoreCompleted = new(2116, nameof(RestoreCompleted));

        public static readonly EventId SetForProductStarted = new(2117, nameof(SetForProductStarted));
        public static readonly EventId SetForProductCompleted = new(2118, nameof(SetForProductCompleted));

        public static readonly EventId SetForVariantStarted = new(2119, nameof(SetForVariantStarted));
        public static readonly EventId SetForVariantCompleted = new(2120, nameof(SetForVariantCompleted));

        public static readonly EventId RemoveFromProductStarted = new(2121, nameof(RemoveFromProductStarted));
        public static readonly EventId RemoveFromProductCompleted = new(2122, nameof(RemoveFromProductCompleted));

        public static readonly EventId RemoveFromVariantStarted = new(2123, nameof(RemoveFromVariantStarted));
        public static readonly EventId RemoveFromVariantCompleted = new(2124, nameof(RemoveFromVariantCompleted));

        public static readonly EventId Failed = new(2199, nameof(Failed));
    }

    public static class Inventory
    {
        public static readonly EventId StockReserved =
            new(3001, nameof(StockReserved));

        public static readonly EventId InsufficientStock =
            new(3002, nameof(InsufficientStock));

        public static readonly EventId ConcurrencyConflict =
            new(3003, nameof(ConcurrencyConflict));

        public static readonly EventId ReservationConfirmed =
            new(3004, nameof(ReservationConfirmed));

        public static readonly EventId ReservationCancelled =
            new(3005, nameof(ReservationCancelled));

        public static readonly EventId StockAdded =
            new(3006, nameof(StockAdded));
    }
    
    public static class Order
    {
        public static readonly EventId CreateStarted =
            new(2201, nameof(CreateStarted));

        public static readonly EventId CreateCompleted =
            new(2202, nameof(CreateCompleted));

        public static readonly EventId ProductNotFound =
            new(2203, nameof(ProductNotFound));

        public static readonly EventId InvalidQuantity =
            new(2204, nameof(InvalidQuantity));

        public static readonly EventId NotFound =
            new(2205, nameof(NotFound));

        public static readonly EventId CancelStarted =
            new(2206, nameof(CancelStarted));

        public static readonly EventId CancelCompleted =
            new(2207, nameof(CancelCompleted));

        public static readonly EventId StatusChangeStarted =
            new(2208, nameof(StatusChangeStarted));

        public static readonly EventId StatusChangeCompleted =
            new(2209, nameof(StatusChangeCompleted));

        public static readonly EventId Failed =
            new(2299, nameof(Failed));
    }
    
    public static class Payment
    {
        public static readonly EventId CreateStarted =
            new(2001, nameof(CreateStarted));

        public static readonly EventId Created =
            new(2002, nameof(Created));

        public static readonly EventId IdempotencyConflict =
            new(2003, nameof(IdempotencyConflict));

        public static readonly EventId IdempotencyCompleted =
            new(2004, nameof(IdempotencyCompleted));

        public static readonly EventId GatewayFailed =
            new(2005, nameof(GatewayFailed));

        public static readonly EventId CallbackStarted =
            new(2006, nameof(CallbackStarted));

        public static readonly EventId TransactionNotFound =
            new(2007, nameof(TransactionNotFound));

        public static readonly EventId OrderNotFound =
            new(2008, nameof(OrderNotFound));

        public static readonly EventId AlreadyPaid =
            new(2009, nameof(AlreadyPaid));

        public static readonly EventId PaymentCompleted =
            new(2010, nameof(PaymentCompleted));

        public static readonly EventId Failed =
            new(2099, nameof(Failed));
    }
    
    public static class Review
    {
        public static readonly EventId GetAllCompleted =
            new(1701, nameof(GetAllCompleted));

        public static readonly EventId CreateStarted =
            new(1702, nameof(CreateStarted));

        public static readonly EventId CreateCompleted =
            new(1703, nameof(CreateCompleted));

        public static readonly EventId ProductNotFound =
            new(1704, nameof(ProductNotFound));

        public static readonly EventId Duplicate =
            new(1705, nameof(Duplicate));

        public static readonly EventId NotFound =
            new(1706, nameof(NotFound));

        public static readonly EventId StatusChanged =
            new(1707, nameof(StatusChanged));

        public static readonly EventId EditStarted =
            new(1708, nameof(EditStarted));

        public static readonly EventId EditCompleted =
            new(1709, nameof(EditCompleted));

        public static readonly EventId DeleteStarted =
            new(1710, nameof(DeleteStarted));

        public static readonly EventId DeleteCompleted =
            new(1711, nameof(DeleteCompleted));

        public static readonly EventId RestoreStarted =
            new(1712, nameof(RestoreStarted));

        public static readonly EventId RestoreCompleted =
            new(1713, nameof(RestoreCompleted));
    }
    
    public static class Wishlist
    {
        public static readonly EventId AddStarted =
            new(2401, nameof(AddStarted));

        public static readonly EventId AddCompleted =
            new(2402, nameof(AddCompleted));

        public static readonly EventId ProductNotFound =
            new(2403, nameof(ProductNotFound));

        public static readonly EventId Duplicate =
            new(2404, nameof(Duplicate));

        public static readonly EventId DeleteStarted =
            new(2405, nameof(DeleteStarted));

        public static readonly EventId DeleteCompleted =
            new(2406, nameof(DeleteCompleted));

        public static readonly EventId ClearStarted =
            new(2407, nameof(ClearStarted));

        public static readonly EventId ClearCompleted =
            new(2408, nameof(ClearCompleted));

        public static readonly EventId NotFound =
            new(2409, nameof(NotFound));

        public static readonly EventId InvalidProduct =
            new(2410, nameof(InvalidProduct));
    }
}