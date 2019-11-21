# Minimal example of a StackOverflowException in an OPC UA Server

This example application reproduces a StackOverflowException in an OPC UA Server that is given many asynchronous requests from the same client session.

## Running only the server

```
dotnet run -c Debug -p OpcUaStackOverflowExample.csproj -- server-only
```

Expected output:

```
Server started
Stack overflow.
```

## Running only the client

```
dotnet run -c Debug -p OpcUaStackOverflowExample.csproj -- client-only
```

Expected output:

```
Client connected
Received root folders
Unhandled exception. Opc.Ua.ServiceResultException: BadSecureChannelClosed
   at Opc.Ua.Bindings.ChannelAsyncOperation`1.End(Int32 timeout, Boolean throwOnError)
   at Opc.Ua.Bindings.UaSCUaBinaryClientChannel.EndSendRequest(IAsyncResult result)
   at Opc.Ua.SessionClient.EndBrowse(IAsyncResult result, BrowseResultCollection& results, DiagnosticInfoCollection& diagnosticInfos)
   at Opc.Ua.Client.Session.EndBrowse(IAsyncResult result, Byte[]& continuationPoint, ReferenceDescriptionCollection& references)
   at OpcUaStackOverflowExample.Program.<>c__DisplayClass4_0.<BrowseAsync>b__1(IAsyncResult result) in C:\Ontwikkel\OpcUaStackOverflowExample\OpcUaStackOverflowExample\Program.cs:line 137
   at System.Threading.Tasks.TaskFactory`1.FromAsyncCoreLogic(IAsyncResult iar, Func`2 endFunction, Action`1 endAction, Task`1 promise, Boolean requiresSynchronization)
--- End of stack trace from previous location where exception was thrown ---
   at OpcUaStackOverflowExample.Program.RunClient(String address) in C:\Ontwikkel\OpcUaStackOverflowExample\OpcUaStackOverflowExample\Program.cs:line 97
   at OpcUaStackOverflowExample.Program.Main(String[] args) in C:\Ontwikkel\OpcUaStackOverflowExample\OpcUaStackOverflowExample\Program.cs:line 39
   at OpcUaStackOverflowExample.Program.<Main>(String[] args)
```

## Run both client and server

```
dotnet run -c Debug -p OpcUaStackOverflowExample.csproj
```

Expected output:

```
Server started
Client connected
Received root folders
Stack overflow.
```

## Running in release mode

To run in release mode change `Debug` from the previous commands into `Release`.