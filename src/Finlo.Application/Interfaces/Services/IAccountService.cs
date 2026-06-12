using Finlo.Application.Common;

namespace Finlo.Application.Interfaces.Services;

public interface IAccountService
{
    Task<Result> CreateAccountAsync(string name, long balance, string? bankName, string? note, int type);
    Task<Result> GetAccountAsync(Guid accountId);
    Task<Result> UpdateAccountAsync(Guid accountId, string name, long balance, string? bankName, string? note, int type);
    Task<Result> DeleteAccountAsync(Guid accountId);
}