using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Controls.Primitives;
using Expression = System.Linq.Expressions.Expression;

namespace NorthHorizon.Samples.InpcTemplate
{
    /// <summary>
    /// Wraps a delegate as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand : ExpressionCommandBase
    {
        private readonly Action _onExecute;
        private readonly Func<bool> _onCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action onExecute, Expression<Func<bool>> onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();
            SubscribeToChanges(onCanExecute);
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            _onExecute();
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public override bool CanExecute(object parameter)
        {
            return _onCanExecute();
        }
    }

    /// <summary>
    /// Wraps a delegate with a parameter as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand<T> : ExpressionCommandBase
    {
        private readonly Action<T> _onExecute;
        private readonly Func<T, bool> _onCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action<T> onExecute, Expression<Func<T, bool>> onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();
            SubscribeToChanges(onCanExecute);
        }

        protected override void OnParameterChanged()
        {
            OnCanExecuteChanged();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            T typedParameter;
            if (TryConvert(parameter, out typedParameter))
                _onExecute((T)parameter);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public override bool CanExecute(object parameter)
        {
            T typedParameter;
            return TryConvert(parameter, out typedParameter) && _onCanExecute(typedParameter);
        }
    }

    /// <summary>
    /// Provides infrastructure for analyzing CanExecute expressions.
    /// </summary>
    public abstract class ExpressionCommandBase : CommandBase, IDisposable
    {
        private readonly List<IWatcher> _watchers = new List<IWatcher>();

        #region CommandParameterChanged reflection hack

