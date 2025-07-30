#nullable enable
#if UNITY_EDITOR || UNITY_6000_1_OR_NEWER
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using UnityEngine.UIElements;

public class MyStructField : BaseField<MyStruct>
{
    public readonly VisualElement visualInput;
    
    public readonly TextField textField;
    public readonly FloatField floatField;
    
    public MyStructField(string label) : base(label, new VisualElement())
    {
        AddToClassList(BaseCompositeField<int, IntegerField, int>.ussClassName);
        visualInput = this.Q<VisualElement>(className: inputUssClassName);
        
        textField = new TextField(nameof(MyStruct.name));
        floatField = new FloatField(nameof(MyStruct.value));

        textField.labelElement.style.minWidth = StyleKeyword.Auto;
        floatField.labelElement.style.minWidth = StyleKeyword.Auto;
        
        visualInput.Add(textField);
        visualInput.Add(floatField);
        
        textField.RegisterValueChangedCallback(x => value = new MyStruct { name = x.newValue, value = value.value } );
        floatField.RegisterValueChangedCallback(x => value = new MyStruct { name = value.name, value = x.newValue } );
    }
    
    public override void SetValueWithoutNotify(MyStruct newValue)
    {
        base.SetValueWithoutNotify(newValue);
        
        textField.SetValueWithoutNotify(newValue.name);
        floatField.SetValueWithoutNotify(newValue.value);
    }
}
#endif