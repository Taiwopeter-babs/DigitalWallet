namespace DigitalWallet.Errors;

public sealed record DigitalWalletError
{
    private static string _errorDescription = string.Empty;
    private static string? _errorCode;

    public DigitalWalletError(string code, string description)
    {
        _errorCode = code;
        _errorDescription = description;
    }

    private const string NotFoundCode = "NotFound";
    private const string BadRequestCode = "BadRequest";
    private const string ForbiddenCode = "Forbidden";
    private const string UnauthorizedCode = "Unauthorized";
    private const string InternalErrorCode = "InternalError";


    public static readonly DigitalWalletError None = new(string.Empty, string.Empty);

    public static DigitalWalletError Custom(string description)
    {
        return new DigitalWalletError(_errorCode ?? "UnknownError",
            description);
    }

    public static DigitalWalletError NotFound(string description)
    {
        return new DigitalWalletError(NotFoundCode, description);
    }

    public static DigitalWalletError BadRequest(string description)
    {
        return new DigitalWalletError(BadRequestCode, description);
    }

    public static DigitalWalletError Forbidden(string description)
    {
        return new DigitalWalletError(ForbiddenCode, description);
    }

    public static DigitalWalletError Unauthorized(string description)
    {
        return new DigitalWalletError(UnauthorizedCode, description);
    }

    public static DigitalWalletError InternalError(string description)
    {
        return new DigitalWalletError(InternalErrorCode, description);
    }
}