        static ExpressionCommandBase()
        {
            var propertyChangedCallbackField = typeof(PropertyMetadata).GetField("_propertyChangedCallback", BindingFlags.NonPublic | BindingFlags.Instance);

            var metadata = ButtonBase.CommandParameterProperty.DefaultMetadata;

            propertyChangedCallbackField.SetValue(metadata, metadata.PropertyChangedCallback + OnButtonBaseCommandParameterChanged);

            var metadataMapField = typeof(DependencyProperty).GetField("_metadataMap", BindingFlags.NonPublic | BindingFlags.Instance);

            var metadataMap = metadataMapField.GetValue(ButtonBase.CommandParameterProperty);
            var getKeyValuePairMethodInfo = metadataMap.GetType().GetMethod("GetKeyValuePair");

            var count = (int)metadataMap.GetType().GetProperty("Count").GetValue(metadataMap, null);

            for (int i = 0; i < count; i++)
            {
                var args = new object[] { i, null, null };
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

        internal ExpressionCommandBase() { }

        /// <summary>
        /// Analyzes an expression for its notifying componetns and subscribes to their changes.
        /// </summary>
        /// <param name="expression">The expression to analyze.</param>
        protected void SubscribeToChanges(Expression expression)
        {
            var visitor = new NotifierFindingExpressionVisitor();
            visitor.Visit(expression);

            foreach (var notifier in visitor.NotifyingMembers)
                _watchers.Add(new NotifyingMemberWatcher(notifier));

            foreach (var notifier in visitor.NotifyingCollections)
                _watchers.Add(new NotifyingCollectionWatcher(notifier));

            foreach (var notifier in visitor.BindingLists)
                _watchers.Add(new BindingListWatcher(notifier));

            foreach (var watcher in _watchers)
            {
                watcher.Changed += OnWatcherChanged;
                watcher.SubscribeToCurrentNotifier();
            }
        }

        /// <summary>
        /// Called when the command parameter has changed.
        /// </summary>
        protected virtual void OnParameterChanged() { }

        private void OnWatcherChanged(object sender, EventArgs e)
        {
            OnCanExecuteChanged();

            foreach (var watcher in _watchers)
                watcher.SubscribeToCurrentNotifier();
        }

        /// <summary>
        /// Unsubscribes from all change notifications.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1063:ImplementIDisposableCorrectly", Justification = "no unmanaged resources necessary")]
        public void Dispose()
        {
            foreach (var watcher in _watchers)
                watcher.Dispose();
        }

        #region Watchers

        private interface IWatcher : IDisposable
        {
            event EventHandler Changed;

            void SubscribeToCurrentNotifier();
        }

        private abstract class Watcher<T> : IWatcher where T : class
        {
            public event EventHandler Changed = delegate { };

            private bool _isConstant;
            private readonly Func<T> _accessor;
            private T _current;

            public Watcher(Expression accessor)
            {
                if (_isConstant = accessor.NodeType == ExpressionType.Constant)
                {
                    // do this outside the closure so we do it only once.
                    var value = (T)((ConstantExpression)accessor).Value;
                    _accessor = () => value;
                }
                else
                    _accessor = GetAccessor(accessor);
            }

            public void SubscribeToCurrentNotifier()
            {
                if (_current != null)
                    Unsubscribe(_current);

                _current = _accessor();

                if (_current != null)
                    Subscribe(_current);
            }

            protected abstract void Subscribe(T notifier);

            protected abstract void Unsubscribe(T notifier);

            protected virtual void OnChanged()
            {
                Changed(this, EventArgs.Empty);
            }

            public virtual void Dispose()
            {
                if (_current != null)
                    Unsubscribe(_current);
            }

            private static Func<T> GetAccessor(Expression expression)
            {
                ConstantExpression root;
                var members = GetMemberChain(expression, out root);

                if (root == null) return null;

                if (root.Value == null) return () => null;

                var nullChecks = members
                    .Select((m, i) => members.Take(i).Aggregate((Expression)root, Expression.MakeMemberAccess))
                    .Where(a => a.Type.IsClass)
                    .Select(a => Expression.NotEqual(a, Expression.Constant(null, a.Type)))
                    .Aggregate(Expression.AndAlso);

                var returnLabel = Expression.Label(typeof(T));
                var lambda = Expression.Lambda<Func<T>>(
                    Expression.Block(
                        Expression.IfThen(nullChecks, Expression.Return(returnLabel, Expression.Convert(expression, typeof(T)))),
                        Expression.Label(returnLabel, Expression.Constant(null, typeof(T)))));

                return lambda.Compile();
            }

            private static IEnumerable<MemberInfo> GetMemberChain(Expression expression, out ConstantExpression root)
            {
                var members = new Stack<MemberInfo>(16);

                var node = expression;

                root = null;

                while (node != null)
                    switch (node.NodeType)
                    {
                        case ExpressionType.MemberAccess:
                            var memberExpr = (MemberExpression)node;
                            members.Push(memberExpr.Member);
                            node = memberExpr.Expression;
                            break;

                        case ExpressionType.Constant:
                            root = (ConstantExpression)node;
                            node = null;
                            break;

                        default: throw new NotSupportedException(node.NodeType.ToString());
                    }

                return members;
            }
        }

        private class NotifyingMemberWatcher : Watcher<INotifyPropertyChanged>
        {
            private readonly string _memberName;

            public NotifyingMemberWatcher(MemberExpression notifyingMember)
                : base(notifyingMember.Expression)
            {
                _memberName = notifyingMember.Member.Name;
            }

            protected override void Subscribe(INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += OnNotifierPropertyChanged;
            }

            protected override void Unsubscribe(INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged -= OnNotifierPropertyChanged;
            }

            private void OnNotifierPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _memberName)
                    OnChanged();
            }
        }

        private class NotifyingCollectionWatcher : Watcher<INotifyCollectionChanged>
        {
            public NotifyingCollectionWatcher(Expression expression) : base(expression) { }

            protected override void Subscribe(INotifyCollectionChanged notifier)
            {
                notifier.CollectionChanged += OnNotifierCollectionChanged;
            }

            protected override void Unsubscribe(INotifyCollectionChanged notifier)
            {
                notifier.CollectionChanged -= OnNotifierCollectionChanged;
            }

