using UnityEngine;

public class CellCollider : MonoBehaviour
{
    private Cell cell;

    private void Awake()
    {
        cell = GetComponent<Cell>();
    }

    private void OnMouseDown()
    {
        cell.gameManager.SelectCell(cell.x, cell.y);
    }
}
