using UnityEngine;
using DungeonArchitect.Data;
using DungeonArchitect.Core;

namespace DungeonArchitect.Systems
{
    public class RoomResolver : MonoBehaviour
    {
        public event System.Action<RoomData> OnRoomResolutionStarted;
        public event System.Action OnRoomResolutionComplete;

        public void ResolveRoom(RoomInstance room)
        {
            OnRoomResolutionStarted?.Invoke(room.Data);

            var resources = GameManager.Instance.Resources;
            resources.SpendTime(room.Data.timeCost);

            if (room.Data.gemCost > 0)
                resources.SpendGems(room.Data.gemCost);

            switch (room.Data.roomType)
            {
                case RoomType.Combat:
                    ResolveCombat(room);
                    break;
                case RoomType.Elite:
                    ResolveCombat(room);
                    break;
                case RoomType.Treasure:
                    ResolveTreasure(room);
                    break;
                case RoomType.Shop:
                    ResolveShop(room);
                    break;
                case RoomType.Rest:
                    ResolveRest(room);
                    break;
                case RoomType.Event:
                    ResolveEvent(room);
                    break;
                case RoomType.Trap:
                    ResolveTrap(room);
                    break;
                case RoomType.Puzzle:
                    ResolvePuzzle(room);
                    break;
                case RoomType.Shrine:
                    ResolveShrine(room);
                    break;
                case RoomType.Stair:
                    ResolveStair(room);
                    break;
                case RoomType.Boss:
                    ResolveBoss(room);
                    break;
            }

            room.Explore();
        }

        private void ResolveCombat(RoomInstance room)
        {
            if (room.Data.enemyEncounter != null)
            {
                GameManager.Instance.Combat.StartCombat(room.Data.enemyEncounter);
                GameManager.Instance.ChangeState(GameState.Combat);
            }
            else
            {
                GrantRewards(room.Data);
                OnRoomResolutionComplete?.Invoke();
            }
        }

        private void ResolveTreasure(RoomInstance room)
        {
            GrantRewards(room.Data);
            OnRoomResolutionComplete?.Invoke();
        }

        private void ResolveShop(RoomInstance room)
        {
            GameManager.Instance.ChangeState(GameState.Shop);
        }

        private void ResolveRest(RoomInstance room)
        {
            GameManager.Instance.Resources.RestoreHPPercent(0.3f);
            OnRoomResolutionComplete?.Invoke();
        }

        private void ResolveEvent(RoomInstance room)
        {
            GameManager.Instance.ChangeState(GameState.Event);
        }

        private void ResolveTrap(RoomInstance room)
        {
            GameManager.Instance.Resources.TakeDamage(15);
            GrantRewards(room.Data);
            OnRoomResolutionComplete?.Invoke();
        }

        private void ResolvePuzzle(RoomInstance room)
        {
            bool success = Random.value > 0.4f;
            if (success)
            {
                GrantRewards(room.Data);
            }
            else
            {
                GameManager.Instance.Resources.SpendTime(3);
            }
            OnRoomResolutionComplete?.Invoke();
        }

        private void ResolveShrine(RoomInstance room)
        {
            GameManager.Instance.Resources.IncreaseMaxHP(5);
            OnRoomResolutionComplete?.Invoke();
        }

        private void ResolveStair(RoomInstance room)
        {
            GameManager.Instance.OnStairFound();
        }

        private void ResolveBoss(RoomInstance room)
        {
            if (room.Data.enemyEncounter != null)
            {
                GameManager.Instance.Combat.StartCombat(room.Data.enemyEncounter);
                GameManager.Instance.ChangeState(GameState.BossEncounter);
            }
        }

        private void GrantRewards(RoomData data)
        {
            var resources = GameManager.Instance.Resources;
            if (data.goldReward > 0) resources.AddGold(data.goldReward);
            if (data.xpReward > 0) resources.AddXP(data.xpReward);
            if (data.keyReward > 0) resources.AddKeys(data.keyReward);
            if (data.gemReward > 0) resources.AddGems(data.gemReward);
        }
    }
}
