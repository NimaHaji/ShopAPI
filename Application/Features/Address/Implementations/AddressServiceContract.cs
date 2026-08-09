using System.Net.NetworkInformation;
using Application.Common.Interfaces;
using Application.Features.Address.DTOs;
using Application.Features.Address.Interfaces;
using Shared.Exceptions;

namespace Application.Features.Address.Implementations;

public class AddressService : AddressServiceContract
{
    private readonly AddressRepositoryContract _addressRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly UnitOfWorkContract _unitOfWorkContract;

    public AddressService(AddressRepositoryContract addressRepositoryContract, IUSerContext userContext,
        UnitOfWorkContract unitOfWorkContract)
    {
        _addressRepositoryContract = addressRepositoryContract;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
    }

    public async Task<ViewAddressDto> GetAddressAsync()
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");
        
        var addresses = await _addressRepositoryContract.GetAllAddressesByUserIdAsync(userId);

        return new ViewAddressDto
        {
            Addresses = addresses.Select(a => new ViewAddressItemDto
            {
                Id = a.Id,
                AddressTitle = a.AddressTitle,
                ReceiverName = a.ReceiverName,
                PhoneNumber = a.PhoneNumber,
                Province = a.Province,
                City = a.City,
                AddressLine = a.AddressLine,
                PostalCode = a.PostalCode,
                IsDefault = a.IsDefault,
                CreatedNow = a.CreatedAt,
                UpdatedAt = a.UpdatedAt
            }).ToList()
        };
    }

    public async Task<ViewAddressItemDto> GetAddressByIdAsync(Guid addressId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");
        
        var address = await _addressRepositoryContract.GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
            throw new NotFoundException("آدرسی یافت نشد .");

        return new ViewAddressItemDto
        {
            Id = address.Id,
            AddressTitle = address.AddressTitle,
            ReceiverName = address.ReceiverName,
            PhoneNumber = address.PhoneNumber,
            Province = address.Province,
            City = address.City,
            AddressLine = address.AddressLine,
            PostalCode = address.PostalCode,
            IsDefault = address.IsDefault,
            CreatedNow = address.CreatedAt,
            UpdatedAt = address.UpdatedAt
        };
    }

    public async Task<string> CreateAddressAsync(CreateAddressDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");

        var address = new Domain.Entities.Address(
            userId: userId,
            addressTitle: dto.AddressTitle,
            receiverName: dto.ReceiverName,
            phoneNumber: dto.PhoneNumber,
            province: dto.Province,
            city: dto.City,
            addressLine: dto.AddressLine,
            postalCode: dto.PostalCode
        );

        await _addressRepositoryContract.CreateAddressAsync(address);
        await _unitOfWorkContract.SaveAsync();
        return "آدرس با موفقیت ساخته شد .";
    }

    public async Task<string> EditAddressAsync(Guid addressId, EditAddressDto dto)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");
        
        var address = await _addressRepositoryContract.GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
            throw new NotFoundException("آدرس یافت نشد .");

        address.Edit(
            addressTitle: dto.AddressTitle,
            receiverName: dto.ReceiverName,
            phoneNumber: dto.PhoneNumber,
            province: dto.Province,
            city: dto.City,
            addressLine: dto.AddressLine,
            postalCode: dto.PostalCode
            );

        await _unitOfWorkContract.SaveAsync();
        return "آدرس با موفقیت تغییر پیدا کرد .";
    }

    public async Task<string> DeleteAddressByIdAsync(Guid addressId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");
        
        var address = await _addressRepositoryContract.GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
            throw new NotFoundException("آدرس یافت نشد .");

        await _addressRepositoryContract.DeleteAddressAsync(address);
        await _unitOfWorkContract.SaveAsync();

        return "آدرس با موفقیت حذف شد .";
    }

    public async Task<string> SetAddressDefaultAsync(Guid addressId)
    {
        var userId = _userContext.UserId ?? throw new UnauthorizedAccessException("کاربر احراز هویت نشده است .");
        
        var address = await _addressRepositoryContract.GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
            throw new NotFoundException("آدرس یافت نشد .");

        address.SetAsDefault();

        await _unitOfWorkContract.SaveAsync();

        return "آدرس با موفقیت به آدرس اصلی ثبت شد .";
    }
}