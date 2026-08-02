namespace Application.Features.Review.DTOs;

public class AdminViewReviewsDto
{
    public List<AdminViewReviewItemDto> Reviews { get; set; } = new();
}

public class AdminViewReviewItemDto
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public string ProductTitle { get; set; }

    public string Comment { get; set; }
    public int StarsCount { get; set; }

    public string ReviewStatus { get; set; }

    public AdminViewReviewItemUserDto User { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public class AdminViewReviewItemUserDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}