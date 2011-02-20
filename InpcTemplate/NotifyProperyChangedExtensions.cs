using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using System.Disposables;

namespace InpcTemplate
{
	/// <summary>
	/// Provides extension methods for subscribing to <see cref="INotifyPropertyChanged"/> and
	/// <see cref="INotifyPropertyChanging"/> in a strongly typed manner.
	/// </summary>
	public static class NotifyProperyChangedExtensions
	{
		/// <summary>
		/// Subscribes the given handler to the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="TSource">The type of the object providing the event.</typeparam>
		/// <typeparam name="TProp">The type of the property.</typeparam>
		/// <param name="source">The object providing the event.</param>
		/// <param name="propertySelector">A selector taking the given object and selecting the property to subscribe.</param>
		/// <param name="onChanged">The handler to call when the property changes.</param>
		/// <returns>A subscription token that, when disposed, will unsubscribe the handler.</returns>
		public static IDisposable SubscribeToPropertyChanged<TSource, TProp>(this TSource source, Expression<Func<TSource, TProp>> propertySelector, Action onChanged)
			where TSource : INotifyPropertyChanged
		{
			if (source == null) throw new ArgumentNullException("source");
			if (propertySelector == null) throw new ArgumentNullException("propertySelector");
			if (onChanged == null) throw new ArgumentNullException("onChanged");

			var subscribedPropertyName = GetPropertyName(propertySelector);

			PropertyChangedEventHandler handler = (s, e) =>
			{
				if (string.Equals(e.PropertyName, subscribedPropertyName, StringComparison.InvariantCulture))
					onChanged();
			};

			source.PropertyChanged += handler;

			return Disposable.Create(() => source.PropertyChanged -= handler);
		}

		/// <summary>
		/// Subscribes the given handler to the <see cref="INotifyPropertyChanging.PropertyChanging"/> event.
		/// </summary>
		/// <typeparam name="TSource">The type of the object providing the event.</typeparam>
		/// <typeparam name="TProp">The type of the property.</typeparam>
		/// <param name="source">The object providing the event.</param>
		/// <param name="propertySelector">A selector taking the given object and selecting the property to subscribe.</param>
		/// <param name="onChanging">The handler to call when the property is changing.</param>
		/// <returns>A subscription token that, when disposed, will unsubscribe the handler.</returns>
		public static IDisposable SubscribeToPropertyChanging<TSource, TProp>(this TSource source, Expression<Func<TSource, TProp>> propertySelector, Action onChanging)
			where TSource : INotifyPropertyChanging
		{
			if (source == null) throw new ArgumentNullException("source");
			if (propertySelector == null) throw new ArgumentNullException("propertySelector");
			if (onChanging == null) throw new ArgumentNullException("onChanged");

			var subscribedPropertyName = GetPropertyName(propertySelector);

			PropertyChangingEventHandler handler = (s, e) =>
			{
				if (string.Equals(e.PropertyName, subscribedPropertyName, StringComparison.InvariantCulture))
					onChanging();
			};

			source.PropertyChanging += handler;

			return Disposable.Create(() => source.PropertyChanging -= handler);
		}

		private static string GetPropertyName<TSource, TProp>(Expression<Func<TSource, TProp>> propertySelector)
		{
			var memberExpr = propertySelector.Body as MemberExpression;

			if (memberExpr == null) throw new ArgumentException("must be a member accessor", "propertySelector");

			var propertyInfo = memberExpr.Member as PropertyInfo;

			if (propertyInfo == null || propertyInfo.DeclaringType != typeof(TSource))
				throw new ArgumentException("must yield a single property on the given object", "propertySelector");

			return propertyInfo.Name;
		}
	}
}
