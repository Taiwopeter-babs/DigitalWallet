namespace DigitalWallet.Data;

public static class WalletExtension
{
    extension(Wallet wallet)
    {
        public WalletDto ToWalletDto()
        {
            return new WalletDto
            {
                Id = wallet.Id,
                UserId = wallet.UserId,
                Balance = wallet.Balance,
                UpdatedAt = wallet.UpdatedAt,
                CreatedAt = wallet.CreatedAt
            };
        }
    }

    extension(WalletDto walletDto)
    {
        public Wallet ToWallet()
        {
            return new Wallet
            {
                Id = walletDto.Id,
                UserId = walletDto.UserId,
                Balance = walletDto.Balance,
                UpdatedAt = walletDto.UpdatedAt,
                CreatedAt = walletDto.CreatedAt
            };
        }
    }

    extension(CreateWalletDto walletDto)
    {
        public Wallet ToWallet()
        {
            return new Wallet
            {
                UserId = walletDto.UserId
            };
        }
    }

    extension(CreateWalletTransactionDto walletTransactionDto)
    {
        public WalletTransaction ToWalletTransaction()
        {
            return new WalletTransaction
            {
                Amount = walletTransactionDto.Amount,
                Type = walletTransactionDto.Type,
                Status = walletTransactionDto.Status,
                WalletId = walletTransactionDto.WalletId
            };
        }
    }

    extension(WalletTransaction walletTransaction)
    {
        public WalletTransactionDto ToWalletTransactionDto()
        {
            return new WalletTransactionDto
            {
                Id = walletTransaction.Id,
                Amount = walletTransaction.Amount,
                Type = walletTransaction.Type,
                Status = walletTransaction.Status,
                WalletId = walletTransaction.WalletId,
                Timestamp = walletTransaction.CreatedAt
            };
        }
    }
}