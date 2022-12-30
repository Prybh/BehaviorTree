using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;

namespace Prybh 
{
    public class BehaviorTreeView : GraphView 
    {
        public Action<NodeView> OnNodeSelected;
        public new class UxmlFactory : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> {}

        private BehaviorTree tree;
        private List<Node> treeNodes = new List<Node>();

        public BehaviorTreeView() 
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(BehaviorTreeSettings.settings.BehaviorTreeStyle);

            Undo.undoRedoPerformed += OnUndoRedo;
        }

        private void OnUndoRedo()
        {
            PopulateView(tree);
            AssetDatabase.SaveAssets();
        }

        public NodeView FindNodeView(Node node) 
        {
            foreach (var n in nodes)
            {
                NodeView nodeView = n as NodeView;
                if (nodeView != null && nodeView.node == node)
                {
                    return nodeView;
                }
            }
            return null;
        }

        internal void PopulateView(BehaviorTree tree) 
        {
            this.tree = tree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            if (tree.GetRootNode() == null)
            {
                tree.SetRootNode(CreateNodeOnTree(typeof(RootNode)) as RootNode);
                EditorUtility.SetDirty(tree);
                AssetDatabase.SaveAssets();
            }

            treeNodes = tree.GetRootNode().GetAllChildren();            

            // Creates node view
            treeNodes.ForEach(n => CreateNodeView(n));

            // Create edges
            treeNodes.ForEach(n =>
            {
                NodeView parentView = FindNodeView(n);
                var children = n.GetChildren();
                if (children != null)
                {
                    children.ForEach(c =>
                    {
                        if (c != null)
                        {
                            NodeView childView = FindNodeView(c);
                            Edge edge = parentView.output.ConnectTo(childView.input);
                            AddElement(edge);
                        }
                    });
                }
            });
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) 
        {
            return ports.ToList().Where(endPort =>
            endPort.direction != startPort.direction &&
            endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) 
        {
            if (graphViewChange.elementsToRemove != null) 
            {
                graphViewChange.elementsToRemove.ForEach(elem => {
                    NodeView nodeView = elem as NodeView;
                    if (nodeView != null) 
                    {
                        DeleteNodeFromTree(nodeView.node);
                    }

                    Edge edge = elem as Edge;
                    if (edge != null) 
                    {
                        NodeView parentView = edge.output.node as NodeView;
                        NodeView childView = edge.input.node as NodeView;

                        if (parentView.node is DecoratorNode decorator)
                        {
                            Undo.RecordObject(decorator, "Behaviour Tree (RemoveChild)");
                            decorator.SetChild(null);
                            EditorUtility.SetDirty(decorator);
                        }
                        else if (parentView.node is CompositeNode composite)
                        {
                            Undo.RecordObject(composite, "Behaviour Tree (RemoveChild)");
                            composite.RemoveChild(childView.node);
                            EditorUtility.SetDirty(composite);
                        }
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null) 
            {
                graphViewChange.edgesToCreate.ForEach(edge => {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;

                    if (parentView.node is DecoratorNode decorator)
                    {
                        Undo.RecordObject(decorator, "Behaviour Tree (AddChild)");
                        decorator.SetChild(childView.node);
                        EditorUtility.SetDirty(decorator);
                    }
                    else if (parentView.node is CompositeNode composite)
                    {
                        Undo.RecordObject(composite, "Behaviour Tree (AddChild)");
                        composite.AddChild(childView.node);
                        EditorUtility.SetDirty(composite);
                    }
                });
            }

            nodes.ForEach((n) => {
                NodeView view = n as NodeView;
                view.SortChildren();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) 
        {
            Vector2 nodePosition = this.ChangeCoordinatesTo(contentViewContainer, evt.localMousePosition);
            {
                var types = TypeCache.GetTypesDerivedFrom<ActionNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Action]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Composite]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<DecoratorNode>();
                foreach (var type in types) {
                    evt.menu.AppendAction($"[Decorator]/{type.Name}", (a) => CreateNode(type, nodePosition));
                }
            }
        }

        private Node CreateNodeOnTree(Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();

            Undo.RecordObject(tree, "Behaviour Tree (CreateNode)");

            treeNodes.Add(node);

            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, tree);
            }

            Undo.RegisterCreatedObjectUndo(node, "Behaviour Tree (CreateNode)");

            AssetDatabase.SaveAssets();

            return node;
        }

        private void CreateNode(Type type, Vector2 position) 
        {
            Node node = CreateNodeOnTree(type);
            node.position = position;
            CreateNodeView(node);
        }

        private void DeleteNodeFromTree(Node node)
        {
            Undo.RecordObject(tree, "Behaviour Tree (DeleteNode)");
            treeNodes.Remove(node);

            AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        private void CreateNodeView(Node node) 
        {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }
    }
}