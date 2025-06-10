# Skyline.DataMiner.Utils.RadToolkit

## About

Toolkit to help you with sending message to DataMiner's [Relational Anomaly Detection (RAD)](https://aka.dataminer.services/RAD).

### About DataMiner

DataMiner is a transformational platform that provides vendor-independent control and monitoring of devices and services. Out of the box and by design, it addresses key challenges such as security, complexity, multi-cloud, and much more. It has a pronounced open architecture and powerful capabilities enabling users to evolve easily and continuously.

The foundation of DataMiner is its powerful and versatile data acquisition and control layer. With DataMiner, there are no restrictions to what data users can access. Data sources may reside on premises, in the cloud, or in a hybrid setup.

A unique catalog of 7000+ connectors already exists. In addition, you can leverage DataMiner Development Packages to build your own connectors (also known as "protocols" or "drivers").

> **Note**
> See also: [About DataMiner](https://aka.dataminer.services/about-dataminer).

### About Skyline Communications

At Skyline Communications, we deal in world-class solutions that are deployed by leading companies around the globe. Check out [our proven track record](https://aka.dataminer.services/about-skyline) and see how we make our customers' lives easier by empowering them to take their operations to the next level.

<!-- Uncomment below and add more info to provide more information about how to use this package. -->
## Getting Started

For use in a DataMiner automation script, you can use the following code snippet to create a RadHelper object:
```csharp
using Skyline.DataMiner.Automation;
using Skyline.DataMiner.Utils.RadToolkit;

var radHelper = new RadHelper(Engine.SLNetRaw, new Logger(s => engine.Log(s, LogType.Error, 0)));
```

For use in a GQI Ad-Hoc Data Source, you can use the following code snippet to create a RadHelper object:
```csharp
using Skyline.DataMiner.Analytics.GenericInterface;
using Skyline.DataMiner.GQI;

[GQIMetaData(Name = "Get Your Data")]
public class YourDataSource : IGQIDataSource, IGQIOnInit
{
	private RadHelper _radHelper;


	public OnInitOutputArgs OnInit(OnInitInputArgs input)
	{
		_radHelper = new RadHelper(args.DMS.GetConnection(), new Logger(s => args.Logger.Error(s)));
		
		return default;
	}
}
```

After creating the RadHelper object, you can then use its methods to fetch, create and update RAD parameter groups.