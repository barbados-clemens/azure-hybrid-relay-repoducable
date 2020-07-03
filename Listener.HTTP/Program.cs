using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Relay;
using Microsoft.Extensions.Configuration;

namespace Listener.HTTP
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

            RunAsync().GetAwaiter().GetResult();
        }

        private static async Task RunAsync()
        {
            var cts = new CancellationTokenSource();

            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(KeyName, Key);
            var listener =
                new HybridConnectionListener(new Uri($"sb://{RelayNamespace}/{ConnectionName}"), tokenProvider);

            // Subscribe to the status events.
            listener.Connecting += (o, e) => { Console.WriteLine("Connecting"); };
            listener.Offline += (o, e) => { Console.WriteLine("Offline"); };
            listener.Online += (o, e) => { Console.WriteLine("Online"); };

            // Provide an HTTP request handler
            listener.RequestHandler = (context) =>
            {
                // Do something with context.Request.Url, HttpMethod, Headers, InputStream...
                var q = context.Request.Headers["X-Query"].Replace("_", " ");
                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.StatusDescription = "OK, Here's what I could find";

                using var sw = new StreamWriter(context.Response.OutputStream);

                var queryResult = Search(q).GetAwaiter().GetResult();
                Console.WriteLine("Source json content length", queryResult.Length);
                // sw.WriteLine(j);
                sw.WriteLine("Source json content", queryResult);


                // The context MUST be closed here
                context.Response.Close();
            };

            // Opening the listener establishes the control channel to
            // the Azure Relay service. The control channel is continuously
            // maintained, and is reestablished when connectivity is disrupted.
            await listener.OpenAsync();
            Console.WriteLine("Server listening");

            // Start a new thread that will continuously read the console.
            await Console.In.ReadLineAsync();

            // Close the listener after you exit the processing loop.
            await listener.CloseAsync();
        }

        private static async Task<string> Search(string q)
        {

            var content = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sample.json"));

            return content;
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