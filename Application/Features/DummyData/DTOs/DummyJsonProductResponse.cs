namespace Application.Features.DummyData.DTOs;

public class DummyJsonProductResponse
{
    public List<DummyJsonProductDto> Products { get; set; }
}

public class DummyJsonProductDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public string Brand { get; set; }

    public decimal Price { get; set; }
    public decimal DiscountPercentage { get; set; }
    public decimal Rating { get; set; }

    public int Stock { get; set; }
}