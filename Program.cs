using CommunityToolkit.Diagnostics;
using DataWinFormApp;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using Polly.Extensions.Http;
using Polly;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using Azure.Identity;

namespace DataWinFormApp
{
    internal static partial class Program
    {
        // Import the AllocConsole function from kernel32.dll
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool AllocConsole();

        // Import the FreeConsole function from kernel32.dll
        [LibraryImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool FreeConsole();


        //TODO: research further and test this code
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            AttachConsole();
            //Task.Run(() => Application.Run(new Form2()));

            IHostBuilder host = Host.CreateDefaultBuilder().ConfigureAppConfiguration((context, config) =>
            {
                config.AddAzureKeyVault(
                    new Uri($"https://{context.Configuration["KeyVaultName"]}.vault.azure.net/"),
                    new DefaultAzureCredential(includeInteractiveCredentials: true)
                );
            })
            .ConfigureServices((context, services) =>
            {
                //this is for typed clients
                //https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#typed-clients
                //services.AddHttpClient<IAPIService, APIService>()
                //        .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                //
                //

                //https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#named-clients
                services.AddHttpClient("named")
                      .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                      .AddPolicyHandler(GetRetryPolicy());
                services.AddHttpClient("cosmos")
                  .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                  .AddPolicyHandler(GetRetryPolicy());

                //services.AddScoped<IHelloService, HelloService>();
                services.AddScoped(services =>
                {
                    IConfiguration configuration = services.GetRequiredService<IConfiguration>();
                    IConfigurationSection azureAdB2CSetting = configuration.GetRequiredSection("AzureAdB2C");

                    //https://github.com/Azure-Samples/active-directory-b2c-dotnet-desktop/blob/msalv3/active-directory-b2c-wpf/App.xaml.cs                
                    string? tenantName = azureAdB2CSetting["TenantName"];
                    Guard.IsNotNullOrWhiteSpace(tenantName);
                    string? clientId = azureAdB2CSetting["ClientId"];
                    Guard.IsNotNullOrWhiteSpace(clientId);
                    string? clientSecret = azureAdB2CSetting["ClientSecret"];
                    Guard.IsNotNullOrWhiteSpace(clientSecret);

                    //expecting file name
                    //not only way
                    //string? clientCertificateFilePath = azureAdB2CSetting["ClientCertificate"];
                    //Guard.IsNotNullOrWhiteSpace(clientCertificate);
                    //X509Certificate certificate = X509Certificate.CreateFromCertFile(clientCertificateFilePath);

                    string[] clientCapabilities = azureAdB2CSetting.GetValue<string[]>("ClientCapabilities") ?? [];

                    string? signUpSignInPolicyId = azureAdB2CSetting["SignUpSignInPolicyId"];
                    Guard.IsNotNullOrWhiteSpace(signUpSignInPolicyId);

                    string azureAdB2CHostname = $"{tenantName}.b2clogin.com";

                    string tenant = $"{tenantName}.onmicrosoft.com";

                    string authorityBase = $"https://{azureAdB2CHostname}/tfp/{tenant}/";
                    string authoritySignUpSignIn = $"{authorityBase}/{signUpSignInPolicyId}";

                    Console.WriteLine("Please enter a Azure B2C tenant id: ");
                    string? b2cTenantID = Console.ReadLine();
                    Guard.IsNotNullOrWhiteSpace(b2cTenantID);


                    //client capabilities
                    //https://learn.microsoft.com/en-us/entra/identity-platform/claims-challenge?tabs=dotnet#client-capabilities

                    //https://learn.microsoft.com/en-us/entra/msal/dotnet/getting-started/instantiate-confidential-client-config-options
                    //IConfidentialClientApplication? b2cTenantConfidentalClientApplication = ConfidentialClientApplicationBuilder.CreateWithApplicationOptions(new ConfidentialClientApplicationOptions()
                    //{
                    //    ClientId = clientId,
                    //    ClientSecret = clientSecret,
                    //    TenantId = b2cTenantID,
                    //    ClientCapabilities = clientCapabilities
                    //})
                    //.WithCertificate(certificate)
                    //.WithB2CAuthority(authoritySignUpSignIn).Build();

                    //https://learn.microsoft.com/en-us/entra/msal/dotnet/getting-started/instantiate-public-client-config-options
                    IPublicClientApplication? b2cTenantConfidentialClientApplication = PublicClientApplicationBuilder.CreateWithApplicationOptions(new PublicClientApplicationOptions()
                    {
                        ClientId = clientId,
                        TenantId = b2cTenantID,
                        ClientCapabilities = clientCapabilities
                    })
                    .WithB2CAuthority(authoritySignUpSignIn).Build();

                    return b2cTenantConfidentialClientApplication;
                });
                services.AddScoped<IAPIService, APIService>();
                services.AddScoped<Form1>();

                //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/best-practice-dotnet
                // Customize this value based on desired DNS refresh timer
                // Registering the Singleton SocketsHttpHandler lets you reuse it across any HttpClient in your application
                //services.AddSingleton(new SocketsHttpHandler()
                //{
                //    PooledConnectionLifetime = TimeSpan.FromMinutes(5)
                //});

                services.AddSingleton(serviceProvider =>
                {
                    IHttpClientFactory httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                    IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                    IConfigurationSection configurationSection = configuration.GetRequiredSection("CosmosDB");
                    string? connectionString = configurationSection["ConnectionString"];
                    Guard.IsNotNullOrWhiteSpace(connectionString);
                    CosmosClientOptions cosmosClientOptions = new CosmosClientOptions()
                    {
                        HttpClientFactory = () => httpClientFactory.CreateClient("cosmos")
                    };
                    return new CosmosClient(connectionString, cosmosClientOptions);
                });

            });

            using (IServiceScope scope = host.Build().Services.CreateScope())
            {
                Form1 form = scope.ServiceProvider.GetRequiredService<Form1>();
                Application.Run(form);
            }
        }

        static void AttachConsole()
        {
            if(!AllocConsole())
            {
                throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
            }
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.WriteLine("Console attached");
        }

        static AsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

    }
}