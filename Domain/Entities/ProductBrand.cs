using Shared.Exceptions;

namespace Domain.Entities;

public class ProductBrand
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ProductBrand()
    {
    }

    public static ProductBrand Create(string title)
    {
        return new ProductBrand
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Edit(string title)
    {
        if (string.IsNullOrEmpty(title))
            throw new BusinessException("عنوان برند نمی تواند خالی باشد .");

        Title = title;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new BusinessException("این برند حذف نشده است .");
        
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}