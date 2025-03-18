using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using ArmyGame.ScriptableObjects.RuntimeSets.Dictionary;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using DrawEditorFor = UnityEditor.CustomEditor;
using CustomEditor = UnityEditor.Editor;
using Object = UnityEngine.Object;


namespace ArmyGame.Editor.ScriptableObjects.RuntimeSets.DictionaryEditor
{
    [DrawEditorFor(typeof(GenericDictionarySet<,>), true)]
    public class EditorScript : CustomEditor
    {
        public VisualTreeAsset visualTree;
        private IList items;

        private enum ElementIds
        {
            ADD_ICON_BUTTON,
            ADD_NEW_PAIR_SECTION,
            ADD_NEW_PAIR_BUTTON,
            DISMISS_BUTTON,
            KEY_FIELD,
            VALUE_FIELD,
        }

        private void OnEnable()
        {
            var propertyInfo = target.GetType().GetProperty("Items", BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo == null)
            {
                throw new NullReferenceException(
                    "This is impossible, there should be a value for items in the GenericDictionarySet. SO");
            }

            items = propertyInfo.GetValue(target) as IList;
            ;
        }

        public override VisualElement CreateInspectorGUI()
        {
            var root = visualTree.CloneTree();
            SetupListView(root);
            SetupAddIconButton(root);
            SetupAddUnitSection(root);
            return root;
        }

        private void SetupListView(VisualElement root)
        {
            var listView = root.Q<MultiColumnListView>();
            var myTarget = (ScriptableObject)target;

            Type myType = myTarget.GetType().BaseType;

            if (myType == null)
            {
                throw new NullReferenceException(
                    "This shoulde be an impossible state as this is expected to be of type GenericDictionarySet.");
            }

            var keyType = myType.GenericTypeArguments[0];
            var valueType = myType.GenericTypeArguments[1];

            if (keyType == null || valueType == null)
            {
                throw new Exception("Key type and value type are required.");
            }

            listView.columns.Add(new Column
                {
                    name = "Key",
                    title = "Key",
                    makeCell = () => BuildCellElement(keyType),
                    bindCell = CreateBindCell(keyType, "key"),
                    width = new Length(50, LengthUnit.Percent)
                }
            );

            listView.columns.Add(new Column
            {
                name = "Value",
                title = "Value",
                makeCell = () => BuildCellElement(valueType),
                bindCell = CreateBindCell(valueType, "value"),
                width = new Length(50, LengthUnit.Percent)
            });

            var itemsValue = (IList)myTarget.GetType().GetProperty("Items").GetValue(target);

            listView.itemsSource = itemsValue;
        }

        private VisualElement BuildCellElement(Type type)
        {
            return type switch
            {
                { } t when typeof(ScriptableObject).IsAssignableFrom(t) || typeof(GameObject).IsAssignableFrom(t) => new
                    ObjectField
                    { objectType = type },
                { } t when t == typeof(string) => new TextField { value = string.Empty },
                _ => null
            };
        }

        private Action<VisualElement, int> CreateBindCell(Type keyType, string propertyKey)
        {
            return (VisualElement e, int index) =>
            {
                var currItem = items[index];
                var propertyValue = currItem.GetType().GetField(propertyKey)?.GetValue(currItem);

                if (propertyValue == null)
                {
                    return;
                }

                if (propertyKey == "key")
                {
                    e.SetEnabled(false);
                }

                switch (keyType)
                {
                    case { } t when typeof(ScriptableObject).IsAssignableFrom(t) ||
                                    typeof(GameObject).IsAssignableFrom(t):
                        (e as ObjectField).value = propertyValue as Object;

                        if (propertyKey == "value")
                        {
                            var keyValue = currItem.GetType().GetField("key")?.GetValue(currItem);
                            (e as ObjectField).RegisterValueChangedCallback(HandleValueChange(keyValue));
                        }

                        break;
                    case { } t when t == typeof(string):
                        (e as TextField).value = propertyValue as String;
                        break;
                    default:
                        break;
                }
            };
        }

