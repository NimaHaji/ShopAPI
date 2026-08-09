namespace Application.Features.Address.DTOs;

public class CreateAddressDto
{
    public string AddressTitle { get; set; } = null!;
    public string ReceiverName { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Province { get; set; } = null!;
    public string City { get; set; } = null!;
    public string AddressLine { get; set; } = null!;
    public string PostalCode { get; set; } = null!;
}