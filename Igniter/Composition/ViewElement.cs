using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Igniter.Composition
{
    /// <summary>
    /// A <see cref="ViewFactory"/> wrapper container to inject view/view model 
    /// pairs into the visual tree.
    /// </summary>
    public sealed class ViewElement : Control
    {
        private bool _isCreateScheduled = true;
        private RecreationOptions _scheduledRecreationOptions;
        private bool _isCreated;

        static ViewElement()
        {
            var template = new ControlTemplate(typeof(ViewElement))
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            };

            template.VisualTree.SetBinding(ContentPresenter.ContentProperty, new Binding
            {
                Path = new PropertyPath("(0)", ResolvedViewProperty),
                RelativeSource = RelativeSource.TemplatedParent
            });

            TemplateProperty.OverrideMetadata(typeof(ViewElement), new FrameworkPropertyMetadata(template));
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ViewElement"/> class.
        /// </summary>
        public ViewElement()
        {
            ViewCreationStrategy = ViewFactory.DefaultViewCreationStrategy;
            ViewModelCreationStrategy = ViewFactory.DefaultViewModelCreationStrategy;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isCreated) return;

            _isCreateScheduled = false;
            CreateView(RecreationOptions.RecreateView | RecreationOptions.RecreateViewModel);
        }

        private static void OnCreateParameterChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ((ViewElement)sender).OnCreateParameterChanged(e);
        }

        private void OnCreateParameterChanged(DependencyPropertyChangedEventArgs e)
        {
            var metadata = (ViewComponentPropertyMetadata)e.Property.GetMetadata(this);

            if (!metadata.AppliesTo(this)) return;

            _scheduledRecreationOptions |= metadata.RecreationOptions;

            if (ReferenceEquals(e.Property, ViewTypeProperty))
                _scheduledRecreationOptions |= RecreationOptions.RecreateView;
            else if (ReferenceEquals(e.Property, ViewModelTypeProperty))
                _scheduledRecreationOptions |= RecreationOptions.RecreateViewModel;

            if (_isCreateScheduled) return;

            // Just in case any other properties are going to change...
            _isCreateScheduled = true;
            Dispatcher.BeginInvoke(() =>
            {
                var recreationOptions = _scheduledRecreationOptions;
                _isCreateScheduled = false;
                _scheduledRecreationOptions = RecreationOptions.None;

                CreateView(recreationOptions);
            });
        }

        private void CreateView(RecreationOptions recreationOptions)
        {
            FrameworkElement view;

            try
            {
                var viewFactory = GetViewFactory(this);

                if (viewFactory == null)
                    throw new InvalidOperationException("No ViewFactory has been set.");

                CreationStrategy viewCreationStrategy = ViewCreationStrategy,
                    viewModelCreationStrategy = ViewModelCreationStrategy;

                view = View;
                var viewModel = ViewModel;

                if (_isCreated)
                {
                    if (view != null && !recreationOptions.HasFlag(RecreationOptions.RecreateView))
                        viewCreationStrategy = CreationStrategy.Inject;

                    if (viewModel != null && !recreationOptions.HasFlag(RecreationOptions.RecreateViewModel))
                        viewCreationStrategy = CreationStrategy.Inject;
                }

                viewFactory.Create(ViewType, ref view, ViewModelType, ref viewModel, viewCreationStrategy, viewModelCreationStrategy);
            }
            catch (Exception error)
            {
                view = new TextBox
                {
                    BorderBrush = Brushes.Red,
                    BorderThickness = new Thickness(10),
                    IsReadOnly = true,
                    AcceptsReturn = true,
                    AcceptsTab = true,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    FontFamily = new FontFamily("Consolas, Courier New, Monospaced"),
                    Text = error.ToString()
                };
            }
            
            ResolvedView = view;

            _isCreated = true;
        }

        #region FrameworkElement ResolvedView { get; private set; }

        private static readonly DependencyPropertyKey ResolvedViewPropertyKey =
            DependencyProperty.RegisterReadOnly("ResolvedView", typeof(FrameworkElement), typeof(ViewElement), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ResolvedView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResolvedViewProperty = ResolvedViewPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ResolvedView.
        /// </summary>
        public FrameworkElement ResolvedView
        {
            get { return (FrameworkElement)GetValue(ResolvedViewProperty); }
            private set { SetValue(ResolvedViewPropertyKey, value); }
        }

        #endregion

        /// <summary>
        /// Gets or sets the options that determine whether a view or 
        /// view model should always be recreated when its counterpart
        /// is changed.
        /// </summary>
        /// <value>
        /// The recreation options.
        /// </value>
        public RecreationOptions RecreationOptions { get; set; }

        #region FrameworkElement View { get; set; }

        /// <summary>
        /// Identifies the <see cref="View"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(FrameworkElement), typeof(ViewElement), 
            new ViewPropertyMetadata(true, OnCreateParameterChanged));

        /// <summary>
        /// Gets or sets the View.
        /// </summary>
        public FrameworkElement View
        {
            get { return (FrameworkElement)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        #endregion

        #region Type ViewType { get; set; }

        /// <summary>
        /// Identifies the <see cref="ViewType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewTypeProperty =
            DependencyProperty.Register("ViewType", typeof(Type), typeof(ViewElement), 
            new ViewPropertyMetadata(false, OnCreateParameterChanged));

        /// <summary>
        /// Gets or sets the ViewType.
        /// </summary>
        public Type ViewType
        {
            get { return (Type)GetValue(ViewTypeProperty); }
            set { SetValue(ViewTypeProperty, value); }
        }

        #endregion

        /// <summary>
        /// Gets or sets the view creation strategy.
        /// </summary>
        /// <value>
        /// The view creation strategy.
        /// </value>
        public CreationStrategy ViewCreationStrategy { get; set; }

        #region object ViewModel { get; set; }

        /// <summary>
        /// Identifies the <see cref="ViewModel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(object), typeof(ViewElement),
            new ViewPropertyMetadata(true, OnCreateParameterChanged));

        /// <summary>
        /// Gets or sets the ViewModel.
        /// </summary>
        public object ViewModel
        {
            get { return GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        #endregion

        #region Type ViewModelType { get; set; }

        /// <summary>
        /// Identifies the <see cref="ViewModelType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewModelTypeProperty =
            DependencyProperty.Register("ViewModelType", typeof(Type), typeof(ViewElement),
            new ViewModelPropertyMetadata(false, OnCreateParameterChanged));

        /// <summary>
        /// Gets or sets the ViewModelType.
        /// </summary>
        public Type ViewModelType
        {
            get { return (Type)GetValue(ViewModelTypeProperty); }
            set { SetValue(ViewModelTypeProperty, value); }
        }

        #endregion

        /// <summary>
        /// Gets or sets the view model creation strategy.
        /// </summary>
        /// <value>
        /// The view model creation strategy.
        /// </value>
        public CreationStrategy ViewModelCreationStrategy { get; set; }

        #region ViewFactory ViewFactory { get; internal set; }

        private static readonly DependencyPropertyKey ViewFactoryPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ViewFactory", typeof(ViewFactory), typeof(ViewElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the <see cref="ViewFactory"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ViewFactoryProperty = ViewFactoryPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ViewFactory.
        /// </summary>
        public static ViewFactory GetViewFactory(FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            return (ViewFactory)element.GetValue(ViewFactoryProperty);
        }

        internal static void SetViewFactory(FrameworkElement element, ViewFactory value)
        {
            if (element == null) throw new ArgumentNullException("element");

            if (element.IsLoaded)
                throw new InvalidOperationException("Element has already been loaded.");

            element.SetValue(ViewFactoryPropertyKey, value);
        }

        #endregion

        #region object RootViewModel { get; internal set; }

        private static readonly DependencyPropertyKey RootViewModelPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("RootViewModel", typeof(object), typeof(ViewElement),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>
        /// Identifies the RootViewModel dependency property.
        /// </summary>
        public static readonly DependencyProperty RootViewModelProperty = RootViewModelPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the RootViewModel.
        /// </summary>
        public static object GetRootViewModel(FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            return element.GetValue(RootViewModelProperty);
        }

        internal static void SetRootViewModel(FrameworkElement element, object value)
        {
            if (element == null) throw new ArgumentNullException("element");

            if (element.IsLoaded)
                throw new InvalidOperationException("Element has already been loaded.");

            element.SetValue(RootViewModelPropertyKey, value);
        }

        #endregion

        private abstract class ViewComponentPropertyMetadata : PropertyMetadata
        {
            private readonly bool _isInjected;

            protected ViewComponentPropertyMetadata(bool isInjected, PropertyChangedCallback propertyChangedCallback)
                : base(propertyChangedCallback)
            {
                _isInjected = isInjected;
            }

            public abstract RecreationOptions RecreationOptions { get; }

            protected abstract CreationStrategy GetCreationStrategy(ViewElement element);

            public bool AppliesTo(ViewElement element)
            {
                var isStrategyInject = GetCreationStrategy(element) == CreationStrategy.Inject;

                return _isInjected == isStrategyInject;
            }
        }

        private class ViewPropertyMetadata : ViewComponentPropertyMetadata
        {
            public ViewPropertyMetadata(bool isInjected, PropertyChangedCallback propertyChangedCallback) 
                : base(isInjected, propertyChangedCallback) {}

            public override RecreationOptions RecreationOptions
            {
                get { return RecreationOptions.RecreateView; }
            }

            protected override CreationStrategy GetCreationStrategy(ViewElement element)
            {
                return element.ViewCreationStrategy;
            }
        }

        private class ViewModelPropertyMetadata : ViewComponentPropertyMetadata
        {
            public ViewModelPropertyMetadata(bool isInjected, PropertyChangedCallback propertyChangedCallback) 
                : base(isInjected, propertyChangedCallback) {}

            public override RecreationOptions RecreationOptions
            {
                get { return RecreationOptions.RecreateViewModel; }
            }

            protected override CreationStrategy GetCreationStrategy(ViewElement element)
            {
                return element.ViewCreationStrategy;
            }
        }
    }
}