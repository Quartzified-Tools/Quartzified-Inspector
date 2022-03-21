using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;

namespace Quartzified.Custom.Inspector
{
    internal static class VisualElementUtils
    {
		static readonly PropertyInfo worldClip = Property(typeof(VisualElement), "worldClip");

		public static Rect GetWorldClip(this VisualElement element)
        {
			if (worldClip != null)
				return (Rect)worldClip.GetValue(element);
			else
				return Rect.zero;
        }

		public static T FindIncludingBaseTypes<T>(Type type, Func<Type, T> func) where T : class
		{
			T t;
			for (; ; )
			{
				t = func(type);
				if (t != null)
				{
					break;
				}
				type = type.BaseType;
				if (type == null)
				{
					goto Block_1;
				}
			}
			return t;
		Block_1:
			return default(T);
		}

		public static PropertyInfo Property(Type type, string name)
		{
			if (type == null)
			{
				return null;
			}
			if (name == null)
			{
				return null;
			}
			PropertyInfo propertyInfo = FindIncludingBaseTypes<PropertyInfo>(type, (Type t) => t.GetProperty(name, all));
			return propertyInfo;
		}

		public static readonly BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
	}


}
