# TCP Benchmarks
fast tcp communication benchmarks tool
## Run Environment
Dotnet Core 2.1 or later
### Install dotnet core

[https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
### Run in windows
`dotnet TCPBenchmarks.dll`
or
`run.bat`

### Run in linux
`dotnet TCPBenchmarks.dll`
or
`./webapibenchmark.sh`
### Open Benchmarks tool
Webbrowser enter `http://[host:9090]`
### change listen port
edit `HttpConfig.json`
```
    "Host": "",
    "Port": 9090,
```
## 100 connections
![](https://github.com/IKende/TCPBenchmarks/blob/master/100c.png?raw=true)
## 200k connections
![](https://github.com/IKende/TCPBenchmarks/blob/master/200kc.png?raw=true)
## 1m connectinos
![](https://github.com/IKende/TCPBenchmarks/blob/master/beetlex_1mc.png?raw=true)
