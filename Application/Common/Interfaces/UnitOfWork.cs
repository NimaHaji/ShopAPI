namespace Application.Common.Interfaces;

public interface UnitOfWorkContract
{
    Task SaveAsync();
}