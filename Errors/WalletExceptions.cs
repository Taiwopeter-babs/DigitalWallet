namespace DigitalWallet.Errors;



public sealed class InvalidAmountException : Exception
{
    public InvalidAmountException(decimal amount) :
        base($"The amount {amount} is invalid.")
    {
    }
}

public sealed class NotFoundException : Exception
{
    public NotFoundException(string? message) :
        base(message ?? $"The resource was not found")
    {
    }
}

public sealed class BadRequestException : Exception
{
    public BadRequestException(string? message) :
        base(message ?? "Bad Request")
    {
    }
}