namespace Application.Features.Discount.DTOs;

public class ViewDiscountDto
{
    public List<ViewDiscountItemsDto> DiscountItems { get; set; }
}

public class ViewDiscountItemsDto
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime StartsAt { get;set; }
    public DateTime EndsAt { get;set; }
    public bool IsActive { get;set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}