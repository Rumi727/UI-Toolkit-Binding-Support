#nullable enable
#if UNITY_EDITOR
using Rumi.CustomBinding.Editor;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MyChild))]
public class MyChildPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        MyChildField myChildField = new MyChildField(property.displayName).SetPropertyPath(property);
        myChildField.ConfigureFieldStyles();
        
        property.Next(true);
        myChildField.textField.bindingPath = property.propertyPath;
        
        property.Next(false);
        myChildField.floatField.bindingPath = property.propertyPath;
        
        property.Next(false);
        myChildField.objectField.bindingPath = property.propertyPath;

        return myChildField;
    }
}
#endif