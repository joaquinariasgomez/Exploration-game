using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class CubeSphere : MonoBehaviour
{
    public int gridSize;
    public float gravity = -9.8f;
    public TerrainType[] regions;
    public Material material;
    public Transform bodyAttracted;

    private float radius;
    private int sqrtChunksPerFace = 5;
    private static float heightMultiplier = 20;

    private static float[,] noiseMap;
    private static Color[] colourMap;
    private static int seed = 1024;
    private static float scale = 30f;
    private static int octaves = 4;
    private static float persistance = 0.36f;
    private static float lacunarity = 1.7f;
    private static Vector2 offset = new Vector2(0, 0);

    private int chunkSize;
    private List<Chunk> chunks;

    private Rigidbody rigidbodyAttracted;

    private Vector3 viewerPositionOld;
    private Vector3 viewerPosition;

    private float secondsCounter = 0;
    private float secondsToCount = 0.25f;

    const float viewerMoveThreshholdForChunkUpdate = 10f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThreshholdForChunkUpdate * viewerMoveThreshholdForChunkUpdate;
    private int closestChunkNumber = 0;

    private List<Vector3> centers;

    private Vector3[] vertices;     //Only for OnDrawGizmos
    private Vector3[] normals;      //Only for OnDrawGizmos

    private static int nH = 1;
    private Thread[] lodThreads=new Thread[nH];   //Testing
    private static List<VerticesData> verticesData; //Testing

    public void Awake()
    {
        viewerPositionOld = viewerPosition = bodyAttracted.position;

        radius = gridSize / 2;
        chunkSize = gridSize / sqrtChunksPerFace;
        GenerateChunks();
    }

    private void Start()
    {
        ClosestChunkHasChanged();   //Set variables for UpdateChunks()
        UpdateChunks();
        VerticesData finalData = CollectData(verticesData);
        vertices = finalData.verticesP();
        normals = finalData.normalsP();
    }

    private void Update()
    {
        /*secondsCounter += Time.deltaTime;
        if (secondsCounter > secondsToCount)
        {
            secondsCounter = 0;
            //DO THINGS EVERY secondsToCount SECONDS
            if (ClosestChunkHasChanged())
            {
                UpdateChunks();
            }
        }*/

        viewerPosition = bodyAttracted.position;

        if((viewerPositionOld-viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            //DO THINGS EACH TIME VIEWER MOVE viewerMoveThreshHoldForChunkUpdate UNITS
            if (ClosestChunkHasChanged())
            {
                UpdateChunks();
            }
        }
    }

    private void GenerateChunks()
    {
        chunks = new List<Chunk>();

        verticesData = new List<VerticesData>();
        centers = new List<Vector3>();

        GenerateChunksOfFace(chunks, "xy", verticesData);
        GenerateChunksOfFace(chunks, "xyz", verticesData);
        GenerateChunksOfFace(chunks, "zy", verticesData);
        GenerateChunksOfFace(chunks, "zyx", verticesData);
        GenerateChunksOfFace(chunks, "xz", verticesData);
        GenerateChunksOfFace(chunks, "xzy", verticesData);

        /*VerticesData finalData = CollectData(verticesData);
        vertices = finalData.verticesP();
        normals = finalData.normalsP();*/
    }

    private VerticesData CollectData(List<VerticesData> verticesData)
    {
        int numVertices = 0;
        int numNormals = 0;

        foreach (VerticesData data in verticesData)
        {
            numVertices += data.verticesP().Length;
            numNormals += data.normalsP().Length;
        }

        Vector3[] finalVertices = new Vector3[numVertices];
        Vector3[] finalNormals = new Vector3[numNormals];
        int contVertices = 0;
        int contNormals = 0;

        foreach (VerticesData data in verticesData)
        {
            Vector3[] vertices_data = data.verticesP();
            Vector3[] normals_data = data.normalsP();
            for (int i = 0; i < data.verticesP().Length; i++)
            {
                finalVertices[contVertices++] = vertices_data[i];
            }
            for (int i = 0; i < data.normalsP().Length; i++)
            {
                finalNormals[contNormals++] = normals_data[i];
            }
        }

        return new VerticesData(finalVertices, finalNormals);
    }

    private Texture2D CreateTexture()
    {
        //Generate Noise Map and ColourMap
        int width = gridSize + 1;
        int height = gridSize + 1;

        noiseMap = Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset);
        colourMap = new Color[width * height];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float currentHeight = noiseMap[j, i];
                for (int k = 0; k < regions.Length; k++)
                {
                    if (currentHeight <= regions[k].height)
                    {
                        colourMap[i * width + j] = regions[k].colour;
                        break;
                    }
                }
            }
        }

        Texture2D texture = TextureGenerator.TextureFromHeightMap(noiseMap, colourMap);
        return texture;
    }

    private void GenerateChunksOfFace(List<Chunk> chunks, string face, List<VerticesData> verticesData)
    {
        Texture2D faceTexture=CreateTexture();
        switch (face)
        {
            case "xy": for (int y = 0; y < sqrtChunksPerFace; y++)
                       {
                           for (int x = 0; x < sqrtChunksPerFace; x++)
                           {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, y * chunkSize, 0, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                           }
                       } break;
            case "xyz": for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, y * chunkSize, 0, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "zy":  for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int z = 0; z < sqrtChunksPerFace; z++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, 0, y * chunkSize, z * chunkSize, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "zyx": for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int z = 0; z < sqrtChunksPerFace; z++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, 0, y * chunkSize, z * chunkSize, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "xz": for (int z = 0; z < sqrtChunksPerFace; z++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, 0, z * chunkSize, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "xzy": for (int z = 0; z < sqrtChunksPerFace; z++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, 0, z * chunkSize, gridSize, regions, faceTexture);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        } break;
        }
    }

    private bool ClosestChunkHasChanged()
    {
        //Select closest Chunk
        float min_distance = float.MaxValue;
        Chunk selectedChunk = chunks[0];
        int closestChunkCount = -1;
        int chunkCount = 0;

        foreach(Chunk chunk in chunks)
        {
            float distance = Vector3.Distance(bodyAttracted.position, chunk.GetCenter());
            if (distance < min_distance)
            {
                min_distance = distance;
                selectedChunk = chunk;
                closestChunkCount=chunkCount;
            }
            ++chunkCount;
        }

        foreach (Chunk chunk in chunks)
        {
            chunk.SetClosestChunk(false);
        }
        selectedChunk.SetClosestChunk(true);

        if(closestChunkCount != closestChunkNumber)
        {
            closestChunkNumber = closestChunkCount;
            //Update adjacent chunks of the selected chunk
            min_distance = float.MaxValue;
            List<Chunk> adjacentChunks = new List<Chunk>();

            foreach (Chunk chunk in chunks)
            {
                if (chunk != selectedChunk)
                {
                    float distance = Vector3.Distance(selectedChunk.GetCenter(), chunk.GetCenter());
                    chunk.SetDistanceToClosestChunk(distance);
                    adjacentChunks.Add(chunk);
                }
            }
            adjacentChunks.Sort(delegate (Chunk a, Chunk b)
            {
                return a.GetDistanceToClosestChunk().CompareTo(b.GetDistanceToClosestChunk());
            });
            selectedChunk.SetAdjacentChunks(adjacentChunks);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void UpdateLODParallel(Chunk chunk, int reason)
    {
        //Work assignment
        for (int i = 0; i < lodThreads.Length; i++)
        {
            lodThreads[i] = new Thread(chunk.ExecuteCalculation);
        }

        if(!chunk.UpdateLOD(reason)) { return; }

        //Work launch
        chunk.fromV = 0;
        chunk.fromUVCont = 0;
        chunk.fromWork = 0;
        chunk.toWork = chunkSize / (nH * reason);
        chunk.fromT = 0;

        chunk.toX = chunk.fromX + chunkSize / nH;
        chunk.toY = chunk.fromY + chunkSize / nH;
        chunk.toZ = chunk.fromZ + chunkSize / nH;
        //print("Chunk en ejecucion");
        for (int i = 0; i < lodThreads.Length; i++)
        {
            lodThreads[i].Start();

            chunk.toX += chunkSize / nH;
            chunk.toY += chunkSize / nH;
            chunk.toZ += chunkSize / nH;

            chunk.fromV += (chunkSize / nH + 1) * (chunkSize / nH + 1);
            chunk.fromUVCont += (chunkSize / nH + 1) * (chunkSize / nH + 1);
            chunk.fromWork = chunk.toWork;
            chunk.toWork += chunkSize / (nH * reason);
            chunk.fromT += 6 * chunkSize / (nH * reason);
        }

        for (int i = 0; i < lodThreads.Length; i++)
        {
            lodThreads[i].Join();
        }

        //Collect vertices data
        if (chunk.GetVerticesData()!=null)
        {
            verticesData.Add(chunk.GetVerticesData());
        }
        else
        {
            //print("Este chunk no ha generado una mierda");
        }

        //chunk.AssignDataToMesh();
    }

    private void UpdateChunks()
    {
        Chunk closestChunk = chunks[0];

        foreach(Chunk chunk in chunks)
        {
            if(chunk.IsClosestChunk())
            {
                closestChunk = chunk;
                UpdateLODParallel(closestChunk, 1);
            }
        }

        int adjacentCount = 1;
        foreach (Chunk adjacent in closestChunk.GetAdjacentChunks())
        {
            UpdateLODParallel(adjacent, 1);
            /*if (adjacentCount < 150)
            {
                adjacent.UpdateLOD(1);
                ++adjacentCount;
            }
            else
            {
                if (adjacentCount < 300)
                {
                    adjacent.UpdateLOD(2);
                    ++adjacentCount;
                }
                else
                {
                    if (adjacentCount < 600)
                    {
                        adjacent.UpdateLOD(4);
                        ++adjacentCount;
                    }
                    else
                    {
                        if(adjacentCount < 900)
                        {
                            adjacent.UpdateLOD(8);
                            ++adjacentCount;
                        }
                        else
                        {
                            if(adjacentCount < 1200)
                            {
                                adjacent.UpdateLOD(16);
                                ++adjacentCount;
                            }
                            else
                            {
                                adjacent.SetActive(false);
                            }
                        }
                    }
                }
            }*/
        }
    }

    public void Attract(Transform body)
    {
        bodyAttracted = body;

        Vector3 gravityUp = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        rigidbodyAttracted = body.GetComponent<Rigidbody>();

        //rigidbodyAttracted.AddForce(gravityUp * gravity);

        Quaternion targetRotation = Quaternion.FromToRotation(bodyUp, gravityUp) * body.rotation;
        body.rotation = Quaternion.Slerp(body.rotation, targetRotation, 50 * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        /*Gizmos.color = Color.blue;
        Gizmos.DrawSphere(viewerPositionOld, 1f);*/

        /*if (centers != null)
        {
            Gizmos.color = Color.yellow;
            foreach (Vector3 center in centers)
            {
                Gizmos.DrawSphere(center, 1f);
            }
        }

        /*if(chunks != null)
        {
            Gizmos.color = Color.red;
            foreach (Chunk chunk in chunks)
            {
                if (chunk.IsClosestChunk())
                {
                    Gizmos.DrawSphere(chunk.GetCenter(), 1f);
                }
            }
        }*/
        if (vertices == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.25f);
            //Gizmos.color = Color.yellow;
            //Gizmos.DrawRay(vertices[i], normals[i]);
            //Vector3 realVertice = vertices[i] + vertices[i] * (20 / radius);
            //Gizmos.DrawRay(new Vector3(0, 0, 0), realVertice);
        }
    }

    public class VerticesData
    {
        private Vector3[] verticesParcial;
        private Vector3[] normalsParcial;

        public VerticesData(Vector3[] verticesParcial, Vector3[] normalsParcial)
        {
            this.verticesParcial = verticesParcial;
            this.normalsParcial = normalsParcial;
        }

        public Vector3[] verticesP() { return verticesParcial; }
        public Vector3[] normalsP() { return normalsParcial; }
    }

    public class Chunk
    {
        GameObject chunkObject;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        Mesh mesh;

        TerrainType[] regions;

        private int chunkSize;
        private int gridSize;
        private float radius;
        private Vector3 center;
        Texture2D faceTexture;   //Texture of the face

        private bool closestChunk;
        private bool isActive;
        private List<Chunk> adjacentChunks;
        private float distanceToClosestChunk;
        private string face;
        public int fromX;
        public int fromY;
        public int fromZ;
        //For parallelism
        public int toX;
        public int toY;
        public int toZ;
        public int fromV;
        public int fromUVCont;
        public int fromWork;
        public int toWork;
        public int fromT;
        //End for parallelism
        private int reason;

        private int numVertices;
        private Vector3[] verticesParcial;
        private Vector3[] normalsParcial;
        private Vector2[] uvs;
        private int[] trianglesParcial;

        private VerticesData data;

        public Chunk(Transform parent, Material material, int chunkSize, float radius, string face, int fromX, int fromY, int fromZ, int gridSize, TerrainType[] regions, Texture2D faceTexture)
        {
            chunkObject = new GameObject("Chunk");
            chunkObject.transform.parent = parent;
            meshRenderer = chunkObject.AddComponent<MeshRenderer>();
            meshFilter = chunkObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            this.chunkSize = chunkSize;
            this.gridSize = gridSize;
            this.radius = radius;
            this.face = face;
            this.fromX = fromX;
            this.fromY = fromY;
            this.fromZ = fromZ;
            this.regions = regions;
            this.faceTexture = faceTexture;
            isActive = true;
            reason = 0;         //Set reason to 0 to generate chunks in the first instance
            distanceToClosestChunk = 0;

            closestChunk = false;
            adjacentChunks = new List<Chunk>();

            meshFilter.mesh = mesh = new Mesh();

            //Asign SubTexture
            meshRenderer.material.mainTexture = faceTexture;
            //Create Collider
            chunkObject.AddComponent<MeshCollider>();
        }

        public void Generate()
        {
            CreateCenters();
        }

        public void SetActive(bool condition)
        {
            isActive = condition;
            chunkObject.SetActive(condition);
        }

        public void SetClosestChunk(bool isClosest)
        {
            closestChunk = isClosest;
        }

        public void SetAdjacentChunks(List<Chunk> chunks)
        {
            adjacentChunks = chunks;
        }

        public void SetDistanceToClosestChunk(float distance)
        {
            distanceToClosestChunk = distance;
        }

        public bool IsClosestChunk()
        {
            return closestChunk;
        }

        private void AssignSize()
        {
            numVertices = (chunkSize / reason + 1) * (chunkSize / reason + 1);
            verticesParcial = new Vector3[numVertices];
            normalsParcial = new Vector3[verticesParcial.Length];
            uvs = new Vector2[(chunkSize / reason + 1) * (chunkSize / reason + 1)];
            trianglesParcial = new int[(chunkSize / reason) * (chunkSize / reason) * 6];
        }

        public bool UpdateLOD(int reason)   //Once per chunk. Says true if it's going to update.
        {
            if (!isActive) { chunkObject.SetActive(true); }
            if (this.reason == reason) { return false; }     //Update only if reason is different
            this.reason = reason;
            AssignSize();

            mesh = meshFilter.mesh;
            mesh.Clear();
            return true;
        }

        public void ExecuteCalculation()    //Once per thread
        {
            CreateVertices();
            CreateTriangles();
        }

        public void AssignDataToMesh()  //Once per chunk
        {
            mesh.vertices = verticesParcial;
            mesh.normals = normalsParcial;
            mesh.uv = uvs;
            mesh.triangles = trianglesParcial;

            AssignCollider();
        }

        public Vector3 GetCenter()
        {
            return center;
        }

        public VerticesData GetVerticesData() { return data; }

        public List<Chunk> GetAdjacentChunks() { return adjacentChunks; }

        public float GetDistanceToClosestChunk() { return distanceToClosestChunk; }

        private Vector3 SquareToSpherePosition(Vector3 position)
        {
            Vector3 w = position * 2f / gridSize - Vector3.one;
            float x2 = w.x * w.x;
            float y2 = w.y * w.y;
            float z2 = w.z * w.z;
            Vector3 s;
            s.x = w.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = w.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = w.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            s *= radius;
            return s;
        }

        private void CreateCenters()
        {
            switch (face)
            {
                case "xy":
                    center = SquareToSpherePosition(new Vector3(fromX + chunkSize / 2, fromY + chunkSize / 2, 0));
                    break;
                case "xyz":
                    center = SquareToSpherePosition(new Vector3(fromX + chunkSize / 2, fromY + chunkSize / 2, gridSize));
                    break;
                case "zy":
                    center = SquareToSpherePosition(new Vector3(0, fromY + chunkSize / 2, fromZ + chunkSize / 2));
                    break;
                case "zyx":
                    center = SquareToSpherePosition(new Vector3(gridSize, fromY + chunkSize / 2, fromZ + chunkSize / 2));
                    break;
                case "xz":
                    center = SquareToSpherePosition(new Vector3(fromX + chunkSize / 2, 0, fromZ + chunkSize / 2));
                    break;
                case "xzy":
                    center = SquareToSpherePosition(new Vector3(fromX + chunkSize / 2, gridSize, fromZ + chunkSize / 2));
                    break;
            }
        }

        private void CreateVertices()
        {
            int v;
            int uvCont;
            switch (face)
            {
                case "xy":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int x = fromX; x <= toX; x+=reason)
                        {
                            float height = noiseMap[x, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, y, 0);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            //verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xyz":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int x = toX; x >= fromX; x-=reason)
                        {
                            float height = noiseMap[x, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, y, gridSize);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            //verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "zy":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int z = toZ; z >= fromZ; z-=reason)
                        {
                            float height = noiseMap[z, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, 0, y, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            //verticesParcial[v - 1] = realVertice;
                            float coorX = (float)z / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "zyx":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int z = fromZ; z <= toZ; z+=reason)
                        {
                            float height = noiseMap[z, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, gridSize, y, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            //verticesParcial[v - 1] = realVertice;
                            float coorX = (float)z / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xz":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int z = fromZ; z <= (fromZ + chunkSize); z+=reason)
                    {
                        for (int x = toX; x >= fromX; x-=reason)
                        {
                            float height = noiseMap[x, z] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, 0, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            //verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)z / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xzy":
                    v = fromV;
                    uvCont = fromUVCont;
                    for (int z = fromZ; z <= (fromZ+chunkSize); z+=reason)
                    {
                        for (int x = fromX; x <= toX; x+=reason)
                        {
                            float height = noiseMap[x, z] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, gridSize, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v-1] * (height/radius);
                            //verticesParcial[v-1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)z / (float)gridSize;
                            uvs[uvCont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
            }

            data = new VerticesData(verticesParcial, normalsParcial);
        }

        private void SetVertex(Vector3[] verticesParcial, Vector3[] normalsParcial, int i, float x, float y, float z)      //Generates the vertex 'i' in the coordinates x, y, z
        {
            Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;
            float x2 = v.x * v.x;
            float y2 = v.y * v.y;
            float z2 = v.z * v.z;
            Vector3 s;
            s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
            s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
            s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
            normalsParcial[i] = v;      //s
            verticesParcial[i] = normalsParcial[i] * radius;
        }

        private void CreateTriangles()
        {
            int t = fromT, v = fromV;

            for (int y = 0; y < chunkSize/reason; y++, v++)
            {
                for (int x = fromWork; x < toWork/*x < chunkSize/reason*/; x++, v++)
                {
                    t = SetQuad(trianglesParcial, t, v, v + 1, v + chunkSize/reason + 1, v + chunkSize/reason + 2);
                }
            }
        }

        private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

        private void AssignCollider()
        {
            chunkObject.GetComponent<MeshCollider>().sharedMesh = mesh;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}
