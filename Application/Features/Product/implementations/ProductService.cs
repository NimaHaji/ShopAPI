using Application.Features.Order.Interfaces;
using Application.Features.Product.Interfaces;

namespace Application.Features.Product.implementations;

public class ProductService:ProductServicesContract
{
    private readonly OrderRepositoryContract _orderRepository;

    public ProductService(OrderRepositoryContract orderRepository)
    {
        _orderRepository = orderRepository;
    }
}
