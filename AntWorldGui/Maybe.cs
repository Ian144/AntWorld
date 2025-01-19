using System;

namespace AntWorldGui;

public interface Maybe<T>
{
    bool HasValue();
    T Value();
}

public class Nothing<T> : Maybe<T>
{
    public bool HasValue()
    {
        return false;
    }

    public T Value()
    {
        throw new ApplicationException("invalid attempt to retrieve value from a Maybe instance");
    }

    public override string ToString()
    {
        return "Nothing";
    }
}

public class Just<T> : Maybe<T>
{
    private readonly T _val;

    public Just(T value)
    {
        _val = value;
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
        return _val.ToString();
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