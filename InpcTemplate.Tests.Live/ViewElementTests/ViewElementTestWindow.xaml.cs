using System;
using System.Runtime.InteropServices;
using System.Windows;
using NorthHorizon.Samples.InpcTemplate.Composition;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live.ViewElementTests
{
    /// <summary>
    /// Interaction logic for ViewElementTestWindow.xaml
    /// </summary>
    [LiveTest("ViewElement")]
    public partial class ViewElementTestWindow : Window
    {
        public ViewElementTestWindow()
        {
            InitializeComponent();
            new ViewFactory(new ViewResolver()).Attach(this);
        }

        private class ViewResolver : IViewFactoryResolver
        {
            public object Resolve(Type type)
            {
                return Activator.CreateInstance(type);
            }

            public T Resolve<T>()
            {
                return Activator.CreateInstance<T>();
            }
        }
    }
}
