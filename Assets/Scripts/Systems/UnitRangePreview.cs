using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class UnitRangePreview : MonoBehaviour
{
    [SerializeField] private BoardManager m_Board;
    [SerializeField] private Tilemap m_PreviewTilemap;
    [SerializeField] private Color m_PreviewColor = new Color(0.1f, 0.8f, 1f, 0.35f);
    [SerializeField] private int m_SortingOrder = 110;

    private Tile m_PreviewTile;
    private readonly List<Vector3Int> m_CurrentCells = new List<Vector3Int>();
    private readonly List<Vector3Int> m_WorkCells = new List<Vector3Int>();
    private Vector3Int m_LastOriginCell;
    private string m_LastUnitId = string.Empty;
    private bool m_HasPreview;

    public BoardManager Board => m_Board;

    public static UnitRangePreview GetOrCreate(BoardManager board)
    {
        if (board == null) return null;

        UnitRangePreview preview = board.GetComponent<UnitRangePreview>();
        if (preview == null)
        {
            preview = board.gameObject.AddComponent<UnitRangePreview>();
        }

        preview.SetBoard(board);
        return preview;
    }

    public void SetBoard(BoardManager board)
    {
        if (board == null) return;

        m_Board = board;
        EnsurePreviewTilemap();
    }

    public void ShowPreview(BaseUnit unitPrefab, Vector3Int originCell, UnitDefinition defOverride)
    {
        if (unitPrefab == null || m_Board == null || m_Board.GameTilemap == null)
        {
            ClearPreview();
            return;
        }

        string unitId = defOverride != null ? defOverride.UnitID : string.Empty;
        if (m_HasPreview && originCell == m_LastOriginCell && unitId == m_LastUnitId)
        {
            return;
        }

        m_WorkCells.Clear();
        unitPrefab.CollectRangePreviewCells(m_Board, originCell, defOverride, m_WorkCells);

        if (m_WorkCells.Count == 0)
        {
            ClearPreview();
            return;
        }

        ApplyCells(m_WorkCells);
        m_LastOriginCell = originCell;
        m_LastUnitId = unitId;
    }

    public void ClearPreview()
    {
        if (!m_HasPreview || m_PreviewTilemap == null) return;

        for (int i = 0; i < m_CurrentCells.Count; i++)
        {
            m_PreviewTilemap.SetTile(m_CurrentCells[i], null);
        }

        m_CurrentCells.Clear();
        m_HasPreview = false;
    }

    private void ApplyCells(List<Vector3Int> cells)
    {
        ClearPreview();
        EnsurePreviewTilemap();
        EnsurePreviewTile();

        for (int i = 0; i < cells.Count; i++)
        {
            Vector3Int cell = cells[i];
            if (m_Board.GameTilemap == null || !m_Board.GameTilemap.HasTile(cell))
            {
                continue;
            }

            m_PreviewTilemap.SetTile(cell, m_PreviewTile);
            m_PreviewTilemap.SetTileFlags(cell, TileFlags.None);
            m_PreviewTilemap.SetColor(cell, m_PreviewColor);
            m_CurrentCells.Add(cell);
        }

        m_HasPreview = m_CurrentCells.Count > 0;
    }

    private void EnsurePreviewTilemap()
    {
        if (m_PreviewTilemap != null || m_Board == null) return;

        GameObject previewObj = new GameObject("UnitRangePreviewTilemap");
        previewObj.transform.SetParent(m_Board.transform, false);
        m_PreviewTilemap = previewObj.AddComponent<Tilemap>();
        TilemapRenderer renderer = previewObj.AddComponent<TilemapRenderer>();
        renderer.sortingOrder = m_SortingOrder;
    }

    private void EnsurePreviewTile()
    {
        if (m_PreviewTile != null) return;

        m_PreviewTile = ScriptableObject.CreateInstance<Tile>();
        m_PreviewTile.sprite = Sprite.Create(
            Texture2D.whiteTexture,
            new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
            new Vector2(0.5f, 0.5f),
            Texture2D.whiteTexture.width);
    }
}
