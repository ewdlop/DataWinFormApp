using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        private void querybutton1_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Querying...");
        }

        private void TextBoxes_TextChanged(object sender, EventArgs e)
        {
            // Check if both text boxes are not empty or null
            if (!string.IsNullOrWhiteSpace(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox2.Text))
            {
                Console.WriteLine("Both text boxes are not empty.");
                Console.WriteLine("Enabling query button.");
                querybutton1.Enabled = true;
            }
            else
            {
                querybutton1.Enabled = false;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            string json = """
            {
                'name': 'John',
                'age': 30,
                'children': [
                    {
                        'name': 'Anna',
                        'age': 10
                    },
                    {
                        'name': 'Peter',
                        'age': 5
                    }
                ]
            }
            """;
            JObject jsonObj = JObject.Parse(json);
            treeView1.Nodes.Clear();
            TreeNode rootNode = new TreeNode("JSON");
            treeView1.Nodes.Add(rootNode);
            AddJsonNodes(jsonObj, rootNode);
            treeView1.ExpandAll();

        }

        private void AddJsonNodes(JToken token, TreeNode parentNode)
        {
            if (token is JValue)
            {
                parentNode.Nodes.Add(new TreeNode(token.ToString()));
            }
            else if (token is JObject obj)
            {
                foreach (var property in obj.Properties())
                {
                    TreeNode node = new TreeNode(property.Name);
                    parentNode.Nodes.Add(node);
                    AddJsonNodes(property.Value, node);
                }
            }
            else if (token is JArray array)
            {
                for (int i = 0; i < array.Count; i++)
                {
                    TreeNode node = new TreeNode($"[{i}]");
                    parentNode.Nodes.Add(node);
                    AddJsonNodes(array[i], node);
                }
            }
        }
    }
}
