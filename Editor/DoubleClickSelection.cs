using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Prybh 
{
    public class DoubleClickSelection : MouseManipulator 
    {
        private double time;
        private double doubleClickDuration = 0.3;

        public DoubleClickSelection() 
        {
            time = EditorApplication.timeSinceStartup;
        }

        protected override void RegisterCallbacksOnTarget() 
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }

        protected override void UnregisterCallbacksFromTarget() 
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            var graphView = target as BehaviorTreeView;
            if (graphView == null)
                return;

            double duration = EditorApplication.timeSinceStartup - time;
            if (duration < doubleClickDuration) 
            {
                SelectChildren(evt);
            }

            time = EditorApplication.timeSinceStartup;
        }

        private void SelectChildren(MouseDownEvent evt) 
        {
            var graphView = target as BehaviorTreeView;
            if (graphView == null)
                return;

            if (!CanStopManipulation(evt))
                return;

            NodeView clickedElement = evt.target as NodeView;
            if (clickedElement == null) 
            {
                var ve = evt.target as VisualElement;
                clickedElement = ve.GetFirstAncestorOfType<NodeView>();
                if (clickedElement == null)
                    return;
            }

            List<Node> allChildren = clickedElement.node.GetAllChildren();
            foreach (var child in allChildren)
            {
                var view = graphView.FindNodeView(child);
                graphView.AddToSelection(view);
            }
        }
    }
}