using BillingService.Services.BTCPayServer;
using BillingService.Services.BTCPayServer.Entities;
using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace BillingService
{
    public class BillerService : Biller.BillerBase
    {
        private readonly BTCPayServerClient _btcPayServerClient;
        public BillerService(BTCPayServerClient btcPayServerClient)
        {
            _btcPayServerClient = btcPayServerClient;
        }
        public override async Task<NewInvoiceResponse> MakeInvoice(NewInvoiceRequest request, ServerCallContext context)
        {
            try
            {
                Invoice createdInvoice = await _btcPayServerClient.CreateInvoiceAsync(new Invoice(request.Amount,
                    request.Currency,
                    string.IsNullOrWhiteSpace(request.RedirectUrl) ? null : new Checkout() { RedirectUrl = request.RedirectUrl, RedirectAutomatically = true, ExpirationMinutes = 590, MonitoringMinutes = 1440 },
                    string.IsNullOrWhiteSpace(request.OrderId) ? null : new MetaData(request.OrderId)
                    ));

                if (createdInvoice is null)
                {
                    return new NewInvoiceResponse() { Successful = false };
                }
                return new NewInvoiceResponse() { CheckOutUrl = createdInvoice.CheckoutLink, Successful = true, InvoiceId = createdInvoice.Id };
            }
            catch (Exception)
            {
                return new NewInvoiceResponse() { Successful = false };
            }

        }
        public override async Task<InvoiceResponse> GetInvoiceStatus(InvoiceRequest request, ServerCallContext context)
        {
            try
            {
                Invoice createdInvoice = await _btcPayServerClient.GetInvoice(request.Id);
                string status = createdInvoice.Status;
                bool completed = status.ToLower().Contains("settled") || status.ToLower().Contains("complete") || status.ToLower().Contains("confirmed") || status.ToLower().Contains("paid");
                return new InvoiceResponse() { IsCompleted = completed, Status = status };
            }
            catch (Exception)
            {
                return new InvoiceResponse() { IsCompleted = false };
            }

        }
    }
}
