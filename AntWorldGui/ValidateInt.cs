namespace AntWorldGui;

public static class ValidateInt
{
    public static Maybe<int> GetMaybeInt(this string str)
    {
        if (string.IsNullOrWhiteSpace(str) || !int.TryParse(str, out var res)) 
            return new Nothing<int>();
        return new Just<int>(res);
    }
}