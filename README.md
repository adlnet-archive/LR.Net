# LR.Net

LR.Net is a client library for the [Learning Registry](http://github.com/LearningRegistry/LearningRegistry.git) project. It is written in C# and provides basic access to a Learning Registry node. Currently, `obtain`, `publish`, and `harvest` are supported, with plans to implement `slice` in the near future.

This library is still in active development. If you encounter a bug, please file an issue to let us
know what needs to be fixed.

## Supported Platforms

This library has been successfully compiled for use with the .Net Framework 4.0 and Mono 2.10.8. It works under 
**Windows XP/Vista/7** and **OS X**, and should also run under linux as well (currently untested).

### Note

The library currently has a dependency on System.Web.Extensions, so if you are using .Net 4.0, you must 
install the full .Net framework, not just the Client Profile (this mostly affects the deployment; Visual Studio
and MonoDevelop users will have no problem during development).

## Services

### Currently Supported:
- Basic Publish
- Basic Obtain
- Basic Harvest

HTTP Basic authentication is supported with each of these services. SSL authentication is planned to be implemented
in a future release.

### Planned Support
- Slice

## Usage 
Please see the [wiki](LR.Net/wiki) for examples of how to publish and consume data.
