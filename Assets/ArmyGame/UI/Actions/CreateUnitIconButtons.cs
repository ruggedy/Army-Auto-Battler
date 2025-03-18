using System;
using System.Linq;
using ArmyGame.ScriptableObjects.EventChannels;
using ArmyGame.ScriptableObjects.RuntimeSets.Dictionary;
using ArmyGame.ScriptableObjects.Units;
using Logic.Units;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Info = Logic.Attributes.Info;

namespace ArmyGame.UI.Actions
{
    public class CreateUnitIconButtons : MonoBehaviour
    {
        private VisualElement rootElement;
        [SerializeField] private SoToGoMap unitSet;
        [SerializeField] private AgentEventChannel spawnUnitEventChannel;
        [SerializeField] private AgentsEnumLike playerAgent;

        private void OnEnable()
        {
            rootElement = GetComponent<UIDocument>().rootVisualElement;

            var container = new VisualElement();
            container.AddToClassList("game-controls-container");

            unitSet.Items.Select(CreateUnitButton).ToList().ForEach(container.Add);

            rootElement.Add(container);
        }

        private Button CreateUnitButton(SoToGoMap.SetPair pair)
        {
            var button = new Button();

            button.AddToClassList("action-button");
            var unitSo = (pair.key as UnitSO);
            button.style.backgroundImage = new StyleBackground(unitSo.Info.Icon);

            if (unitSo.Info.Icon != null)
            {
                button.style.backgroundImage = new StyleBackground(unitSo.Info.Icon);
            }

            button.clicked += CreatOnClickUnitButton(unitSo);

            return button;
        }

        private Action CreatOnClickUnitButton(UnitSO unit) => () => HandleSpawnSubmit(unit);

        private void HandleSpawnSubmit(UnitSO unit)
        {
            if (spawnUnitEventChannel == null)
            {
                Debug.LogWarning("No event channel assigned");
                return;
            }

            spawnUnitEventChannel.RaiseEvent(new AgentChannelEventParams(unit: unit, agent: playerAgent));
        }
    }
}