namespace Application.Features.Product.DTOs;

public class EditProductDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
}