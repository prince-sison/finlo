using Finlo.Domain.Entities;

namespace Finlo.Application.Interfaces;

public interface IAccountRepository : IGenericRepository<Account, Guid>
{
    
}