using Logic.Units;
using UnityEngine;

namespace ArmyGame.ScriptableObjects.RuntimeSets.Dictionary
{
    [CreateAssetMenu(menuName = "RuntimeSets/PrefabForUnitSet", order = 0)]
    public class SoToGoMap : GenericDictionarySet<ScriptableObject, GameObject>
    { }
}