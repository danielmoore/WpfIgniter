using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Igniter")]
[assembly: AssemblyDescription("A micro-library to provide support for easy view model creation and view/view model composition.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("North Horizon")]
[assembly: AssemblyProduct("Igniter")]
[assembly: AssemblyCopyright("Copyright © Daniel Moore 2011-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3f0053c0-da75-48a6-8bb4-65ccb4a77da3")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.1")]

[assembly: XmlnsPrefix("http://schemas.northhorizon.net/samples/inpctemplate", "inpc")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/samples/inpctemplate", "NorthHorizon.Samples.InpcTemplate")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/samples/inpctemplate", "NorthHorizon.Samples.InpcTemplate.Markup")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/samples/inpctemplate", "NorthHorizon.Samples.InpcTemplate.Composition")]

[assembly: XmlnsPrefix("http://schemas.northhorizon.net/igniter", "ign")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter.Markup")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter.Composition")]