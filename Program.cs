using CommunityToolkit.Diagnostics;
using DataWinFormApp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using System;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace DataWinFormApp
{
    internal static class Program
    {
        // Import the AllocConsole function from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        // Import the FreeConsole function from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FreeConsole();


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
            Task.Run(() => Application.Run(new Form2()));

            IHostBuilder host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                //services.AddScoped<IHelloService, HelloService>();
                services.AddScoped<Form1>();
            });


            //TODO: research further and test this code
            using (IServiceScope scope = host.Build().Services.CreateScope())
            {
                IConfiguration configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
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


                //https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/desktop-mobile/acquiring-tokens-interactively
                string[] scopes = ["user.read"];
                AuthenticationResult result = b2cTenantConfidentialClientApplication.AcquireTokenInteractive(scopes).ExecuteAsync().GetAwaiter().GetResult();

                Console.WriteLine($"Token: {result.AccessToken}");

                Application.Run(scope.ServiceProvider.GetRequiredService<Form1>());
            }
        }

        static void AttachConsole()
        {
            AllocConsole();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            Console.SetError(new StreamWriter(Console.OpenStandardError()) { AutoFlush = true });
            Console.SetIn(new StreamReader(Console.OpenStandardInput()));
            Console.WriteLine("Console attached");
        }
    }
}