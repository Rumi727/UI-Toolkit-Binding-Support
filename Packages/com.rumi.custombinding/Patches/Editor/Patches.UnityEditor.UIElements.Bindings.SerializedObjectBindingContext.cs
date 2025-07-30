#nullable enable
#if !RUNI_ENGINE
using HarmonyLib;
using System;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_6000_0_OR_NEWER
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

using Label = System.Reflection.Emit.Label;
#endif

namespace Rumi.CustomBinding.Editor.Patches
{
    public static partial class Patches
    {
        public static partial class UnityEditor
        {
            public static partial class UIElements
            {
                public static partial class Bindings
                {
                    public static class SerializedObjectBindingContext
                    {
                        public static Type targetType => AccessTools.TypeByName("UnityEditor.UIElements.Bindings.SerializedObjectBindingContext");

                        [HarmonyPatch]
                        public static class CreateBindingObjectForProperty
                        {
                            public static MethodBase TargetMethod() => AccessTools.DeclaredMethod(targetType, "CreateBindingObjectForProperty");

#if UNITY_6000_0_OR_NEWER
                            public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
                            {
                                CodeMatcher matcher = new CodeMatcher(instructions, generator);
                                
                                // --- 1단계: `case SerializedPropertyType.Generic:` 블록의 시작점 찾기 ---
                                // `prop.type == "ToggleButtonGroupState"` 체크가 시작되는 지점을 찾습니다.
                                // IL_0963: ldarg.2      // prop
                                // IL_0964: callvirt     instance string [UnityEditor.CoreModule]UnityEditor.SerializedProperty::get_type()
                                // IL_0969: ldstr        "ToggleButtonGroupState"
                                // 이 패턴의 시작점 (`ldarg.2`)을 찾습니다.
                                matcher.MatchStartForward(
                                    Code.Ldarg_2,
                                    Code.Callvirt[typeof(SerializedProperty).DeclaredPropertyGetter(nameof(SerializedProperty.type))],
                                    Code.Ldstr[nameof(ToggleButtonGroupState)]
                                );
                                
                                if (matcher.IsInvalid)
                                {
                                    Debug.LogWarning($"Harmony Transpiler: Could not find insertion point for ToggleButtonGroupState check in {nameof(CreateBindingObjectForProperty)}.");
                                    return instructions;
                                }

                                int insertionIndex = matcher.Pos; // 삽입될 시작 인덱스
                                
                                // --- 2단계: 원래 코드의 첫 번째 인스트럭션에 붙어있는 레이블 처리 ---
                                // 이 레이블들은 이제 새로운 삽입 코드를 건너뛰고 원래 코드로 점프할 때 사용됩니다.
                                List<Label> originalLabelsAtInsertionPoint = matcher.Labels.ToList();
                                matcher.Labels.Clear(); // 원래 인스트럭션에서 레이블 제거
                                
                                // --- 3단계: `if (Method())`가 `false`일 때 점프할 레이블 생성 및 연결 ---
                                // 이 레이블은 원래 코드의 시작점으로 점프할 때 사용됩니다.
                                matcher.CreateLabel(out Label jumpIfFalseLabel);
                                
                                // --- 4단계: `if (Method())`가 `true`일 때 `break`할 최종 레이블 찾기 ---
                                // `case SerializedPropertyType.Generic` 블록의 끝, 즉 `break;` 지점을 찾습니다.
                                // 원래 IL: IL_0977: brfalse.s IL_09a8
                                // `IL_09a8`이 `prop.type == "ToggleButtonGroupState"`가 false일 때 점프하는 지점입니다.
                                // 이 지점이 `case` 블록의 나머지 로직을 건너뛰는 `break` 지점으로 가장 적합합니다.
                                matcher.MatchStartForward(Code.Br);
                                
                                if (matcher.IsInvalid)
                                {
                                    Debug.LogWarning($"Harmony Transpiler: Could not find original `br` for ToggleButtonGroupState check in {nameof(CreateBindingObjectForProperty)}.");
                                    return instructions;
                                }
                                
                                CodeInstruction finalCaseBreak = matcher.Instruction;
                                
                                // --- 5단계: 새로운 코드 삽입 ---
                                // `matcher`를 다시 원래 삽입 위치로 설정합니다.
                                matcher.Start().Advance(insertionIndex);
                                
                                matcher.Insert(
                                    Code.Ldarg_0.WithLabels(originalLabelsAtInsertionPoint), // instance
                                    Code.Ldarg_1, // element
                                    Code.Ldarg_2, // prop
                                    Code.Call[ReflectionUtility.GetMethodInfo((Func<object, VisualElement, SerializedProperty, bool>)CustomPreCondition)],
                                    Code.Brfalse_S[jumpIfFalseLabel], // CustomPreCondition()의 결과가 false이면 `jumpIfFalseLabel`로 점프 (원래 코드 시작 지점) 
                                    finalCaseBreak // CustomPreCondition()의 결과가 true이면 `finalCaseBreakLabel`로 점프 (case 블록의 최종 break 지점)
                                );

                                return matcher.InstructionEnumeration();
                            }
#else
                            public static bool Prefix(object __instance, VisualElement element, SerializedProperty prop)
                            {
                                if (prop.propertyType == SerializedPropertyType.Generic)
                                    return !CustomPreCondition(__instance, element, prop);

                                return true;
                            }
#endif

