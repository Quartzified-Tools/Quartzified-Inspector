using UnityEngine.UIElements;

namespace Quartzified.Tools.Inspector
{
    internal class InspectorElement : VisualElement
    {
        protected internal QuartzifiedInspector Inspector { get; internal set; } = QuartzifiedInspector.LastInjected;
    }
}