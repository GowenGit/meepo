# Meepo

### Socket based duplex communication framework for .NET Core

Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.

### Example

You can initialize a new node like this:

```
public static void Main()
{
    // IP Address to expose
    var address = new TcpAddress(IPAddress.Loopback, 9201);

    // Nodes to connect to
    var serverAddress = new[] { new TcpAddress(IPAddress.Loopback, 9200) };

    var meepo = new Meepo(address, serverAddress);

    meepo.Start();

    meepo.MessageReceived += (x) => System.Console.WriteLine(Encoding.UTF8.GetString(x.Bytes));

    while (true)
    {
        var text = System.Console.ReadLine();

        if (text.ToLower() == "q")
        {
            meepo.Stop();
            break;
        }

        var task = meepo.Send(Encoding.UTF8.GetBytes(text));

        task.Wait();
    }
}
```

### Run on Windows or Linux

* dotnet restore
* dotnet run