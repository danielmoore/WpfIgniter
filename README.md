# WPF Igniter

A micro-library (not a framework!) that accelerates UI development with high-quality base components that leverage WPF's internals and stay out of your way.

## Installation

From the Nuget package manager console:

    Install-Package WpfIgniter

## Features

### View Modeling

#### BindableBase

`BindableBase` provides a simple interface on which to build view models. It implements `INotifyPropertyChanged` and `INotifyPropertyChanging` for maximum flexibility. The primary interaction with `BindableBase` is through its `SetProperty` protected method. `SetProperty` can be used to create bindable properties:

```csharp
public class MyViewModel : BindableBase {
    private string _myProperty;
    public string MyProperty {
        get { return _myProperty; }
        set { SetProperty(ref _myProperty, value); }
    }

    // etc
}
```

`SetProperty` automatically filters out cases where a property is set but its value is not changed. It also optionally accepts `onChanged` and `onChanging` handlers to help keep you property setters clean. Finally, you can provide a coercion handler to change incoming values before they're set.

#### Commands

Most modern WPF applications make use of Prism's `DelegateCommand` or a similar class that wraps an `Action` or a `Action<object>` in an `ICommand` interface. One of the major drawbacks of these classes is that they rely on `CommandManager.RequerySuggested`, which [admits][invalidaterequerysuggested]:

>The CommandManager only pays attention to certain conditions in determining when the command target has changed, such as change in keyboard focus. In situations where the CommandManager does not sufficiently determine a change in conditions that cause a command to not be able to execute, InvalidateRequerySuggested can be called to force the CommandManager to raise the RequerySuggested event.

So, if your command's `OnCanExecute` is dependent on, say, *changes in your view model*, your command will remain enabled or disabled *until a UI event occurs* or you call a non-UI static method in your view model, breaking the MVVM pattern.

Igniter solves this problem in two ways, depending on your needs.

[invalidaterequerysuggested]: http://msdn.microsoft.com/en-us/library/system.windows.input.commandmanager.invalidaterequerysuggested(v=vs.110).aspx

###### Expression Command

An `ExpressionCommand` is set up like any other command. 

```csharp
public class MyViewModel : BindableBase {
    public MyViewModel() {
        AddCommand = new ExpressionCommand<int>(DoSomething, 
            cmdParam => cmdParam > 0 && Value > 0);
    }

    public ICommand AddCommand { get; private set; }

    private void DoSomething(int cmdParameter) {
        // ...
    }

    private int _value;
    public int Value {
        get { return _value; }
        set { SetProperty(ref _value, value); }
    }
}
```

In this example, `AddCommand` will only be enabled if all of the following are true:

- the associated `CommandParameter` is convertible to an `int` (the string "123" is convertible, for instance)
- the value of the converted `CommandParameter` is greater than 0
- the value of `Value` is greater than 0

Additionally, any time the `CommandParameter` changes or `MyViewModel` emits a `PropertyChanged` event for `Value`, the `CanExecute` expression will be evaluated.

Even more usefully, `CanExecute` expressions may be composed of any valid C# expression and will automatically update if it finds:

- `INotifyPropertyChanged`
    + notified properties
- `DependencyObject`
    + dependency properties
- `INotifyCollectionChanged` and `IBindingList`
    + indexer
    + methods

This essentially accomplishes the thing you probably expected Prism's `DelegateCommand` to do in the first place.

###### Delegate Command

Like its Prism spiritual ancestor, `Igniter.DelegateCommand` takes an `Action` or an `Action<T>` for commands that accept a `T` command parameter. It also optionally takes a `bool canExecute` parameter at construction to determine if it's available to execute initially. Later on down the line, this can be mutated by calling `SetCanExecute` and thus triggering watchers.

The `DelegateCommand` has two use cases that differentiate it from `ExpressionCommand`:

1. A `DelegateCommand` may always be allowed to execute, so it is simpler to setup than an `ExpressionCommand`, which requires an expression describing when the command may execute.
2. If the lifetime of the view model is short or if the view model is one of many instances, an `ExpressionCommand` may be too heavy for use due to its expression analysis. In this case, you may simply need to take matters into your own hands.

For example,

```csharp
public class MyViewModel {
    public MyViewModel() {
        FooCommand = new DelegateCommand(
            () => _barCommand.SetCanExecute(true));

        _barCommand = new DelegateCommand(
            () => _barCommand.SetCanExecute(false),
            canExecute: false); // canExecute defaults to true
    }

    public ICommand FooCommand { get; private set; }

    private readonly DelegateCommand _barCommand;
    public ICommand BarCommand { get { return _barCommand; } }
}
```

