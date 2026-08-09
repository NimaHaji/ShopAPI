using Application.Features.Address.DTOs;

namespace Application.Features.Address.Interfaces;

public interface AddressServiceContract
{
    Task<ViewAddressDto> GetAddressAsync();
    Task<ViewAddressItemDto> GetAddressByIdAsync(Guid addressId);
    Task<string> CreateAddressAsync(CreateAddressDto dto);
    Task<string> EditAddressAsync(Guid addressId, EditAddressDto dto);
    Task<string> DeleteAddressByIdAsync(Guid addressId);
    Task<string> SetAddressDefaultAsync(Guid addressId);
}