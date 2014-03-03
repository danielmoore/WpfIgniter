using System.Windows;

namespace NorthHorizon.Samples.InpcTemplate.Composition
{
    /// <summary>
    /// Provides factory methods for creating and binding views and view modelos
    /// </summary>
    public interface IViewFactory {
        /// <summary>
        /// Binds a view model to a view and potentially creates or resolves one or both.
        /// </summary>
        /// <typeparam name="TView">The type of the view</typeparam>
        /// <typeparam name="TViewModel">The type of the view model</typeparam>
        /// <param name="view">An <c>in</c> parameter for injected views or an <c>out</c> parameter for created and resolved views.</param>
        /// <param name="viewModel">An <c>in</c> parameter for injected view models or an <c>out</c> parameter for created and resolved view models.</param>
        /// <param name="viewCreationStrategy">The stategy for how to create the view.</param>
        /// <param name="viewModelCreationStrategy">The strategy for how to create the view model.</param>
        void Create<TView, TViewModel>(
            ref TView view, ref TViewModel viewModel,
            CreationStrategy viewCreationStrategy = ViewFactory.DefaultViewCreationStrategy,
            CreationStrategy viewModelCreationStrategy = ViewFactory.DefaultViewModelCreationStrategy)
            where TView : FrameworkElement
            where TViewModel : class;

        /// <summary>
        /// Binds a view model to a view (represented dynamically) and potentially creates or resolves one or both.
        /// </summary>
        /// <typeparam name="TView">The type of the view</typeparam>
        /// <typeparam name="TViewModel">The type of the view model</typeparam>
        /// <param name="view">An <c>in</c> parameter for injected views or an <c>out</c> parameter for created and resolved views.</param>
        /// <param name="viewModel">An <c>in</c> parameter for injected view models or an <c>out</c> parameter for created and resolved view models.</param>
        /// <param name="viewCreationStrategy">The stategy for how to create the view.</param>
        /// <param name="viewModelCreationStrategy">The strategy for how to create the view model.</param>
        void Create<TView, TViewModel>(
            ref dynamic view, ref TViewModel viewModel,
            CreationStrategy viewCreationStrategy = ViewFactory.DefaultViewCreationStrategy,
            CreationStrategy viewModelCreationStrategy = ViewFactory.DefaultViewModelCreationStrategy)
            where TView : FrameworkElement
            where TViewModel : class;
    }
}