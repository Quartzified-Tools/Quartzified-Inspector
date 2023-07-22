using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;

namespace Quartzified.Tools.Inspector
{
    internal class QuartzifiedInspector
    {
        public class EditorElement
        {
            public VisualElement element;
            public VisualElement inspector;
            public Editor editor;
            public int index;
            
            public Object target => editor.target;
        }

        internal static QuartzifiedInspector LastInjected;
        
        internal EditorWindow propertyEditor;
        internal ActiveEditorTracker tracker;
        internal VisualElement root;
        internal VisualElement mainContainer;
        internal VisualElement editorsList;
        internal VisualElement gameObjectEditor;
        internal TooltipElement tooltip { get; private set; }

        internal List<EditorElement> editors;

        QuartzifiedInspector(EditorWindow propertyEditor)
        {
            this.propertyEditor = propertyEditor;
            this.tracker = PropertyEditorRef.GetTracker(propertyEditor);

            this.root = propertyEditor.rootVisualElement;
            this.mainContainer = root.Query(className: "unity-inspector-main-container").First();
            this.editorsList = root.Query(className: "unity-inspector-editors-list").First();

            EditorApplication.update += EditorAwake;    
            propertyEditor.autoRepaintOnSceneChange = true;
            Undo.undoRedoPerformed += RebuildToolbar;
        }

        public static bool TryInject(EditorWindow propertyEditor, out QuartzifiedInspector inspector)
        {
            inspector = new QuartzifiedInspector(propertyEditor);
            return inspector.TryInject();
        }
        
        public bool TryInject()
        {
            LastInjected = this;
            
            if (editorsList == null)
                return false;

            tooltip = root.Query<TooltipElement>("QuartzifiedInspectorTooltip").First();

            if(tooltip == null)
            {
                tooltip = new TooltipElement { name = "QuartzifiedInspectorTooltip" };
                root.Add(tooltip);
            }
            
            root.styleSheets.Add(UIResources.Asset.scrollViewStyle);
            editorsList.styleSheets.Add(UIResources.Asset.componentsHeaderStyle);
            
            var scrollView = root.Query(className: "unity-inspector-root-scrollview").First();
            var contentViewport = scrollView.Get("unity-content-viewport");
            
            contentViewport.RegisterCallback<GeometryChangedEvent>(_ => contentViewport.style.marginRight = 0);
                
            RetrieveEditorElements();
            
            var hasMainToolbar = mainContainer.Query<InspectorMainToolbar>().HasAny();
            if (!hasMainToolbar)
            {
                var inspectorToolbar = new InspectorMainToolbar(propertyEditor);
                inspectorToolbar.RegisterCallback<DetachFromPanelEvent>(_ => InspectorInjection.TryReinjectWindow(propertyEditor));
                InspectorInjection.onInspectorRebuild?.Invoke(propertyEditor, InspectorInjection.RebuildStage.EndBeforeRepaint);
            }
            
            if (gameObjectEditor == null)
                return false;

            var componentsToolbar = new InspectorComponentsToolbar();
            componentsToolbar.RegisterCallback<DetachFromPanelEvent>(_ => InspectorInjection.TryReinjectWindow(propertyEditor));
            
            gameObjectEditor.Insert(2, componentsToolbar);
            
            return true;
        }

        public void OnRebuildContent(EditorWindow window, InspectorInjection.RebuildStage stage)
        {
            if (window != propertyEditor)
                return;

            if(stage == InspectorInjection.RebuildStage.EndBeforeRepaint)
                RebuildToolbar();
        }


        public void RebuildToolbar()
        { 
            RetrieveEditorElements();
            var toolbar = root.Query<InspectorComponentsToolbar>().First();
            toolbar?.Rebuild();
        }

        void RetrieveEditorElements()
        {
            this.editors = new List<EditorElement>();

            var allEditors = editorsList.Children().ToList();

            foreach (var editorElement in allEditors)
            {
                if (editorElement.GetType() != EditorElementRef.type)
                    continue;

                var inspector = editorElement.Get(".unity-inspector-element");
                if (inspector == null)
                    continue;

                var editor = EditorElementRef.GetEditor(editorElement);
                var editorIndex = EditorElementRef.GetEditorIndex(editorElement);

                if (editor == null)
                    continue;

                var target = editor.target;
                var isGo = target is GameObject;
                var isTransform = target is Transform;

                if(!inspector.ClassListContains("inspector"))
                {
                    SetupInspectorElement();
                    inspector.AddToClassList("inspector");
                }

                void SetupInspectorElement()
                {
                    inspector.RegisterCallback<GeometryChangedEvent>(evt =>
                    {
                        var isExpanded = InternalEditorUtility.GetIsInspectorExpanded(target);
                        editorElement.EnableInClassList("is-expanded", isExpanded);
                    });
                }

                if (isGo)
                    gameObjectEditor = editorElement;

                editorElement.EnableInClassList("game-object", isGo);
                editorElement.EnableInClassList("transform", isTransform);
                editorElement.EnableInClassList("component", target is Component);
                editorElement.EnableInClassList("material", target is Material);

                if (!isGo)
                    editors.Add(new EditorElement
                    {
                        element = editorElement,
                        inspector = inspector,
                        editor = editor,
                        index = editorIndex
                    });
            }
        }

        void EditorAwake()
        {
            for (int i = 0; i < LastInjected.editors.Count; i++)
            {
                OnEditorUpdate();
            }

            EditorApplication.update -= EditorAwake;
        }


        public void OnEditorUpdate()
        {
            if(propertyEditor != null)
            {
                propertyEditor.titleContent.text = "Quartzified Inspector";
            }
        }

        public static string GetInspectorTitle(Object obj)
        {
            return ObjectNames.GetInspectorTitle(obj).Replace("(Script)", "");
        }
    }
}