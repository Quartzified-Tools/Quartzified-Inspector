using System;
using System.Reflection;
using UnityEditor;
using static System.Linq.Expressions.Expression;
using Object = UnityEngine.Object;

namespace Quartzified.Tools.Inspector
{
    internal static class PropertyEditorRef
    {
        internal static Type type { get; } = typeof(Editor).Assembly.GetType("UnityEditor.PropertyEditor");

        static readonly PropertyInfo tracker = type.GetProperty("tracker");
        
        public static readonly Func<EditorWindow, Object> GetInspectedObject;
        
        static PropertyEditorRef()
        {
            var getInspectedObjectRef = type.GetMethod("GetInspectedObject", BindingFlags.NonPublic | BindingFlags.Instance);
            
            var windowParam = Parameter(typeof(EditorWindow));
            GetInspectedObject = Lambda<Func<EditorWindow, Object>>(Call(Convert(windowParam, type), getInspectedObjectRef), windowParam).Compile();
        }

        public static ActiveEditorTracker GetTracker(object propertyEditor)
        {
            return (ActiveEditorTracker)tracker.GetValue(propertyEditor);
        }
    }
}