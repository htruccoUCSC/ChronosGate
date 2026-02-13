using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public int height;
    public int width;

    // Setters
    public void SetHeight(int newHeight)
    {
        height = newHeight;
    }
    public void SetWidth(int newWidth)
    {
        width = newWidth;
    }
}

