using System;

namespace BananaMpq.View.Infrastructure
{
    public static class DelegateUtil
    {
        public static T ConvertTo<T>(this Delegate d) where T: class
        {
            return (T)(object)Delegate.CreateDelegate(typeof(T), d.Target, d.Method);
        }
    }
}