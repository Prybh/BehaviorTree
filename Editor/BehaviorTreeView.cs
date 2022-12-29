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
        private BehaviorTreeSettings settings;
        private List<Node> treeNodes = new List<Node>();

        public struct ScriptTemplate 
        {
            public TextAsset templateFile;
            public string defaultFileName;
            public string subFolder;
        }

        public ScriptTemplate[] scriptFileAssets = {
            
            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateActionNode, defaultFileName="NewActionNode.cs", subFolder="Actions" },
            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateCompositeNode, defaultFileName="NewCompositeNode.cs", subFolder="Composites" },
            new ScriptTemplate{ templateFile=BehaviorTreeSettings.GetOrCreateSettings().scriptTemplateDecoratorNode, defaultFileName="NewDecoratorNode.cs", subFolder="Decorators" },
        };

        public BehaviorTreeView() 
        {
            settings = BehaviorTreeSettings.GetOrCreateSettings();

            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new DoubleClickSelection());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(settings.BehaviorTreeStyle);

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

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt) {

            //base.BuildContextualMenu(evt);

            // New script functions
            evt.menu.AppendAction($"Create Script.../New Action Node", (a) => CreateNewScript(scriptFileAssets[0]));
            evt.menu.AppendAction($"Create Script.../New Composite Node", (a) => CreateNewScript(scriptFileAssets[1]));
            evt.menu.AppendAction($"Create Script.../New Decorator Node", (a) => CreateNewScript(scriptFileAssets[2]));
            evt.menu.AppendSeparator();

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

        private void SelectFolder(string path) 
        {
            // https://forum.unity.com/threads/selecting-a-folder-in-the-project-via-button-in-editor-window.355357/
            // Check the path has no '/' at the end, if it does remove it,
            // Obviously in this example it doesn't but it might
            // if your getting the path some other way.

            if (path[path.Length - 1] == '/')
                path = path.Substring(0, path.Length - 1);

            // Load object
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));

            // Select the object in the project folder
            Selection.activeObject = obj;

            // Also flash the folder yellow to highlight it
            EditorGUIUtility.PingObject(obj);
        }

        private void CreateNewScript(ScriptTemplate template) 
        {
            SelectFolder($"{settings.newNodeBasePath}/{template.subFolder}");
            var templatePath = AssetDatabase.GetAssetPath(template.templateFile);
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, template.defaultFileName);
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

            //AssetDatabase.RemoveObjectFromAsset(node);
            Undo.DestroyObjectImmediate(node);

            AssetDatabase.SaveAssets();
        }

        private void CreateNodeView(Node node) 
        {
            NodeView nodeView = new NodeView(node);
            nodeView.OnNodeSelected = OnNodeSelected;
            AddElement(nodeView);
        }

        public void UpdateNodeStates() 
        {
            nodes.ForEach(n => {
                NodeView view = n as NodeView;
                view.UpdateState();
            });
        }
    }
}