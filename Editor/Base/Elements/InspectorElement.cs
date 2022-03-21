using UnityEngine.UIElements;

namespace Quartzified.Custom.Inspector
{
    internal class InspectorElement : VisualElement
    {
        protected internal QuartzifiedInspector Inspector { get; internal set; } = QuartzifiedInspector.LastInjected;
    }
}