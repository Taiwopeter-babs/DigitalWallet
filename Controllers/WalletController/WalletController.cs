using DigitalWallet.Data;
using DigitalWallet.Extensions;
using DigitalWallet.Results;
using DigitalWallet.Services.WalletService;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Storage.Response;

namespace DigitalWallet.Controllers.WalletController;

public static class MapWalletRoutes
{
    extension(IEndpointRouteBuilder app)
    {
        public void MapWalletEndpoints()
        {
            var walletGroup = app.MapGroup("/api/wallets")
                .WithTags("Wallets");

            walletGroup.MapGet("/", GetAllWallets);
            walletGroup.MapPost("/", CreateWallet);
            walletGroup.MapGet("/{id:int}/", GetWallet);
            walletGroup.MapPost("/{id:int}/fund", FundWallet);
            walletGroup.MapPost("/{senderWalletId:int}/transfer", Transfer);

            var userWalletGroup = app.MapGroup("/api/users")
                .WithTags("User Wallets");

            userWalletGroup.MapGet("/{id:Guid}/wallets", GetUserWallets);
        }
    }

    private static async Task<Results<Ok<ApiResponse<List<WalletDto>>>, BadRequest<ApiResponse<List<WalletDto>>>>>
        GetAllWallets(
            [FromServices] IWalletService walletService,
            CancellationToken cancellationToken)
    {
        var result = await walletService.GetAllWallets(cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }


    private static async Task<Results<Ok<ApiResponse<WalletDto>>, BadRequest<ApiResponse<WalletDto>>>> GetWallet(
        [FromServices] IWalletService walletService,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        var result = await walletService.Get(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }

    private static async Task<Results<Ok<ApiResponse<List<WalletDto>>>, BadRequest<ApiResponse<List<WalletDto>>>>>
        GetUserWallets(
            [FromServices] IWalletService walletService,
            [FromRoute] Guid id,
            CancellationToken cancellationToken)
    {
        var result = await walletService.GetUserWallets(id, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }

    private static async Task<Results<Ok<ApiResponse<WalletDto>>, BadRequest<ApiResponse<WalletDto>>>> CreateWallet(
        [FromServices] IWalletService walletService,
        [FromBody] CreateWalletDto createWalletDto,
        CancellationToken cancellationToken)
    {
        var result = await walletService.Create(createWalletDto, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }

    private static async Task<Results<Ok<ApiResponse<WalletDto>>, BadRequest<ApiResponse<WalletDto>>>> FundWallet(
        IValidator<FundWalletDto> validator,
        [FromServices] IWalletService walletService,
        [FromRoute] int id,
        [FromBody] FundWalletDto fundWalletDto,
        CancellationToken cancellationToken)
    {
        var errors = await validator.ValidateModelAsync(fundWalletDto, cancellationToken);
        if (errors != null)
            return TypedResults.BadRequest(new ApiResponse<WalletDto>
            {
                StatusCode = StatusCode.BadRequest,
                Error = errors,
                Success = false
            });


        var result = await walletService.Fund(id, fundWalletDto.Amount, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }

    private static async Task<Results<Ok<ApiResponse<WalletDto>>, BadRequest<ApiResponse<WalletDto>>>> Transfer(
        IValidator<WalletTransferDto> validator,
        [FromServices] IWalletService walletService,
        [FromRoute] int senderWalletId,
        [FromBody] WalletTransferDto walletTransferDto,
        CancellationToken cancellationToken)
    {
        var errors = await validator.ValidateModelAsync(walletTransferDto, cancellationToken);
        if (errors != null)
            return TypedResults.BadRequest(new ApiResponse<WalletDto>
            {
                StatusCode = StatusCode.BadRequest,
                Error = errors,
                Success = false
            });


        var result = await walletService.Transfer(walletTransferDto.SenderWalletId, walletTransferDto.Amount,
            walletTransferDto.ReceiverWalletId, cancellationToken);

        return result.IsSuccess
            ? TypedResults.Ok(result.ToApiResponse())
            : TypedResults.BadRequest(result.ToApiResponse());
    }
}