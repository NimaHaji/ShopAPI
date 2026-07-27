namespace Application.Features.Product.DTOs;

public class ViewProductCategoryDto
{
    public List<ViewProductCategoryItemDto> Items { get; set; }
}

public class ViewProductCategoryItemDto
{
    public string Title { get; set; }
}