using UnityEngine;
using System.Collections.Generic;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class ShopManager : MonoBehaviour
    {
        [Header("Shop Config")]
        [SerializeField] private int itemsPerShop = 4;
        [SerializeField] private List<ItemData> itemPool = new List<ItemData>();
        [SerializeField] private List<RelicData> relicPool = new List<RelicData>();

        private List<ShopItem> currentShopItems = new List<ShopItem>();

        public IReadOnlyList<ShopItem> CurrentItems => currentShopItems;

        public event System.Action<List<ShopItem>> OnShopOpened;
        public event System.Action<ShopItem> OnItemPurchased;
        public event System.Action OnShopClosed;

        public void OpenShop()
        {
            GenerateShopItems();
            OnShopOpened?.Invoke(currentShopItems);
        }

        public bool PurchaseItem(int index)
        {
            if (index < 0 || index >= currentShopItems.Count) return false;

            var item = currentShopItems[index];
            if (item.purchased) return false;

            var resources = GameManager.Instance.Resources;
            if (!resources.SpendGold(item.cost)) return false;

            item.purchased = true;
            ApplyPurchase(item);
            OnItemPurchased?.Invoke(item);
            return true;
        }

        public void CloseShop()
        {
            currentShopItems.Clear();
            OnShopClosed?.Invoke();
            GameManager.Instance.OnRoomResolved();
        }

        private void GenerateShopItems()
        {
            currentShopItems.Clear();

            for (int i = 0; i < itemsPerShop; i++)
            {
                bool isRelic = Random.value < 0.25f && relicPool.Count > 0;

                if (isRelic)
                {
                    var relic = relicPool[Random.Range(0, relicPool.Count)];
                    currentShopItems.Add(new ShopItem
                    {
                        itemName = relic.relicName,
                        description = relic.description,
                        cost = 50 + (int)relic.rarity * 30,
                        isRelic = true,
                        relicData = relic
                    });
                }
                else if (itemPool.Count > 0)
                {
                    var item = itemPool[Random.Range(0, itemPool.Count)];
                    currentShopItems.Add(new ShopItem
                    {
                        itemName = item.itemName,
                        description = item.description,
                        cost = item.goldCost,
                        isRelic = false,
                        itemData = item
                    });
                }
            }
        }

        private void ApplyPurchase(ShopItem item)
        {
            var resources = GameManager.Instance.Resources;

            if (item.isRelic && item.relicData != null)
            {
                var relicManager = GetComponent<RelicManager>();
                if (relicManager != null)
                    relicManager.AddRelic(item.relicData);
            }
            else if (item.itemData != null)
            {
                if (item.itemData.hpRestore > 0)
                    resources.RestoreHP(item.itemData.hpRestore);
                if (item.itemData.timeRestore > 0)
                    resources.RestoreTime(item.itemData.timeRestore);
                if (item.itemData.keysGiven > 0)
                    resources.AddKeys(item.itemData.keysGiven);
            }
        }
    }

    [System.Serializable]
    public class ShopItem
    {
        public string itemName;
        public string description;
        public int cost;
        public bool isRelic;
        public bool purchased;
        public ItemData itemData;
        public RelicData relicData;
    }
}
