using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

class BehaviorTreeSettings : ScriptableObject 
{
    public VisualTreeAsset BehaviorTreeXml;
    public StyleSheet BehaviorTreeStyle;
    public VisualTreeAsset NodeXml;
    public StyleSheet NodeStyle;

    public TextAsset scriptTemplateActionNode;
    public TextAsset scriptTemplateCompositeNode;
    public TextAsset scriptTemplateDecoratorNode;

    public string newNodeBasePath = "Assets/";

    private static BehaviorTreeSettings m_settings = null;
    public static BehaviorTreeSettings settings => m_settings;

    private void OnEnable()
    {
        m_settings = this;
    }

    public static BehaviorTreeSettings FindOrCreateSettings()
    {
        if (m_settings == null)
        {
            m_settings = FindSettings(); 
            if (m_settings == null)
            {
                m_settings = CreateSettings();
            }
        }
        return m_settings;
    }

    private static BehaviorTreeSettings FindSettings()
    {
        var guids = AssetDatabase.FindAssets("t:BehaviorTreeSettings");
        if (guids.Length > 1) 
        {
            Debug.LogWarning($"Found multiple settings files, using the first.");
        }

        switch (guids.Length) 
        {
            case 0:
                return null;
            default:
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<BehaviorTreeSettings>(path);
        }
    }

    private static BehaviorTreeSettings CreateSettings()
    {
        var settings = ScriptableObject.CreateInstance<BehaviorTreeSettings>();

        string packageEditorPath = "Packages/com.prybh.BehaviorTree/Editor/";

        settings.BehaviorTreeXml = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(packageEditorPath + "UIBuilder/BehaviorTreeEditor.uxml", typeof(VisualTreeAsset));
        settings.BehaviorTreeStyle = (StyleSheet)AssetDatabase.LoadAssetAtPath(packageEditorPath + "UIBuilder/BehaviorTreeEditorStyle.uss", typeof(StyleSheet));
        settings.NodeXml = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(packageEditorPath + "UIBuilder/NodeView.uxml", typeof(VisualTreeAsset));
        settings.NodeStyle = (StyleSheet)AssetDatabase.LoadAssetAtPath(packageEditorPath + "UIBuilder/NodeViewStyle.uss", typeof(StyleSheet));

        settings.scriptTemplateActionNode = (TextAsset)AssetDatabase.LoadAssetAtPath(packageEditorPath + "ScriptTemplates/NewActionNode.cs.txt", typeof(TextAsset));
        settings.scriptTemplateCompositeNode = (TextAsset)AssetDatabase.LoadAssetAtPath(packageEditorPath + "ScriptTemplates/NewCompositeNode.cs.txt", typeof(TextAsset));
        settings.scriptTemplateDecoratorNode = (TextAsset)AssetDatabase.LoadAssetAtPath(packageEditorPath + "ScriptTemplates/NewDecoratorNode.cs.txt", typeof(TextAsset));

        AssetDatabase.CreateAsset(settings, "Assets/BehaviorTreeSettings.asset");
        AssetDatabase.SaveAssets();

        return settings;
    }

    [SettingsProvider]
    public static SettingsProvider CreateSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
        var provider = new SettingsProvider("Project/MyCustomUIElementsSettings", SettingsScope.Project)
        {
            label = "BehaviorTree",
            // activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (searchContext, rootElement) =>
            {
                var settings = new SerializedObject(BehaviorTreeSettings.settings);

                // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                // isn't called because the SettingsProvider uses the UIElements drawing framework.
                var title = new Label()
                {
                    text = "Behavior Tree Settings"
                };
                title.AddToClassList("title");
                rootElement.Add(title);

                var properties = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
                properties.AddToClassList("property-list");
                rootElement.Add(properties);

                properties.Add(new InspectorElement(settings));

                rootElement.Bind(settings);
            },
        };

        return provider;
    }
}
