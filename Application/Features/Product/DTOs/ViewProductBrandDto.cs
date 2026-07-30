namespace Application.Features.Product.DTOs;

public class ViewProductBrandDto
{
    public List<ViewProductBrandItemDto> Items { get; set; }
}

public class ViewProductBrandItemDto
{
    public string Title { get; set; }
}