using CoinService.Data;
using CoinService.Models;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoinService
{
    public class CoinerService : Coiner.CoinerBase
    {
        private readonly ILogger<CoinerService> _logger;
        private readonly CoinsDbContext _coinsDbContext;
        public CoinerService(ILogger<CoinerService> logger, CoinsDbContext coinsDbContext)
        {
            _logger = logger;
            _coinsDbContext = coinsDbContext;
        }
        public override async Task<IssueCoinsResponse> IssueCoins(IssueCoinsRequest request, ServerCallContext context)
        {
            User dbUser = await _coinsDbContext.Users.FindAsync(request.UserId);
            if (dbUser is null)
            {
                await _coinsDbContext.Users.AddAsync(new User { Id = request.UserId, Coins = request.Coins, IsPremium = false });
                await _coinsDbContext.SaveChangesAsync();
                return new IssueCoinsResponse { Coins = request.Coins };

            }
            int total = dbUser.Coins + request.Coins;
            dbUser.Coins = total;
            await _coinsDbContext.SaveChangesAsync();
            return new IssueCoinsResponse { Coins = total };
        }
        public override async Task<ShareCoinsResponse> ShareCoins(ShareCoinsRequest request, ServerCallContext context)
        {
            User firstUser = await _coinsDbContext.Users.FindAsync(request.FirstUserId);
            if (firstUser is null || (firstUser.Coins - request.Coins) < 0)
            {
                return new ShareCoinsResponse { Succeeded = false };
            }
            User secondUser = await _coinsDbContext.Users.FindAsync(request.SecondUserId);
            if (secondUser is null)
            {
                await _coinsDbContext.Users.AddAsync(new User { Id = request.SecondUserId, Coins = request.Coins, IsPremium = false });
                firstUser.Coins -= request.Coins;
                await _coinsDbContext.SaveChangesAsync();
                return new ShareCoinsResponse { Succeeded = true };
            }
            secondUser.Coins += request.Coins;
            firstUser.Coins -= request.Coins;
            await _coinsDbContext.SaveChangesAsync();
            return new ShareCoinsResponse { Succeeded = true };
        }
        public override async Task<UserStatsResponse> GetUserStats(UserStatsRequest request, ServerCallContext context)
        {
            try
            {
                User dbUser = await _coinsDbContext.Users.FindAsync(request.UserId);
                if (dbUser is null)
                {
                    await _coinsDbContext.Users.AddAsync(new User { Id = request.UserId, Coins = 0, IsPremium = false });
                    await _coinsDbContext.SaveChangesAsync();
                    return new UserStatsResponse { Succeeded = true, IsPremium = false, Coins = 0 };
                }
                if (dbUser.IsPremium && !dbUser.IsPartner)
                {
                    if (dbUser.MadePremiumAt.ToUniversalTime().AddDays(30) < DateTime.UtcNow)
                    {
                        dbUser.IsPremium = false;
                        await _coinsDbContext.SaveChangesAsync();
                    }
                }
                return new UserStatsResponse { Succeeded = true, IsPremium = dbUser.IsPremium, Coins = dbUser.Coins, IsPartner = dbUser.IsPartner };
            }
            catch (System.Exception)
            {
                return new UserStatsResponse { Succeeded = false };
            }
        }
        public override async Task<MakePremiumResponse> MakePremium(MakePremiumRequest request, ServerCallContext context)
        {
            try
            {
                if (string.IsNullOrEmpty(request.InvoiceId))
                {
                    return new MakePremiumResponse { Succeeded = false };
                }
                Invoice invoice = await _coinsDbContext.Invoices.FindAsync(request.InvoiceId);
                if (invoice is null || invoice.IsUsed)
                {
                    return new MakePremiumResponse { Succeeded = invoice is not null && invoice.IsUsed };
                }
                invoice.IsUsed = true;
                User dbUser = await _coinsDbContext.Users.FindAsync(invoice.UserId);
                if (dbUser is null)
                {
                    await _coinsDbContext.Users.AddAsync(new User
                    {
                        Id = invoice.UserId,
                        Coins = 10000,
                        IsPremium = true,
                        IsPartner = false,
                        MadePremiumAt = DateTime.Now,
                    });
                }
                else
                {
                    dbUser.IsPremium = true;
                    dbUser.MadePremiumAt = DateTime.Now;
                    dbUser.Coins += 10000;
                }
                int rowCount = await _coinsDbContext.SaveChangesAsync();
                return new MakePremiumResponse { Succeeded = rowCount > 0 };
            }
            catch (Exception)
            {
                return new MakePremiumResponse { Succeeded = false };
            }

        }
        public override async Task<DeductCoinResponse> DeductCoins(DeductCoinRequest request, ServerCallContext context)
        {
            User dbUser = await _coinsDbContext.Users.FindAsync(request.UserId);
            if (dbUser is null)
            {
                return null;

            }
            dbUser.Coins -= request.Amount;
            if (dbUser.Coins < 0)
            {
                dbUser.Coins = 0;
            }
            int coins = dbUser.Coins;
            await _coinsDbContext.SaveChangesAsync();
            return new DeductCoinResponse { Coins = coins };
        }
        public override async Task<MakeInvoiceResponse> MakeInvoice(MakeInvoiceRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.InvoiceId) || string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.OrderId))
            {
                return new MakeInvoiceResponse { Succeeded = false };
            }
            await _coinsDbContext.Invoices.AddAsync(new Invoice
            {
                Id = request.InvoiceId,
                Email = request.Email,
                UserId = request.UserId,
                DateAndTime = DateTime.UtcNow,
                IsUsed = false,
                OrderId = request.OrderId
            });
            int rowCount = await _coinsDbContext.SaveChangesAsync();
            return new MakeInvoiceResponse { Succeeded = rowCount > 0 };
        }
        public override async Task<GetInvoiceInfoResponse> GetInvoiceInfo(GetInvoiceInfoRequest request, ServerCallContext context)
        {
            switch (request.IdentifierCase)
            {
                case GetInvoiceInfoRequest.IdentifierOneofCase.InvoiceId:
                    Invoice invoice = await _coinsDbContext.Invoices.FindAsync(request.InvoiceId);
                    if (invoice is not null)
                    {
                        return new GetInvoiceInfoResponse
                        {
                            UserId = invoice.UserId,
                            Email = invoice.Email,
                            Succeeded = true,
                            InvoiceId = invoice.Id
                        };
                    }
                    break;
                case GetInvoiceInfoRequest.IdentifierOneofCase.OrderId:
                    invoice = _coinsDbContext.Invoices.Where(x => x.OrderId == request.OrderId).FirstOrDefault();
                    if (invoice is not null)
                    {
                        return new GetInvoiceInfoResponse
                        {
                            UserId = invoice.UserId,
                            Email = invoice.Email,
                            Succeeded = true,
                            InvoiceId = invoice.Id
                        };
                    }
                    break;
                case GetInvoiceInfoRequest.IdentifierOneofCase.None:
                    break;
            }

            return new GetInvoiceInfoResponse { Succeeded = false };
        }
    }
}
