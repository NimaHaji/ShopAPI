using Domain.Enums;

namespace Application.Features.Discount.DTOs;

public class EditDiscountDto
{
    public string? Title { get; set; } = null!;

    public DiscountType? DiscountType { get; set; }

    public decimal? Value { get; set; }

    public decimal? MaxDiscountAmount { get; set; }

    public DateTime? StartsAt { get; set; }

    public DateTime? EndsAt { get; set; }
}