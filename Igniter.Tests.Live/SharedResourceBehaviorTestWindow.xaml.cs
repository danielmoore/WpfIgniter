using System.Windows;

namespace Igniter.Tests.Live
{
   [LiveTest("Shared Resource Dictionary")]
    public partial class SharedResourceBehaviorTestWindow : Window
    {
        public SharedResourceBehaviorTestWindow()
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
