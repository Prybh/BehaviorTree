using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.Callbacks;

namespace Prybh 
{
    public class BehaviorTreeEditor : EditorWindow 
    {
        BehaviorTreeView treeView;
        BehaviorTree tree;
        InspectorView inspectorView;
        IMGUIContainer blackboardView;
        ToolbarMenu toolbarMenu;
        TextField treeNameField;
        TextField locationPathField;
        Button createNewTreeButton;
        VisualElement overlay;
        BehaviorTreeSettings settings;

        SerializedObject treeObject;
        SerializedProperty blackboardProperty;

        [MenuItem("Window/BehaviorTreeEditor")]
        public static void OpenWindow() 
        {
            BehaviorTreeEditor wnd = GetWindow<BehaviorTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviorTreeEditor");
            wnd.minSize = new Vector2(800, 600);
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line) 
        {
            if (Selection.activeObject is BehaviorTree) 
            {
                OpenWindow();
                return true;
            }
            return false;
        }

        private List<T> LoadAssets<T>() where T : UnityEngine.Object 
        {
            string[] assetIds = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            List<T> assets = new List<T>();
            foreach (var assetId in assetIds) 
            {
                string path = AssetDatabase.GUIDToAssetPath(assetId);
                T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                assets.Add(asset);
            }
            return assets;
        }

        public void CreateGUI() 
        {
            settings = BehaviorTreeSettings.GetOrCreateSettings();

            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // Import UXML
            var visualTree = settings.BehaviorTreeXml;
            visualTree.CloneTree(root);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = settings.BehaviorTreeStyle;
            root.styleSheets.Add(styleSheet);

            // Main treeview
            treeView = root.Q<BehaviorTreeView>();
            treeView.OnNodeSelected = OnNodeSelectionChanged;

            // Inspector View
            inspectorView = root.Q<InspectorView>();

            // Blackboard view
            blackboardView = root.Q<IMGUIContainer>();
            blackboardView.onGUIHandler = () => {
                if (treeObject != null && treeObject.targetObject != null)
                {
                    treeObject.Update();
                    EditorGUILayout.PropertyField(blackboardProperty);
                    treeObject.ApplyModifiedProperties();
                }
            };

            // Toolbar assets menu
            toolbarMenu = root.Q<ToolbarMenu>();
            var BehaviorTrees = LoadAssets<BehaviorTree>();
            BehaviorTrees.ForEach(tree => {
                toolbarMenu.menu.AppendAction($"{tree.name}", (a) => {
                    Selection.activeObject = tree;
                });
            });
            toolbarMenu.menu.AppendSeparator();
            toolbarMenu.menu.AppendAction("New Tree...", (a) => CreateNewTree("New BehaviorTree"));

            // New Tree Dialog
            treeNameField = root.Q<TextField>("TreeName");
            locationPathField = root.Q<TextField>("LocationPath");
            overlay = root.Q<VisualElement>("Overlay");
            createNewTreeButton = root.Q<Button>("CreateButton");
            createNewTreeButton.clicked += () => CreateNewTree(treeNameField.value);

            if (tree == null) 
            {
                OnSelectionChange();
            } 
            else 
            {
                SelectTree(tree);
            }
        }

        private void OnEnable() 
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable() 
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange obj) 
        {
            switch (obj) 
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
            }
        }

        private void OnSelectionChange() 
        {
            EditorApplication.delayCall += () => {
                BehaviorTree tree = Selection.activeObject as BehaviorTree;
                if (!tree) 
                {
                    if (Selection.activeGameObject) 
                    {
                        BehaviorTreeRunner runner = Selection.activeGameObject.GetComponent<BehaviorTreeRunner>();
                        if (runner) 
                        {
                            tree = runner.GetTree();
                        }
                    }
                }

                SelectTree(tree);
            };
        }

        private void SelectTree(BehaviorTree newTree) 
        {
            if (treeView == null || newTree == null) 
            {
                return;
            }

            tree = newTree;

            overlay.style.visibility = Visibility.Hidden;

            treeView.PopulateView(tree);

            treeObject = new SerializedObject(tree);
            blackboardProperty = treeObject.FindProperty("blackboard");

            EditorApplication.delayCall += () => {
                treeView.FrameAll();
            };
        }

        private void OnNodeSelectionChanged(NodeView node)
        {
            inspectorView.UpdateSelection(node);
        }

        private void OnInspectorUpdate() 
        {
            treeView?.UpdateNodeStates();
        }

        private void CreateNewTree(string assetName) 
        {
            string path = System.IO.Path.Combine(locationPathField.value, $"{assetName}.asset");
            BehaviorTree tree = ScriptableObject.CreateInstance<BehaviorTree>();
            tree.name = treeNameField.ToString();
            AssetDatabase.CreateAsset(tree, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = tree;
            EditorGUIUtility.PingObject(tree);
        }
    }
}