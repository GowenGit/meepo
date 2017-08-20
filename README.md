# <img src ="./icon.png" width="23px" /> Meepo

### Socket based duplex communication framework for .NET Core

Cross platform lightweight communication framework based on TCP Sockets. Provides basic
exception handling and automatic reconnects once the network is restored.

### Example

You can initialize a new node like this:

```
// IP Address to expose
var address = new TcpAddress(IPAddress.Loopback, 9201);

// Nodes to connect to
var serverAddresses = new[] { new TcpAddress(IPAddress.Loopback, 9200) };

using (var meepo = new Meepo(address, serverAddresses))
{
    meepo.Start();

    meepo.MessageReceived += x => System.Console.WriteLine(x.Bytes.Decode());

    while (true)
    {
        var text = System.Console.ReadLine();

        meepo.SendAsync(text).Wait();
    }
}
```

You can pass in a `MeepoConfig` object that lets you to change the behavior of the server:

```
var config = new MeepoConfig
{
    BufferSizeInBytes = 1000,
    Logger = new ConsoleLogger()
};

...

var meepo = new Meepo(address, serverAddresses, config);
```

### Run on Windows or Linux

* Restore solution: `dotnet restore`
* Run the console app: `dotnet run`