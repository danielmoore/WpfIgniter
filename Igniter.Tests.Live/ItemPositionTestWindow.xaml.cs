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

namespace Igniter.Tests.Live
{
    /// <summary>
    /// Interaction logic for ItemPositionTestWindow.xaml
    /// </summary>
    [LiveTest("ItemPositionBehavior")]
    public partial class ItemPositionTestWindow : Window
    {
        public ItemPositionTestWindow()
        {
            DataContext = new ItemPositionViewModel();

            InitializeComponent();
        }
    }
}
