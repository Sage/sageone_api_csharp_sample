# Sage Business Cloud Accounting API Sample application (C#) [![Travis Status](https://travis-ci.org/Sage/sageone_api_csharp_sample.svg?branch=master)](https://travis-ci.org/github/Sage/sageone_api_csharp_sample)

Sample C# project that integrates with Sage Accounting via the Sage Accounting API. This Application uses .NET Core 2.2 and [Newtonsoft Json.NET](https://github.com/JamesNK/Newtonsoft.Json).

* Authentication and API calls are handled in [app/Startup.cs](app/Startup.cs)

## Setup

Clone the repo:

`git clone git@github.com:Sage/sageone_api_csharp_sample.git`

Switch to the project directory to run the subsequent commands:

```
cd sageone_api_csharp_sample
```

## Run the app locally

switch to the app folder:

```
cd app
```

install all dependencies:

```
dotnet restore
```

build and run the application:

```
dotnet build
dotnet run
```

Then jump to the section [Usage](#Usage).

## Run the app in Docker

Build the image:

```
./script/setup.sh
```

Start the container:

```
./script/start.sh
```

Restart the container:

```
./script/restart.sh
```

If you need, stop.sh will stop the container:

```
./script/stop.sh
```

## Usage

You can now access [http://localhost:8080/](http://localhost:8080/), authorize and make an API call. Depending on your setup, it could also be [http://192.168.99.100:8080/](http://192.168.99.100:8080/) or similar.

## License

This sample application is available as open source under the terms of the
[MIT licence](LICENSE).

Copyright (c) 2019 Sage Group Plc. All rights reserved.
