#nullable enable
#if UNITY_EDITOR
using Rumi.CustomBinding.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MyParent))]
public class MyParentPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        MyParentField MyParentField = new MyParentField(property.displayName).SetPropertyPath(property);
        MyParentField.ConfigureFieldStyles();
        
        property.Next(true);
        MyParentField.textField.bindingPath = property.propertyPath;
        
        property.Next(false);
        MyParentField.floatField.bindingPath = property.propertyPath;

        return MyParentField;
    }
}
#endif