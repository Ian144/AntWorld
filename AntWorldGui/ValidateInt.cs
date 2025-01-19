namespace AntWorldGui;

public static class ValidateInt
{
    public static Maybe<int> GetMaybeInt(this string str)
    {
        if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str)) return new Nothing<int>();
        if (!int.TryParse(str, out var res))
            return new Nothing<int>();
        return new Just<int>(res);
    }
}