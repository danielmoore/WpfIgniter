using System.Windows;

namespace Igniter.Tests.Live
{
    [LiveTest("Command Parameter")]
    public partial class CommandParameterTestWindow : Window
    {
        public CommandParameterTestWindow()
        {
            DataContext = new CommandParameterTestViewModel();

            InitializeComponent();
        }
    }
}
