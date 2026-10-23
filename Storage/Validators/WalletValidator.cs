using FluentValidation;
using DigitalWallet.Data;

namespace DigitalWallet;

public class CreateWalletValidator : AbstractValidator<CreateWalletDto>
{
    public CreateWalletValidator()
    {
        RuleFor(w => w.UserId)
            .Must(val => Guid.TryParse(val, out _))
            .WithMessage("Invalid user id. Must be a valid guid")
            .When(w => !string.IsNullOrWhiteSpace(w.UserId));
    }
}

public class WalletFundValidator : AbstractValidator<FundWalletDto>
{
    public WalletFundValidator()
    {
        RuleFor(w => w.WalletId)
            .GreaterThan(0)
            .NotNull()
            .WithMessage("Invalid wallet id. Must be a valid decimal");

        RuleFor(w => w.Amount)
            .NotNull()
            .GreaterThan(0)
            .PrecisionScale(19, 4, false);
    }
}

// public class WalletFundValidator : AbstractValidator<FundWalletDto>
// {
//     public WalletFundValidator()
//     {
//         RuleFor(w => w.Amount).NotNull().GreaterThan(0).PrecisionScale(19, 4, false);
//     }
// }