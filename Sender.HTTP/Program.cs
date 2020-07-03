using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Relay;
using Shared;
using Microsoft.Extensions.Configuration;

namespace Sender.HTTP
{
    class Program
    {
        // replace {RelayNamespace} with the name of your namespace
        private static string RelayNamespace;

        // replace {HybridConnectionName} with the name of your hybrid connection
        private static string ConnectionName;

        // replace {SAKKeyName} with the name of your Shared Access Policies key, which is RootManageSharedAccessKey by default
        private static string KeyName;

        // replace {SASKey} with the primary key of the namespace you saved earlier
        private static string Key;


        static void Main(string[] args)
        {
            SetUp();
            var jsonContent = Send("doesn't matter").GetAwaiter().GetResult();
            Console.WriteLine(jsonContent);

            Console.WriteLine($"Sender json content length {jsonContent.Length}", jsonContent);

            var parsed = SearchResult<SearchSchema>.Parse(jsonContent);

            Console.WriteLine("Parsed", parsed);
        }

        private static async Task<string> Send(string q)
        {
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(
                KeyName, Key);
            var uri = new Uri($"https://{RelayNamespace}/{ConnectionName}");
            var token = (await tokenProvider.GetTokenAsync(uri.AbsoluteUri, TimeSpan.FromHours(1))).TokenString;
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("ServiceBusAuthorization", token);
            client.DefaultRequestHeaders.Add("X-Query", q);
            var res = await client.GetAsync(uri);
            res.EnsureSuccessStatusCode();
            var body = await res.Content.ReadAsStringAsync();
            return body;
        }

        private static void SetUp()
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile($"appsettings.json", false, true);
            var config = builder.Build();
            var relaySettings = config.GetSection("Relay")
                .GetChildren()
                .ToDictionary(s => s.Key, s => s.Value);

            RelayNamespace = relaySettings["RelayNamespace"];
            ConnectionName = relaySettings["ConnectionName"];
            KeyName = relaySettings["KeyName"];
            Key = relaySettings["Key"];

        }
    }
}