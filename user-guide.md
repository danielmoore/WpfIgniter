# Igniter Namespace

All Igniter namespaces are available in the `ign:` XML namespace (http://schemas.northhorizon.net/igniter) for convenience.

## BindableBase

`BindableBase` provides a simple base class on which to build view models that update views when properties change.

### SetProperty

Predominately, implementers will be interested in using the `SetProperty` protected method which, given a new value and a `ref`'d current field, applies coercion, calls events as necessary, and sets the backing field. `SetProperty` makes use of the `CallerMemberNameAttribute`, so user code does not have to provide the name of the property as a string or lambda.

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

#### Property Change Delegates

If a new, coerced value provided to `SetProperty` has been evaluated to be different than the old value by `EqualityComparer<T>.Default`, `SetProperty` will call its provided delegates before calling the common event methods. 

```csharp
public class MyViewModel : BindableBase {
    private string _myProperty;
    public string MyProperty {
        get { return _myProperty; }
        set { SetProperty(ref _myProperty, value); }
    }

    private void OnMyPropertyChanging(string newValue) {
        // This will be called first.
    }

    protected override void OnPropertyChanging(string propertyName) {
        // This will be called second.

        // calling base will cause the PropertyChanging event to be raised.
        base.OnPropertyChanging(propertyName);
    }

    private void OnMyPropertyChanged() {
        // This will be called third.
        // NOTE: the value of _myProperty now represents the new value!
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs args) {
        // This will be called last.
        // PropertyChangedEventArgs will actually be a PropertyChangedEventArgs<T>

        // calling base will cause the PropertyChanged event to be raised.
        base.OnPropertyChanging(propertyName);
    }
}
```

#### Coercion

Before a new value is evaluated for equality versus an old value, a coercion delegate may be applied. 

```csharp
public class MyViewModel : BindableBase {
    private string _myProperty;
    public string MyProperty {
        get { return _myProperty; }
        set { SetProperty(ref _myProperty, value, coerceValue: CoerceMyProperty); }
    }

    private string CoerceMyProperty(string newValue) {
        if (string.IsNullOrEmpty(newValue))
            return _myProperty;

        if (newValue.Length > 5)
            return newValue.Substring(0, 5);

        return newValue;
    }
}
```

The coercion delegate may return

- the previous value
- the new value
- a different value altogether

In the case where the values do not change (again, according to `EqualityComparer<T>.Default`), `SetProperty` will [notify the UI directly][coercion-blog] (but does not raise `PropertyChanging` or `PropertyChanged`) to refresh its value so it will be in sync.

[coercion-blog]: http://northhorizon.net/2011/coercing-viewmodel-values-with-inotifypropertychanged/

### Event Methods

Other than `SetProperty`, `BindableBase` has a number of convenience methods for raising events and a couple "core" event methods. The source code itself is simple enough to suffice for documentation:

```csharp
protected void OnPropertyChanged(string propertyName) {
    OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
}

protected void OnPropertyChanged<T>(string propertyName, T oldValue, T newValue) {
    OnPropertyChanged(new PropertyChangedEventArgs<T>(propertyName, oldValue, newValue));
}

protected virtual void OnPropertyChanged(PropertyChangedEventArgs args) {
    PropertyChanged(this, args);
}

protected virtual void OnPropertyChanging(string propertyName) {
    PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
}
```

## DelegateCommand

`DelegateCommand` provides a simple wrapper for an arbitrary delegate into an `ICommand` interface.

```csharp
public class MyViewModel {
    public MyViewModel() {
        FooCommand = new DelegateCommand(DoFoo);
        BarCommand = new DelegateCommand<int>(DoBar);
    }

    public ICommand FooCommand { get; private set; }
    public ICommand BarCommand { get; private set; }

    private void DoFoo() {
        // code here
    }

    private void DoBar(int cmdParameter) {
        // code here
    }
}
```

A `DelegateCommand<T>` provides the additional ability to accept a `CommandParameter`. If the type of the command parameter implements `IConvertible` (as all of the primitive types do), `DelegateCommand<T>` will attempt to convert them to the desired type. This can be useful in circumstances where a command parameter comes from user input or is hard-coded in XAML (and thus a string).

If the provided command parameter is not of the desired type and cannot be converted, the `DelegateCommand<T>` will evaluate its `CanExecute` to `false`.

### Controlling Executability

By default, `DelegateCommand` allows execution, notwithstanding when the command parameter is incompatible. However, user code may control the executability of `DelegateCommand` by calling `SetCanExecute`. Additionally, the initial state of executability may be overridden by providing the optional parameter `canExecute` to the constructor of the `DelegateCommand`.

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

`DelegateCommand`s do not accept a delegate for the `canExecute` argument as it would lead the user to [believe the command would update][commands-blog-post] when the lambda changes values. To accomplish this, use an [`ExpressionCommand`][] instead.

[commands-blog-post]: http://northhorizon.net/2012/better-commands-in-wpf/

## ExpressionCommand

`ExpressionCommand` allows you to use the familiar `DelegateCommand` instantiation API, but infers when your `canExecute` has changed.

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

`CanExecute` expressions support monitoring updates from members of multiple source types, based on events:

|        Source Type         |                Member                |                Triggering Event                |
|----------------------------|--------------------------------------|------------------------------------------------|
| `INotifyPropertyChanged`   | Properties, Fields                   | `PropertyChanged` for referenced member        |
| `DependencyObject`         | Dependency Properties                | Dependency Property Changed<sup>&dagger;</sup> |
| `INotifyCollectionChanged` | Indexer, Methods <sup>&Dagger;</sup> | `CollectionChanged`                            |
| `IBindingList`             | Indexer, Methods <sup>&Dagger;</sup> | `ListChanged` for adds, deletes, and resets    |

<p class="footnote">
&dagger; Dependency property changes are monitored using [`SubscribeToDependencyPropertyChanges`][]<br/>
&Dagger; Property changes should be managed by `INotifyPropertyChanged`.
</p>

## Extension Methods

`Extensions` is a static class that contains extension methods for miscellaneous gaps in WPF.

`Extensions.GetService<T>` provides a more concise syntax for working with `IServiceProvider`:

```csharp
public class MyExtension : MarkupExension {
    public override object ProvideValue(IServiceProvider serviceProvider) {
        IUriContext uriContext = serviceProvider.GetService<IUriContext>();
        // ...
    }
}
```

<div class="clear-both"></div>

`Extensions.ResolvePartUri` is a wrapper for `PackUriHelper.ResolvePartUri` but uses an `IUriContext` as the base path and an arbitrary URI to resolve, relative to that URI context. If the given URI is absolute, then that URI will be returned as a part URI.

```csharp
// In File: pack://application:,,,/Igniter.Tests.Live;component/test.xaml

uriContext.ResolvePartUri(new Uri("foo.xaml", UriKind.Relative))
// -> /Igniter.Tests.Live;component/foo.xaml
```

### INotifyPropertyChanged

`NotifyPropertyChangedExtensions` is a static class with extension methods for `INotifyProeprtyChanged` allowing user code to subscribe to changes in a type-safe, refactorable manner.

```csharp
INotifyPropertyChanged myViewModel = // ...
IDisposable subscription = myViewModel
    .SubscribeToPropertyChanged(vm => vm.MyProperty, OnMyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

<div class="clear-both"></div>

A stream of property changes can easily be obtained using the `GetPropertyChanges` extension method, returning an `IObservable<T>`.

```csharp
INotifyPropertyChanged myViewModel = // ...
IDisposable subscription = myViewModel
    .GetPropertyChanges(vm => vm.MyProperty)
    .Subscribe(OnMyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

If the underlying implementation of `INotifyPropertyChanged` raises `PropertyChangedEventArgs<T>` (as `BindableBase` does), instead of compiling the given lambda expression, `GetPropertyChanges` will simply obtain the new values from the event arguments.

#### INotifyPropertyChanging

There is also a parallel subscription method for `INotifyPropertyChanging`.

```csharp
INotifyPropertyChanging myViewModel = // ...
IDisposable subscription = myViewModel
    .SubscribeToPropertyChanges(vm => vm.MyProperty, OnMyPropertyChanging);

// later, to unsubscribe:
subscription.Dispose();
```

### DependencyObject

<a name="SubscribeToDependencyPropertyChanges"></a>

`DependencyObjectExtensions.SubscribeToDependencyPropertyChanges` is an extension method that allows user code to subscribe to the changes of a `DependencyProperty` without creating a [memory leak](http://support.microsoft.com/kb/938416).

```csharp
MyDependencyObject myDepObj = // ...
IDisposable subscription = myDepObj.SubscribeToDependencyPropertyChanges(
    myDepObj, MyDependencyObject.MyDependencyProperty, 
    OnMyDependencyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

This is accomplished by using an attached behavior bound to the desired property to proxy value changes. The proxy maintains a hard reference to subscribing delegates, but, once the target `DependencyObject` is released from memory, the proxies may also be garbage collected.

<div class="clear-all"></div>

As a convenience, there is also a `GetDependencyPropertyChanges` which returns an `IObservable<object>` for monitoring changes.

```csharp
MyDependencyObject myDepObj = // ...
IDisposable subscription = myDepObj
    .GetDependencyPropertyChanges(myDepObj, MyDependencyObject.MyDependencyProperty)
    .Subscribe(OnMyDependencyPropertyChanged);

// later, to unsubscribe:
subscription.Dispose();
```

# Igniter.Composition Namespace

## ViewFactory

`ViewFactory` can create and bind views and view models based on specified strategies. It implements the `IViewFactory` interface for dependency injection into view models and relies on the `IViewFactoryResolver` to shim the IoC container of the library user's choice.

### Configuration and Setup

`ViewFactory` is meant to be part of your dependency injection environment and is agnostic to what library you are using in your application. To use it, 

1. register an implementation of `IViewFactoryResolver` wrapping a resolution object from your dependency injection framework.
2. register `ViewFactory` as the implementation of `IViewFactory` at whatever level and for whatever scopes necessary.

#### Example: AutoFac

When using AutoFac, it's generally considered good practice to use modules to contain your registrations. Along these lines, a very simple module could be created to register `ViewFactory` correctly. A private class is being used to hide the `IContainer` shim.

```csharp
public class IgniterModule : Module {
    protected override void Load(ContainerBuilder builder) {
        base.Load(builder);

        builder.RegisterType<ViewFactoryResolver>().As<IViewFactoryResolver>();
        builder.RegisterType<ViewFactory>().As<IViewFactory>();
    }

    private class ViewFactoryResolver : IViewFactoryResolver {
        private readonly IContainer _container;

        public ViewFactoryResolver(IContainer container) {
            _container = container;
        }

        public object Resolve(Type type) {
            return _container.Resolve(type);
        }

        public T Resolve<T>() {
            return _container.Resolve<T>();
        }
    }
}
```

#### Example: Unity

To use `ViewFactory` with Unity, simply add it to your registrations. In this example, a private class is used to hide the `IUnityContainer` shim.

```csharp
public partial class App : Application {
    public App() {
        var container = new UnityContainer();

        container
            .RegisterType<IViewFactoryResolver, ViewFactoryResolver>()
            .RegisterType<IViewFactory, ViewFactory>();
    }

    private class ViewFactoryResolver : IViewFactoryResolver {
        private readonly IUnityContainer _container;

        public ViewFactoryResolver(IUnityContainer container) {
            _container = container;
        }

        public object Resolve(Type type) {
            return _container.Resolve(type);
        }

        public T Resolve<T>() {
            return _container.Resolve<T>();
        }
    }
}
```

#### Example: Castle Windsor

With Castle Windsor, an installer can easily be configured to register `ViewFactory`. A private class is being used to hide the `IWindsorContainer` shim.

```csharp
public class IgniterInstaller : IWindsorInstaller {
    public void Install(IWindsorContainer container, IConfigurationStore store) {
        container
            .Register(Component
                .For<IViewFactoryResolver>()
                .ImplementedBy<ViewFactoryResolver>())
            .Register(Component
                .For<IViewFactory>()
                .ImplementedBy<ViewFactory>());
    }

    private class ViewFactoryResolver : IViewFactoryResolver {
        private readonly IWindsorContainer _container;

        public ViewFactoryResolver(IWindsorContainer container) {
            _container = container;
        }

        public object Resolve(Type type) {
            return _container.Resolve(type);
        }

        public T Resolve<T>() {
            return _container.Resolve<T>();
        }
    }
}
```

### Creating Views and View Models

<a name="ViewFactory.Create"></a>

Once you have registered `IViewFactory` correctly, you can take advantage of its methods in a mockable fashion in your view models.

```csharp
public class MyViewModel {
    public MyViewModel(IViewFactory viewFactory) {
        MyView childView;
        MyViewModel childViewModel;

        viewFactory.Create(ref myView, ref myViewModel);

        myViewModel.DoSomething();

        ChildView = myView;
    }

    public MyView ChildView { get; private set; }
}
```

<div class="clear-both"></div>

To further improve mockability, you may also choose to receive a `dynamic` for a view, instead of a strongly-typed view. Of course, this also means you must specify the type parameters to `Create`.

```csharp
public class MyViewModel {
    public MyViewModel(IViewFactory viewFactory) {
        dynamic childView;
        MyViewModel childViewModel;

        viewFactory.Create<MyView, MyViewModel>(ref myView, ref myViewModel);

        childView.Background = Brushes.White;
        myViewModel.DoSomething();

        ChildView = myView;
    }

    public dynamic ChildView { get; private set; }
}
```

<div class="clear-both"></div>

<a name="creation-strategies"></a>

`Create` employs one of three creation strategies to construct your view and view model:

| `CreationStrategy` |     Underlying Service     | Parameter Direction |
|--------------------|----------------------------|---------------------|
| `Activate`         | `Activator.CreateInstance` | out                 |
| `Resolve`          | `IViewFactoryResolver`     | out                 |
| `Inject`           | Calling code               | in                  |

By default, `Create` will activate your views and resolves your view models. This can be overridden by supplying one or both of the optional `creationStrategy` parameters.

```csharp
var myView = GetSomeView();
MyViewModel myViewModel;

viewFactory.Create(
    ref myView, ref myViewModel, 
    viewCreationStrategy: CreationStrategy.Inject, 
    viewModelCreationStrategy: CreationStrategy.Activate);
```

## ViewElement

`ViewElement` is a XAML proxy for the `ViewFactory` allowing users to compose views more easily without relying on a view model.

Before `ViewElement` can be used, a `ViewFactory` must be attached to an ancestor in the visual tree. This is done automatically by [`ViewFactory.Create`][]. If you have a parent tree that does not have a `ViewFactory` attached, you can attach one manually by calling `viewFactory.Attach(frameworkElement)`. Note that `Attach` is on `ViewFactory` itself, **not** `IViewFactory`.


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

### Creation Strategies in XAML

As an analog to `ViewFactory`, `ViewElement` uses the same [creation strategies and defaults][creation-strategies] as [`ViewFactory.Create`][]. Similarly, these creation strategies can be overridden with one or both of their respective `CreationStrategy` attributes:

```xml
<ign:ViewElement ViewType="local:MyFirstView"
                 ViewCreationStrategy="Resolve"
                 ViewModel="{Binding MyViewModel}"
                 ViewModelCreationStrategy="Inject"/>
```

When using `CreationStrategy.Activate` or `CreationStrategy.Resolve`, provide the `ViewType` or `ViewModelType` as appropriate. Conversely, when using `CreationStrategy.Inject`, provide `View` or `ViewModel` as appropriate:

|  `ViewCreationStrategy` | `ViewModelCreationStrategy` |     Required Attributes     |
|-------------------------|-----------------------------|-----------------------------|
| `Activate` or `Resolve` | `Activate` or `Resolve`     | `ViewType`, `ViewModelType` |
| `Activate` or `Resolve` | `Inject`                    | `ViewType`, `ViewModel`     |
| `Inject`                | `cAtivate` or `Resolve`     | `View`, `ViewModelType`     |
| `Inject`                | `Inject`                    | `View`, `ViewModel`         |

### Recreation Options

Finally, `ViewElement` has an attribute called `RecreationOptions` to configure whether a view or view model should be recreated when its view model or view definition changes. 

```xml
<ign:ViewElement ViewType="local:MyFirstView"
                 ViewModelType="local:MyFirstViewModel"
                 RecreationOptions="RecreateView, RecreateViewModel"/>
```

|        `RecreationOptions`        |                 Changed Attribute                 | Components Recreated |
|-----------------------------------|---------------------------------------------------|----------------------|
| `None`                            | `View`, `ViewType`                                | View                 |
| `None`                            | `ViewModel`, `ViewModelType`                      | View Model           |
| `RecreateView`                    | `View`, `ViewType`                                | View                 |
| `RecreateView`                    | `ViewModel`, `ViewModelType`                      | View, View Model     |
| `RecreateViewModel`               | `View`, `ViewType`                                | View, View Model     |
| `RecreateViewModel`               | `ViewModel`, `ViewModelType`                      | View Model           |
| `RecreateView, RecreateViewModel` | `View`, `ViewType`,  `ViewModel`, `ViewModelType` | View, View Model     |

# Igniter.Markup Namespace

## RootViewModelBinding

`RootViewModelBindingExtension` is a markup extension allowing user code to access the view model bound to the current view regardless of what the current data context is.

```xml
<StackPanel>
    <TextBlock Text="{Binding MyProperty}"/>
    <Border DataContext="{x:Null}">
        <TextBlock Text="{ign:RootViewModelBinding MyProperty}"/>
    </Border>
</StackPanel>
```

A `RootViewModelBinding` supports virtually all of the properties of a normal `Binding` except for, of course, `Source`, `RelativeSource`, and `ElementName`. 

# Igniter.Behaviors Namespace

## SharedResourceBehavior

The `SharedResourceBehavior` attached behavior allows multiple `FrameworkElement`s to share resource dictionaries so that each reference does not re-instantiate that dictionary's resources. 

```xml
<UserControl xmlns:ign="http://schemas.northhorizon.net/igniter"
             xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity">
    <i:Interaction.Behaviors>
        <ign:SharedResourceBehavior Source="../path/to/resources.xaml"/>
    </i:Interaction.Behaviors>

    <Border Background="{StaticResource MyBackgroundResource}"/>
</UserControl>
```

The `SharedResourceBehavior` retrieves the desired dictionary from the cache and adds it to the `Resources` of its associated object when it is attached. 

The references to dictionaries are weak, so once all referencing views are garbage-collectable, the shared resources will be garbage-collectable as well. Resources that should be available permanently in the application should be added to the App resources.

## DirectoryResources&#8203;Behavior

To refer to all of the resource dictionaries in a given directory and (optionally) its subdirectories, use a `DirectoryResourcesBehavior` attached behavior. The behavior can be attached to any `FrameworkElement` and merges in all of the XAML resources found in the folder.

```xml
<UserControl xmlns:ign="http://schemas.northhorizon.net/igniter"
             xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity">
    <i:Interaction.Behaviors>
        <ign:DirectoryResourcesBehavior Directory="../path/to/resources_folder"/>
    </i:Interaction.Behaviors>

    <Border Background="{StaticResource MyBackgroundResource}"/>
</UserControl>
```

<div class="clear-both"></div>

By default, `SharedResourcesBehavior` includes subdirectories and uses the same cache as [`SharedResourceBehavior`][] to add resources. These defaults can be overridden with the `IsSubdirectoriesIncluded` and `IsShared` attributes, respectively.

```xml
<UserControl xmlns:ign="http://schemas.northhorizon.net/igniter"
             xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity">
    <i:Interaction.Behaviors>
        <ign:DirectoryResourcesBehavior Directory="../path/to/resources_folder"
                                        IsSubdirectoriesIncluded="false"
                                        IsShared="false"/>
    </i:Interaction.Behaviors>

    <Border Background="{StaticResource MyBackgroundResource}"/>
</UserControl>
```

[`ViewFactory.Create`]: #ViewFactory.Create
[creation-strategies]: #creation-strategies
[`ExpressionCommand`]: #expressioncommand
[`SubscribeToDependencyPropertyChanges`]: #SubscribeToDependencyPropertyChanges
[`SharedResourceBehavior`]: #SharedResourceBehavior