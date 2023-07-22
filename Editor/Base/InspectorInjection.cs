using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Quartzified.Tools.Inspector
{
    [InitializeOnLoad]
    internal static class InspectorInjection
    {
        public enum RebuildStage
        {
            EndBeforeRepaint,
            PostfixAfterRepaint
        }

        const int UpdateRate = 10;

        internal static Action<EditorWindow, RebuildStage> onInspectorRebuild;

        static int editorFrames = UpdateRate;
        static EditorWindow lastFocusedWindow;
        
        static Dictionary<EditorWindow, QuartzifiedInspector> InjectedInspectors = new Dictionary<EditorWindow, QuartzifiedInspector>();
        
        
        static InspectorInjection()
        {
            EditorApplication.update += OnEditorUpdate;
            Selection.selectionChanged += FindInspectorWindowsAndInject;
        }

        static void OnEditorUpdate()
        {
            editorFrames = (editorFrames + 1) % UpdateRate; // only run every N frames
            if (editorFrames != 0) 
                return;

            var focusedWindow = EditorWindow.focusedWindow ?? EditorWindow.mouseOverWindow;

            if (focusedWindow == lastFocusedWindow)
                return;

            lastFocusedWindow = focusedWindow;

            FindInspectorWindowsAndInject();
        }
        
        static void FindInspectorWindowsAndInject()
        {
            //var windows = InspectorReflection.GetInspectors()
            //    .Where(x => x.GetType() == InspectorReflection.WindowType);
            var windows = Resources.FindObjectsOfTypeAll(PropertyEditorRef.type);
            
            foreach (EditorWindow window in windows)
            {
                if (!InjectedInspectors.ContainsKey(window))
                {
                    if (QuartzifiedInspector.TryInject(window, out var inspector))
                    {
                        onInspectorRebuild += inspector.OnRebuildContent;
                        InjectedInspectors.Add(window, inspector);
                    }

                }
            }
        }

        internal static void TryReinjectWindow(EditorWindow window)
        {
            if(InjectedInspectors.TryGetValue(window, out var inspector))
            {
                onInspectorRebuild -= inspector.OnRebuildContent;
                InjectedInspectors.Remove(window);
            }

            FindInspectorWindowsAndInject();
            EditorApplication.delayCall += FindInspectorWindowsAndInject;
        }

        internal static bool TryGetInspector(EditorWindow window, out QuartzifiedInspector inspector)
        {
            return InjectedInspectors.TryGetValue(window, out inspector);
        }
    }
}