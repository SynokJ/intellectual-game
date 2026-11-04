using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IntellectualGame.Core
{
    /// <summary>Picks the column count and cell size of a <see cref="GridLayoutGroup"/> from the width it is given.</summary>
    [RequireComponent(typeof(GridLayoutGroup))]
    public class AdaptiveGrid : UIBehaviour
    {
        [SerializeField] float minCellWidth = 480f;
        [SerializeField] float maxCellWidth = 640f;
        [Tooltip("Cell height divided by cell width.")]
        [SerializeField] float cellAspect = 0.75f;
        [Tooltip("Cell height divided by cell width on a portrait screen; 0 = same as landscape.")]
        [SerializeField] float portraitCellAspect;
        [SerializeField] int maxColumns = 4;

        GridLayoutGroup grid;

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnRectTransformDimensionsChange() => Refresh();

        void Refresh()
        {
            if (!isActiveAndEnabled) return;
            if (grid == null) grid = GetComponent<GridLayoutGroup>();

            float available = ((RectTransform)transform).rect.width - grid.padding.horizontal;
            if (available <= 0f) return;

            float gap = grid.spacing.x;
            int columns = Mathf.Clamp(Mathf.FloorToInt((available + gap) / (minCellWidth + gap)), 1, maxColumns);
            float width = Mathf.Min((available - gap * (columns - 1)) / columns, maxCellWidth);

            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            float aspect = Screen.height > Screen.width && portraitCellAspect > 0f ? portraitCellAspect : cellAspect;
            grid.cellSize = new Vector2(width, width * aspect);
        }
    }
}
