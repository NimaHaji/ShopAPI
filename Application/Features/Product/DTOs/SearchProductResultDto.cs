namespace Application.Features.Product.DTOs;

public class SearchProductResultDto
{
    public List<SearchProductItemsResultDto> Items { get; set; }
}

public class SearchProductItemsResultDto
{
    public string Title { get; set; }
    public string Category { get; set; }
}