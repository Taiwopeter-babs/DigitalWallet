using DigitalWallet.Data;
using DigitalWallet.Results;
using DigitalWallet.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Storage.Response;

namespace DigitalWallet.Controllers.HistoryController;

public static class MapHistoryRoutes
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapHistoryEndpoints()
        {
            var historyGroup = app.MapGroup("/api/wallets")
                .WithTags("History");

            historyGroup.MapGet("/{id:int}/history", GetWalletHistory);


            var userHistoryGroup = app.MapGroup("/api/users")
                .WithTags("User Wallets");

            userHistoryGroup.MapGet("/{id:Guid}/activity", GetUserActivity);
            userHistoryGroup.MapGet("/{id:Guid}/transactions", GetUserWalletTransactions);
        }
    }

    private static async Task<Results<Ok<ApiResponse<List<TransactionEventDto>>>, BadRequest<ApiResponse<List<TransactionEventDto>>>>>
        GetUserActivity(
            [FromRoute] Guid id,
            [FromServices] IHistoryService historyService,
            CancellationToken cancellationToken)
    {
        var result = await historyService.GetUserActivity(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }

    private static async Task<Results<Ok<ApiResponse<List<WalletTransactionDto>>>, BadRequest<ApiResponse<List<WalletTransactionDto>>>>>
        GetUserWalletTransactions(
            [FromRoute] Guid id,
            [FromServices] IHistoryService historyService,
            CancellationToken cancellationToken)
    {
        var result = await historyService.GetUserWalletTransactions(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }


    private static async Task<Results<Ok<ApiResponse<List<WalletTransactionDto>>>, BadRequest<ApiResponse<List<WalletTransactionDto>>>>> GetWalletHistory(
        [FromRoute] int id,
        [FromServices] IHistoryService historyService,
        CancellationToken cancellationToken)
    {
        var result = await historyService.GetWalletHistory(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }
}