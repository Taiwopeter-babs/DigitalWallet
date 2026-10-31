using Microsoft.EntityFrameworkCore;
using DigitalWallet.Errors;
using DigitalWallet.Repository;
using Storage.Response;

namespace DigitalWallet.Results;

public class Result
{
    protected Result(bool isSuccess, DigitalWalletError error)
    {
        if (isSuccess && error != DigitalWalletError.None)
            throw new ArgumentException("Success result cannot have an error.", nameof(error));

        if (!isSuccess && error == DigitalWalletError.None)
            throw new ArgumentException("Failure result must have an error.", nameof(error));

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    public DigitalWalletError Error { get; }


    public static Result<T> Success<T>(T? value)
    {
        return Result<T>.Success(value);
    }


    public static Result<T> Failure<T>(string message)
    {
        return Result<T>.Failure(message);
    }


    public static Result<T> NotFound<T>(string message)
    {
        return Result<T>.NotFound(message);
    }

    public static Result<T> BadRequest<T>(string message)
    {
        return Result<T>.BadRequest(message);
    }


    public static async Task<Result<T>> TryAsync<T>(IRepositoryManager repo,
        Func<IRepositoryManager, CancellationToken, Task<Result<T>>> func, CancellationToken ct = default)
    {
        try
        {
            var value = await func(repo, ct);

            return value;
        }
        catch (Exception ex)
        {
            await repo.RollbackTransactionAsync(ct);
            return ResultExtensions.FromException<T>(ex);
        }
    }
}

public class Result<T> : Result
{
    private readonly T? _value;

    protected Result(bool isSuccess, T? value, DigitalWalletError error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T? Value => IsSuccess
        ? _value!
        : default;

    public static Result<T> Success(T? value)
    {
        return new Result<T>(true, value, DigitalWalletError.None);
    }

    public static Result<T> NotFound(string description)
    {
        return new Result<T>(false, default, DigitalWalletError.NotFound(description));
    }

    public static Result<T> BadRequest(string description)
    {
        return new Result<T>(false, default, DigitalWalletError.BadRequest(description));
    }

    public static Result<T> Forbidden(string description)
    {
        return new Result<T>(false, default, DigitalWalletError.Forbidden(description));
    }


    public static Result<T> Failure(string message)
    {
        return new Result<T>(false, default, DigitalWalletError.Custom(message));
    }
}

public static class ResultExtensions
{
    extension<T>(Result<T> result)
    {
        public ApiResponse<T> ToApiResponse(string successMessage = "Operation successful")
        {
            if (result.IsSuccess) return ApiResult<T>.Ok(result.Value!, successMessage);

            // NOTE: Adjust 'result.Error.Message' if your DigitalWalletError property 
            // is named differently (e.g., result.Error.Description).
            var errorMessage = result.Error.ErrorDescription;

            // Map the error type/code to the correct ApiResponse status method.
            // Adjust 'result.Error.Type' or 'result.Error.Code' based on how DigitalWalletError is structured.
            return result.Error.ErrorCode switch
            {
                ErrorCode.NotFound => ApiResult<T>.NotFound(errorMessage),
                ErrorCode.BadRequest => ApiResult<T>.BadRequest(errorMessage),
                ErrorCode.Forbidden => ApiResult<T>
                    .ServerError(errorMessage), // Map forbidden to server error or handle as needed
                _ => ApiResult<T>.ServerError(errorMessage)
            };
        }
    }

    public static Result<T> FromException<T>(Exception ex)
    {
        return ex switch
        {
            NotFoundException => Result<T>.NotFound(ex.Message),
            InvalidAmountException => Result<T>.BadRequest(ex.Message),
            DbUpdateConcurrencyException => Result<T>.Forbidden(ex.Message),
            _ => Result<T>.Failure(ex.Message)
        };
    }
}