using Finlo.Application.Common;
using Finlo.Application.Interfaces;
using Finlo.Application.Interfaces.Services;
using Finlo.Domain.Entities;

namespace Finlo.Application.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;

    public AccountService(IAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<Result> CreateAccountAsync(string name, long balance, string? bankName, string? note, int type)
    {
        
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.FailureResult("Name is required.");
        }

        if (string.IsNullOrWhiteSpace(bankName))
        {
            return Result.FailureResult("Bank name is required.");
        }

        if (string.IsNullOrWhiteSpace(note))
        {
            return Result.FailureResult("Note is required.");
        }

        var account = new Account
        {
            Id = Guid.NewGuid(),
            Name = name,
            Balance = balance,
            BankName = bankName,
            Note = note,
            Type = (Finlo.Domain.Enums.AccountType)type,
            CreatedAt = DateTime.UtcNow
        };

        await _accountRepository.AddAsync(account);
        return Result.SuccessResult("Account created successfully.");
    }

    public async Task<Result> DeleteAccountAsync(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            return Result.FailureResult("Account ID is required.");
        }

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            return Result.FailureResult("Account not found.");
        }

        await _accountRepository.DeleteAsync(account.Id);
        return Result.SuccessResult("Account deleted successfully.");
    }

    public async Task<Result> GetAccountAsync(Guid accountId)
    {
        if (accountId == Guid.Empty)
        {
            return Result.FailureResult("Account ID is required.");
        }

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            return Result.FailureResult("Account not found.");
        }

        return Result.SuccessResult("Account retrieved successfully.", account);
    }

    public async Task<Result> UpdateAccountAsync(Guid accountId, string name, long balance, string? bankName, string? note, int type)
    {
        if (accountId == Guid.Empty)
        {
            return Result.FailureResult("Account ID is required.");
        }

        var account = await _accountRepository.GetByIdAsync(accountId);
        if (account == null)
        {
            return Result.FailureResult("Account not found.");
        }

        // Update account properties here as needed
        account.Name = name;
        account.Balance = balance;
        account.BankName = bankName;
        account.Note = note;
        account.Type = (Finlo.Domain.Enums.AccountType)type;
        account.UpdatedAt = DateTime.UtcNow;

        await _accountRepository.UpdateAsync(account);
        return Result.SuccessResult("Account updated successfully.");
    }
}