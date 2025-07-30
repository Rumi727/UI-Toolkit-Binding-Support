#nullable enable
#if UNITY_EDITOR || UNITY_6000_1_OR_NEWER
using UnityEditor.UIElements;
using UnityEngine.UIElements;

public class MyChildField : BaseField<MyChild>
{
    public readonly VisualElement visualInput;
    
    public readonly TextField textField;
    public readonly FloatField floatField;
    public readonly ObjectField objectField;
    
    public MyChildField(string label) : base(label, new VisualElement())
    {
        AddToClassList(BaseCompositeField<int, IntegerField, int>.ussClassName);
        visualInput = this.Q<VisualElement>(className: inputUssClassName);
        
        textField = new TextField(nameof(MyChild.name));
        floatField = new FloatField(nameof(MyChild.value));
        objectField = new ObjectField(nameof(MyChild.uniObject));

        textField.labelElement.style.minWidth = StyleKeyword.Auto;
        floatField.labelElement.style.minWidth = StyleKeyword.Auto;
        objectField.labelElement.style.minWidth = StyleKeyword.Auto;
        
        visualInput.Add(textField);
        visualInput.Add(floatField);
        visualInput.Add(objectField);
        
        textField.RegisterValueChangedCallback(x => value.name = x.newValue );
        floatField.RegisterValueChangedCallback(x => value.value = x.newValue );
        objectField.RegisterValueChangedCallback(x => value.uniObject = x.newValue );
    }
    
    public override void SetValueWithoutNotify(MyChild newValue)
    {
        base.SetValueWithoutNotify(newValue);
        
        textField.SetValueWithoutNotify(newValue.name);
        floatField.SetValueWithoutNotify(newValue.value);
        objectField.SetValueWithoutNotify(newValue.uniObject);
    }
}
#endif