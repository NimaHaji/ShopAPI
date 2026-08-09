namespace Application.Features.Review.DTOs;

public class ViewReviewsDto
{
    public List<ViewReviewItemsDto> Reviews { get; set; }
}

public class ViewReviewItemsDto
{
    public string Comment { get; set; }
    public int StarsCount { get; set; }
    public ViewReviewItemUserDto User { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ViewReviewItemUserDto
{
    public string Name { get; set; }
}