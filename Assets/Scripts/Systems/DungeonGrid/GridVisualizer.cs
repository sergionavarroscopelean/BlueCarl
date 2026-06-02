using UnityEngine;
using DungeonArchitect.Data;

namespace DungeonArchitect.Systems
{
    public class GridVisualizer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private DungeonGridManager gridManager;

        [Header("Room Type Colors")]
        [SerializeField] private Color combatColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color treasureColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color shopColor = new Color(0.2f, 0.6f, 0.2f);
        [SerializeField] private Color restColor = new Color(0.2f, 0.8f, 0.4f);
        [SerializeField] private Color eventColor = new Color(0.5f, 0.5f, 0.8f);
        [SerializeField] private Color stairColor = new Color(1f, 1f, 1f);
        [SerializeField] private Color eliteColor = new Color(0.8f, 0f, 0f);
        [SerializeField] private Color trapColor = new Color(0.6f, 0f, 0.6f);
        [SerializeField] private Color shrineColor = new Color(0.9f, 0.9f, 0.2f);
        [SerializeField] private Color bossColor = new Color(1f, 0f, 0f);
        [SerializeField] private Color defaultColor = Color.gray;

        [Header("Grid Lines")]
        [SerializeField] private bool showGridLines = true;
        [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        public Color GetRoomColor(RoomType type)
        {
            return type switch
            {
                RoomType.Combat => combatColor,
                RoomType.Elite => eliteColor,
                RoomType.Treasure => treasureColor,
                RoomType.Shop => shopColor,
                RoomType.Rest => restColor,
                RoomType.Event => eventColor,
                RoomType.Trap => trapColor,
                RoomType.Shrine => shrineColor,
                RoomType.Stair => stairColor,
                RoomType.Boss => bossColor,
                _ => defaultColor
            };
        }

        private void OnDrawGizmos()
        {
            if (!showGridLines || gridManager == null) return;

            Gizmos.color = gridLineColor;
            float cw = gridManager.CellWidth;
            float ch = gridManager.CellHeight;
            float offsetX = -(gridManager.GridWidth * cw) / 2f;
            float offsetY = -(gridManager.GridHeight * ch) / 2f;

            for (int x = 0; x <= gridManager.GridWidth; x++)
            {
                var start = new Vector3(x * cw + offsetX, offsetY, 0f);
                var end = new Vector3(x * cw + offsetX, gridManager.GridHeight * ch + offsetY, 0f);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= gridManager.GridHeight; y++)
            {
                var start = new Vector3(offsetX, y * ch + offsetY, 0f);
                var end = new Vector3(gridManager.GridWidth * cw + offsetX, y * ch + offsetY, 0f);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}
