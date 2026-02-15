using UnityEngine;

public class Renovations : MonoBehaviour
{
public BoardManager board;
public TileMapManager tileMap;
public void RenovationsCall()
{
tileMap.SetHeight(tileMap.Height-2);
tileMap.SetWidth(tileMap.Width+2);
}

}
