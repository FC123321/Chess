using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChessBoardSim
{
    private ChessPiece[,] boardLayout = new ChessPiece[8, 8];
    private int[] enPassantTile = new int[2];
    private int enPassantDir;
    private bool[] castlingOpportunities = new bool[2];
    private int turnNumber;
    private List<int[]>[] allMovementOptions; //Each piece has a list of movement options, which are collected into an array of lists.
    private int totalNumberOfMovementOptions;
    private int[] numberOfMovementOptions;

    private List<ChessPiece> whitePiecesRemaining = new List<ChessPiece>();
    private List<ChessPiece> blackPiecesRemaining = new List<ChessPiece>();
    //public ChessBoardSim(ChessPiece[,] boardLayout, int turnNumber, ChessPiece piece, int[]moveLoc)
    public ChessBoardSim(ChessPiece[,] boardLayout, int turnNumber)
    {
        this.boardLayout = (ChessPiece[,])boardLayout.Clone();
        this.turnNumber = turnNumber;
        ChessPiece temp;
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                temp = boardLayout[i, j];
                if (temp != null)
                {
                    if (temp.isWhite)
                    {
                        whitePiecesRemaining.Add(temp);
                    }
                    else
                    {
                        blackPiecesRemaining.Add(temp);
                    }
                }
            }
        }
    }

    public bool IsWhiteTurn()
    {
        return turnNumber % 2 == 0 ? false : true;
    }
        

    public string GetBoardString()
    {
        string str = "";
        for (int i = 7; i >=0; i--)
        {
            str += i + ": ";
            for (int j = 0; j < 8; j++)
            {
                if (boardLayout[j, i] == null)
                {
                    str += "X";
                }
                else
                {
                    str += "O";
                }
            }
            str += "\n";
        }
        return str;
    }

    public void AddPiece(ChessPiece piece, int xPos, int yPos)
    {
        if (piece.isWhite)
        {
            whitePiecesRemaining.Add(piece);
        }
        else
        {
            blackPiecesRemaining.Add(piece);
        }
        SetPiece(xPos, yPos, piece);
    }

    public void RemovePiece(ChessPiece piece)
    {
        if (piece.isWhite)
        {
            whitePiecesRemaining.Remove(piece);
        }
        else
        {
            blackPiecesRemaining.Remove(piece);
        }
        boardLayout[piece.GetPosition()[0], piece.GetPosition()[1]] = null;
    }

    private List<int[]> GetMovementOptions(ChessPiece piece)
    {
        List<int[]> pieceMovementOptions = new List<int[]>();
        int xpos = piece.GetPosition()[0];
        int ypos = piece.GetPosition()[1];
        switch (piece.movementType)
        {
            case "King":
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (TestTileForMovementAndAttack(xpos + i, ypos + j, piece.isWhite)[0])
                        {
                            pieceMovementOptions.Add(new int[2] { xpos + i, ypos + j });
                        }
                    }
                }
                //castling
                break;
            case "Queen":
                TestCrossMovement(xpos, ypos, piece.isWhite, pieceMovementOptions);
                TestDiagonalMovement(xpos, ypos, piece.isWhite, pieceMovementOptions);
                break;
            case "Bishop":
                TestDiagonalMovement(xpos, ypos, piece.isWhite, pieceMovementOptions);
                break;
            case "Knight":
                int[,] options = new int[,] { { 1, 2 }, { 1, -2 }, { -1, 2 }, { -1, -2 }, { 2, 1 }, { 2, -1 }, { -2, 1 }, { -2, -1 } };
                for (int i = 0; i < 8; i++)
                {
                    if (TestTileForMovementAndAttack(xpos + options[i, 0], ypos + options[i, 1], piece.isWhite)[0])
                    {
                        pieceMovementOptions.Add(new int[2] { xpos + options[i, 0], ypos + options[i, 1] });
                    }
                }
                break;
            case "Rook":
                TestCrossMovement(xpos, ypos, piece.isWhite, pieceMovementOptions);
                break;
            case "Pawn":
                int direction;
                if (piece.isWhite)
                {
                    direction = 1;
                }
                else
                {
                    direction = -1;
                }
                if (TestTileForMovement(xpos, ypos + direction, piece.isWhite))
                {
                    pieceMovementOptions.Add(new int[2] { xpos, ypos + direction });
                    if (TestTileForMovement(xpos, ypos + direction * 2, piece.isWhite) && !piece.GetHasMoved())
                    {
                        pieceMovementOptions.Add(new int[2] { xpos, ypos + direction * 2 });
                    }
                }
                if (TestTileForAttack(xpos - 1, ypos + direction, piece.isWhite))
                {
                    pieceMovementOptions.Add(new int[2] { xpos - 1, ypos + direction });
                }
                if (TestTileForAttack(xpos + 1, ypos + direction, piece.isWhite))
                {
                    pieceMovementOptions.Add(new int[2] { xpos + 1, ypos + direction });
                }
                //en passant
                for (int i = -1; i <= 1; i += 2)
                {
                    if (xpos + i < 8 && xpos + i > -1 && boardLayout[xpos + i, ypos] != null && boardLayout[xpos + i, ypos].movementType == "Pawn" && boardLayout[xpos + i, ypos].GetTurnDoubleMoved() == turnNumber - 1)
                    {
                        //create En Passant square (NEED TO DETECT THE CURRENT BOARD)
                        enPassantTile = new int[2] { xpos + i, ypos + direction };
                        enPassantDir = -direction;
                        pieceMovementOptions.Add(new int[2] { xpos + i, ypos + direction });
                        break;
                    }
                }
                break;
        }
        return pieceMovementOptions;
    }

    public void CalculateAllMovementOptions()
    {
        allMovementOptions = new List<int[]>[16] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
        numberOfMovementOptions = new int[GetAlliedPiecesRemaining().Count];
        totalNumberOfMovementOptions = 0;
        for (int i = 0; i < GetAlliedPiecesRemaining().Count; i++)
        {
            allMovementOptions[i] = GetMovementOptions(GetAlliedPiecesRemaining()[i]);
            for (int j = 0; j < allMovementOptions[i].Count; j++)
            {
                //Remove movement options which expose King
                TestMovementOptionsForCheck(GetAlliedPiecesRemaining()[i], allMovementOptions[i]);
                numberOfMovementOptions[i] = allMovementOptions[i].Count;
                totalNumberOfMovementOptions += numberOfMovementOptions[i];
            }
        }
    }

    private bool TestTileForMovement(int xpos, int ypos, bool isWhite)
    {
        bool ret = false;// ret == canMove
        ChessPiece target;
        if (xpos > -1 && xpos < 8 && ypos > -1 && ypos < 8) //If position is on the board
        {
            target = boardLayout[xpos, ypos];
            if (target == null) //If the space is unoccupied
            {
                ret = true;
            }
        }
        return ret;
    }

    private bool TestTileForAttack(int xpos, int ypos, bool isWhite)
    {
        bool ret = false; // ret == enemyOccupies
        ChessPiece target;
        if (xpos > -1 && xpos < 8 && ypos > -1 && ypos < 8) //If position is on the board
        {
            target = boardLayout[xpos, ypos];
            if (target != null && target.isWhite != isWhite) //If the space is occupied by an enemy
            {
                ret = true;
            }
        }
        return ret;
    }

    private bool[] TestTileForMovementAndAttack(int xpos, int ypos, bool isWhite)
    {
        bool[] ret = new bool[2] { false, false }; // ret[0] == canMove, ret[1] == enemyOccupies
        ChessPiece target;
        if (xpos > -1 && xpos < 8 && ypos > -1 && ypos < 8) //If position is on the board
        {
            target = boardLayout[xpos, ypos];
            if (target == null) //If the space is unoccupied
            {
                ret[0] = true;
            }
            else if (target.isWhite != isWhite) //If the space is occupied by an enemy
            {
                ret = new bool[2] { true, true };
            }
            //else the space is occupied by an ally
        }
        return ret;
    }

    private void TestDiagonalMovement(int xpos, int ypos, bool isWhite, List<int[]> list)
    {
        int offset; //used for rook, bishop, and queen LoS
        bool[] returnValues = new bool[2] { false, false }; //[CanMove, enemyOccupies]
                                                            //ur
        offset = 1;
        while (xpos + offset < 8 && ypos + offset < 8 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos + offset, ypos + offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos + offset, ypos + offset });
            }
            else
            {
                break;
            }
            offset++;
        }
        //ul
        offset = 1;
        returnValues[1] = false;
        while (xpos - offset > -1 && ypos + offset < 8 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos - offset, ypos + offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos - offset, ypos + offset });
            }
            else
            {
                break;
            }
            offset++;
        }
        //dr
        offset = 1;
        returnValues[1] = false;
        while (xpos + offset < 8 && ypos - offset > -1 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos + offset, ypos - offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos + offset, ypos - offset });
            }
            else
            {
                break;
            }
            offset++;
        }
        //dl
        offset = 1;
        returnValues[1] = false;
        while (xpos - offset > -1 && ypos - offset > -1 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos - offset, ypos - offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos - offset, ypos - offset });
            }
            else
            {
                break;
            }
            offset++;
        }
    }

    private void TestCrossMovement(int xpos, int ypos, bool isWhite, List<int[]> list)
    {
        int offset; //used for rook, bishop, and queen LoS
        bool[] returnValues = new bool[2] { false, false }; //[CanMove, enemyOccupies]
        //up
        offset = 1;
        while (ypos + offset < 8 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos, ypos + offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos, ypos + offset });
            }
            else
            {
                break;
            }
            offset++;
        }
        //down
        offset = 1;
        returnValues[1] = false;
        while (ypos - offset > -1 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos, ypos - offset, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos, ypos - offset });
            }
            else
            {
                break;
            }
            offset++;
        }
        //right
        offset = 1;
        returnValues[1] = false;
        while (xpos + offset < 8 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos + offset, ypos, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos + offset, ypos });
            }
            else
            {
                break;
            }
            offset++;
            //castling
        }
        //left
        offset = 1;
        returnValues[1] = false;
        while (xpos - offset > -1 && !returnValues[1])
        {
            returnValues = TestTileForMovementAndAttack(xpos - offset, ypos, isWhite);
            if (returnValues[0])
            {
                list.Add(new int[2] { xpos - offset, ypos });
            }
            else
            {
                break;
            }
            offset++;
        }
    }

    public void MovePiece(ChessPiece piece, int xPos, int yPos, bool firstMoveOfTurn)
    {
        int[] oldLoc = piece.GetPosition();
        Debug.Log("OLD LOCATION" + oldLoc[0] + " " + oldLoc[1]);
        //Remove old piece
        if(oldLoc != new int[2] { xPos, yPos })
        {
            if (boardLayout[xPos, yPos] != null)
            {
                RemovePiece(boardLayout[xPos, yPos]);
            }
            boardLayout[oldLoc[0], oldLoc[1]] = null;
        }
        boardLayout[xPos, yPos] = piece;

        if (piece.movementType == "Pawn" && enPassantTile[0] == xPos && enPassantTile[1] == yPos)
        {
            RemovePiece(boardLayout[xPos, yPos + enPassantDir]);
        }
        else if (piece.movementType == "King")
        {
            int[,] spacesToCheck = new int[4, 2] { { 1, 0 }, { 5, 0 }, { 2, 7 }, { 6, 7 } };
            for (int i = 0; i < 2; i++)
                if (xPos == spacesToCheck[i * 2, 0] && yPos == spacesToCheck[i * 2, 1] && castlingOpportunities[0])
                {
                    //Move rook to old king pos -1
                    MovePiece(boardLayout[0, i * 7], piece.GetPosition()[0] - 1, piece.GetPosition()[1], false);
                }
                else if (xPos == spacesToCheck[i * 2 + 1, 0] && yPos == spacesToCheck[i * 2 + 1, 1] && castlingOpportunities[1])
                {
                    //Move rook to old king pos +1
                    MovePiece(boardLayout[7, i * 7], piece.GetPosition()[0] + 1, piece.GetPosition()[1], false);
                }
        }
        if (piece.movementType == "Pawn" && Mathf.Abs(yPos - piece.GetPosition()[1]) == 2)
        {
            piece.SetTurnDoubleMoved(turnNumber);
        }
        piece.SetHasMoved(true);
        piece.SetPosition(xPos, yPos);
        if (firstMoveOfTurn)
        {
            turnNumber++;
        }
        //The following is wrong if the moved piece was a pawn which moved 2, but this is irrelevant at the moment
        enPassantTile = new int[2] { -1, -1 };
    }

    public List<ChessPiece> GetAlliedPiecesRemaining()
    {
        return IsWhiteTurn() ? whitePiecesRemaining : blackPiecesRemaining;
    }

    public List<ChessPiece> GetEnemyPiecesRemaining()
    {
        return IsWhiteTurn() ? blackPiecesRemaining : whitePiecesRemaining;
    }

    public ChessPiece GetPiece(int xPos, int yPos)
    {
        return boardLayout[xPos, yPos];
    }

    public void SetPiece(int xPos, int yPos, ChessPiece piece)
    {
        boardLayout[xPos, yPos] = piece;
        piece.SetPosition(xPos, yPos);
    }

    public int[] GetEnPassantTile()
    {
        return enPassantTile;
    }

    public int GetEnPassantDir()
    {
        return enPassantDir;
    }

    public ChessPiece[,] GetBoardLayout()
    {
        return boardLayout;
    }

    private bool checkIfKingIsChecked()
    {
        List<int[]>[] allMovementOptionsC = new List<int[]>[16] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null };
        for (int i = 0; i < GetAlliedPiecesRemaining().Count; i++)
        {
            allMovementOptionsC[i] = GetMovementOptions(GetAlliedPiecesRemaining()[i]);
            for (int j = 0; j < allMovementOptionsC[i].Count; j++)
            {
                //If a certain movement leads to the capture of the enemy king, return true. Otherwise, continue.
                if (allMovementOptionsC[i][j][0] == GetEnemyKing().GetPosition()[0] && allMovementOptionsC[i][j][1] == GetEnemyKing().GetPosition()[1])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private bool[] CheckCastling()
    {
        //If the pieces are in the right locations AND the pieces haven't moved AND there are no pieces between them AND the King would not move through Check
        //ret[0] is for left castle, ret[1] is for right castle
        bool[] ret;
        ChessPiece king;
        ChessPiece leftRook;
        ChessPiece rightRook;
        bool[] noSpacesBetween = new bool[2];
        bool[] checkTests = new bool[2];
        bool kingIsNotInCheck;
        king = boardLayout[GetAlliedKing().GetPosition()[0], GetAlliedKing().GetPosition()[1]];
        if (IsWhiteTurn())
        {
            leftRook = boardLayout[0, 0] != null && boardLayout[0, 0].movementType == "Rook" ? boardLayout[0, 0] : null;
            rightRook = boardLayout[0, 7] != null && boardLayout[0, 7].movementType == "Rook" ? boardLayout[0, 7] : null;
            noSpacesBetween[0] = boardLayout[1, 0] == null && boardLayout[2, 0] == null;
            noSpacesBetween[1] = boardLayout[4, 0] == null && boardLayout[5, 0] == null && boardLayout[6, 0] == null;
            checkTests[0] = ValidateKingIsSafeOnSquare(new int[2] { 1, 0 }) && ValidateKingIsSafeOnSquare(new int[2] { 2, 0 });
            checkTests[1] = ValidateKingIsSafeOnSquare(new int[2] { 4, 0 }) && ValidateKingIsSafeOnSquare(new int[2] { 5, 0 });
        }
        else
        {
            leftRook = boardLayout[7, 0] != null && boardLayout[7, 0].movementType == "Rook" ? boardLayout[7, 0] : null;
            rightRook = boardLayout[7, 7] != null && boardLayout[7, 7].movementType == "Rook" ? boardLayout[7, 7] : null;
            noSpacesBetween[0] = boardLayout[1, 7] == null && boardLayout[2, 7] == null && boardLayout[3, 7] == null;
            noSpacesBetween[1] = boardLayout[5, 7] == null && boardLayout[6, 7] == null;
            checkTests[0] = ValidateKingIsSafeOnSquare(new int[2] { 2, 7 }) && ValidateKingIsSafeOnSquare(new int[2] { 3, 7 });
            checkTests[1] = ValidateKingIsSafeOnSquare(new int[2] { 5, 7 }) && ValidateKingIsSafeOnSquare(new int[2] { 6, 7 });
        }
        //Validate king is not currently in check
        kingIsNotInCheck = ValidateKingIsSafeOnSquare(GetAlliedKing().GetPosition());
        ret = new bool[2] { leftRook != null && !leftRook.GetHasMoved() && !king.GetHasMoved() && noSpacesBetween[0] && checkTests[0] && kingIsNotInCheck, rightRook != null && !rightRook.GetHasMoved() && !king.GetHasMoved() && noSpacesBetween[1] && checkTests[1] && kingIsNotInCheck };
        return ret;
    }

    private bool ValidateKingIsSafeOnSquare(int[] square)
    {
        bool isSafe;
        ChessBoardSim simBoard = SimulateBoard(GetAlliedKing(), square);
        isSafe = simBoard.checkIfKingIsChecked();
        return isSafe;
    }

    private ChessBoardSim SimulateBoard(ChessPiece piece, int[] newPositionCoords)
    {
        ChessBoardSim simBoard = new ChessBoardSim((ChessPiece[,]) boardLayout.Clone(), turnNumber);
        simBoard.MovePiece(piece, newPositionCoords[0], newPositionCoords[1], true);
        Debug.Log(whitePiecesRemaining.Count + " " + blackPiecesRemaining.Count);
        Debug.Log(simBoard.whitePiecesRemaining.Count + " " + simBoard.blackPiecesRemaining.Count);
        return simBoard;
    }

    private void TestMovementOptionsForCheck(ChessPiece piece, List<int[]> MovementOptions)
    {
        ChessBoardSim simBoard;
        int[] newPositionCoords = new int[2];
        for (int i = 0; i < MovementOptions.Count; i++)
        {
            simBoard = SimulateBoard(piece, MovementOptions[i]);
            if (simBoard.checkIfKingIsChecked())
            {
                int[] temp = MovementOptions.Find(e => e[0] == newPositionCoords[0] && e[1] == newPositionCoords[1]);
                if (temp != null)
                {
                    MovementOptions.Remove(temp);
                    i--;
                }
            }

        }
    }

    public bool MovementOptionsAreAvailable()
    {
        return totalNumberOfMovementOptions > 0;
    }

    public int GetNumberOfMovementOptions(int i)
    {
        return numberOfMovementOptions[i];
    }

    public List<int[]>[] GetAllMovementOptions()
    {
        return allMovementOptions;
    }

    private ChessPiece GetAlliedKing()
    {
        return GetAlliedPiecesRemaining().Find(e => e.movementType == "King");
    }

    private ChessPiece GetEnemyKing()
    {
        return GetEnemyPiecesRemaining().Find(e => e.movementType == "King");
    }
}