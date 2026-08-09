namespace Application.Features.Address.Interfaces;

public interface AddressRepositoryContract
{
    Task<List<Domain.Entities.Address>> GetAllAddressesAsync();
    Task<Domain.Entities.Address?> GetAddressByIdAndUserIdAsync(Guid userId, Guid dtoAddressId);
    Task<Domain.Entities.Address?> GetAddressByIdAsync(Guid addressId);
    Task CreateAddressAsync(Domain.Entities.Address address);
    Task DeleteAddressAsync(Domain.Entities.Address address);
    Task<List<Domain.Entities.Address>> GetAllAddressesByUserIdAsync(Guid userId);
}