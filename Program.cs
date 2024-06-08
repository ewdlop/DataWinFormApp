using DataWinFormApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Runtime.InteropServices;

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
            using (IServiceScope scope = host.Build().Services.CreateScope())
            {
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