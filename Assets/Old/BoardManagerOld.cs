using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*public class BoardManager : MonoBehaviour {
    private const float TILE_SIZE = 1.0f;
    private const float OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;
    public Material[] material;
    private GameObject[] squares = new GameObject[64];

    private void Start()
    {
        DrawChessBoard();
    }
    private void Update()
    {
        UpdateSelection();
        //RedrawChessBoard();
    }

    private void DrawChessBoard()
    {
        string color = "";
        for(int i = 0; i <=7; i++)
        {
            for (int j = 0; j <= 7; j++)
            {
                color = ((i + j) % 2 == 0) ? "black" : "white";
                squares[i * 8 + j] = DrawSquare(i, j, color);
            }
        }
    }
    
    private void RedrawChessBoard()
    {
        for (int i = 0; i<64; i++)
        {
            Destroy(squares[i]);
        }
        DrawChessBoard();
    }

    private GameObject DrawSquare(float xpos, float zpos, string color)
    {
        GameObject aSquare = new GameObject();
        aSquare.layer = 9;
        MeshFilter meshFilter = aSquare.AddComponent<MeshFilter>();
        MeshCollider col = aSquare.AddComponent<MeshCollider>();
        meshFilter.mesh = new Mesh();
        MeshRenderer meshRenderer = aSquare.AddComponent<MeshRenderer>();
        Vector3[] vertices = new Vector3[] { new Vector3(xpos, 0, zpos), new Vector3(xpos + TILE_SIZE, 0, zpos), new Vector3(xpos, 0, zpos + TILE_SIZE), new Vector3(xpos + TILE_SIZE, 0, zpos + TILE_SIZE) };
        int[] triangles = new int[6] { 0, 2, 1, 3, 1, 2 };
        Vector2[] uv = new Vector2[4] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
        //Create two triangles with vertices specified above, then fill them in.
        meshFilter.mesh.vertices = vertices;
        meshFilter.mesh.triangles = triangles;
        meshFilter.mesh.uv = uv;
        meshFilter.mesh.RecalculateNormals();
        if (color == "white")
        {
            meshRenderer.material = material[0];
        }
        else
        {
            meshRenderer.material = material[1];
        }
        aSquare.SetActive(true);
        return aSquare;
    }

    private GameObject GetTile(int xpos, int ypos)
    {
        return squares[xpos * 8 + ypos];
    }

    private void UpdateSelection()
    {
        if(!Camera.main)
        {
            return;
        }

        RaycastHit hit;
        if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 50.0f, 9))
        {
            Destroy(hit.collider.gameObject);
            Debug.Log("hi");
        }
    }
}
*/