using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

public class CubeSphere : MonoBehaviour
{
    public int gridSize;
    public float gravity = -9.8f;
    public TerrainType[] regions;
    public Material material;
    public Transform bodyAttracted;

    private Noise.NormalizeMode normalizeMode=Noise.NormalizeMode.Global;
    private float radius;
    private int sqrtChunksPerFace = 5;     //25
    private static float heightMultiplier = 20;

    private static float[,] noiseMap;
    private static Dictionary<string, float[,]> noiseMaps=new Dictionary<string, float[,]>();  //Face, noiseMap
    private static Color[] colourMap;
    private static int seed = 2048; //2048
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

    //private Thread[] myThreads=new Thread[4];   //Testing
    // Timer tests
    public static Stopwatch timer = new Stopwatch();

    public void Awake()
    {
        viewerPositionOld = viewerPosition = bodyAttracted.position;

        radius = gridSize / 2;
        chunkSize = gridSize / sqrtChunksPerFace;

        GenerateChunks();     //DECOMMENT
    }

    private void Start()
    {
        ClosestChunkHasChanged();   //Set variables for UpdateChunks()    //DECOMMENT
        StartCoroutine(UpdateChunks());                                   //DECOMMENT
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
                StartCoroutine(UpdateChunks());       //DECOMMENT
            }
        }
    }

    private void GenerateChunks()
    {
        chunks = new List<Chunk>();

        List<VerticesData> verticesData = new List<VerticesData>();
        centers = new List<Vector3>();

        GenerateNoiseMapOfFace("xy");
        GenerateNoiseMapOfFace("xyz");
        GenerateNoiseMapOfFace("zy");
        GenerateNoiseMapOfFace("zyx");
        GenerateNoiseMapOfFace("xz");
        GenerateNoiseMapOfFace("xzy");
        //Ahora mezclar los noiseMap con una funcion que coja los maps del diccionario
        MixNoiseMaps();     //Cose las brechas ocasionadas por la generación de Chunks entre diferentes caras
        GenerateChunksOfFace(chunks, "xy", verticesData);
        GenerateChunksOfFace(chunks, "xyz", verticesData);
        GenerateChunksOfFace(chunks, "zy", verticesData);
        GenerateChunksOfFace(chunks, "zyx", verticesData);
        GenerateChunksOfFace(chunks, "xz", verticesData);
        GenerateChunksOfFace(chunks, "xzy", verticesData);

        VerticesData finalData = CollectData(verticesData);
        vertices = finalData.verticesP();
        normals = finalData.normalsP();
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

    private float[,] FLipMatrix(float[,] noiseMap, int width, int height, string face)
    {
        float[,] matrix = new float[width, height];
        for(int i=0; i<width; i++)
        {
            for(int j=0; j<height; j++)
            {
                switch(face)
                {
                    case "zy": matrix[i, j] = noiseMap[width - 1 - i, j]; break;
                    case "xz": matrix[i, j] = noiseMap[i, height - 1 - j]; break;
                    case "xyz": matrix[i, j] = noiseMap[width - 1 - i, j]; break;
                }
            }
        }
        return matrix;
    }

    private Texture2D CreateTexture(string face)
    {
        int width = gridSize + 1;
        int height = gridSize + 1;

        colourMap = new Color[width * height];

        noiseMap = noiseMaps[face];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                float currentHeight = noiseMap[j, i];
                for (int k = 0; k < regions.Length; k++)
                {
                    if (currentHeight >= regions[k].height)
                    {
                        colourMap[i * width + j] = regions[k].colour;
                    }
                    else
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

    private void GenerateNoiseMapOfFace(string face)
    {
        switch (face)
        {
            case "xy": offset = new Vector2(0, 0); break;
            case "xyz": offset = new Vector2(-2 * gridSize, 0); break;
            case "zy": offset = new Vector2(-gridSize, 0); break;
            case "zyx": offset = new Vector2(gridSize, 0); break;
            case "xz": offset = new Vector2(0, gridSize); break;
            case "xzy": offset = new Vector2(0, -gridSize); break;
        }

        //Generate Noise Map
        int width = gridSize + 1;
        int height = gridSize + 1;

        noiseMap = Noise.GenerateNoiseMap(width, height, seed, scale, octaves, persistance, lacunarity, offset, normalizeMode);

        if (face == "zy" || face == "xz" || face == "xyz") { noiseMap = FLipMatrix(noiseMap, width, height, face); }

        noiseMaps.Add(face, noiseMap);
    }

    private void MixNoiseMaps()
    {
        int width = gridSize + 1;
        int height = gridSize + 1;
        int tope = 15;

        float[,] noiseMapXZY = noiseMaps["xzy"];
        float[,] noiseMapZY = noiseMaps["zy"];
        float[,] noiseMapXY = noiseMaps["xy"];
        float[,] noiseMapXZ = noiseMaps["xz"];
        float[,] noiseMapZYX = noiseMaps["zyx"];
        float[,] noiseMapXYZ = noiseMaps["xyz"];

        //Lateral ZY - XZY
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXZY[0, j] + noiseMapZY[j, height - 1]) / 2;
            noiseMapXZY[0, j] = medium;
            noiseMapZY[j, height - 1] = medium;
            for (int prof=1; prof<tope; prof++)
            {   
                float mediumXZY = (noiseMapXZY[prof, j] + noiseMapXZY[prof - 1, j]) / 2;
                float mediumZY = (noiseMapZY[j, height - 1 - prof] + noiseMapZY[j, height - prof]) / 2;
                noiseMapXZY[prof, j] = mediumXZY;
                noiseMapZY[j, height - 1 - prof] = mediumZY;
            }
        }
        //Corner ZY - XZY - XY
        for(int i=1; i<tope; i++)
        {
            float medium;
            noiseMapXY[i, height - 1] = noiseMapXZY[i, 0];
            noiseMapXY[0, height - 1 - i] = noiseMapZY[0, height - 1 - i];
            for(int prof=1; prof<tope; prof++)
            {
                if(i+prof<tope)
                {
                    medium = (noiseMapXY[i + prof, height - 1 - prof] + noiseMapXY[i + prof, height - prof]) / 2;
                    noiseMapXY[i + prof, height - 1 - prof] = medium;
                    medium = (noiseMapXY[prof, height - 1 - i - prof] + noiseMapXY[prof - 1, height - 1 - i - prof]) / 2;
                    noiseMapXY[prof, height - 1 - i - prof] = medium;
                }
            }
            //Diagonal
            noiseMapXY[i, height - 1 - i] = (noiseMapXY[i - 1, height - 1 - i] + noiseMapXY[i, height - i]) / 2;
        }
        //Lateral ZY - XZ
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXZ[0, j] + noiseMapZY[j, 0]) / 2;
            noiseMapXZ[0, j] = medium;
            noiseMapZY[j, 0] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumXZ = (noiseMapXZ[prof, j] + noiseMapXZ[prof - 1, j]) / 2;
                float mediumZY = (noiseMapZY[j, prof] + noiseMapZY[j, prof - 1]) / 2;
                noiseMapXZ[prof, j] = mediumXZ;
                noiseMapZY[j, prof] = mediumZY;
            }
        }
        //Corner ZY - XZ - XY
        for(int i=1; i<tope; i++)
        {
            float medium;
            noiseMapXY[0, i] = noiseMapZY[0, i];
            noiseMapXY[i, 0] = noiseMapXZ[i, 0];
            for(int prof=1; prof<tope; prof++)
            {
                if (i+prof<tope)
                {
                    medium = (noiseMapXY[prof, i + prof] + noiseMapXY[prof - 1, i + prof]) / 2;
                    noiseMapXY[prof, i + prof] = medium;
                    medium = (noiseMapXY[i + prof, prof] + noiseMapXY[i + prof, prof - 1]) / 2;
                    noiseMapXY[i + prof, prof] = medium;
                }
            }
            //Diagonal
            noiseMapXY[i, i] = (noiseMapXY[i, i - 1] + noiseMapXY[i - 1, i]) / 2;
        }
        //Lateral ZYX - XZY
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapZYX[j, height - 1] + noiseMapXZY[height - 1, j]) / 2;
            noiseMapZYX[j, height - 1] = medium;
            noiseMapXZY[height - 1, j] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumZYX = (noiseMapZYX[j, height - 1 - prof] + noiseMapZYX[j, height - prof]) / 2;
                float mediumXZY = (noiseMapXZY[height - 1 - prof, j] + noiseMapXZY[height - prof, j]) / 2;
                noiseMapZYX[j, height - 1 - prof] = mediumZYX;
                noiseMapXZY[height - 1 - prof, j] = mediumXZY;
            }
        }
        //Corner ZYX - XZY - XY
        for(int i=1; i<tope; i++)
        {
            float medium;
            noiseMapXY[height - 1 - i, height - 1] = noiseMapXZY[height - 1 - i, 0];
            noiseMapXY[height - 1, height - 1 - i] = noiseMapZYX[0, height - 1 - i];
            for(int prof=1; prof<tope; prof++)
            {
                if(i+prof<tope)
                {
                    medium = (noiseMapXY[height - 1 - prof, height - i - 1 - prof] + noiseMapXY[height - prof, height - i - 1 - prof]) / 2;
                    noiseMapXY[height - 1 - prof, height - i - 1 - prof] = medium;
                    medium = (noiseMapXY[height - i - 1 - prof, height - 1 - prof] + noiseMapXY[height - i - 1 - prof, height - prof]) / 2;
                    noiseMapXY[height - i - 1 - prof, height - 1 - prof] = medium;
                }
            }
            //Diagonal
            noiseMapXY[height - 1 - i, height - 1 - i] = (noiseMapXY[height - i, height - 1 - i] + noiseMapXY[height - 1 - i, height - i]) / 2;
        }
        //Lateral XZ - ZYX
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXZ[height - 1, j] + noiseMapZYX[j, 0]) / 2;
            noiseMapXZ[height - 1, j] = medium;
            noiseMapZYX[j, 0] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumXZ = (noiseMapXZ[height - 1 - prof, j] + noiseMapXZ[height - prof, j]) / 2;
                float mediumZYX = (noiseMapZYX[j, prof] + noiseMapZYX[j, prof - 1]) / 2;
                noiseMapXZ[height - 1 - prof, j] = mediumXZ;
                noiseMapZYX[j, prof] = mediumZYX;
            }
        }
        //Corner XZ - ZYX - XY
        for(int i=1; i<tope; i++)
        {
            float medium;
            noiseMapXY[height - 1, i] = noiseMapZYX[0, i];
            noiseMapXY[height - 1 - i, 0] = noiseMapXZ[height - 1 - i, 0];
            for(int prof=1; prof<tope; prof++)
            {
                if(i+prof<tope)
                {
                    medium = (noiseMapXY[height - 1 - i - prof, prof] + noiseMapXY[height - 1 - i - prof, prof - 1]) / 2;
                    noiseMapXY[height - 1 - i - prof, prof] = medium;
                    medium = (noiseMapXY[height - 1 - prof, i + prof] + noiseMapXY[height - prof, i + prof]) / 2;
                    noiseMapXY[height - 1 - prof, i + prof] = medium;
                }
            }
            //Diagonal
            noiseMapXY[height - 1 - i, i] = (noiseMapXY[height - i, i] + noiseMapXY[height - 1 - i, i - 1]) / 2;
        }
        //Lateral XYZ - XZY
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXYZ[j, height - 1] + noiseMapXZY[j, height - 1]) / 2;
            noiseMapXYZ[j, height - 1] = medium;
            noiseMapXZY[j, height - 1] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumXYZ = (noiseMapXYZ[j, height - 1 - prof] + noiseMapXYZ[j, height - prof]) / 2;
                float mediumXZY = (noiseMapXZY[j, height - 1 - prof] + noiseMapXZY[j, height - prof]) / 2;
                noiseMapXYZ[j, height - 1 - prof] = mediumXYZ;
                noiseMapXZY[j, height - 1 - prof] = mediumXZY;
            }
        }
        //Lateral XYZ - XZ
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXYZ[j, 0] + noiseMapXZ[j, height - 1]) / 2;
            noiseMapXYZ[j, 0] = medium;
            noiseMapXZ[j, height - 1] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumXYZ = (noiseMapXYZ[j, prof] + noiseMapXYZ[j, prof - 1]) / 2;
                float mediumXZ = (noiseMapXZ[j, height - 1 - prof] + noiseMapXZ[j, height - prof]) / 2;
                noiseMapXYZ[j, prof] = mediumXYZ;
                noiseMapXZ[j, height - 1 - prof] = mediumXZ;
            }
        }
        //Lateral XYZ - ZYX
        for(int j=0; j<width; j++)
        {
            float medium = (noiseMapXYZ[height - 1, height - 1 - j] + noiseMapZYX[height - 1, height - 1 - j]) / 2;
            noiseMapXYZ[height - 1, height - 1 - j] = medium;
            noiseMapZYX[height - 1, height - 1 - j] = medium;
            for(int prof=1; prof<tope; prof++)
            {
                float mediumXYZ = (noiseMapXYZ[height - 1 - prof, height - 1 - j] + noiseMapXYZ[height - prof, height - 1 - j]) / 2;
                float mediumZYX = (noiseMapZYX[height - 1 - prof, height - 1 - j] + noiseMapZYX[height - prof, height - 1 - j]) / 2;
                noiseMapXYZ[height - 1 - prof, height - 1 - j] = mediumXYZ;
                noiseMapZYX[height - 1 - prof, height - 1 - j] = mediumZYX;
            }
        }

        noiseMaps["xzy"] = noiseMapXZY;
        noiseMaps["zy"] = noiseMapZY;
        noiseMaps["xy"] = noiseMapXY;
        noiseMaps["xz"] = noiseMapXZ;
        noiseMaps["zyx"] = noiseMapZYX;
        noiseMaps["xyz"] = noiseMapXYZ;
    }

    private void GenerateChunksOfFace(List<Chunk> chunks, string face, List<VerticesData> verticesData)
    {
        //Texture2D faceTexture=CreateTexture();
        switch (face)
        {
            case "xy": Texture2D faceTextureXY = CreateTexture("xy");
                       for (int y = 0; y < sqrtChunksPerFace; y++)
                       {
                           for (int x = 0; x < sqrtChunksPerFace; x++)
                           {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, y * chunkSize, 0, gridSize, regions, faceTextureXY);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                           }
                       } break;
            case "xyz": Texture2D faceTextureXYZ = CreateTexture("xyz");
                        for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, y * chunkSize, 0, gridSize, regions, faceTextureXYZ);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "zy":  Texture2D faceTextureZY = CreateTexture("zy");
                        for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int z = 0; z < sqrtChunksPerFace; z++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, 0, y * chunkSize, z * chunkSize, gridSize, regions, faceTextureZY);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "zyx": Texture2D faceTextureZYX = CreateTexture("zyx");
                        for (int y = 0; y < sqrtChunksPerFace; y++)
                        {
                            for (int z = 0; z < sqrtChunksPerFace; z++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, 0, y * chunkSize, z * chunkSize, gridSize, regions, faceTextureZYX);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "xz":  Texture2D faceTextureXZ = CreateTexture("xz");
                        for (int z = 0; z < sqrtChunksPerFace; z++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, 0, z * chunkSize, gridSize, regions, faceTextureXZ);
                                chunk.Generate();
                                chunks.Add(chunk);
                                //verticesData.Add(chunk.GetVerticesData());
                                //centers.Add(chunk.GetCenter());
                            }
                        }
                        break;
            case "xzy": Texture2D faceTextureXZY = CreateTexture("xzy");
                        for (int z = 0; z < sqrtChunksPerFace; z++)
                        {
                            for (int x = 0; x < sqrtChunksPerFace; x++)
                            {
                                Chunk chunk = new Chunk(transform, material, chunkSize, radius, face, x * chunkSize, 0, z * chunkSize, gridSize, regions, faceTextureXZY);
                                chunk.Generate();
                                chunks.Add(chunk);
                                verticesData.Add(chunk.GetVerticesData());
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

    private IEnumerator UpdateChunks()
    {
        Chunk closestChunk = chunks[0];

        foreach(Chunk chunk in chunks)
        {
            if(chunk.IsClosestChunk())
            {
                closestChunk = chunk;
                closestChunk.UpdateLOD(1);
            }
        }

        int adjacentCount = 1;
        foreach (Chunk adjacent in closestChunk.GetAdjacentChunks())
        {
            if (adjacentCount < 149)    //150
            {
                adjacent.UpdateLOD(1);
                ++adjacentCount;
            }
            else
            {
                if (adjacentCount < 250)    //300
                {
                    adjacent.UpdateLOD(2);
                    ++adjacentCount;
                }
                else
                {
                    if (adjacentCount < 500)    //600
                    {
                        adjacent.UpdateLOD(4);
                        ++adjacentCount;
                    }
                    else
                    {
                        if(adjacentCount < 650)     //900
                        {
                            adjacent.UpdateLOD(8);
                            ++adjacentCount;
                        }
                        else
                        {
                            if(adjacentCount < 900)    //1200
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
            }
        }
        yield return null;
    }

    public void Attract(Transform body)
    {
        bodyAttracted = body;

        Vector3 gravityUp = (body.position - transform.position).normalized;
        Vector3 bodyUp = body.up;

        rigidbodyAttracted = body.GetComponent<Rigidbody>();

        //rigidbodyAttracted.AddForce(gravityUp * gravity);   //Comment this line for no attraction force

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
        /*if (vertices == null)
        {
            return;
        }
        Gizmos.color = Color.black;
        for (int i = 0; i < vertices.Length; i++)
        {
            //Gizmos.color = Color.black;
            //Gizmos.DrawSphere(vertices[i], 10f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
            //Vector3 realVertice = vertices[i] + vertices[i] * (20 / radius);
            //Gizmos.DrawRay(new Vector3(0, 0, 0), realVertice);
        }*/
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
        private int[] triangles;
        private Vector3[] vertices;

        private Vector3[] borderVertices;
        private int[] borderTriangles;

        private bool closestChunk;
        private bool isActive;
        private List<Chunk> adjacentChunks;
        private float distanceToClosestChunk;
        private string face;
        private int fromX;
        private int fromY;
        private int fromZ;
        private int reason;

        private VerticesData data;

        int contBorderTriangles;

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
            reason = 1;
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
            //Asign Colliders
            CreateVertices();
            CreateTriangles();
            //CalculateNormals();
            AssignCollider();
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

        public void UpdateLOD(int reason)
        {
            if (!isActive) { chunkObject.SetActive(true); }
            if (this.reason == reason) { return; }     //Update only if reason is different
            this.reason = reason;

            mesh = meshFilter.mesh;
            mesh.Clear();

            CreateVertices();
            CreateTriangles();
            //CalculateNormals();
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
            int numBorderVertices = (chunkSize / reason + 1) * (chunkSize / reason + 1);
            
            borderVertices = new Vector3[numBorderVertices];
            int numVertices = (chunkSize/reason + 1) * (chunkSize/reason + 1);
            Vector3[] verticesParcial = new Vector3[numVertices];
            Vector3[] normalsParcial = new Vector3[verticesParcial.Length];
            Vector2[] uvs = new Vector2[(chunkSize/reason+1)*(chunkSize/reason+1)];

            int v;
            int cont = 0;
            switch (face)
            {
                case "xy":
                    noiseMap = noiseMaps["xy"];
                    v = 0;
                    cont = 0;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int x = fromX; x <= (fromX + chunkSize); x+=reason)
                        {
                            float height = noiseMap[x, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, y, 0);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xyz":
                    noiseMap = noiseMaps["xyz"];
                    v = 0;
                    cont = 0;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int x = (fromX + chunkSize); x >= fromX; x-=reason)
                        {
                            float height = noiseMap[x, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, y, gridSize);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "zy":
                    noiseMap = noiseMaps["zy"];
                    v = 0;
                    cont = 0;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int z = (fromZ + chunkSize); z >= fromZ; z-=reason)
                        {
                            float height = noiseMap[z, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, 0, y, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            verticesParcial[v - 1] = realVertice;
                            float coorX = (float)z / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "zyx":
                    noiseMap = noiseMaps["zyx"];
                    v = 0;
                    cont = 0;
                    for (int y = fromY; y <= (fromY + chunkSize); y+=reason)
                    {
                        for (int z = fromZ; z <= (fromZ + chunkSize); z+=reason)
                        {
                            float height = noiseMap[z, y] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, gridSize, y, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            verticesParcial[v - 1] = realVertice;
                            float coorX = (float)z / (float)gridSize;
                            float coorY = (float)y / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xz":
                    noiseMap = noiseMaps["xz"];
                    v = 0;
                    cont = 0;
                    for (int z = fromZ; z <= (fromZ + chunkSize); z+=reason)
                    {
                        for (int x = (fromX + chunkSize); x >= fromX; x-=reason)
                        {
                            float height = noiseMap[x, z] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, 0, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v - 1] * (height / radius);
                            verticesParcial[v - 1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)z / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
                case "xzy":
                    noiseMap = noiseMaps["xzy"];
                    v = 0;
                    cont = 0;
                    for (int z = fromZ; z <= (fromZ+chunkSize); z+=reason)
                    {
                        for (int x = fromX; x <= (fromX+chunkSize); x+=reason)
                        {
                            float height = noiseMap[x, z] * heightMultiplier;
                            SetVertex(verticesParcial, normalsParcial, v++, x, gridSize, z);
                            Vector3 realVertice = verticesParcial[v - 1] + verticesParcial[v-1]*(height/radius);
                            verticesParcial[v-1] = realVertice;
                            float coorX = (float)x / (float)gridSize;
                            float coorY = (float)z / (float)gridSize;
                            uvs[cont++] = new Vector2(coorX, coorY);
                        }
                    }
                    break;
            }

            vertices = verticesParcial;
            mesh.vertices = verticesParcial;
            mesh.normals = normalsParcial;
            mesh.uv = uvs;

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
            normalsParcial[i] = s;    //s
            verticesParcial[i] = normalsParcial[i] * radius;

            //Assign border vertice if it is
            if (i <= (chunkSize / reason) || i % (chunkSize / reason + 1) == 0 || (i+1) % (chunkSize / reason + 1) == 0 || i >= (chunkSize / reason + 1) * (chunkSize / reason))
            {
                borderVertices[i] = verticesParcial[i];
            }
        }

        private void CreateTriangles()
        {
            contBorderTriangles = 0;
            int numBorderTriangles = (chunkSize / reason -1) * 24;
            borderTriangles = new int[numBorderTriangles];
            int[] trianglesParcial = new int[(chunkSize/reason) * (chunkSize/reason) * 6];
            int t = 0, v = 0;

            for (int y = 0; y < chunkSize/reason; y++, v++)
            {
                for (int x = 0; x < chunkSize/reason; x++, v++)
                {
                    t = SetQuad(trianglesParcial, t, v, v + 1, v + chunkSize/reason + 1, v + chunkSize/reason + 2);
                }
            }

            triangles = trianglesParcial;
            mesh.triangles = trianglesParcial;
        }

        private int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
        {
            //Check if one of the vertices that form the 2 triangles are border vertices
            if(v00 <= (chunkSize / reason) || v00 % (chunkSize / reason + 1) == 0 || (v00+1) % (chunkSize / reason + 1) == 0 || v00 >= (chunkSize / reason + 1) * (chunkSize / reason))
            {
                borderTriangles[contBorderTriangles] = v00;
                borderTriangles[contBorderTriangles + 1] = borderTriangles[contBorderTriangles + 4] = v01;
                borderTriangles[contBorderTriangles + 2] = borderTriangles[contBorderTriangles + 3] = v10;
                borderTriangles[contBorderTriangles + 5] = v11;
                contBorderTriangles += 6;
            }
            else
            {
                if(v01 <= (chunkSize / reason) || v01 % (chunkSize / reason + 1) == 0 || (v01+1) % (chunkSize / reason + 1) == 0 || v01 >= (chunkSize / reason + 1) * (chunkSize / reason))
                {
                    borderTriangles[contBorderTriangles] = v00;
                    borderTriangles[contBorderTriangles + 1] = borderTriangles[contBorderTriangles + 4] = v01;
                    borderTriangles[contBorderTriangles + 2] = borderTriangles[contBorderTriangles + 3] = v10;
                    borderTriangles[contBorderTriangles + 5] = v11;
                    contBorderTriangles += 6;
                }
                else
                {
                    if(v10 <= (chunkSize / reason) || v10 % (chunkSize / reason + 1) == 0 || (v10+1) % (chunkSize / reason + 1) == 0 || v10 >= (chunkSize / reason + 1) * (chunkSize / reason))
                    {
                        borderTriangles[contBorderTriangles] = v00;
                        borderTriangles[contBorderTriangles + 1] = borderTriangles[contBorderTriangles + 4] = v01;
                        borderTriangles[contBorderTriangles + 2] = borderTriangles[contBorderTriangles + 3] = v10;
                        borderTriangles[contBorderTriangles + 5] = v11;
                        contBorderTriangles += 6;
                    }
                    else
                    {
                        if(v11 <= (chunkSize / reason) || v11 % (chunkSize / reason + 1) == 0 || (v11+1) % (chunkSize / reason + 1) == 0 || v11 >= (chunkSize / reason + 1) * (chunkSize / reason))
                        {
                            borderTriangles[contBorderTriangles] = v00;
                            borderTriangles[contBorderTriangles + 1] = borderTriangles[contBorderTriangles + 4] = v01;
                            borderTriangles[contBorderTriangles + 2] = borderTriangles[contBorderTriangles + 3] = v10;
                            borderTriangles[contBorderTriangles + 5] = v11;
                            contBorderTriangles += 6;
                        }
                    }
                }
            }
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

        private void CalculateNormals()
        {
            int numNormals = (chunkSize / reason + 1) * (chunkSize / reason + 1);
            Vector3[] vertexNormals = new Vector3[numNormals];
            int triangleCount = triangles.Length/3;
            for(int i=0; i<triangleCount; i++) {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }
            //Border triangles
            int borderTriangleCount = borderTriangles.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = borderTriangles[normalTriangleIndex];
                int vertexIndexB = borderTriangles[normalTriangleIndex + 1];
                int vertexIndexC = borderTriangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            for (int i=0; i<vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }

            mesh.normals = vertexNormals;
            //data = new VerticesData(vertices, vertexNormals);
        }

        Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            /*if(indexA <= (chunkSize / reason) || indexA % (chunkSize / reason + 1) == 0 || (indexA + 1) % (chunkSize / reason + 1) == 0 || indexA >= (chunkSize / reason + 1) * (chunkSize / reason))
            {
                pointA = borderVertices[indexA];
            }

            if (indexB <= (chunkSize / reason) || indexB % (chunkSize / reason + 1) == 0 || (indexB + 1) % (chunkSize / reason + 1) == 0 || indexB >= (chunkSize / reason + 1) * (chunkSize / reason))
            {
                pointB = borderVertices[indexB];
            }

            if (indexC <= (chunkSize / reason) || indexC % (chunkSize / reason + 1) == 0 || (indexC + 1) % (chunkSize / reason + 1) == 0 || indexC >= (chunkSize / reason + 1) * (chunkSize / reason))
            {
                pointC = borderVertices[indexC];
            }*/

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC);
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
