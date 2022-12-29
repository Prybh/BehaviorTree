using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

namespace Prybh 
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node 
    {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;

        public NodeView(Node node) 
            : base(AssetDatabase.GetAssetPath(BehaviorTreeSettings.GetOrCreateSettings().NodeXml)) 
        {
            this.node = node;
            this.node.name = node.GetType().Name;
            this.title = node.name.Replace("(Clone)", "").Replace("Node", "");
            this.viewDataKey = node.guid;

            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClasses();
            SetupDataBinding();
        }

        private void SetupDataBinding() 
        {
            Label descriptionLabel = this.Q<Label>("description");
            descriptionLabel.bindingPath = "description";
            descriptionLabel.Bind(new SerializedObject(node));
        }

        private void SetupClasses() 
        {
            if (node is ActionNode) 
            {
                AddToClassList("action");
            } 
            else if (node is CompositeNode) 
            {
                AddToClassList("composite");
            }
            else if (node is RootNode) // Check before DecoratorNode
            {
                AddToClassList("root");
            }
            else if (node is DecoratorNode) 
            {
                AddToClassList("decorator");
            }
        }

        private void CreateInputPorts()
        {
            if (node is RootNode) // Check before DecoratorNode
            {
                // Nothing
            }
            else if (node is ActionNode)
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }
            else if (node is CompositeNode) 
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            } 
            else if (node is DecoratorNode) 
            {
                input = new NodePort(Direction.Input, Port.Capacity.Single);
            }

            if (input != null) 
            {
                input.portName = "";
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts() 
        {
            if (node is ActionNode) 
            {
                // Nothing
            } 
            else if (node is CompositeNode) 
            {
                output = new NodePort(Direction.Output, Port.Capacity.Multi);
            }
            else if (node is DecoratorNode) 
            {
                output = new NodePort(Direction.Output, Port.Capacity.Single);
            }

            if (output != null) 
            {
                output.portName = "";
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) 
        {
            base.SetPosition(newPos);
            Undo.RecordObject(node, "Behaviour Tree (Set Position)");
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        public override void OnSelected() 
        {
            base.OnSelected();
            if (OnNodeSelected != null) 
            {
                OnNodeSelected.Invoke(this);
            }
        }

        public void SortChildren() 
        {
            if (node is CompositeNode composite) 
            {
                composite.SortChildren();
            }
        }

        public void UpdateState() 
        {
            RemoveFromClassList("running");
            RemoveFromClassList("failure");
            RemoveFromClassList("success");

            if (Application.isPlaying) 
            {
                switch (node.state) 
                {
                    case Node.State.Running:
                        if (node.started) 
                        {
                            AddToClassList("running");
                        }
                        break;
                    case Node.State.Failure:
                        AddToClassList("failure");
                        break;
                    case Node.State.Success:
                        AddToClassList("success");
                        break;
                }
            }
        }
    }
}