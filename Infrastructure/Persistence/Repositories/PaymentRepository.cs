using Application.Features.Payment.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class PaymentRepository : PaymentRepositoryContract
{
    private readonly ShopDbContext _dbContext;

    public PaymentRepository(ShopDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task CreatePaymentAsync(Payment payment)
    {
        await _dbContext
            .Payments
            .AddAsync(payment);
    }

    public async Task SaveAsync()
    {
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Payment?> GetPaymentByResNumAsync(string resNum)
    {
        return await _dbContext
            .Payments
            .Where(p => p.ResNum == resNum)
            .FirstOrDefaultAsync();
    }

    public async Task<Payment?> GetPaymentByAuthorityAsync(string authority)
    {
        return await _dbContext
            .Payments
            .Where(p => p.Authority == authority)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> IsExistByRefNum(string refNum)
    {
        return await _dbContext
            .Payments
            .AnyAsync(p => p.RefNum == refNum);
    }

    public async Task<Payment?> GetPaymentByIdAsync(Guid paymentId)
    {
        return await _dbContext
            .Payments
            .Where(p=>p.Id == paymentId)
            .FirstOrDefaultAsync();
    }
}