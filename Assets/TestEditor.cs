#nullable enable
#if UNITY_EDITOR && !UNITY_2022_1_OR_NEWER
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomEditor(typeof(Test))]
public class TestEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement element = new VisualElement();
        element.Add(new PropertyField { bindingPath = nameof(Test.myStruct) });
        element.Add(new PropertyField { bindingPath = nameof(Test.rect) });
        
        return element;
    }
}
#endif