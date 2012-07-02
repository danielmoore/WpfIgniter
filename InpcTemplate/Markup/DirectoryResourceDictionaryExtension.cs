using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Navigation;

namespace NorthHorizon.Samples.InpcTemplate.Markup
{
    public class DirectoryResourceDictionaryExtension : MarkupExtension
    {
        public DirectoryResourceDictionaryExtension()
        {
            IsSubdirectoriesIncluded = true;
        }

        public Uri Directory { get; set; }

        public bool IsSubdirectoriesIncluded { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var partUri = ResolveDirectoryUri(serviceProvider);

            var resourceMgr = GetResourceManager(partUri);

            var assemblyPath = partUri.OriginalString.Substring(0, partUri.OriginalString.IndexOf('/', 1));

            var resources = new ResourceDictionary();

            using (var set = resourceMgr.GetResourceSet(CultureInfo.CurrentCulture, true, true))
            {
                foreach (var item in set.Cast<DictionaryEntry>())
                {
                    if (Path.GetExtension((string)item.Key) != ".baml") continue;

                    var resourceUri = string.Format("{0}/{1}", assemblyPath, item.Key);

                    if (resourceUri.StartsWith(partUri.OriginalString, StringComparison.OrdinalIgnoreCase) &&
                        (IsSubdirectoriesIncluded || resourceUri.IndexOf('/', partUri.OriginalString.Length) < 0))
                        resources.MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(Path.ChangeExtension(resourceUri, ".xaml"), UriKind.Relative) });
                }
            }

            return resources;
        }

        private Uri ResolveDirectoryUri(IServiceProvider serviceProvider)
        {
            if (Directory == null || Directory.OriginalString == ".")
            {
                var baseUri = serviceProvider.GetService<IUriContext>().BaseUri;
                var directoryUri = new Uri(baseUri, Path.GetDirectoryName(baseUri.AbsolutePath) + '/');

                return PackUriHelper.GetPartUri(directoryUri);
            }

            var directory = Directory.OriginalString.EndsWith("/") ? Directory : new Uri(Directory.OriginalString + '/', UriKind.RelativeOrAbsolute);
            return serviceProvider.GetService<IUriContext>().ResolvePartUri(directory);
        }

        private static ResourceManager GetResourceManager(Uri partUri)
        {
            string partName, assemblyName, assemblyVersion, assemblyKey;

            GetAssemblyNameAndPart(partUri, out partName, out assemblyName, out assemblyVersion, out assemblyKey);

            var asm = GetAssembly(assemblyName, assemblyVersion, assemblyKey);

            return asm != null ? new ResourceManager(string.Format("{0}.g", assemblyName), asm) : null;
        }

        private static Assembly GetAssembly(string assemblyName, string assemblyVersion, string assemblyKey)
        {
            var asmNameBuilder = new StringBuilder(assemblyName);

            if (!string.IsNullOrEmpty(assemblyVersion))
                asmNameBuilder.AppendFormat(", Version={0}", assemblyVersion);

            if (!string.IsNullOrEmpty(assemblyKey))
                asmNameBuilder.AppendFormat(", PublicKeyToken={0}", assemblyKey);

            var asmNameRef = new AssemblyName(asmNameBuilder.ToString());

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                if (AssemblyName.ReferenceMatchesDefinition(asmNameRef, asm.GetName()))
                    return asm;

            return null;
        }

        private static readonly MethodInfo GetAssemblyNameAndPartMethodInfo = typeof(BaseUriHelper).GetMethod("GetAssemblyNameAndPart", BindingFlags.NonPublic | BindingFlags.Static);
        private static void GetAssemblyNameAndPart(Uri uri, out string partName, out string assemblyName, out string assemblyVersion, out string assemblyKey)
        {
            var args = new object[] { uri, null, null, null, null };
            GetAssemblyNameAndPartMethodInfo.Invoke(null, args);

            partName = (string)args[1];
            assemblyName = (string)args[2];
            assemblyVersion = (string)args[3];
            assemblyKey = (string)args[4];
        }
    }
}
