namespace DigitalWallet.Errors;

public sealed class DigitalWalletError
{
    private static string _errorDescription = string.Empty;
    private static ErrorCode? _errorCode;

    public DigitalWalletError(ErrorCode code, string description)
    {
        _errorCode = code;
        _errorDescription = description;
    }

    private const ErrorCode NotFoundCode = ErrorCode.NotFound;
    private const ErrorCode BadRequestCode = ErrorCode.BadRequest;
    private const ErrorCode ForbiddenCode = ErrorCode.Forbidden;
    private const ErrorCode InternalErrorCode = ErrorCode.InternalError;

    public ErrorCode ErrorCode => _errorCode ?? InternalErrorCode;
    public string ErrorDescription => _errorDescription;


    public static readonly DigitalWalletError None = new(ErrorCode.None, string.Empty);

    public static DigitalWalletError Custom(string description)
    {
        return new DigitalWalletError(_errorCode ?? ErrorCode.InternalError,
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


    public static DigitalWalletError InternalError(string description)
    {
        return new DigitalWalletError(InternalErrorCode, description);
    }
}

public enum ErrorCode
{
    None,
    InternalError,
    BadRequest,
    Forbidden,
    NotFound
}