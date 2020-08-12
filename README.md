# BitMeterCollector

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