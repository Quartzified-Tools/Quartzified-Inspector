using UnityEngine;

namespace Quartzified.Custom.Inspector
{
    internal class Index : ScriptableObject
    {
        public static Index Asset => asset ? asset : asset = Resources.Load<Index>("Index");
        private static Index asset;

        public UIResources UIResources;
    }
}