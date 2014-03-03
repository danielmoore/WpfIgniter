using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using NorthHorizon.Samples.InpcTemplate.Composition;

namespace NorthHorizon.Samples.InpcTemplate.Markup
{
    /// <summary>
    /// A binding that uses the view model attached to the view by <see cref="O:ViewFactory.Create{TView, TViewModel}"/> as its source.
    /// </summary>
    /// <remarks>
    /// This is just a shortcut for <c>{Binding Path=RootViewModel.<see cref="Path"/>, RelativeSource={RelativeSource Self}}</c>
    /// </remarks>
    public class RootViewModelBindingExtension : MarkupExtension
    {
        private readonly Binding _binding = new Binding();

        /// <summary>
        /// Initializes a new instance of the <see cref="RootViewModelBindingExtension"/> class.
        /// </summary>
        public RootViewModelBindingExtension() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="RootViewModelBindingExtension"/> class.
        /// </summary>
        /// <param name="path">The path to the binding source property.</param>
        public RootViewModelBindingExtension(string path) : this()
        {
            Path = path;
        }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            var path = string.IsNullOrEmpty(Path) ? "(0)" : string.Format("(0).{0}", Path);
            _binding.Path = new PropertyPath(path, ViewElement.RootViewModelProperty);

            _binding.RelativeSource = RelativeSource.Self;

            return _binding.ProvideValue(serviceProvider);
        }

        /// <summary>Gets or sets the path to the binding source property.</summary>
        /// <value>The path to the binding source. The default is null.</value>
        [ConstructorArgument("path")]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets a string that specifies how to format the binding if it displays the bound value as a string.
        /// </summary>
        /// <value>
        /// A string that specifies how to format the binding if it displays the bound value as a string.
        /// </value>
        public string StringFormat
        {
            get { return _binding.StringFormat; }
            set { _binding.StringFormat = value; }
        }


        /// <summary>
        /// Gets or sets the name of the <see cref="BindingGroup"/> to which this binding belongs.
        /// </summary>
        /// <value>
        /// The <see cref="BindingGroup"/> to which this binding belongs.
        /// </value>
        public string BindingGroupName
        {
            get { return _binding.BindingGroupName; }
            set { _binding.BindingGroupName = value; }
        }

        /// <summary>
        /// Gets or sets the converter to use.
        /// </summary>
        /// <value>
        /// A value of type <see cref="IValueConverter"/>. The default is null.
        /// </value>
        public IValueConverter Converter
        {
            get { return _binding.Converter; }
            set { _binding.Converter = value; }
        }

        /// <summary>
        /// Gets or sets the culture in which to evaluate the converter.
        /// </summary>
        /// <value>
        /// The default is null.
        /// </value>
        public CultureInfo ConverterCulture
        {
            get { return _binding.ConverterCulture; }
            set { _binding.ConverterCulture = value; }
        }

        /// <summary>
        /// Gets or sets the parameter to pass to the System.Windows.Data.Binding.Converter.
        /// </summary>
        /// <value>
        /// The parameter to pass to the <see cref="Converter"/>. The default is null.
        /// </value>
        public object ConverterParameter
        {
            get { return _binding.ConverterParameter; }
            set { _binding.ConverterParameter = value; }
        }

        /// <summary>
        /// Gets or sets the value to use when the binding is unable to return a value.
        /// </summary>
        /// <value>
        /// The default value is <see cref="DependencyProperty.UnsetValue"/>.
        /// </value>
        public object FallbackValue
        {
            get { return _binding.FallbackValue; }
            set { _binding.FallbackValue = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates the direction of the data flow in the binding.
        /// </summary>
        public BindingMode Mode
        {
            get { return _binding.Mode; }
            set { _binding.Mode = value; }
        }

        /// <summary>
        /// Gets or sets the value that is used in the target when the value of the source is null.
        /// </summary>
        /// <value>
        /// The value that is used in the target when the value of the source is null.
        /// </value>
        public object TargetNullValue
        {
            get { return _binding.TargetNullValue; }
            set { _binding.TargetNullValue = value; }
        }

        /// <summary>
        /// Gets or sets a value that determines the timing of binding source updates.
        /// </summary>
        public UpdateSourceTrigger UpdateSourceTrigger
        {
            get { return _binding.UpdateSourceTrigger; }
            set { _binding.UpdateSourceTrigger = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to include the <see cref="DataErrorValidationRule"/>.
        /// </summary>
        /// <value><c>true</c> to include the <see cref="DataErrorValidationRule"/>; otherwise, <c>false</c>.</value>
        public bool ValidatesOnDataErrors
        {
            get { return _binding.ValidatesOnDataErrors; }
            set { _binding.ValidatesOnDataErrors = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to include the <see cref="ExceptionValidationRule"/>.
        /// </summary>
        /// <value><c>true</c> to include the <see cref="ExceptionValidationRule"/>; otherwise, <c>false</c>.</value>
        public bool ValidatesOnExceptions
        {
            get { return _binding.ValidatesOnExceptions; }
            set { _binding.ValidatesOnExceptions = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to evaluate the <see cref="Path"/> relative to the data item or the <see cref="DataSourceProvider"/> object.
        /// </summary>
        /// <value><c>false</c> to evaluate the path relative to the data item itself; otherwise, <c>true</c>. The default is <c>false</c>.</value>
        public bool BindsDirectlyToSource
        {
            get { return _binding.BindsDirectlyToSource; }
            set { _binding.BindsDirectlyToSource = value; }
        }
    }
}