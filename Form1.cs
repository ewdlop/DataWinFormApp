using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DataWinFormApp
{
    public partial class Form1 : Form
    {

        protected readonly IPublicClientApplication _publicClientApplication;
        protected readonly IHttpClientFactory _httpClientFactory;
        protected readonly CosmosClient _cosmosClient;

        public string AccessToken { get; protected set; } = string.Empty;

        public Form1(IPublicClientApplication publicClientApplication,
            CosmosClient cosmosClient,
            IHttpClientFactory httpClientFactory)
        {
            _publicClientApplication = publicClientApplication;
            _cosmosClient = cosmosClient;
            _httpClientFactory = httpClientFactory;
            InitializeComponent();
        }

        protected void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.ShowDialog();
            Console.WriteLine(openFileDialog1.FileName);
        }

        protected async Task GetAccessTokenAsync()
        {
            try
            {
                string[] scopes = ["user.read"];
                //https://learn.microsoft.com/en-us/entra/msal/dotnet/acquiring-tokens/desktop-mobile/acquiring-tokens-interactively
                AuthenticationResult result = await _publicClientApplication.AcquireTokenInteractive(scopes).ExecuteAsync().ConfigureAwait(false);
                if (result.Account != null)
                {
                    Console.WriteLine($"Signed in {result.Account.Username}");
                }
                InvokeHelper(() =>
                {
                    AccessToken = result.AccessToken;
                    loginStripMenuItem1.Enabled = true;
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                InvokeHelper(() =>
                {
                    MessageBox.Show(ex.Message);
                });
            }
        }

        protected async Task CallApi()
        {

            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccessToken);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            using StringContent jsonContent = new(
                   System.Text.Json.JsonSerializer.Serialize(new
                   {
                       userId = 77,
                       id = 1,
                       title = "write code sample",
                       completed = false
                   }),
                   Encoding.UTF8,
                   "application/json");
            using HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                Version = HttpVersion.Version20,
                Method = HttpMethod.Get,
                RequestUri = new Uri("todos/1", UriKind.Relative),
                Content = jsonContent
            };
#if DEBUG
            Console.WriteLine(SynchronizationContext.Current?.GetHashCode());
#endif
            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(requestMessage).ConfigureAwait(false);

#if DEBUG
            Console.WriteLine(SynchronizationContext.Current?.GetHashCode());
#endif

            Console.WriteLine(httpResponseMessage.StatusCode);

            httpResponseMessage.EnsureSuccessStatusCode();

            if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
            {
                string response = await httpResponseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
#if DEBUG
                Console.WriteLine(SynchronizationContext.Current?.GetHashCode());
#endif
                Console.WriteLine(response);

                //string response = await httpResponseMessage.Content.ReadFromJsonAsync<string>().ConfigureAwait(false);
                //await foreach(var item in httpResponseMessage.Content.ReadFromJsonAsAsyncEnumerable<IEnumerable<dynamic>>())
                //{
                //    Console.WriteLine(item);
                //}


                //if (httpResponseMessage.Content.Headers.ContentType?.MediaType == "application/json")
                //{
                //    using Stream contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                //    contentStream.Position = 0;
                //    using var streamReader = new StreamReader(contentStream);
                //    using var jsonReader = new JsonTextReader(streamReader);

                //    Newtonsoft.Json.JsonSerializer serializer = new Newtonsoft.Json.JsonSerializer();

                //    try
                //    {
                //        serializer.Deserialize<User>(jsonReader);
                //    }
                //    catch (JsonReaderException)
                //    {
                //        Console.WriteLine("Invalid JSON.");
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
                //}

                //if (httpResponseMessage.Content.Headers.ContentType?.MediaType == "application/json")
                //{
                //    var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                //    contentStream.Position = 0;
                //    try
                //    {
                //        await JsonSerializer.DeserializeAsync<User>(contentStream, new System.Text.Json.JsonSerializerOptions { IgnoreNullValues = true, PropertyNameCaseInsensitive = true });
                //    }
                //    catch (JsonException) // Invalid JSON
                //    {
                //        Console.WriteLine("Invalid JSON.");
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("HTTP Response was invalid and cannot be deserialised.");
                //}

            }
        }

        protected void InvokeHelper(Action action)
        {
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        protected void loginMenuItem1_Click(object sender, EventArgs e)
        {
#if DEBUG
            Console.WriteLine(SynchronizationContext.Current?.GetHashCode());
#endif
            loginStripMenuItem1.Enabled = false;
            Console.WriteLine("Logging in background...");
            _ = GetAccessTokenAsync();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
