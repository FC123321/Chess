using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ChessPiece : MonoBehaviour {
    private int[] Position = new int[2] { -1, -1 };
    public bool isWhite;
    public string movementType;
    private bool hasMoved = false;
    private int turnDoubleMoved = -1;

    public void SetPosition(int[] position)
    {
        Position = position;
    }

    public void SetPosition(int xPos, int yPos)
    {
        Position = new int[2] { xPos, yPos };
    }

    public int[] GetPosition()
    {
        return Position;
    }

    public void SetHasMoved(bool b)
    {
        hasMoved = b;
    }

    public bool GetHasMoved()
    {
        return hasMoved;
    }

    public void SetTurnDoubleMoved(int turn)
    {
        turnDoubleMoved = turn;
    }

    public int GetTurnDoubleMoved()
    {
        return turnDoubleMoved;
    }
}
