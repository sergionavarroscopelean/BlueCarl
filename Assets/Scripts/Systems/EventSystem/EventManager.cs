using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class EventManager : MonoBehaviour
    {
        [SerializeField] private List<DungeonEvent> eventPool = new List<DungeonEvent>();

        public event System.Action<DungeonEvent> OnEventTriggered;
        public event System.Action<DungeonEventChoice> OnChoiceMade;

        public DungeonEvent GetRandomEvent()
        {
            if (eventPool.Count == 0) return null;
            return eventPool[Random.Range(0, eventPool.Count)];
        }

        public void TriggerEvent(DungeonEvent evt)
        {
            OnEventTriggered?.Invoke(evt);
        }

        public void MakeChoice(DungeonEvent evt, int choiceIndex)
        {
            if (choiceIndex < 0 || choiceIndex >= evt.choices.Count) return;

            var choice = evt.choices[choiceIndex];
            ApplyChoiceEffects(choice);
            OnChoiceMade?.Invoke(choice);
            GameManager.Instance.OnRoomResolved();
        }

        private void ApplyChoiceEffects(DungeonEventChoice choice)
        {
            var resources = GameManager.Instance.Resources;

            if (choice.hpChange != 0)
            {
                if (choice.hpChange > 0) resources.RestoreHP(choice.hpChange);
                else resources.TakeDamage(-choice.hpChange);
            }

            if (choice.timeChange != 0)
            {
                if (choice.timeChange > 0) resources.RestoreTime(choice.timeChange);
                else resources.SpendTime(-choice.timeChange);
            }

            if (choice.goldChange != 0)
            {
                if (choice.goldChange > 0) resources.AddGold(choice.goldChange);
                else resources.SpendGold(-choice.goldChange);
            }

            if (choice.keyChange > 0) resources.AddKeys(choice.keyChange);
            if (choice.gemChange > 0) resources.AddGems(choice.gemChange);
        }
    }

    [System.Serializable]
    public class DungeonEvent
    {
        public string eventName;
        [TextArea(3, 6)]
        public string description;
        public List<DungeonEventChoice> choices = new List<DungeonEventChoice>();
    }

    [System.Serializable]
    public class DungeonEventChoice
    {
        public string choiceText;
        public int hpChange;
        public int timeChange;
        public int goldChange;
        public int keyChange;
        public int gemChange;
        [TextArea(1, 3)]
        public string outcomeText;
    }
}