        private EventCallback<ChangeEvent<UnityEngine.Object>> HandleValueChange(object key)
        {
            return (evt) =>
            {
                var args = new object[] { key, evt.newValue };
                InvokeReflectedMethod("Update", args);

                SaveSetAsset();
            };
        }

        private void InvokeReflectedMethod(string methodName, object[] args)
        {
            var method = ((ScriptableObject)target).GetType().GetMethod(methodName);

            if (method == null)
            {
                throw new Exception($"{methodName} method could not be found.");
            }

            method.Invoke(target, args);
        }


        private void SetupAddIconButton(VisualElement root)
        {
            var button = root.Q<Button>(ElementIds.ADD_ICON_BUTTON.ToString());

            if (button == null)
            {
                Debug.LogWarning("Add Icon button");
                return;
            }

            button.clicked += CreateToggleAddNewUnitSection(root);
        }

        private void SetupAddUnitSection(VisualElement root)
        {
            var addNewUnitSection = root.Q<VisualElement>(ElementIds.ADD_NEW_PAIR_SECTION.ToString());

            var dismissButton = root.Q<Button>(ElementIds.DISMISS_BUTTON.ToString());
            var addNewUnitButton = root.Q<Button>(ElementIds.ADD_NEW_PAIR_BUTTON.ToString());


            var myTarget = (ScriptableObject)target;

            Type myType = myTarget.GetType().BaseType;

            if (myType == null)
            {
                throw new NullReferenceException(
                    "This should be be an impossible state as this is expected to be of type GenericDictionarySet.");
            }

            var keyType = myType.GenericTypeArguments[0];
            var valueType = myType.GenericTypeArguments[1];


            var newKeyElem = BuildCellElement(keyType);
            newKeyElem.name = ElementIds.KEY_FIELD.ToString();
            if (newKeyElem is ObjectField)
            {
                (newKeyElem as ObjectField).label = "Key";
            }

            newKeyElem.AddToClassList("update-field");

            var newValueElem = BuildCellElement(valueType);
            newValueElem.name = ElementIds.VALUE_FIELD.ToString();
            if (newValueElem is ObjectField)
            {
                (newValueElem as ObjectField).label = "Value";
            }

            newValueElem.AddToClassList("update-field");

            addNewUnitSection.Insert(1, newKeyElem);
            addNewUnitSection.Insert(2, newValueElem);


            if (addNewUnitSection == null || dismissButton == null || addNewUnitButton == null)
            {
                Debug.LogWarning("add new unit section or update button or dismiss button is missing ");
                return;
            }


            // setup dismiss section 
            dismissButton.clicked += CreateToggleAddNewUnitSection(root);
            // setup add new unit section 
            addNewUnitButton.clicked += CreateAddNewUnitButtonHandler(root);
        }

        private Action CreateToggleAddNewUnitSection(VisualElement root)
        {
            return () =>
            {
                var newPairSection = root.Q<VisualElement>(ElementIds.ADD_NEW_PAIR_SECTION.ToString());
                if (newPairSection == null)
                {
                    Debug.LogWarning("add new unit section is missing ");
                    return;
                }

                newPairSection.style.display = newPairSection.resolvedStyle.display == DisplayStyle.Flex
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            };
        }

        private Action CreateAddNewUnitButtonHandler(VisualElement root)
        {
            return () =>
            {
                var keyField = root.Q<ObjectField>(ElementIds.KEY_FIELD.ToString());
                
             
                var valueField = root.Q<ObjectField>(ElementIds.VALUE_FIELD.ToString());

                if (keyField == null || valueField == null)
                {
                    return;
                }

                var key = keyField.value;
                var value = valueField.value;

                if (key == null || value == null)
                {
                    return;
                }

                var args = new object[] { key, value };

                InvokeReflectedMethod("Add", args);
                SaveSetAsset();

                var closeSectionHandler = CreateToggleAddNewUnitSection(root);
                closeSectionHandler.Invoke();
            };
        }

        private void SaveSetAsset()
        {
            var runtimeSet = (ScriptableObject)target;
            EditorUtility.SetDirty(runtimeSet);
            AssetDatabase.SaveAssets();
        }
    }
}