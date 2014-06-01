using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using Expression = System.Linq.Expressions.Expression;

namespace Igniter.Core
{
    /// <summary>
    /// Provides infrastructure for analyzing CanExecute expressions.
    /// </summary>
    public abstract class ExpressionCommandBase : CommandBase, IDisposable
    {
        private readonly ExpressionWatcher _watcher;

        #region CommandParameterChanged reflection hack

        static ExpressionCommandBase()
        {
            var propertyChangedCallbackField = typeof(PropertyMetadata).GetField("_propertyChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance);

            var metadata = ButtonBase.CommandParameterProperty.DefaultMetadata;

            propertyChangedCallbackField.SetValue(metadata, metadata.PropertyChangedCallback + OnButtonBaseCommandParameterChanged);

            // _metadataMap contains all the metadata overrides for the DP.
            var metadataMapField = typeof(DependencyProperty).GetField("_metadataMap", BindingFlags.NonPublic | BindingFlags.Instance);

            var metadataMap = metadataMapField.GetValue(ButtonBase.CommandParameterProperty);
            var getKeyValuePairMethodInfo = metadataMap.GetType().GetMethod("GetKeyValuePair");

            var count = (int)metadataMap.GetType().GetProperty("Count").GetValue(metadataMap, null);

            for (int i = 0; i < count; i++)
            {
                var args = new object[] {i, null, null};
                getKeyValuePairMethodInfo.Invoke(metadataMap, args);

                metadata = (PropertyMetadata)args[2];

                propertyChangedCallbackField.SetValue(metadata, metadata.PropertyChangedCallback + OnButtonBaseCommandParameterChanged);
            }
        }

        private static void OnButtonBaseCommandParameterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var expressionCommand = sender.GetValue(ButtonBase.CommandProperty) as ExpressionCommandBase;

            if (expressionCommand != null)
                expressionCommand.OnParameterChanged();
        }

        #endregion

        ~ExpressionCommandBase()
        {
            Dispose(false);
        }

        protected ExpressionCommandBase(Expression onCanExecute)
        {
            _watcher = new ExpressionWatcher(onCanExecute);

            _watcher.ExpressionChanged += OnExpressionChanged;
        }

        private void OnExpressionChanged(object sender, EventArgs eventArgs)
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Called when the command parameter has changed.
        /// </summary>
        protected virtual void OnParameterChanged() {}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _watcher.Dispose();
        }
    }
}