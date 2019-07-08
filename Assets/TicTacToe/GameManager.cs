using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int width = 3;
    public int height = 3;
    public Team[,] board;

    public enum GameState
    {
        InGame, GameWon, GameDraw
    }
    public GameState gameState;

    public Team winner;

    public Cell cellPrefab;

    public Cell[,] cells;

    public Team currentTeam;

    public Text text;

    private void Start()
    {
        board = new Team[width, height];
        cells = new Cell[width, height];
        gameState = GameState.InGame;

        for (int x = 0; x < width; ++x)
        {
            for (int y = 0; y < height; ++y)
            {
                cells[x, y] = Instantiate(cellPrefab, new Vector2(x, y), Quaternion.identity, transform);
                cells[x, y].Init(x, y, this);
            }
        }

        currentTeam = Team.X;
        text.text = $"{Enum.GetName(typeof(Team), currentTeam)} turn";
    }

    public void SelectCell(int x, int y)
    {
        if (gameState != GameState.InGame)
            return;

        if (!SetCellState(x, y, currentTeam))
            return;

        EndTurn();
    }

    private void EndTurn()
    {
        EvaluateBoard();


        switch (gameState)
        {
            case GameState.InGame:
                currentTeam = currentTeam == Team.X ? Team.O : Team.X;
                text.text = $"{Enum.GetName(typeof(Team), currentTeam)} Turn";
                break;
            case GameState.GameWon:
                text.text = $" {Enum.GetName(typeof(Team), winner)} Wins";
                break;
            case GameState.GameDraw:
                text.text = "DRAW";
                break;
        }
    }

    public bool SetCellState(int x, int y, Team cellState)
    {
        if (board[x, y] != Team.EMPTY)
            return false;

        board[x, y] = cellState;

        cells[x, y].SetTeam(cellState);
        return true;
    }

    public void EvaluateBoard()
    {
        // Check Rows for Winner
        for (int y = 0; y  < height; ++y)
        {
            Team team = board[0, y];
            if (team == Team.EMPTY)
                continue;

            bool isCompleteRow = true;
            for (int x = 1; x < width; ++x)
            {
                if (board[x,y] != team)
                {
                    isCompleteRow = false;
                    break;
                }
            }

            if (isCompleteRow)
            {
                DeclareWinner(team);
                return;
            }
        }

        // Check Columns for Winner
        for (int x = 0; x < width; ++x)
        {
            Team team = board[0, x];
            if (team == Team.EMPTY)
                continue;

            bool isCompleteColumn = true;
            for (int y = 1; y < height; ++y)
            {
                if (board[x, y] != team)
                {
                    isCompleteColumn = false;
                    break;
                }
            }

            if (isCompleteColumn)
            {
                DeclareWinner(team);
                return;
            }
        }

        // Check Diagonal
        if (board[0, 0] != Team.EMPTY 
            && board[0, 0] == board[1, 1]
            & board[1, 1] == board[2, 2])
        {
            DeclareWinner(board[0, 0]);
            return;
        }

        if (board[0, 2] != Team.EMPTY
            && board[0, 2] == board[1, 1]
            & board[1, 1] == board[2, 0])
        {
            DeclareWinner(board[0, 2]);
            return;
        }

        // Check for Draw
        foreach(var cell in board)
        {
            if (cell == Team.EMPTY)
                return;
        }
        DeclareDraw();
    }

    public void DeclareDraw()
    {
        SetGameState(GameState.GameDraw);
    }

    public void DeclareWinner(Team team)
    {
        winner = team;
        SetGameState(GameState.GameWon);
    }

    public void SetGameState(GameState gameState)
    {
        this.gameState = gameState;
    }
}
