namespace Application.Features.Address.DTOs;

public class ViewAddressDto
{
    public List<ViewAddressItemDto> Addresses { get; set; }
}

public class ViewAddressItemDto
{
    public Guid Id { get; set; }

    public string AddressTitle { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;

    public string Province { get; set; } = null!;
    public string City { get; set; }= null!;
    public string AddressLine { get; set; }= null!;
    public string PostalCode { get; set; }= null!;

    public bool IsDefault { get; set; }

    public DateTime CreatedNow { get; set; }
    public DateTime UpdatedAt { get; set; }
}