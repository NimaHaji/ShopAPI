using Shared.Exceptions;

namespace Domain.Entities;

public class ProductCategory
{
    public Guid Id { get; private set; }
    public string Title { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    private readonly List<Product> _products = new();
    public IReadOnlyCollection<Product> Products => _products;

    private ProductCategory()
    {
    }

    public static ProductCategory Create(string title)
    {
        return new ProductCategory
        {
            Id = Guid.NewGuid(),
            Title = title,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false,
            DeletedAt = null
        };
    }

    public void Edit(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new BusinessException("عنوان دسته بندی نمی تواند خالی باشد .");
        
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
            throw new BusinessException("این دسته بندی حذف نشده است .");
        
        IsDeleted = false;
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}