using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace Igniter
{
    public class UntypedTuple : IEnumerable<object>, IEquatable<UntypedTuple>
    {
        private readonly object[] _values;
        private TypeConverter[] _converters;

        public UntypedTuple(params object[] values)
        {
            _values = new object[values.Length];
            Array.Copy(values, _values, values.Length);
        }

        public object this[int index]
        {
            get { return _values[index]; }
        }

        public int Length
        {
            get { return _values.Length; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return ((IEnumerable<object>)_values).GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as UntypedTuple);
        }

        public bool Equals(UntypedTuple other)
        {
            if (other == null) return false;
            if (ReferenceEquals(other, this)) return true;
            if (other._values.Length != _values.Length) return false;

            if (_converters == null)
                _converters = new TypeConverter[_values.Length];

            try
            {
                for (var i = 0; i < _values.Length; i++)
                {
                    object x = _values[i], y = other._values[i];

                    if (ReferenceEquals(x, y)) continue;
                    if (x == null || y == null) return false;

                    Type xType = x.GetType(), yType = y.GetType();

                    // Try to find a base type to compare with

                    if (xType.IsInstanceOfType(y))
                        if (x.Equals(y)) continue;
                        else return false;

                    if (yType.IsInstanceOfType(x))
                        if (y.Equals(x)) continue;
                        else return false;


                    // No base type... try type conversion.

                    var yConverted = GetTypeConverter(i).ConvertFrom(y);

                    if (!x.Equals(yConverted)) return false;
                }
            }
            catch
            {
                // Something must be incompatible, and therefore not equal.
                return false;
            }

            return true;
        }

        private TypeConverter GetTypeConverter(int index)
        {
            if (_converters == null)
                _converters = new TypeConverter[_values.Length];

            return _converters[index] ?? (_converters[index] = TypeDescriptor.GetConverter(_values[index]));
        }

        public override int GetHashCode()
        {
            return _values.GetHashCode();
        }

        #region TypedAs

        private T ApplyType<T>(int index)
        {
            var value = _values[index];

            if (value == null)
                if (typeof(T).IsClass)
                    return default(T); // aka null
                else
                    throw new InvalidCastException("Cannot cast null to type " + typeof(T).FullName);

            if (value is T) return (T)value;

            return (T)GetTypeConverter(index).ConvertTo(value, typeof(T));
        }

        public Tuple<T1> TypedAs<T1>()
        {
            if (_values.Length != 1)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0));
        }
        public Tuple<T1, T2> TypedAs<T1, T2>()
        {
            if (_values.Length != 2)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1));
        }
        public Tuple<T1, T2, T3> TypedAs<T1, T2, T3>()
        {
            if (_values.Length != 3)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1), ApplyType<T3>(2));
        }
        public Tuple<T1, T2, T3, T4> TypedAs<T1, T2, T3, T4>()
        {
            if (_values.Length != 4)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1), ApplyType<T3>(2), 
                ApplyType<T4>(3));
        }
        public Tuple<T1, T2, T3, T4, T5> TypedAs<T1, T2, T3, T4, T5>()
        {
            if (_values.Length != 5)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1), ApplyType<T3>(2), 
                ApplyType<T4>(3), ApplyType<T5>(4));
        }
        public Tuple<T1, T2, T3, T4, T5, T6> TypedAs<T1, T2, T3, T4, T5, T6>()
        {
            if (_values.Length != 6)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1), ApplyType<T3>(2), 
                ApplyType<T4>(3), ApplyType<T5>(4), ApplyType<T6>(5));
        }
        public Tuple<T1, T2, T3, T4, T5, T6, T7> TypedAs<T1, T2, T3, T4, T5, T6, T7>()
        {
            if (_values.Length != 7)
                throw new InvalidOperationException("Count mismatch.");

            return Tuple.Create(ApplyType<T1>(0), ApplyType<T2>(1), ApplyType<T3>(2), 
                ApplyType<T4>(3), ApplyType<T5>(4), ApplyType<T6>(5), ApplyType<T7>(6));
        }

        #endregion
    }
}