using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
   [LiveTest("Shared Resource Dictionary")]
    public partial class SharedResourceDictionaryTestWindow : Window
    {
        public SharedResourceDictionaryTestWindow()
        {
            InitializeComponent();
        }
    }

    public class TestResourceDictionary : ResourceDictionary
    {
        public TestResourceDictionary()
        {
            MessageBox.Show("Constructed TestResourceDictionary");
        }
    }
}
