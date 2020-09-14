using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;
using Object = UnityEngine.Object;
using GameStateMachineCore;

public class GameStatePrefabsWindow : EditorWindow    
{
    private Object _selection;
    private string _filterValue = string.Empty;

    [MenuItem("Window/Ishimine/GameStatePrefabs", priority = 1)]
    public static void ShowExample()
    {
        GameStatePrefabsWindow wnd = GetWindow<GameStatePrefabsWindow>();
        wnd.titleContent = new GUIContent("GameStatePrefabs");
    }

    public void OnEnable()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree =
            Resources.Load<VisualTreeAsset>("GameStatePrefabs_Main");

        visualTree.CloneTree(root);
    
        // A stylesheet can be added to a VisualElement.    
        // The style will be applied to the VisualElement and all of its children.
        var styleSheet =
             Resources.Load<StyleSheet>("GameStatePrefabs_Style");

        root.styleSheets.Add(styleSheet);

        PopulatePresetList();
    }

    private void Update()
    {
        TextField searchField = rootVisualElement.Q<TextField>("SearchField");
        if (searchField.value == null || searchField.value == _filterValue) return;

        _filterValue = searchField.value;
        PopulatePresetList();
    }

    private void PopulatePresetList()
    {
        ListView list = rootVisualElement.Q<ListView>("ListView");
        list.Clear();

        GameStatePrefabsManager gsm = GameStatePrefabsManager.Instance;

        string lowerFilterValue = this._filterValue.ToLowerInvariant();
        for (int i = 0; i < gsm.gameStatePrefabs.Count; i++)
        {
            string fieldName = gsm.gameStatePrefabs[i].name.ToLowerInvariant();
            if (!fieldName.Contains(lowerFilterValue)) continue;

            VisualElement listContainer = new VisualElement {name = "ListContainer"};
            Button button = new Button {text = gsm.gameStatePrefabs[i].name.Replace("_Prefabs",string.Empty)};

            //Applying a CSS class to an element
            button.AddToClassList("ListLabel");

            listContainer.Add(button);

            //Inserting element into list
            list.Insert(list.childCount, listContainer);
            GameStatePrefabReferences value = gsm.gameStatePrefabs[i];

            if (_selection == value)
                button.style.backgroundColor = new StyleColor(new Color(0.25f, 0.35f, 0.6f, 1f));

            button.clicked += () => UpdateSelection(rootVisualElement, value);
        }
    }

    private void UpdateSelection(VisualElement root, Object target)
    {
        _selection = target;
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(target);
        root.Q<IMGUIContainer>("Target").onGUIHandler = () => editor.OnInspectorGUI();
        PopulatePresetList();
    }
}
