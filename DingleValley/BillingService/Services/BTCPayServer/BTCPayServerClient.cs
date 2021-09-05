using BillingService.Services.BTCPayServer.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BillingService.Services.BTCPayServer
{
    public class BTCPayServerClient
    {
        private readonly string BaseAddress = "https://btcpay.dinglevalley.net/api/v1";
        private readonly string Token = "hithere";
        private const string StoreId = "hithere";
        private readonly IHttpClientFactory _httpClientFactory;
        public BTCPayServerClient(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<List<Invoice>> GetAllInvoicesAsync()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/stores/{StoreId}/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("token", Token);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<List<Invoice>>(await response.Content.ReadAsStringAsync());
            }
            return null;

        }
        public async Task<Invoice> GetInvoice(string id)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, $"{BaseAddress}/stores/{StoreId}/invoices/{id}");
            request.Headers.Authorization = new AuthenticationHeaderValue("token", Token);
            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Invoice>(await response.Content.ReadAsStringAsync());
            }
            return null;

        }
        public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{BaseAddress}/stores/{StoreId}/invoices");
            request.Headers.Authorization = new AuthenticationHeaderValue("token", Token);
            request.Content = new StringContent(JsonConvert.SerializeObject(invoice), Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Invoice>(await response.Content.ReadAsStringAsync());
            }
            return null;

        }
    }
}
