using Azure.Core;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;

namespace DataWinFormApp;

public class APIService(IHttpClientFactory httpClientFactory) : IAPIService
{
    public async Task CallApi(string accessToken)
    {

        //need to grab named from the services.AddHttpClient("named") in Program.cs
        //https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#named-clients
        //should move "named" to a constant
        HttpClient httpClient = httpClientFactory.CreateClient("named");
        httpClient.BaseAddress = new Uri("https://jsonplaceholder.typicode.com");
        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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
}