                            static readonly object[] defaultBindParms = new object[5];
                            public static bool CustomPreCondition(object instance, VisualElement element, SerializedProperty prop)
                            {
                                try
                                {
#if !UNITY_6000_0_OR_NEWER
                                    if (ActiveEditorTracker.sharedTracker.inspectorMode != InspectorMode.Normal)
                                        return false;
#endif
                                    // DefaultBind 메소드를 포함하는 대상 인스턴스의 타입을 가져옵니다.
                                    Type targetType = instance.GetType();
                                    // 대상 인스턴스에서 "DefaultBind" 제네릭 메소드의 정의를 찾아옵니다.
                                    MethodInfo? defaultBindGenericDefinition = targetType.DeclaredMethod("DefaultBind");

                                    // DefaultBind 메소드 정의를 찾을 수 없거나 제네릭 메소드가 아닌 경우 예외를 발생시킵니다.
                                    if (defaultBindGenericDefinition == null || !defaultBindGenericDefinition.IsGenericMethodDefinition)
                                    {
                                        Debug.LogError($"Definition of generic method 'DefaultBind<TValue>' not found in '{targetType}'");
                                        return false;
                                    }

                                    // SerializedProperty로부터 필드의 실제 타입(propertyType)을 가져옵니다.
                                    prop.GetFieldInfoFromProperty(out Type? propertyType);

                                    // 필드 타입을 찾지 못한 경우 경고를 로깅하고 false를 반환합니다.
                                    if (propertyType == null)
                                    {
                                        Debug.LogWarning($"FieldInfo not found for {prop.propertyPath}");
                                        return false;
                                    }

                                    // 해당 propertyType에 맞는 PropertyBinder를 찾습니다.
                                    PropertyBinder? binder = null;
                                    foreach ((Type type, CustomPropertyBinderAttribute attribute) in PropertyBinder.propertyBinderTypes)
                                    {
                                        // propertyType이 바인더의 targetType과 정확히 일치하거나,
                                        // 바인더가 하위 타입 호환성을 지원하고 propertyType이 targetType에 할당 가능한 경우
                                        if (propertyType == attribute.targetType || (attribute.isSubtypeCompatible && propertyType.IsAssignableToAny(attribute.targetType)))
                                        {
                                            // 해당 PropertyBinder 인스턴스를 생성하고 루프를 종료합니다 (가장 적합한 바인더 선택).
                                            binder = (PropertyBinder)Activator.CreateInstance(type);
                                            break;
                                        }
                                    }

                                    // 적합한 PropertyBinder를 찾지 못한 경우 false를 반환합니다.
                                    if (binder == null)
                                    {
                                        Debug.LogWarning($"No suitable PropertyBinder found for property type {propertyType}. Property: {prop.propertyPath}");
                                        return false;
                                    }
                                    
                                    // propertyType이 Nullable인지 확인합니다
                                    bool isNullable = NullableType.isNullable.GetInvocationList()
                                        .OfType<Func<Type, bool>>()
                                        .Any(item => item.Invoke(propertyType));

                                    Type? nullableUnderlyingType = NullableType.getNullableUnderlyingType.GetInvocationList()
                                        .OfType<Func<Type, Type?>>()
                                        .Select(x => x.Invoke(propertyType))
                                        .FirstOrDefault(static x => x != null);

                                    if ((nullableUnderlyingType == null && !propertyType.HasDefaultConstructor()) || (nullableUnderlyingType != null && !nullableUnderlyingType.HasDefaultConstructor()))
                                    {
                                        Debug.LogWarning($"Property '{prop.propertyPath}' of type '{propertyType}' cannot be bound because it requires a default public constructor but doesn't have one, or it's a nullable type whose underlying type lacks a default public constructor.");
                                        return false;
                                    }

                                    // Read 델리게이트 (Func<SerializedProperty, TValue>)를 동적으로 생성합니다.
                                    Delegate readFunc;
                                    // 델리게이트의 반환 타입으로 propertyType을 사용하여 DefaultBind가 기대하는 시그니처를 맞춥니다.
                                    Type readFuncType = typeof(Func<,>).MakeGenericType(typeof(SerializedProperty), propertyType);
                                    {
                                        // Read 작업을 수행할 내부 로컬 함수를 정의합니다.
                                        // 이 함수는 'binder', 'element', 'propertyType', 'isNullable'을 클로저로 캡처합니다.
                                        object? InternalReadFunc(SerializedProperty property)
                                        {
                                            try
                                            {
                                                property = property.Copy();
                                                
                                                // 바인더를 통해 값을 읽어옵니다.
                                                object? value = binder.Read(element, property, propertyType);

                                                // 읽어온 값이 null인 경우의 처리 로직
                                                if (value == null)
                                                {
                                                    // propertyType이 참조 타입, 인터페이스, 또는 Nullable<T>인 경우 null을 허용합니다.
                                                    if (isNullable)
                                                        return null;

                                                    // non-nullable 값 타입인데 null이 반환된 경우 경고를 로깅하고 기본값을 반환합니다.
                                                    Debug.LogWarning($"Expected {propertyType.Name} but {binder.GetType().Name} returned null for non-nullable type. Property: {property.propertyPath}");
                                                }
                                                else // 읽어온 값이 null이 아닌 경우
                                                {
                                                    // 읽어온 값이 propertyType에 할당 가능한지 확인합니다 (TValue로 안전하게 캐스팅될 수 있는지).
                                                    if (propertyType.IsInstanceOfType(value))
                                                        return value; // 유효한 경우 값을 반환합니다.

                                                    // 타입 불일치 경고를 로깅합니다.
                                                    Debug.LogWarning($"Expected type {propertyType.Name} but binder returned incompatible type {value.GetType().Name} for {property.propertyPath}.");
                                                }

                                                // 타입 불일치 시 타입의 기본값을 반환합니다.
                                                return propertyType.GetDefaultValueNotNull();
                                            }
                                            catch (Exception e) // 바인더 내부에서 예외가 발생한 경우 처리
                                            {
                                                Debug.LogException(e);
                                                Debug.LogError($"Binding operation failed due to an exception in binder {binder.GetType().Name} at {property.propertyPath}!");

                                                return propertyType.GetDefaultValue(); // 예외 발생 시에도 기본값 반환
                                            }
                                        }

                                        // InternalReadFunc 로컬 함수를 Func<SerializedProperty, object?> 델리게이트로 캡슐화합니다.
                                        Func<SerializedProperty, object?> internalFunc = InternalReadFunc;
                                        // 캡슐화된 델리게이트의 MethodInfo를 가져옵니다.
                                        MethodInfo internalFuncInfo = internalFunc.Method;

                                        // Expression Tree 생성을 위한 매개변수 표현식을 정의합니다.
                                        ParameterExpression propertyParam = Expression.Parameter(typeof(SerializedProperty), "property");

                                        // InternalReadFunc를 호출하는 MethodCallExpression을 생성합니다.
                                        MethodCallExpression callInternalFunc;
                                        if (internalFuncInfo.IsStatic) // static 메소드인 경우 (일반적이지 않음, 클로저 함수는 인스턴스)
                                            callInternalFunc = Expression.Call(internalFuncInfo, propertyParam);
                                        else // 인스턴스 메소드인 경우 (클로저 함수에 해당)
                                        {
                                            // 델리게이트의 타겟(클로저 인스턴스)을 상수로 만듭니다.
                                            ConstantExpression targetConstant = Expression.Constant(internalFunc.Target);
                                            // 타겟 인스턴스를 통해 메소드를 호출하는 표현식을 생성합니다.
                                            callInternalFunc = Expression.Call(targetConstant, internalFuncInfo, propertyParam);
                                        }

                                        // InternalReadFunc의 결과를 propertyType으로 캐스팅하는 표현식을 생성합니다.
                                        Expression castResult = Expression.Convert(callInternalFunc, propertyType);
                                        // 전체 람다 표현식 (Func<SerializedProperty, TValue>)을 생성합니다.
                                        LambdaExpression lambda = Expression.Lambda(readFuncType, castResult, propertyParam);

                                        // 람다 표현식을 컴파일하여 실제 델리게이트(readFunc)를 만듭니다.
                                        readFunc = lambda.Compile();
                                    }

                                    // Write 델리게이트 (Action<SerializedProperty, TValue>)를 동적으로 생성합니다.
                                    Delegate writeFunc;
                                    {
                                        // Write 작업을 수행할 내부 로컬 함수를 정의합니다.
                                        // 이 함수는 'binder', 'element', 'propertyType'을 클로저로 캡처합니다.
                                        void InternalWriteFunc(SerializedProperty property, object? value)
                                        {
                                            try
                                            {
                                                property = property.Copy();
                                                
                                                // 바인더를 통해 값을 씁니다.
                                                binder.Write(element, property, propertyType, value);
                                            }
                                            catch (Exception e) // 바인더 내부에서 예외가 발생한 경우 처리
                                            {
                                                Debug.LogException(e);
                                                Debug.LogError($"Binding operation failed due to an exception in binder {binder.GetType()} at {property.propertyPath}!");
                                            }
                                        }

                                        // InternalWriteFunc 로컬 함수를 Action<SerializedProperty, object?> 델리게이트로 캡슐화합니다.
                                        Action<SerializedProperty, object?> internalFunc = InternalWriteFunc;
                                        MethodInfo internalFuncInfo = internalFunc.Method;

                                        // Expression Tree 생성을 위한 매개변수 표현식을 정의합니다.
                                        ParameterExpression propertyParam = Expression.Parameter(typeof(SerializedProperty), "property");
                                        ParameterExpression valueParam = Expression.Parameter(propertyType, "value");

                                        // value 매개변수를 object?로 캐스팅하는 표현식을 생성합니다.
                                        Expression castValueParam = Expression.Convert(valueParam, typeof(object));

                                        // InternalWriteFunc를 호출하는 MethodCallExpression을 생성합니다.
                                        MethodCallExpression callInternalAction;
                                        if (internalFuncInfo.IsStatic)
                                            callInternalAction = Expression.Call(internalFuncInfo, propertyParam, castValueParam);
                                        else
                                        {
                                            ConstantExpression targetConstant = Expression.Constant(internalFunc.Target);
                                            callInternalAction = Expression.Call(targetConstant, internalFuncInfo, propertyParam, castValueParam);
                                        }

                                        // 델리게이트의 TValue 타입 매개변수로 propertyType을 사용합니다.
                                        Type funcType = typeof(Action<,>).MakeGenericType(typeof(SerializedProperty), propertyType);

                                        // 전체 람다 표현식 (Action<SerializedProperty, TValue>)을 생성합니다.
                                        LambdaExpression lambda = Expression.Lambda(funcType, callInternalAction, propertyParam, valueParam);

                                        // 람다 표현식을 컴파일하여 실제 델리게이트(writeFunc)를 만듭니다.
                                        writeFunc = lambda.Compile();
                                    }

                                    // Comparer 델리게이트 (Func<TValue, SerializedProperty, Func<SerializedProperty, TValue>, bool>)를 동적으로 생성합니다.
                                    Delegate comparerFunc;
                                    {
                                        // Compare 작업을 수행할 내부 로컬 함수를 정의합니다.
                                        // 이 함수는 'binder', 'element', 'propertyType'을 클로저로 캡처합니다.
                                        bool InternalComparerFunc(object? valueToCompare, SerializedProperty property, Delegate readFunc)
                                        {
                                            try
                                            {
                                                property = property.Copy();
                                                
                                                // 전달받은 readFunc 델리게이트를 사용하여 현재 속성 값을 가져옵니다.
                                                object? currentValue = readFunc.DynamicInvoke(property);

                                                // 바인더의 Comparer 메소드를 호출하여 값을 비교합니다.
                                                return binder.Comparer(element, property, propertyType, currentValue, valueToCompare);
                                            }
                                            catch (Exception e) // 바인더 내부에서 예외가 발생한 경우 처리
                                            {
                                                Debug.LogException(e);
                                                Debug.LogError($"Binding comparison failed due to an exception in binder {binder.GetType().Name} at {property.propertyPath}!");
                                            }

                                            return false; // 예외 발생 시 false 반환
                                        }

                                        // InternalComparerFunc 로컬 함수를 Func<object?, SerializedProperty, Delegate, bool> 델리게이트로 캡슐화합니다.
                                        Func<object?, SerializedProperty, Delegate, bool> internalFunc = InternalComparerFunc;
                                        MethodInfo internalFuncMethodInfo = internalFunc.Method;

                                        // Expression Tree 생성을 위한 매개변수 표현식을 정의합니다.
                                        ParameterExpression valueParam = Expression.Parameter(propertyType, "valueToCompare");
                                        ParameterExpression propertyParam = Expression.Parameter(typeof(SerializedProperty), "property");
                                        ParameterExpression readFuncParam = Expression.Parameter(readFuncType, "readFunc");

                                        // 매개변수들을 object? 또는 Delegate 타입으로 캐스팅하는 표현식을 생성합니다.
                                        Expression castValue = Expression.Convert(valueParam, typeof(object));
                                        Expression castReadFunc = Expression.Convert(readFuncParam, typeof(Delegate));

                                        // InternalComparerFunc를 호출하는 MethodCallExpression을 생성합니다.
                                        MethodCallExpression callInternalFunc;
                                        if (internalFuncMethodInfo.IsStatic)
                                            callInternalFunc = Expression.Call(internalFuncMethodInfo, castValue, propertyParam, castReadFunc);
                                        else
                                        {
                                            ConstantExpression targetConstant = Expression.Constant(internalFunc.Target);
                                            callInternalFunc = Expression.Call(targetConstant, internalFuncMethodInfo, castValue, propertyParam, castReadFunc);
                                        }

                                        // 델리게이트의 제네릭 타입을 정의합니다.
                                        Type funcType = typeof(Func<,,,>).MakeGenericType(propertyType, typeof(SerializedProperty), readFuncType, typeof(bool));

                                        // 전체 람다 표현식 (Func<TValue, SerializedProperty, Func<SerializedProperty, TValue>, bool>)을 생성합니다.
                                        LambdaExpression comparerLambda = Expression.Lambda(funcType, callInternalFunc, valueParam, propertyParam, readFuncParam);

                                        // 람다 표현식을 컴파일하여 실제 델리게이트(comparerFunc)를 만듭니다.
                                        comparerFunc = comparerLambda.Compile();
                                    }

                                    // DefaultBind 메소드 호출에 필요한 매개변수들을 배열에 할당합니다.
                                    defaultBindParms[0] = element;
                                    defaultBindParms[1] = prop;
                                    defaultBindParms[2] = readFunc;
                                    defaultBindParms[3] = writeFunc;
                                    defaultBindParms[4] = comparerFunc;

                                    // DefaultBind 메소드의 제네릭 버전을 propertyType에 맞춰 만듭니다.
                                    MethodInfo defaultBind = defaultBindGenericDefinition.MakeGenericMethod(propertyType);

                                    // DefaultBind 메소드를 호출합니다.
                                    defaultBind.Invoke(instance, defaultBindParms);

                                    return true; // 바인딩 설정 성공
                                }
                                catch (Exception e)
                                {
                                    Debug.LogException(e);
                                    Debug.LogError($"A fatal exception occurred during binding at {prop.propertyPath}, leading to failure.");
                                }

                                return false;
                            }
                        }
                    }
                }
            }
        }
    }
}
#endif