using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    private const float TILE_SIZE = 1.0f;
    private const float PIECE_SCALING = 0.7f;

    private int selectionX = -1;
    private int selectionY = -1;
    private int turnNumber = 1;

    public Material boardMaterial;
    public Material selectorMaterial;
    public Material movementOptionMaterial;
    public Material attackOptionMaterial;

    public AudioClip movePieceSound;
    public AudioClip destroyPieceSound;
    public AudioClip selectPieceSound;
    private GameObject sfx;

    public Shader highlightShader;
    public Shader defaultShader;

    private GameObject chessBoard;
    private GameObject selectionBox;

    public List<GameObject> chessPiecesPrefabs;

    public ChessBoardSim ChessPieces { get; set; }
    private ChessPiece selectedPiece;

    private List<GameObject> movementOptionTiles = new List<GameObject>();
    private bool[,] MovementOptions { get; set; }

    private List<int[]>[] AlliedMovementOptions { get; set; }

    //public bool isWhiteTurn = true;
    private float graveyardWhite = 0.0f;
    private float graveyardBlack = TILE_SIZE * 8.0f;

    public Canvas canv;

    private void Start()
    {
        chessBoard = this.gameObject;
        ChessPieces = new ChessBoardSim(new ChessPiece[8, 8], turnNumber);
        MovementOptions = new bool[8, 8];
        DrawChessBoard();   
        PopulateBoard();
        InstantiateSelectionBox();
        CreateAudioSource();
        //SetUpUI();
        StartTurn();
    }

    private void Update()
    {
        UpdateSelection();
        CheckMouseDown();
    }


    private void CheckMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                //Check if a piece is currently selected
                if (selectedPiece == null)
                {
                    ChessPiece targetPiece = ChessPieces.GetPiece(selectionX, selectionY);
                    //Check for piece on selected tile, and if it belongs to the player
                    if (targetPiece != null && targetPiece.isWhite == ChessPieces.IsWhiteTurn() && CanMove(targetPiece))
                    {
                        //Display available movement options
                        DisplayMovementOptions(targetPiece);
                        HighlightPiece(targetPiece);
                        selectedPiece = targetPiece;
                    }
                }
                else
                {
                    //Check if valid movement option
                    if (MovementOptions[selectionX, selectionY])
                    {
                        MovePiece(selectedPiece, selectionX, selectionY);
                        //selectedPiece.SetHasMoved(true);
                        //If pawn reached other side, upgrade!!!
                        turnNumber += 1;
                        StartTurn();
                    }
                    //Deselect Piece
                    UnHighlightPiece(selectedPiece);
                    selectedPiece = null;
                    MovementOptions = new bool[8, 8];
                    foreach (GameObject square in movementOptionTiles)
                    {
                        Destroy(square);
                    }
                }
            }
        }
    }

    private void HighlightPiece(ChessPiece piece)
    {
        piece.gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", new Color(1.0f, 1.0f, 0.0f, 1.0f));
        piece.gameObject.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", 1.2f);
    }

    private void UnHighlightPiece(ChessPiece piece)
    {
        piece.gameObject.GetComponent<Renderer>().material.SetColor("_OutlineColor", new Color(1.0f, 1.0f, 1.0f, 0.0f));
        piece.gameObject.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", 1.0f);
    }

    private void UnHighlightPiece(GameObject piece)
    {
        piece.GetComponent<Renderer>().material.SetColor("_OutlineColor", new Color(1.0f, 1.0f, 1.0f, 0.0f));
        piece.GetComponent<Renderer>().material.SetFloat("_OutlineWidth", 1.0f);
    }

    private bool CanMove(ChessPiece piece)
    {
        return ChessPieces.GetNumberOfMovementOptions(ChessPieces.GetAlliedPiecesRemaining().IndexOf(piece)) > 0;
    }

    private void DisplayMovementOptions(ChessPiece piece)
    {
        foreach (int[] location in ChessPieces.GetAllMovementOptions()[ChessPieces.GetAlliedPiecesRemaining().IndexOf(piece)])
        {
            InstantiateMovementBox("Movement", location[0], location[1]);
        }
    }

    private void CreateAudioSource()
    {
        sfx = new GameObject();
        AudioSource comp = sfx.AddComponent<AudioSource>();
        comp.pitch = Random.Range(0.9f, 1.1f);
        comp.volume = 0.25f;
    }

    private void InstantiateAudioSource(int xpos, int ypos, AudioClip sound)
    {
        GameObject a = Instantiate(sfx);
        a.GetComponent<AudioSource>().clip = sound;
        a.GetComponent<AudioSource>().Play();
        Destroy(a, sound.length);
    }

    private void DrawChessBoard()
    {
        BoxCollider col = chessBoard.AddComponent<BoxCollider>();
        col.center = new Vector3(4, 0, 4);
        col.size = new Vector3(8, 0, 8);
        MeshFilter meshFilter = chessBoard.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, TILE_SIZE * 8), new Vector3(TILE_SIZE * 8, 0, 0), new Vector3(TILE_SIZE * 8, 0, TILE_SIZE * 8) };
        meshFilter.mesh.triangles = new int[] { 0, 1, 2, 3, 2, 1 };
        meshFilter.mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        meshFilter.mesh.RecalculateNormals();
        MeshRenderer meshRenderer = chessBoard.AddComponent<MeshRenderer>();
        meshRenderer.material = boardMaterial;
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
        {
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, LayerMask.GetMask("ChessBoard")))
        {
            Vector3 localTouchPoint = hit.transform.InverseTransformPoint(hit.point);
            selectionX = (int)localTouchPoint.x;
            selectionY = (int)localTouchPoint.z;
            MoveSelectionBox();
        }
        else
        {
            selectionBox.SetActive(false);
        }
    }

    private void InstantiateSelectionBox()
    {
        selectionBox = CreateBox("Selector");
        selectionBox.SetActive(false);
    }

    private GameObject CreateBox(string Type)
    {
        GameObject box = new GameObject();
        box.transform.parent = chessBoard.transform;
        box.transform.localRotation = Quaternion.identity;
        MeshFilter meshFilter = box.AddComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshFilter.mesh.vertices = new Vector3[] { new Vector3(0, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 0), new Vector3(1, 0, 1) };
        meshFilter.mesh.triangles = new int[] { 0, 1, 2, 3, 2, 1 };
        meshFilter.mesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) };
        meshFilter.mesh.RecalculateNormals();
        MeshRenderer meshRenderer = box.AddComponent<MeshRenderer>();
        switch (Type)
        {
            case "Selector":
                meshRenderer.material = selectorMaterial;
                break;
            case "Movement":
                meshRenderer.material = movementOptionMaterial;
                break;
            case "Attack":
                meshRenderer.material = attackOptionMaterial;
                break;
        }
        return box;
        //var color = meshRenderer.material.color;
        //color.a = 0.5f;
        //meshRenderer.material.color = color;
    }

    private void MoveSelectionBox()
    {
        if (selectionX < 0 || selectionY < 0)
        {
            selectionBox.SetActive(false);
            return;
        }
        selectionBox.transform.localPosition = TileToVector(selectionX, selectionY, 0.0f, 0.01f);
        selectionBox.SetActive(true);
    }

    private void SpawnChessPiece(int index, int xPos, int yPos)
    {
        GameObject piece = Instantiate(chessPiecesPrefabs[index], TileToVector(xPos, yPos, 0.5f, PIECE_SCALING / 2.0f), Quaternion.identity) as GameObject;
        piece.transform.localScale = Vector3.one * PIECE_SCALING;
        piece.transform.parent = chessBoard.transform;
        UnHighlightPiece(piece);
        ChessPieces.AddPiece(piece.GetComponent<ChessPiece>(), xPos, yPos);
    }

    private void PopulateBoard()
    {
        SpawnChessPiece(4, 0, 0);
        SpawnChessPiece(3, 1, 0);
        SpawnChessPiece(2, 2, 0);
        SpawnChessPiece(0, 3, 0);
        SpawnChessPiece(1, 4, 0);
        SpawnChessPiece(2, 5, 0);
        SpawnChessPiece(3, 6, 0);
        SpawnChessPiece(4, 7, 0);
        SpawnChessPiece(5, 0, 1);
        SpawnChessPiece(5, 1, 1);
        SpawnChessPiece(5, 2, 1);
        SpawnChessPiece(5, 3, 1);
        SpawnChessPiece(5, 4, 1);
        SpawnChessPiece(5, 5, 1);
        SpawnChessPiece(5, 6, 1);
        SpawnChessPiece(5, 7, 1);
        SpawnChessPiece(10, 0, 7);
        SpawnChessPiece(9, 1, 7);
        SpawnChessPiece(8, 2, 7);
        SpawnChessPiece(7, 3, 7);
        SpawnChessPiece(6, 4, 7);
        SpawnChessPiece(8, 5, 7);
        SpawnChessPiece(9, 6, 7);
        SpawnChessPiece(10, 7, 7);
        SpawnChessPiece(11, 0, 6);
        SpawnChessPiece(11, 1, 6);
        SpawnChessPiece(11, 2, 6);
        SpawnChessPiece(11, 3, 6);
        SpawnChessPiece(11, 4, 6);
        SpawnChessPiece(11, 5, 6);
        SpawnChessPiece(11, 6, 6);
        SpawnChessPiece(11, 7, 6);
    }

    private void DestroyPiece(ChessPiece piece)
    {
        InstantiateAudioSource(piece.GetPosition()[0], piece.GetPosition()[1], destroyPieceSound);
        //Move Piece to the graveyard, and remove it from active pieces/piece list.
        if (piece.isWhite)
        {
            piece.gameObject.transform.localPosition = new Vector3(graveyardWhite, 0, -1);
            graveyardWhite += 1.0f;
        }
        else
        {
            piece.gameObject.transform.localPosition = new Vector3(graveyardBlack, 0, TILE_SIZE * 8 + 1);
            graveyardBlack -= 1.0f;
        }
        ChessPieces.RemovePiece(piece);
    }

    private void MovePiece(ChessPiece piece, int xpos, int ypos)
    {
        ChessPieces.MovePiece(piece, xpos, ypos,true);
        piece.transform.localPosition = TileToVector(xpos, ypos, 0.5f, PIECE_SCALING / 2.0f);
        //piece.SetPosition(new int[] { xpos, ypos });
        //ChessPieces.SetPiece(xpos, ypos, piece);
        InstantiateAudioSource(xpos, ypos, movePieceSound);
    }

    private Vector3 TileToVector(int xpos, int ypos, float offset, float height)
    {
        return new Vector3((xpos + offset) * TILE_SIZE, height, (ypos + offset) * TILE_SIZE);
    }

    private void InstantiateMovementBox(string Type, int xpos, int ypos)
    {
        GameObject movementOptionBox = CreateBox(Type);
        movementOptionBox.transform.localPosition = TileToVector(xpos, ypos, 0.0f, 0.01f);
        movementOptionTiles.Add(movementOptionBox);
        MovementOptions[xpos, ypos] = true;
    }

    private void StartTurn()
    {
        ChessPieces.CalculateAllMovementOptions();
        ChessPieces.GetAllMovementOptions();
        if (ChessPieces.MovementOptionsAreAvailable())
        {
            Debug.Log("Moves available");
        }
        else
        {
            string winner = ChessPieces.IsWhiteTurn() ? "Black" : "White";
            Debug.Log(winner + " wins!");
        }
    }

    private ChessBoardSim SimulateBoard(ChessPiece piece, int[] newPositionCoords)
    {
        ChessBoardSim simBoard = new ChessBoardSim(ChessPieces.GetBoardLayout(), turnNumber);
        simBoard.MovePiece(piece, newPositionCoords[0], newPositionCoords[1], true);
        return simBoard;
    }
}
