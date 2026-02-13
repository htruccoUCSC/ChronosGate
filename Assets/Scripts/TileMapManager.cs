using UnityEngine;

public class TileMapManager : MonoBehaviour
{
    public int x;
    public int y;
    
    // Setters
    public void SetX(int newX)
    {
        x = newX;
    }

    public void SetY(int newY)
    {
        y = newY;
    }

    // Set both at once
    public void SetPosition(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
}

