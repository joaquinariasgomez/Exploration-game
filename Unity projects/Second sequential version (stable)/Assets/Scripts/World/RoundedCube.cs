using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour {

    public int xSize, ySize, zSize;
    public int roundness;

    private Vector3[] normals;

    private Mesh mesh;
    private Vector3[] vertices;

    public void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Cube";
        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateVertices()
    {
        int cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;
        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];

        int v = 0;
        for (int y = 0; y <= ySize; y++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }
            for (int z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);
            }
            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);
            }
            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, ySize, z);
            }
        }
        for (int z = 1; z < zSize; z++)
        {
            for (int x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }
        mesh.vertices = vertices;
        mesh.normals = normals;
    }

    private void SetVertex(int i, int x, int y, int z)
    {
        Vector3 inner = vertices[i] = new Vector3(x, y, z);

        if(x<roundness)
        {
            inner.x = roundness;
        }
        else if(x>xSize-roundness)
        {
            inner.x = xSize - roundness;
        }
        if (y < roundness)
        {
            inner.y = roundness;
        }
        else if (y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }
        if (z < roundness)
        {
            inner.z = roundness;
        }
        else if (z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }

        normals[i] = (vertices[i] - inner).normalized;
        vertices[i] = inner + normals[i] * roundness;
    }

    private void CreateTriangles()
    {
        int[] trianglesZ = new int[(xSize * ySize) * 12];
        int[] trianglesX = new int[(ySize * zSize) * 12];
        int[] trianglesY = new int[(xSize * zSize) * 12];
        int ring = (xSize + zSize) * 2;
        int tZ = 0, tX = 0, tY = 0, v = 0;

        for (int y = 0; y < ySize; y++, v++)
        {
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }
            for (int q = 0; q < zSize - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }
            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);

        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
    }

    private int CreateTopFace(int[] triangles, int t, int ring)
    {
        int v = ring * ySize;
        for(int x = 0; x < xSize - 1; x++)
        {
            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            ++v;
        }
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

        int vMin = ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for(int z=1; z<zSize-1; z++)
        {
            t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
            for (int x = 1; x < xSize - 1; x++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                ++vMid;
            }
            t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            --vMin;
            ++vMid;
            ++vMax;
        }

        int vTop = vMin - 2;
        t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
        for(int x=1; x<xSize-1; x++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            --vTop;
            ++vMid;
        }
        t = SetQuad(triangles, t, vMid, vTop-2, vTop, vTop - 1);

        return t;
    }

    private int CreateBottomFace(int[] triangles, int t, int ring)
    {
        int v = 1;
        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
        t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
        for (int x = 1; x < xSize - 1; x++, v++, vMid++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
        }
        t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

        int vMin = ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
            for (int x = 1; x < xSize - 1; x++, vMid++)
            {
                t = SetQuad(
                    triangles, t,
                    vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }
            t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;
        t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
        for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

        return t;
    }

    private void CreateColliders()
    {
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
    }

    private void AddBoxCollider(float x, float y, float z)
    {
        BoxCollider c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(x, y, z);
    }

    private void AddCapsuleCollider(int direction, float x, float y, float z)
    {
        CapsuleCollider c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(x, y, z);
        c.direction = direction;
        c.radius = roundness;
        c.height = c.center[direction] * 2f;
    }

    private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
    {
        triangles[i] = v00;
        triangles[i + 1] = triangles[i + 4] = v01;
        triangles[i + 2] = triangles[i + 3] = v10;
        triangles[i + 5] = v11;
        return i + 6;
    }

    private void OnDrawGizmos()
    {
        if(vertices==null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for(int i=0; i<vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }
}

/*private VerticesData CreateVertices(string cara)
    {
        int cornerVertices = 4;
        int edgeVertices = (gridSize - 1) * 4;
        int faceVertices = (gridSize - 1) * (gridSize - 1);
        Vector3[] verticesParcial = new Vector3[cornerVertices + edgeVertices + faceVertices];
        Vector3[] normalsParcial = new Vector3[verticesParcial.Length];

        int v;
        switch (cara)
        {
            case "xy":
                v = 0;
                for (int y = 0; y <= gridSize; y++)
                {
                    for (int x = 0; x <= gridSize; x++)
                    {
                        SetVertex(verticesParcial, normalsParcial, v++, x, y, 0);
                    }
                }
                break;
            case "xyz":
                v = 0;
                for (int y = 0; y <= gridSize; y++)
                {
                    for (int x = gridSize; x >= 0; x--)
                    {
                        SetVertex(verticesParcial, normalsParcial, v++, x, y, gridSize);
                    }
                }
                break;
            case "zy":
                v = 0;
                for (int y = 0; y <= gridSize; y++)
                {
                    for (int z = gridSize; z >= 0; z--)
                    {
                        SetVertex(verticesParcial,normalsParcial, v++, 0, y, z);
                    }
                }
                break;
            case "zyx":
                v = 0;
                for (int y = 0; y <= gridSize; y++)
                {
                    for (int z = 0; z <= gridSize; z++)
                    {
                        SetVertex(verticesParcial, normalsParcial, v++, gridSize, y, z);
                    }
                }
                break;
            case "xz":
                v = 0;
                for (int z = 0; z <= gridSize; z++)
                {
                    for (int x = gridSize; x >= 0; x--)
                    {
                        SetVertex(verticesParcial, normalsParcial, v++, x, 0, z);
                    }
                }
                break;
            case "xzy":
                v = 0;
                for (int z = 0; z <= gridSize; z++)
                {
                    for (int x = 0; x <= gridSize; x++)
                    {
                        SetVertex(verticesParcial, normalsParcial, v++, x, gridSize, z);
                    }
                }
                break;
        }

        VerticesData data=new VerticesData(verticesParcial, normalsParcial);
        return data;
    }

    private void SetVertex(Vector3[] verticesParcial, Vector3[] normalsParcial, int i, int x, int y, int z)      //Generates the vertex 'i' in the coordinates x, y, z
    {
        Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;
        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
        s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
        s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
        normalsParcial[i] = s;
        verticesParcial[i] = normalsParcial[i] * radius;
    }*/

/*private void CreateVertices()
{
    int cornerVertices = 8;
    int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
    int faceVertices = ((gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1)) * 2;
    vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
    normals = new Vector3[vertices.Length];

    int v = 0;
    for (int y = 0; y <= gridSize; y++)
    {
        for (int x = 0; x <= gridSize; x++)
        {
            SetVertex(v++, x, y, 0);
        }
        for (int z = 1; z <= gridSize; z++)
        {
            SetVertex(v++, gridSize, y, z);
        }
        for (int x = gridSize - 1; x >= 0; x--)
        {
            SetVertex(v++, x, y, gridSize);
        }
        for (int z = gridSize - 1; z > 0; z--)
        {
            SetVertex(v++, 0, y, z);
        }
    }
    for (int z = 1; z < gridSize; z++)
    {
        for (int x = 1; x < gridSize; x++)
        {
            SetVertex(v++, x, gridSize, z);
        }
    }
    for (int z = 1; z < gridSize; z++)
    {
        for (int x = 1; x < gridSize; x++)
        {
            SetVertex(v++, x, 0, z);
        }
    }
    mesh.vertices = vertices;
    mesh.normals = normals;
}*/

/*private void SetVertex(int i, int x, int y, int z)      //Generates the vertex 'i' in the coordinates x, y, z
{
    Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
    float x2 = v.x * v.x;
    float y2 = v.y * v.y;
    float z2 = v.z * v.z;
    Vector3 s;
    s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
    s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
    s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
    normals[i] = s;
    vertices[i] = normals[i] * radius;
}*/

/*private void CreateTriangles()
{
    int[] trianglesZ = new int[(gridSize * gridSize) * 12];     //12 debido a que son 2 caras, 6 vertices cada cuadrado.
    int[] trianglesX = new int[(gridSize * gridSize) * 12];
    int[] trianglesY = new int[(gridSize * gridSize) * 12];
    int ring = gridSize * 4;
    int tZ = 0, tX = 0, tY = 0, v = 0;

    for (int y = 0; y < gridSize; y++, v++)
    {
        for (int q = 0; q < gridSize; q++, v++)
        {
            tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
        }
        for (int q = 0; q < gridSize; q++, v++)
        {
            tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
        }
        for (int q = 0; q < gridSize; q++, v++)
        {
            tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
        }
        for (int q = 0; q < gridSize - 1; q++, v++)
        {
            tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
        }
        tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
    }

    tY = CreateTopFace(trianglesY, tY, ring);
    tY = CreateBottomFace(trianglesY, tY, ring);

    mesh.triangles = trianglesY;
}

private int CreateTopFace(int[] triangles, int t, int ring)
{
    int v = ring * gridSize;
    for (int x = 0; x < gridSize - 1; x++)
    {
        t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
        ++v;
    }
    t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

    int vMin = ring * (gridSize + 1) - 1;
    int vMid = vMin + 1;
    int vMax = v + 2;

    for (int z = 1; z < gridSize - 1; z++)
    {
        t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
        for (int x = 1; x < gridSize - 1; x++)
        {
            t = SetQuad(triangles, t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            ++vMid;
        }
        t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
        --vMin;
        ++vMid;
        ++vMax;
    }

    int vTop = vMin - 2;
    t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
    for (int x = 1; x < gridSize - 1; x++)
    {
        t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
        --vTop;
        ++vMid;
    }
    t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

    return t;
}

private int CreateBottomFace(int[] triangles, int t, int ring)
{
    int v = 1;
    int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
    t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
    for (int x = 1; x < gridSize - 1; x++, v++, vMid++)
    {
        t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
    }
    t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

    int vMin = ring - 2;
    vMid -= gridSize - 2;
    int vMax = v + 2;

    for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
    {
        t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
        for (int x = 1; x < gridSize - 1; x++, vMid++)
        {
            t = SetQuad(
                triangles, t,
                vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
        }
        t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
    }

    int vTop = vMin - 1;
    t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
    for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++)
    {
        t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
    }
    t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

    return t;
}

private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
{
    triangles[i] = v00;
    triangles[i + 1] = triangles[i + 4] = v01;
    triangles[i + 2] = triangles[i + 3] = v10;
    triangles[i + 5] = v11;
    return i + 6;
}*/

/*private void GenerateChunks()
{
    GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Sphere";
        CreateVertices();
        CreateTriangles();
        CreateColliders();

    //INITIALIZE MESH
    //GetComponent<MeshFilter>().mesh = mesh = new Mesh();

    // CREATE VERTICES
    /*List<VerticesData> verticesData = new List<VerticesData>();
    verticesData.Add(CreateVertices("xy"));
    verticesData.Add(CreateVertices("xyz"));
    verticesData.Add(CreateVertices("zy"));
    verticesData.Add(CreateVertices("zyx"));
    verticesData.Add(CreateVertices("xz"));
    verticesData.Add(CreateVertices("xzy"));

    VerticesData finalData = CollectData(verticesData);
    vertices = finalData.verticesP();
    normals = finalData.normalsP();

    mesh.vertices = vertices;
    mesh.normals = normals;

    //CREATE TRIANGLES
    //CreateTriangles();
}*/
