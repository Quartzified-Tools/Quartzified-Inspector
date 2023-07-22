using UnityEditor;
using UnityEngine;

namespace Quartzified.Tools.Inspector
{
    internal static class AssetUtils
    {
        public static T LoadFromGUID<T>(string guid) where T : Object
        {
            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
        }
    }
}