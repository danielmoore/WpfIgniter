using System;
using System.Windows.Input;

namespace Igniter
{
    public interface IDisposableCommand : ICommand, IDisposable { }
}