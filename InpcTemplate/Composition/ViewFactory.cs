using System;
using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Composition
{
    /// <summary>
    /// A factory to bind and potentially create or resolve views and view models.
    /// </summary>
    public class ViewFactory : IViewFactory
    {
        /// <summary>
        /// The default view creation strategy
        /// </summary>
        public const CreationStrategy DefaultViewCreationStrategy = CreationStrategy.Activate;

        /// <summary>
        /// The default view model creation strategy
        /// </summary>
        public const CreationStrategy DefaultViewModelCreationStrategy = CreationStrategy.Resolve;

        private readonly IViewFactoryResolver _resolver;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewFactory"/> class.
        /// </summary>
        /// <param name="resolver">The IoC container shim to use for resolving views and view models.</param>
        /// <exception cref="System.ArgumentNullException">resolver</exception>
        public ViewFactory(IViewFactoryResolver resolver)
        {
            if (resolver == null) throw new ArgumentNullException("resolver");

            _resolver = resolver;
        }

        /// <summary>
        /// Binds a view model to a view and potentially creates or resolves one or both.
        /// </summary>
        /// <typeparam name="TView">The type of the view</typeparam>
        /// <typeparam name="TViewModel">The type of the view model</typeparam>
        /// <param name="view">An <c>in</c> parameter for injected views or an <c>out</c> parameter for created and resolved views.</param>
        /// <param name="viewModel">An <c>in</c> parameter for injected view models or an <c>out</c> parameter for created and resolved view models.</param>
        /// <param name="viewCreationStrategy">The stategy for how to create the view.</param>
        /// <param name="viewModelCreationStrategy">The strategy for how to create the view model.</param>
        public void Create<TView, TViewModel>(
            ref TView view, ref TViewModel viewModel,
            CreationStrategy viewCreationStrategy = DefaultViewCreationStrategy,
            CreationStrategy viewModelCreationStrategy = DefaultViewModelCreationStrategy)
            where TView : FrameworkElement
            where TViewModel : class
        {
            Create(ref view, viewCreationStrategy);
            Create(ref viewModel, viewCreationStrategy);
            Bind(view, viewModel);
        }

        /// <summary>
        /// Binds a view model to a view (represented dynamically) and potentially creates or resolves one or both.
        /// </summary>
        /// <typeparam name="TView">The type of the view</typeparam>
        /// <typeparam name="TViewModel">The type of the view model</typeparam>
        /// <param name="view">An <c>in</c> parameter for injected views or an <c>out</c> parameter for created and resolved views.</param>
        /// <param name="viewModel">An <c>in</c> parameter for injected view models or an <c>out</c> parameter for created and resolved view models.</param>
        /// <param name="viewCreationStrategy">The stategy for how to create the view.</param>
        /// <param name="viewModelCreationStrategy">The strategy for how to create the view model.</param>
        public void Create<TView, TViewModel>(
            ref dynamic view, ref TViewModel viewModel,
            CreationStrategy viewCreationStrategy = DefaultViewCreationStrategy,
            CreationStrategy viewModelCreationStrategy = DefaultViewModelCreationStrategy)
            where TView : FrameworkElement
            where TViewModel : class
        {
            Create(typeof (TView), ref view, viewCreationStrategy);
            Create(ref viewModel, viewCreationStrategy);
            Bind(view, viewModel);
        }

        /// <summary>
        /// Binds a view model to a view  and potentially creates or resolves one or both.
        /// </summary>
        /// <param name="viewType">Type of the view.</param>
        /// <param name="view">An <c>in</c> parameter for injected views or an <c>out</c> parameter for created and resolved views.</param>
        /// <param name="viewModelType">Type of the view model.</param>
        /// <param name="viewModel">An <c>in</c> parameter for injected view models or an <c>out</c> parameter for created and resolved view models.</param>
        /// <param name="viewCreationStrategy">The stategy for how to create the view.</param>
        /// <param name="viewModelCreationStrategy">The strategy for how to create the view model.</param>
        public void Create(
            Type viewType, ref FrameworkElement view,
            Type viewModelType, ref object viewModel,
            CreationStrategy viewCreationStrategy = DefaultViewCreationStrategy,
            CreationStrategy viewModelCreationStrategy = DefaultViewModelCreationStrategy)
        {
            object untypedView = view;
            Create(viewType, ref untypedView, viewCreationStrategy);
            Create(viewModelType, ref viewModel, viewCreationStrategy);
            view = (FrameworkElement) untypedView;

            Bind(view, viewModel);
        }

        private void Create<T>(ref T obj, CreationStrategy creationStrategy) where T : class
        {
            switch (creationStrategy)
            {
                case CreationStrategy.Inject:
                    if (obj == null)
                        throw new ArgumentNullException("obj");

                    // Everything looks good, so there's nothing to do here.
                    break;
                case CreationStrategy.Resolve:
                    obj = _resolver.Resolve<T>();
                    break;
                case CreationStrategy.Activate:
                    obj = Activator.CreateInstance<T>();
                    break;
                default:
                    throw new ArgumentOutOfRangeException("creationStrategy");
            }
        }

        private void Create(Type type, ref object obj, CreationStrategy creationStrategy)
        {
            switch (creationStrategy)
            {
                case CreationStrategy.Inject:
                    if (obj == null)
                        throw new ArgumentNullException("obj");

                    if (type != null && !type.IsInstanceOfType(obj))
                        throw new ArgumentException("Injected object is not an instance of the given type.");

                    // Everything looks good, so there's nothing to do here.
                    break;
                case CreationStrategy.Resolve:
                    obj = _resolver.Resolve(type);
                    break;
                case CreationStrategy.Activate:
                    obj = Activator.CreateInstance(type);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("creationStrategy");
            }
        }

        private void Bind(FrameworkElement view, object viewModel)
        {
            Attach(view);
            view.DataContext = viewModel;
            ViewElement.SetRootViewModel(view, viewModel);
        }

        /// <summary>
        /// Attaches this instance to a specified view so that a <see cref="ViewElement"/> can be used.
        /// </summary>
        /// <param name="view">The view onto which this instance should be attached.</param>
        public void Attach(FrameworkElement view)
        {
            if (view == null) throw new ArgumentNullException("view");

            ViewElement.SetViewFactory(view, this);
        }
    }
}