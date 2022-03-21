using System;
using System.Reflection;

namespace Quartzified.Custom.Inspector
{
    internal static class ExpressionUtils
    {
        public static void CreateDelegate<T>(MethodInfo method, out T lambda) where T : Delegate
        {
            lambda = (T)Delegate.CreateDelegate(typeof(T), method);
        }
    }
}