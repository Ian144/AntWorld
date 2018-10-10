using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntWorldGui
{
    public interface Maybe<T>
    {
        bool HasValue();
        T Value();
    }   

    public class Nothing<T> : Maybe<T>
    {
        public override string ToString()
        {
            return "Nothing";
        }

        public bool HasValue()
        {
            return false;
        }

        public T Value()
        {
            throw new ApplicationException("invalid attempt to retrieve value from a Maybe instance");
        }
    }

    
    public class Just<T> : Maybe<T>
    {
        private readonly T _val;

        public Just(T value)
        {
            this._val = value;
        }

        public bool HasValue()
        {
            return true;
        }

        public T Value()
        {
            return _val;
        }

        public override string ToString()
        {
            return this._val.ToString();
        }
    }

    public static class MaybeLinqProvider
    {
        public static Maybe<T> ToMaybe<T>(this T value)
        {
            return new Just<T>(value);
        }

        public static Maybe<B> Bind<A, B>(this Maybe<A> a, Func<A, Maybe<B>> func)
        {
            return a.HasValue() ? func(a.Value()) : new Nothing<B>();
        }

        public static Maybe<C> SelectMany<A, B, C>(this Maybe<A> a, Func<A, Maybe<B>> func, Func<A, B, C> select)
        {
            return a.Bind(aval =>
                    func(aval).Bind(bval =>
                    select(aval, bval).ToMaybe()));
        }
    }
}
