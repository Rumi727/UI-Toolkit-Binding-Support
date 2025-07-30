#nullable enable
#if !RUNI_ENGINE
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Rumi.CustomBinding.Editor
{
    /// <summary>
    /// Provides utility methods for PropertyField related operations.
    /// <br/>
    /// PropertyField 관련 작업을 위한 유틸리티 메서드를 제공합니다.
    /// </summary>
    public static class PropertyFieldUtility
    {
        /// <summary>
        /// Sets the binding path of a <see cref="BindableElement"/> to the path of a <see cref="SerializedProperty"/>.
        /// <br/>
        /// <see cref="BindableElement"/>의 바인딩 경로를 <see cref="SerializedProperty"/>의 경로로 설정합니다.
        /// </summary>
        /// <typeparam name="TField">
        /// The type of the field.
        /// <br/>
        /// 필드의 타입입니다.</typeparam>
        /// <typeparam name="TValue">
        /// The value type of the field.
        /// <br/>
        /// 필드의 값 타입입니다.</typeparam>
        /// <param name="field">
        /// The bindable element to set the path for.
        /// <br/>
        /// 경로를 설정할 바인딩 가능한 요소입니다.
        /// </param>
        /// <param name="property">
        /// The serialized property whose path will be used.
        /// <br/>
        /// 경로가 사용될 직렬화된 속성입니다.</param>
        /// <returns>
        /// The modified bindable element.<br/>
        /// 수정된 바인딩 가능한 요소입니다.
        /// </returns>
        public static TField SetProperty<TField, TValue>(this TField field, SerializedProperty property) where TField : BaseField<TValue>
        {
            field.label = property.displayName;
            field.bindingPath = property.propertyPath;

            return field.ConfigureFieldStyles<TField, TValue>();
        }

        /// <summary>
        /// Configures the USS (Unity Style Sheets) styles for a <see cref="BaseField{TValueType}"/> to apply alignment.
        /// <br/>
        /// <see cref="BaseField{TValueType}"/>에 정렬을 적용하기 위한 USS(Unity Style Sheets) 스타일을 구성합니다.
        /// </summary>
        /// <typeparam name="TField">
        /// The type of the field.
        /// <br/>
        /// 필드의 타입입니다.</typeparam>
        /// <typeparam name="TValue">
        /// The value type of the field.
        /// <br/>
        /// 필드의 값 타입입니다.</typeparam>
        /// <param name="field">The field to configure.
        /// <br/>
        /// 구성할 필드입니다.</param>
        /// <returns>The configured field.
        /// <br/>
        /// 구성된 필드입니다.</returns>
        public static TField ConfigureFieldStyles<TField, TValue>(this TField field) where TField : BaseField<TValue>
        {
            field.labelElement.AddToClassList(PropertyField.labelUssClassName);
            field.AddToClassList(BaseField<TValue>.alignedFieldUssClassName);
            
            VisualElement? visualInput = field.Q<VisualElement>(BaseField<TValue>.inputUssClassName);
            visualInput?.AddToClassList(PropertyField.inputUssClassName);
            visualInput?.Query<VisualElement>(null, BaseField<TValue>.ussClassName, BaseCompositeField<int, IntegerField, int>.ussClassName)
                .ForEach(static x => x.AddToClassList(BaseField<TValue>.alignedFieldUssClassName));

            return field;
        }
    }
}
#endif