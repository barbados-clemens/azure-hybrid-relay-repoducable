# Minimum reproducible repo for json truncation issues with Azure Hybrid HTTP relay

# FIX Don't use the [new C# 8.0 using statements](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-8#using-declarations)

i.e. 
```csharp
using (var sw = new StreamWriter(context.Response.OutputStream))
{ // <= gotta use these bad boys
    var queryResult = Search(q).GetAwaiter().GetResult();
    sw.WriteLine("{0}", queryResult);
}
```

# Steps to reproduce
1. Create a Hybrid Relay connection in the Azure portal
1. Rename `appsettings.example.json` to `appsettings.json` in the Listener.HTTP and Sender.HTTP projects
1. Fill in the info in the `Relay` object block with your relay settings
1. Run the listener project
1. Run the sender project
1. View console and that the JSON content is empty and if you increase the JSON content you'll see the truncation of the content.

For issue reported here https://github.com/MicrosoftDocs/azure-docs/issues/58347