#### Change Subscription

Subscribing to changes in WPF is strangely very error prone. If you subscribe to a `INotifyPropertyChanged.PropertyChanged` event, you can only tell which property changed by inspecting the `PropertyName` string - a big problem if you refactor your code. Alternatively, if you subscribe to a `DependencyObject`'s `DependencyProperty` using `PropertyDescriptor.AddValueChanged`, you can create a [memory leak](http://support.microsoft.com/kb/938416)!

Igniter solves both of these problems with a couple extension methods:

```csharp
INotifyPropertyChanged myViewModel = // ...
IDisposable subscription = myViewModel
    .SubscribeToPropertyChanged(vm => vm.MyProperty, OnMyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

```csharp
MyDependencyObject myDepObj = // ...
IDisposable subscription = myDepObj.SubscribeToDependencyPropertyChanges(
    myDepObj, MyDependencyObject.MyDependencyProperty, 
    OnMyDependencyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

### View Composition

One of the most important aspects of a modern WPF app is its ability to compose views out of smaller views. In Prism, this is accomplished with magic strings and a great amount of complexity in the [`IRegionManager`](http://msdn.microsoft.com/en-us/library/ff921076(v=pandp.20).aspx).

Even the most complex of application can be broken down much more simply and more (type-)safely.

Predominately, developers simply want to stick a view/view model pair in a control in a parent view. Igniter accomplishes this with the simplicity you'd expect:

```xml
<UserControl xmlns:ign="http://schemas.northhorizon.net/igniter">
    <StackPanel>
        <ign:ViewElement ViewType="local:MyFirstView"
                         ViewModelType="local:MyFirstViewModel"/>

        <ign:ViewElement ViewType="local:MySecondView"
                         ViewModelType="local:MySecondViewModel"/>
    </StackPanel>
</UserControl>
```

By defailt, `ViewElement` will activate (i.e., instantiate via `Activator.CreateInstance`) the given view type and resolve the given view model type via your IoC container through the `IViewFactoryResolver` shim provided to the attached `ViewFactory`'s constructor.

#### Root View Model

In many casses, your logical tree may reflect your view model object structure and so often times it is prudent to continue to adjust your data context to follow suit. Additionally, when using controls like `ItemsControl` your data context necessarily changes to track an item in the list. In both of these cases, it's often useful to be able to get back to your root view model to access a command or a property. To accomplish this, developers typically use `{RelativeSource FindAncestor}` to refer to that view model. Aside from the bulkiness associated with this syntax, using a relative source can cause a significant performance penalty when used too often. 

To work around these problems, Igniter has the `RootViewModelBindingExtension` that will be automatically sourced to the view model bound to the view:

```xml
<StackPanel>
    <TextBlock Text="{Binding MyProperty}"/>
    <Border DataContext="{x:Null}">
        <TextBlock Text="{ign:RootViewModelBinding MyProperty}"/>
    </Border>
</StackPanel>
```

### XAML Resource Management

#### Shared Resources

An unseen peril of WPF is that if you source a `ResourceDictionary` in more than one place, all of its definitions are instantiated *once per reference*. For more complex apps with many resources this can be a disaster for memory usage.

Using Igniter, you can easily share resources by simply using the `SharedResourceDictionaryExtension` wherever you would normally use a `<ResourceDictionary Source="..."/>`:

```xml
<UserControl xmlns:ign="http://schemas.northhorizon.net/igniter">
    <UserControl.Resources>
        <ResourceDictionary>
            <ResourceDictionary.MergedDictionaries>
                <ign:SharedResourceDictionary Source="../path/to/resources.xaml"/>
            </ResourceDictionary.MergedDictionaries>
        </ResourceDictionary>
    </UserControl.Resources>

    <Border Background="{StaticResource MyBackgroundResource}"/>
</UserControl>
```

#### Directory Resources

Some apps with many resources choose to break up those resources into separate files for better organization. The major downside to this approach is that they typically end up with "index.xaml" files that simply list the contents of the folder.

This can be solved more elegantly with `DirectoryResourceDictionaryExtension`. Like `SharedResourceDictionaryExtension`, it may be used anywhere you would normally use a `<ResourceDictionary Source="..."/>`.