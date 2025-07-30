#nullable enable
#if UNITY_EDITOR || UNITY_6000_1_OR_NEWER
#if !UNITY_2022_1_OR_NEWER
using UnityEditor.UIElements;
#endif
using UnityEngine.UIElements;

public class MyParentField : BaseField<MyParent>
{
    public readonly VisualElement visualInput;
    
    public readonly TextField textField;
    public readonly FloatField floatField;
    
    public MyParentField(string label) : base(label, new VisualElement())
    {
        AddToClassList(BaseCompositeField<int, IntegerField, int>.ussClassName);
        visualInput = this.Q<VisualElement>(className: inputUssClassName);
        
        textField = new TextField(nameof(MyParent.name));
        floatField = new FloatField(nameof(MyParent.value));

        textField.labelElement.style.minWidth = StyleKeyword.Auto;
        floatField.labelElement.style.minWidth = StyleKeyword.Auto;
        
        visualInput.Add(textField);
        visualInput.Add(floatField);
        
        textField.RegisterValueChangedCallback(x =>
        {
            if (value != null)
                value.name = x.newValue;
            else
                value = new MyChild { name = x.newValue };
        });
        floatField.RegisterValueChangedCallback(x =>
        {
            if (value != null)
                value.value = x.newValue;
            else
                value = new MyChild { value = x.newValue };
        });
    }
    
    public override void SetValueWithoutNotify(MyParent newValue)
    {
        base.SetValueWithoutNotify(newValue);
        
        textField.SetValueWithoutNotify(newValue.name);
        floatField.SetValueWithoutNotify(newValue.value);
    }
}
#endif