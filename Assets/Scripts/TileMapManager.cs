using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public int Height=1;
    public int Width=1;

    // Setters
    public void SetHeight(int newHeight)
    {
        Height = newHeight;
    }
    public void SetWidth(int newWidth)
    {
        Width = newWidth;
    }
}

