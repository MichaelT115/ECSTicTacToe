using UnityEngine;

public class Cell : MonoBehaviour
{
    public GameManager gameManager;
    SpriteRenderer spriteRenderer;
    public int x, y;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Init(int x, int y, GameManager gameManager)
    {
        this.x = x;
        this.y = y;
        this.gameManager = gameManager;
    }

    public void SetTeam(Team team)
    {
        switch (team)
        {
            case Team.X:
                spriteRenderer.color = Color.red;
                break;
            case Team.O:
                spriteRenderer.color = Color.yellow;
                break;
        }
    }
}
