using System;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class LiveTestAttribute : Attribute
    {
        public LiveTestAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; private set; }
    }
}
