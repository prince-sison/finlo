using Finlo.Application.Interfaces;
using Finlo.Domain.Entities;
using Finlo.Infrastructure.Persistence.Data;

namespace Finlo.Infrastructure.Repositories;

public class AccountRepository : GenericRepository<Account, Guid>, IAccountRepository
{
    private readonly AppDbContext _context;

    public AccountRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }
}