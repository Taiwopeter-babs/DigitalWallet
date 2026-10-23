namespace DigitalWallet.Errors;

public abstract class WalletException(string message) : Exception
{
}

public sealed class InvalidAmountException : WalletException
{
    public InvalidAmountException(decimal amount) :
        base($"The amount {amount} is invalid.")
    {
    }
}

public sealed class NotFoundException : WalletException
{
    public NotFoundException(string? message) :
        base(message ?? $"The resource was not found")
    {
    }
}

public sealed class BadRequestException : WalletException
{
    public BadRequestException(string? message) :
        base(message ?? "Bad Request")
    {
    }
}