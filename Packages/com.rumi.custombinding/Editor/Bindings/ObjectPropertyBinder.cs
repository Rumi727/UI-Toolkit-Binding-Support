#nullable enable
#if UNITY_2022_1_OR_NEWER
using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace Rumi.CustomBinding.Editor.Bindings
{
    /// <summary>
    /// A concrete implementation of <see cref="PropertyBinder"/> that handles properties of type <see cref="object"/>.
    /// <br/>This binder uses <see cref="SerializedProperty.boxedValue"/> for reading and writing.
    /// <br/><br/>
    /// <see cref="object"/> 타입의 속성을 처리하는 <see cref="PropertyBinder"/>의 구체적인 구현입니다.
    /// <br/>이 바인더는 읽기 및 쓰기에 <see cref="SerializedProperty.boxedValue"/>를 사용합니다.
    /// </summary>
    [CustomPropertyBinder(typeof(object), true)]
    public class ObjectPropertyBinder : PropertyBinder
    {
        /// <summary>
        /// Reads the value from the specified <see cref="SerializedProperty"/> using <see cref="SerializedProperty.boxedValue"/>.
        /// <br/><see cref="SerializedProperty.boxedValue"/>를 사용하여 지정된 <see cref="SerializedProperty"/>에서 값을 읽습니다.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> associated with the property.<br/>속성과 연결된 <see cref="VisualElement"/>입니다.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to read from.<br/>값을 읽어올 <see cref="SerializedProperty"/>입니다.</param>
        /// <param name="propertyType">The expected <see cref="Type"/> of the property value (ignored as <see cref="object"/> type handles all).<br/>속성 값의 예상 <see cref="Type"/>입니다.</param>
        /// <returns>The boxed value from the property.<br/>속성의 박싱된 값입니다.</returns>
        public override object? Read(VisualElement element, SerializedProperty property, Type propertyType) => property.boxedValue;
        
        /// <summary>
        /// Writes the specified value to the <see cref="SerializedProperty"/> using <see cref="SerializedProperty.boxedValue"/>.
        /// <br/><see cref="SerializedProperty.boxedValue"/>를 사용하여 지정된 값을 <see cref="SerializedProperty"/>에 씁니다.
        /// </summary>
        /// <param name="element">The <see cref="VisualElement"/> associated with the property.<br/>속성과 연결된 <see cref="VisualElement"/>입니다.</param>
        /// <param name="property">The <see cref="SerializedProperty"/> to write to.<br/>값을 쓸 <see cref="SerializedProperty"/>입니다.</param>
        /// <param name="propertyType">The <see cref="Type"/> of the property value to write (ignored as <see cref="object"/> type handles all).<br/>쓸 속성 값의 <see cref="Type"/>입니다.</param>
        /// <param name="value">The object value to write to the property.<br/>속성에 쓸 객체 값입니다.</param>
        public override void Write(VisualElement element, SerializedProperty property, Type propertyType, object? value) => property.boxedValue = value;
    }
}
#endif