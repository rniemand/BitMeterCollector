# BitMeterCollector
Holder...

## Building
Holder.

Windows x64

    dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true

Windows x86:

    dotnet publish -r win-x86 -c Release /p:PublishSingleFile=true

## Windows Service Controls
Helper commands for running BitMeterCollector as a Windows service.

### Creating the service
In elevated PowerShell window:

    sc.exe create BitMeterCollector DisplayName="BitMeter Collector" binPath="C:\MyApps\BitMeterCollector\BitMeterCollector.exe" start=auto

### Starting and Stopping the service
To Start the service:

    sc.exe start BitMeterCollector

To Stop the service:

    sc.exe stop BitMeterCollector

### Uninstalling the service
To uninstall the service:

    sc.exe delete BitMeterCollector