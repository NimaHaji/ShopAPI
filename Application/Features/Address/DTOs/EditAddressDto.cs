namespace Application.Features.Address.DTOs;

public class EditAddressDto
{
    public string? AddressTitle { get; set; }
    public string? ReceiverName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }
    public string? AddressLine { get; set; }
    public string? PostalCode { get; set; }
}