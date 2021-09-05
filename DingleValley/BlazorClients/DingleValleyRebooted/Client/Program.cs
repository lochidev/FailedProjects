using DingleValleyRebooted.Client.Helpers;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace DingleValleyRebooted.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);
            //builder.RootComponents.Add<App>("#app");
            builder.Services.AddScoped<CustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("WebAPI",
                client => client.BaseAddress = new Uri("https://sonic.dinglevalley.net/"))
                .AddHttpMessageHandler<CustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient("Public",
                client => client.BaseAddress = new Uri("https://sonic.dinglevalley.net/"));
            builder.Services.AddOidcAuthentication(options =>
            {
                options.ProviderOptions.ClientId = "dinglevalley_client";
                options.ProviderOptions.Authority = "https://auth.dinglevalley.net/";
                options.ProviderOptions.ResponseType = "code";
                options.ProviderOptions.DefaultScopes.Add("dinglevalley_client_api");

                // Note: response_mode=fragment is the best option for a SPA. Unfortunately, the Blazor WASM
                // authentication stack is impacted by a bug that prevents it from correctly extracting
                // authorization error responses (e.g error=access_denied responses) from the URL fragment.
                // For more information about this bug, visit https://github.com/dotnet/aspnetcore/issues/28344.
                //
                options.ProviderOptions.ResponseMode = "query";
                options.AuthenticationPaths.RemoteRegisterPath = "https://auth.dinglevalley.net/Account/Register";
            });
            await builder.Build().RunAsync();
        }
    }
}
