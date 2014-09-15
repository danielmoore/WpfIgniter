using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

[assembly: AssemblyTitle("Igniter")]
[assembly: AssemblyDescription("A micro-library to provide support for easy view model creation and view/view model composition.")]
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("3f0053c0-da75-48a6-8bb4-65ccb4a77da3")]

[assembly: XmlnsPrefix("http://schemas.northhorizon.net/igniter", "ign")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter.Markup")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter.Behaviors")]
[assembly: XmlnsDefinition("http://schemas.northhorizon.net/igniter", "Igniter.Composition")]

[assembly: InternalsVisibleTo("Igniter.Tests")]