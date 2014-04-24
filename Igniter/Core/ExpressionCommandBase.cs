using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
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
        private readonly List<IWatcher> _watchers = new List<IWatcher>();

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

        internal ExpressionCommandBase() {}

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

            foreach (var pair in visitor.DependencyProperties)
                _watchers.Add(new DependencyPropertyWatcher(pair.Key.Expression, pair.Value));

            foreach (var watcher in _watchers)
            {
                watcher.Changed += OnWatcherChanged;
                watcher.SubscribeToCurrentNotifier();
            }
        }

        /// <summary>
        /// Called when the command parameter has changed.
        /// </summary>
        protected virtual void OnParameterChanged() {}

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

            private readonly SerialDisposable _disposable = new SerialDisposable();

            private readonly Func<T> _accessor;
            private T _current;

            public Watcher(Expression accessor)
            {
                if (accessor.NodeType == ExpressionType.Constant)
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
                _current = _accessor();

                _disposable.Disposable = _current != null ? Subscribe(_current) : null;
            }

            protected abstract IDisposable Subscribe(T notifier);

            protected virtual void OnChanged()
            {
                Changed(this, EventArgs.Empty);
            }

            public virtual void Dispose()
            {
                _disposable.Dispose();
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

                        default:
                            throw new NotSupportedException(node.NodeType.ToString());
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

            protected override IDisposable Subscribe(INotifyPropertyChanged notifier)
            {
                notifier.PropertyChanged += OnNotifierPropertyChanged;
                return Disposable.Create(() => notifier.PropertyChanged -= OnNotifierPropertyChanged);
            }

            private void OnNotifierPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == _memberName)
                    OnChanged();
            }
        }

        private class NotifyingCollectionWatcher : Watcher<INotifyCollectionChanged>
        {
            public NotifyingCollectionWatcher(Expression expression) : base(expression) {}

            protected override IDisposable Subscribe(INotifyCollectionChanged notifier)
            {
                notifier.CollectionChanged += OnNotifierCollectionChanged;
                return Disposable.Create(() => notifier.CollectionChanged -= OnNotifierCollectionChanged);
            }

            private void OnNotifierCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (e.Action != NotifyCollectionChangedAction.Move)
                    OnChanged();
            }
        }

        private class BindingListWatcher : Watcher<IBindingList>
        {
            public BindingListWatcher(Expression accessor) : base(accessor) {}

            protected override IDisposable Subscribe(IBindingList notifier)
            {
                notifier.ListChanged += OnNotifierListChanged;
                return Disposable.Create(() => notifier.ListChanged -= OnNotifierListChanged);
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

        private class DependencyPropertyWatcher : Watcher<DependencyObject>
        {
            private readonly DependencyProperty _property;

            public DependencyPropertyWatcher(Expression ownerExpression, DependencyProperty property)
                : base(ownerExpression)
            {
                _property = property;
            }

            protected override IDisposable Subscribe(DependencyObject notifier)
            {
                return notifier.SubscribeToDependencyPropertyChanges(_property, OnPropertyChanged);
            }

            private void OnPropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
            {
                OnChanged();
            }
        }

        #endregion

        private class NotifierFindingExpressionVisitor : ExpressionVisitor
        {
            public readonly HashSet<MemberExpression> NotifyingMembers = new HashSet<MemberExpression>(PropertyChainEqualityComparer.Instance);

            public readonly Dictionary<MemberExpression, DependencyProperty> DependencyProperties =
                new Dictionary<MemberExpression, DependencyProperty>(PropertyChainEqualityComparer.Instance);

            public readonly HashSet<Expression> NotifyingCollections = new HashSet<Expression>(PropertyChainEqualityComparer.Instance);

            public readonly HashSet<Expression> BindingLists = new HashSet<Expression>(PropertyChainEqualityComparer.Instance);

            protected override Expression VisitMember(MemberExpression node)
            {
                if (IsPropertyChain(node.Expression))
                {
                    var dependencyProperty = GetDependencyProperty(node.Expression.Type, node.Member.Name);

                    if (dependencyProperty != null)
                        DependencyProperties.Add(node, dependencyProperty);
                    else if (typeof(INotifyPropertyChanged).IsAssignableFrom(node.Expression.Type))
                        NotifyingMembers.Add(node);
                }

                return base.VisitMember(node);
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Object != null) // Is this a static method call?
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

            private static DependencyProperty GetDependencyProperty(Type type, string name)
            {
                if (!typeof(DependencyObject).IsAssignableFrom(type)) return null;

                return TypeDescriptor
                    .GetProperties(type, new[] {new PropertyFilterAttribute(PropertyFilterOptions.All)})
                    .Cast<PropertyDescriptor>()
                    .Where(p => p.Name == name)
                    .Select(DependencyPropertyDescriptor.FromProperty)
                    .Where(dpd => dpd != null)
                    .Select(dpd => dpd.DependencyProperty)
                    .OrderBy(dpd => dpd.OwnerType, SubclassCompareer.Instance)
                    .FirstOrDefault();
            }

            private class SubclassCompareer : IComparer<Type>
            {
                public static readonly SubclassCompareer Instance = new SubclassCompareer();

                private SubclassCompareer() {}

                public int Compare(Type x, Type y)
                {
                    return x == y ? 0 : x.IsSubclassOf(y) ? 1 : -1;
                }
            }

            private class PropertyChainEqualityComparer : IEqualityComparer<Expression>
            {
                public static readonly PropertyChainEqualityComparer Instance = new PropertyChainEqualityComparer();

                private PropertyChainEqualityComparer() {}

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

                                return hash*31 + (constantExpr.Value != null ? constantExpr.Value.GetHashCode() : 0);

                            case ExpressionType.MemberAccess:
                                var memberExpr = (MemberExpression)node;

                                hash = hash*31 + memberExpr.Member.GetHashCode();

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