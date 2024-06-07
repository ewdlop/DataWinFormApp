using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DataWinFormApp
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            //not working
            Console.WriteLine("Form2 constructor");
            InitializeComponent();
            Task task = webView.EnsureCoreWebView2Async();
            while (!task.IsCompleted)
            {
                Console.WriteLine("Waiting for CoreWebView2 initialization...");
            }
            if(task.IsFaulted)
            {
                Console.WriteLine($"CoreWebView2 initialization failed with error: {task.Exception}");
            }
            else
            {
                Console.WriteLine("CoreWebView2 initialization completed");
            }
        }

        private void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            Console.WriteLine("CoreWebView2InitializationCompleted event handler called");
            if (e.IsSuccess)
            {
                Console.WriteLine("Navigate to https://www.bing.com");
                webView.CoreWebView2.Navigate("https://www.bing.com");
            }
            else
            {
                Console.WriteLine($"Initialization failed with error code {e.InitializationException}");
            }
        }
    }
}
