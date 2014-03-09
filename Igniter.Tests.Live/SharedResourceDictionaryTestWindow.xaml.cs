using System.Windows;

namespace Igniter.Tests.Live
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
