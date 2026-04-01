using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public int Height = 1;
    public int Width = 1;
    public BoardManager board;

    private void Awake()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<BoardManager>();
        }
    }

    // Setters
    public void SetHeight(int newHeight)
    {
        Height = Mathf.Max(1, newHeight);
        RegenerateBoard();
    }
    public void expansion(int amount)
    {
        Height+=amount;
        Height = Mathf.Max(1, Height);
        RegenerateBoard();
    }

    public void SetWidth(int newWidth)
    {
        Width = Mathf.Max(1, newWidth);
        RegenerateBoard();
    }

    private void RegenerateBoard()
    {
        if (board == null)
        {
            board = FindFirstObjectByType<BoardManager>();
        }

        if (board == null)
        {
            Debug.LogError("TileMapManager: No BoardManager found to regenerate the board.");
            return;
        }

        board.GenerateBoard();
    }
}

