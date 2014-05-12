using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Igniter.Controls;

namespace Igniter.Tests.Live
{
    /// <summary>
    /// Interaction logic for SwitchControlTestWindow.xaml
    /// </summary>
    [LiveTest("SwitchControl")]
    public partial class SwitchControlTestWindow : Window
    {
        public SwitchControlTestWindow()
        {
            InitializeComponent();
        }
    }

    public enum TestEnum
    {
        First, Second, Third, Fourth
    }
}
