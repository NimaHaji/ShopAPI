namespace Domain.Entities;

public class Address
{
    public Guid Id { get; private set; }

    public User User { get; private set; } = null!;
    public Guid UserId { get; private set; }

    public string AddressTitle { get; private set; } = null!;
    public string ReceiverName { get; private set; } = null!;
    public string PhoneNumber { get; private set; } = null!;

    public string Province { get; private set; } = null!;
    public string City { get; private set; } = null!;
    public string AddressLine { get; private set; } = null!;
    public string PostalCode { get; private set; } = null!;

    public bool IsDefault { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Address()
    {
    }

    public Address(Guid userId,
        string addressTitle,
        string receiverName,
        string phoneNumber,
        string province,
        string city,
        string addressLine,
        string postalCode)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        AddressTitle = addressTitle;
        ReceiverName = receiverName;
        PhoneNumber = phoneNumber;
        Province = province;
        City = city;
        AddressLine = addressLine;
        PostalCode = postalCode;

        IsDefault = false;
        CreatedAt = DateTime.Now;
        UpdatedAt = CreatedAt;
    }

    public void Edit(
        string? addressTitle,
        string? receiverName,
        string? phoneNumber,
        string? province,
        string? city,
        string? addressLine,
        string? postalCode)
    {
        if (addressTitle is not null)
            AddressTitle = addressTitle;

        if (receiverName is not null)
            ReceiverName = receiverName;

        if (phoneNumber is not null)
            PhoneNumber = phoneNumber;

        if (province is not null)
            Province = province;

        if (city is not null)
            City = city;

        if (addressLine is not null)
            AddressLine = addressLine;

        if (postalCode is not null)
            PostalCode = postalCode;

        UpdatedAt = DateTime.Now;
    }

    public void SetAsDefault()
    {
        IsDefault = true;
        UpdatedAt = DateTime.UtcNow;
    }
}