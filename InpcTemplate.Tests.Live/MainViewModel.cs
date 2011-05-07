using System;
using System.Collections;
using System.Linq;
using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
    public class MainViewModel
    {
        public MainViewModel()
        {
            Tests =
                typeof(MainViewModel)
                .Assembly
                .GetTypes()
                .Where(t => typeof(Window).IsAssignableFrom(t))
                .Where(t => Attribute.IsDefined(t, typeof(LiveTestAttribute)))
                .Select(t => new
                {
                    TestName = ((LiveTestAttribute)Attribute.GetCustomAttribute(t, typeof(LiveTestAttribute))).Name,
                    RunCommand = new DelegateCommand(() => ((Window)Activator.CreateInstance(t)).Show())
                });
        }

        public IEnumerable Tests { get; private set; }
    }
}
