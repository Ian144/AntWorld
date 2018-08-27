using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AntWorldGui
{
    public static class ValidateInt
    {
        public static Maybe<int> GetMaybeInt( this string str )
        {
            if( String.IsNullOrEmpty(str) || String.IsNullOrWhiteSpace(str) )
            {
                return new Nothing<int>();
            }

            if (!int.TryParse(str, out int res))
                return new Nothing<int>();

            return new Just<int>(res);
        }
    }
}
