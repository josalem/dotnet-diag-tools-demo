# Run `dotnet-trace` demo

## Prerequisites

### Build demo app

The TraceApi demo app is an ASP.NET Core Web API with a collection of endpoints you can connect to.
Each endpoint will create different behavior in the runtime and fire different events.  In order
to ensure that data is sent correctly, you will need to run the demo app using a *sufficiently
current* version of CoreCLR.  The easiest way to do this, is to use the nightly builds of CoreSDK.

From the root of the demo app's source tree run the applicable snippet:

For Windows:
```powershell
Invoke-WebRequest 'https://dot.net/v1/dotnet-install.ps1' -OutFile 'dotnet-install.ps1';
./dotnet-install.ps1 -InstallDir '.dotnet-test' -Channel master -v '3.0.100-preview5-011250';
```

For Mac/Linux:
```bash
curl https://dot.net/v1/dotnet-install.sh > dotnet-install.sh && chmod +x dotnet-install.sh
source ./dotnet-install.sh -InstallDir '.dotnet-test' -Channel master -v '3.0.100-preview5-011250'
```

> specifying `-v '3.0.100-preview5-011250'` is a workaround for some breaking changes from AspNetCore;
> should be solved in upcoming builds.

Subsequently, you should be able to run the following to start the server:
```bash
dotnet run
```

### Get `dotnet-trace`

1) in another terminal run `git clone https://github.com/dotnet/diagnostics`
2) navigate to `.../diagnostics/src/Tools/dotnet-trace`
3) run `./build.cmd` on Windows or `./build.sh` on Mac/Linux

## Demo script

1) In one terminal (Terminal 1), run the demo app (`dotnet run` in the project directory of the demo app) and note down what port the app is listening for https traffic on.  Typically it is 5001, e.g., https://localhost:5001
2) In another terminal (Terminal 2), navigate to `.../diagnostics/src/Tools/dotnet-trace`
3) In the dotnet-trace directory, run `dotnet run --no-build --no-restore -- ports`.  This will display a list of all available ports for the tool to listen on.  Find the port of the dotnet that is running the `TraceApi` demo app and note down its process ID.
4) In the dotnet-trace directory, run `dotnet run --no-build --no-restore -- collect -pid <pid from step 3> --providers MyEventSource,Microsoft-DotNETCore-SampleProfiler`
5) In a _third_ terminal (Terminal 3), make calls to the following endpoints: `/api/trace/eventsource`, `/api/trace/eventsource/mymsg`, `/api/trace/gcstress`, `/api/trace/cpustress`.  Refer to the following snippets for examples of making the requests:

Mac/Linux:
```bash
curl https://localhost:5001/api/trace/eventsource -k
```

Windows (powershell):
```powershell
Invoke-WebRequest https://localhost:5001/api/trace/eventsource
```

> On Windows you may need to run `dotnet dev-certs https --trust` if you see certificate errors.

6) In terminal 2, hit `Enter` or `Ctrl-c` to stop the trace.  If things went well, you should see something like this:
```bash
> dotnet run --no-restore --no-build -- collect -pid 27108 --providers MyEventSource,Microsoft-DotNETCore-SampleProfiler
press <Enter> or <Ctrl-c> to exit...
Recording tracing session to: C:\git\diagnostics\src\Tools\dotnet-trace\eventpipe-20190415_165102.netperf
  Session Id: 0x00000280C2855990
  Recording trace 153.7628 (MB)
Trace completed.
```
> If you see a message about being unable to read past the end of the stream, then the version of CoreCLR that the SDK you pulled down earlier may not be new enough.  In that case, please follow the alternative instructions at the end of this document.

7) You should be able to open the created trace files in PerfView (install from https://github.com/Microsoft/PerfView)

# Alternative CoreCLR Instructions

## Build CoreCLR from source

1) run `git clone https://github.com/dotnet/coreclr`
2) run `cd coreclr`
3) run `./build` (note you will need all the dependencies to build CoreCLR)
4) When complete, note where the output was.  Typically, this is in `/path/to/coreclr/bin/Product/Windows_NT.x64.Debug`.  Open an explorer window here

## Publish the `TraceApi` demo app

1) from the root of the `TraceApi` demo app project, run `dotnet publish -r <rid> --self-contained`.  Appropriate rids are `win-x64`, `osx-x64`, and `linux-x64` depending on your platform.
2) Once complete, navigate to the published location.  Typically, this will be `./bin/Debug/netcoreapp3.0/<rid>/publish`.  Then open an explorer window here.

## Run the `TraceApi` demo app on your private CoreCLR

1) Copy _everything_ from the CoreCLR explorer window _into_ the publish output explorer window.  Choose to overwrite any duplicate files.
2) Navigate a terminal to the publish directory and run `CoreRun.exe TraceApi.dll`

You should be able to then use the original instructions for using dotnet-trace.