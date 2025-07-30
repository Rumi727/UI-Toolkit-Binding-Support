#nullable enable
#if UNITY_EDITOR
using Rumi.CustomBinding.Editor.UIElements;
using UnityEditor;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(MyStruct))]
public class MyStructPropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        MyStructField myStructField = new MyStructField(property.displayName).SetPropertyPath(property);
        myStructField.ConfigureFieldStyles();
        
        property.Next(true);
        myStructField.textField.bindingPath = property.propertyPath;
        
        property.Next(false);
        myStructField.floatField.bindingPath = property.propertyPath;

        return myStructField;
    }
}
#endif