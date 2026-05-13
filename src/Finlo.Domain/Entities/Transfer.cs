using Finlo.Domain.Enums;

namespace Finlo.Domain.Entities;

public class Transfer : Transaction
{
    // Source account for the transfer
    public Guid SourceAccountId { get; set; }
    public Account SourceAccount { get; set; } = null!;

    // Destination account for the transfer
    public Guid DestinationAccountId { get; set; }
    public Account DestinationAccount { get; set; } = null!;

    public override TransactionType Type => TransactionType.Transfer;

    public long? TransferFee { get; set; }
}