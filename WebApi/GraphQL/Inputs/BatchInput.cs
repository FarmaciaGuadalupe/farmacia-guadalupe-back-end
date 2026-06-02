using System;

namespace WebApi.GraphQL.Inputs
{
    public record AddBatchInput(
        int ProductId,
        string BatchCode,
        DateTime ExpirationDate,
        int QuantityUnits
    );

    public record UpdateBatchInput(
        int BatchId,
        int ProductId,
        string BatchCode,
        DateTime ExpirationDate,
        int CurrentQuantityUnits,
        bool IsActive
    );
}