            private void OnNotifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action != NotifyCollectionChangedAction.Move)
                    OnChanged();
            }
        }

        private class BindingListWatcher : Watcher<IBindingList>
        {
            public BindingListWatcher(Expression accessor) : base(accessor) { }

            protected override void Subscribe(IBindingList notifier)
            {
                notifier.ListChanged += OnNotifierListChanged;
            }

            protected override void Unsubscribe(IBindingList notifier)
            {
                notifier.ListChanged -= OnNotifierListChanged;
            }

            private void OnNotifierListChanged(object sender, ListChangedEventArgs e)
            {
                switch (e.ListChangedType)
                {
                    case ListChangedType.ItemAdded:
                    case ListChangedType.ItemDeleted:
                    case ListChangedType.Reset:
                        OnChanged();
                        break;
                }
            }
        }

        #endregion

        private class NotifierFindingExpressionVisitor : ExpressionVisitor
        {
            public readonly HashSet<MemberExpression> NotifyingMembers = new HashSet<MemberExpression>(PropertyChainEqualityComparer.Instance);

            public readonly HashSet<Expression> NotifyingCollections = new HashSet<Expression>(PropertyChainEqualityComparer.Instance);

            public readonly HashSet<Expression> BindingLists = new HashSet<Expression>(PropertyChainEqualityComparer.Instance);

            protected override Expression VisitMember(MemberExpression node)
            {
                if (IsPropertyChain(node.Expression) && typeof(INotifyPropertyChanged).IsAssignableFrom(node.Expression.Type))
                    NotifyingMembers.Add(node);

                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                RegisterNotifyingCollection(node.Object);

                return base.VisitMethodCall(node);
            }

            protected override Expression VisitIndex(IndexExpression node)
            {
                RegisterNotifyingCollection(node.Object);

                return base.VisitIndex(node);
            }

            private void RegisterNotifyingCollection(Expression node)
            {
                if (IsPropertyChain(node))
                    // if node is both of these types, we only want one registration.
                    if (typeof(INotifyCollectionChanged).IsAssignableFrom(node.Type))
                        NotifyingCollections.Add(node);
                    else if (typeof(IBindingList).IsAssignableFrom(node.Type))
                        BindingLists.Add(node);
            }

            private static bool IsPropertyChain(Expression node)
            {
                while (true)
                {
                    switch (node.NodeType)
                    {
                        case ExpressionType.Constant:
                            return true;

                        case ExpressionType.MemberAccess:
                            var memberExpr = (MemberExpression)node;
                            node = memberExpr.Expression;
                            break;

                        default:
                            return false;
                    }
                }
            }

            private class PropertyChainEqualityComparer : IEqualityComparer<Expression>
            {
                public static readonly PropertyChainEqualityComparer Instance = new PropertyChainEqualityComparer();

                private PropertyChainEqualityComparer() { }

                public bool Equals(Expression x, Expression y)
                {
                    while (true)
                    {
                        if (x.NodeType != y.NodeType) return false;

                        switch (x.NodeType)
                        {
                            case ExpressionType.Constant:
                                var xConstantExpr = (ConstantExpression)x;
                                var yConstantExpr = (ConstantExpression)y;

                                return ReferenceEquals(xConstantExpr.Value, yConstantExpr.Value);

                            case ExpressionType.MemberAccess:
                                var xMemberExpr = (MemberExpression)x;
                                var yMemberExpr = (MemberExpression)y;

                                if (xMemberExpr.Member != yMemberExpr.Member)
                                    return false;

                                x = xMemberExpr.Expression;
                                y = yMemberExpr.Expression;
                                break;

                            default:
                                throw new InvalidOperationException(x.NodeType.ToString());
                        }
                    }
                }

                public int GetHashCode(Expression node)
                {
                    var hash = 17;

                    while (true)
                    {
                        switch (node.NodeType)
                        {
                            case ExpressionType.Constant:
                                var constantExpr = (ConstantExpression)node;

                                return hash * 31 + (constantExpr.Value != null ? constantExpr.Value.GetHashCode() : 0);

                            case ExpressionType.MemberAccess:
                                var memberExpr = (MemberExpression)node;

                                hash = hash * 31 + memberExpr.Member.GetHashCode();

                                node = memberExpr.Expression;
                                break;

                            default:
                                throw new InvalidOperationException(node.NodeType.ToString());
                        }
                    }
                }
            }
        }
    }
}
