namespace Application.Features.Auth.DTOs;

public class IdentifyByEmailResponseDto
{
    public List<IdentityResponseDto> IdentityResponseDtos { get; set; }
}
public class IdentityResponseDto
{
    public Guid TenantId { get; set; }
    public string Name { get; set; }
}