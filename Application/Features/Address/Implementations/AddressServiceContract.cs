using Application.Common.Interfaces;
using Application.Common.Observability;
using Application.Features.Address.DTOs;
using Application.Features.Address.Interfaces;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Exceptions;

namespace Application.Features.Address.Implementations;

public class AddressService : AddressServiceContract
{
    private readonly AddressRepositoryContract _addressRepositoryContract;
    private readonly IUSerContext _userContext;
    private readonly UnitOfWorkContract _unitOfWorkContract;
    private readonly ILogger<AddressService> _logger;

    public AddressService(AddressRepositoryContract addressRepositoryContract, IUSerContext userContext,
        UnitOfWorkContract unitOfWorkContract, ILogger<AddressService> logger)
    {
        _addressRepositoryContract = addressRepositoryContract;
        _userContext = userContext;
        _unitOfWorkContract = unitOfWorkContract;
        _logger = logger;
    }

    public async Task<ViewAddressDto> GetAddressAsync()
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAddressAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        var addresses =
            await _addressRepositoryContract
                .GetAllAddressesByUserIdAsync(userId);

        _logger.LogInformation(
            LogEvents.Address.GetStarted,
            "Getting user addresses. UserId: {UserId}",
            userId);

        activity?.SetTag("user.id", userId);
        activity?.SetTag("address.count", addresses.Count);

        _logger.LogInformation(
            LogEvents.Address.GetCompleted,
            "User addresses retrieved successfully. UserId: {UserId}, AddressCount: {AddressCount}",
            userId,
            addresses.Count);

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
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(GetAddressByIdAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("address.id", addressId);
        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Address.GetByIdStarted,
            "Getting address by id. AddressId: {AddressId}",
            addressId);

        var address =
            await _addressRepositoryContract
                .GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
        {
            _logger.LogWarning(
                LogEvents.Address.NotFound,
                "Address not found. AddressId: {AddressId}",
                addressId);

            throw new NotFoundException("آدرسی یافت نشد.");
        }

        _logger.LogInformation(
            LogEvents.Address.GetByIdCompleted,
            "Address retrieved successfully. AddressId: {AddressId}",
            addressId);

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
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(CreateAddressAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Address.CreateStarted,
            "Creating address for user. UserId: {UserId}",
            userId);

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

        activity?.SetTag("address.id", address.Id);

        _logger.LogInformation(
            LogEvents.Address.CreateCompleted,
            "Address created successfully. AddressId: {AddressId}, UserId: {UserId}",
            address.Id,
            userId);

        return "آدرس با موفقیت ساخته شد.";
    }

    public async Task<string> EditAddressAsync(Guid addressId, EditAddressDto dto)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(EditAddressAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("address.id", addressId);
        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Address.EditStarted,
            "Editing address. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        var address =
            await _addressRepositoryContract
                .GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
        {
            _logger.LogWarning(
                LogEvents.Address.NotFound,
                "Address not found for edit. AddressId: {AddressId}",
                addressId);

            throw new NotFoundException("آدرس یافت نشد.");
        }

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

        _logger.LogInformation(
            LogEvents.Address.EditCompleted,
            "Address edited successfully. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        return "آدرس با موفقیت تغییر پیدا کرد.";
    }

    public async Task<string> DeleteAddressByIdAsync(Guid addressId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(DeleteAddressByIdAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("address.id", addressId);
        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Address.DeleteStarted,
            "Deleting address. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        var address =
            await _addressRepositoryContract
                .GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
        {
            _logger.LogWarning(
                LogEvents.Address.NotFound,
                "Address not found for deletion. AddressId: {AddressId}",
                addressId);

            throw new NotFoundException("آدرس یافت نشد.");
        }

        await _addressRepositoryContract.DeleteAddressAsync(address);
        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Address.DeleteCompleted,
            "Address deleted successfully. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        return "آدرس با موفقیت حذف شد.";
    }

    public async Task<string> SetAddressDefaultAsync(Guid addressId)
    {
        using var activity =
            ActivitySources.ShopApi.StartActivity(nameof(SetAddressDefaultAsync));

        var userId = _userContext.UserId
                     ?? throw new UnauthorizedAccessException(
                         "کاربر احراز هویت نشده است.");

        activity?.SetTag("address.id", addressId);
        activity?.SetTag("user.id", userId);

        _logger.LogInformation(
            LogEvents.Address.SetDefaultStarted,
            "Setting address as default. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        var address =
            await _addressRepositoryContract
                .GetAddressByIdAndUserIdAsync(userId, addressId);

        if (address is null)
        {
            _logger.LogWarning(
                LogEvents.Address.NotFound,
                "Address not found for setting as default. AddressId: {AddressId}",
                addressId);

            throw new NotFoundException("آدرس یافت نشد.");
        }

        address.SetAsDefault();

        await _unitOfWorkContract.SaveAsync();

        _logger.LogInformation(
            LogEvents.Address.SetDefaultCompleted,
            "Address set as default successfully. AddressId: {AddressId}, UserId: {UserId}",
            addressId,
            userId);

        return "آدرس با موفقیت به آدرس اصلی ثبت شد.";
    }
}