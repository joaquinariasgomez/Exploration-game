using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticTerrainFunctions : MonoBehaviour {

    public static void AdjustBorderNormalsXY(CubeSphere.Chunk thisChunk, int sqrtChunksPerFace, List<CubeSphere.Chunk> chunks)  //Modificar todas las normales de los bordes
    {
        int chunkSize = thisChunk.chunkSize;
        int reason = thisChunk.reason;
        int posX = thisChunk.posX;
        int posY = thisChunk.posY;
        Vector3[] borderVertices = thisChunk.borderVertices;
        int id = thisChunk.id;
        string face = thisChunk.face;

        Vector3[] normals = thisChunk.mesh.normals;

        int numVertices = thisChunk.GetNumVertices();
        Vector3[] borderNormals = new Vector3[numVertices];

        for (int i = 0; i < numVertices; i++)
        {
            borderNormals[i] = normals[i];

            if (borderVertices[i] != new Vector3(0, 0, 0))
            {
                if (posX > 0 && posY > 0 && posX < sqrtChunksPerFace - 1 && posY < sqrtChunksPerFace - 1)    //Chunks internos
                {
                    if (i < (chunkSize / reason + 1))    //Normales de abajo
                    {
                        if (chunks[id - sqrtChunksPerFace].isActive && reason >= chunks[id - sqrtChunksPerFace].reason) borderNormals[i] = chunks[id - sqrtChunksPerFace].mesh.normals[thisChunk.GetStartingI(id - sqrtChunksPerFace, i) + chunks[id - sqrtChunksPerFace].GetNumVertices() - 1 - (chunkSize / chunks[id - sqrtChunksPerFace].reason)];
                    }
                    else
                    {
                        if (i >= ((chunkSize / reason + 1) * (chunkSize / reason)))    //Normales de arriba
                        {
                            if (chunks[id + sqrtChunksPerFace].isActive && reason >= chunks[id + sqrtChunksPerFace].reason) borderNormals[i] = chunks[id + sqrtChunksPerFace].mesh.normals[thisChunk.GetStartingI(id + sqrtChunksPerFace, i - (thisChunk.GetNumVertices() - 1 - chunkSize / reason))];
                        }
                        else
                        {
                            if (i % (chunkSize / reason + 1) == 0)    //Normales de la izquierda
                            {
                                if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) borderNormals[i] = chunks[id - 1].mesh.normals[thisChunk.GetStartingI(id - 1, i) + (chunkSize / chunks[id - 1].reason)];
                            }
                            else    //Normales de la derecha
                            {
                                if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) borderNormals[i] = chunks[id + 1].mesh.normals[thisChunk.GetStartingI(id + 1, i - (chunkSize / reason))];
                            }
                        }
                    }
                    //Esquinas
                    if (i == 0)
                    {
                        if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals(i + (chunkSize / chunks[id - 1].reason), borderNormals[i]);
                        if (chunks[id - sqrtChunksPerFace - 1].isActive && reason >= chunks[id - sqrtChunksPerFace - 1].reason) chunks[id - sqrtChunksPerFace - 1].RebuildNormals(chunks[id - sqrtChunksPerFace - 1].GetNumVertices() - 1, borderNormals[i]);
                    }
                    if (i == (chunkSize / reason))
                    {
                        if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals(0, borderNormals[i]);
                        if (chunks[id - sqrtChunksPerFace + 1].isActive && reason >= chunks[id - sqrtChunksPerFace + 1].reason) chunks[id - sqrtChunksPerFace + 1].RebuildNormals(chunks[id - sqrtChunksPerFace + 1].GetNumVertices() - 1 - (chunkSize / chunks[id - sqrtChunksPerFace + 1].reason), borderNormals[i]);
                    }
                    if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))
                    {
                        if (chunks[id + sqrtChunksPerFace - 1].isActive && reason >= chunks[id + sqrtChunksPerFace - 1].reason - 1) chunks[id + sqrtChunksPerFace - 1].RebuildNormals(chunkSize / chunks[id + sqrtChunksPerFace - 1].reason, borderNormals[i]);
                        if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals((chunkSize / chunks[id - 1].reason + 1) * (chunkSize / chunks[id - 1].reason + 1) - 1, borderNormals[i]);
                    }
                    if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))
                    {
                        if (chunks[id + sqrtChunksPerFace + 1].isActive && reason >= chunks[id + sqrtChunksPerFace + 1].reason) chunks[id + sqrtChunksPerFace + 1].RebuildNormals(0, borderNormals[i]);
                        if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals((chunkSize / chunks[id + 1].reason + 1) * (chunkSize / chunks[id + 1].reason), borderNormals[i]);
                    }
                }
                else    //Chunks externos
                {
                    //Normales de arriba
                    if (CubeSphere.GetRelativeChunkId(id, face) >= (sqrtChunksPerFace * (sqrtChunksPerFace - 1)))    //Chunk de arriba
                    {
                        if (i >= ((chunkSize / reason + 1) * (chunkSize / reason)))    //Normales de arriba deben de ser de otra cara
                        {
                            if (chunks[CubeSphere.GetChunkId(id, "up", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "up", face)].reason) borderNormals[i] = chunks[CubeSphere.GetChunkId(id, "up", face)].mesh.normals[thisChunk.GetStartingI(CubeSphere.GetChunkId(id, "up", face), i - (thisChunk.GetNumVertices() - 1 - chunkSize / reason))];
                        }
                        //Esquinas de arriba
                        if (CubeSphere.GetRelativeChunkId(id, face) == (sqrtChunksPerFace * (sqrtChunksPerFace - 1)))   //Chunk de arriba a la izquierda
                        {
                            if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))  //Esquina arriba izquierda
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "left", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face)].reason) chunks[CubeSphere.GetChunkId(id, "left", face)].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason + 1) - 1, borderNormals[i]);
                            }
                            if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "up", face) + 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "up", face) + 1].reason) chunks[CubeSphere.GetChunkId(id, "up", face) + 1].RebuildNormals(0, borderNormals[i]);
                                if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals((chunkSize / chunks[id + 1].reason + 1) * (chunkSize / chunks[id + 1].reason), borderNormals[i]);
                            }
                        }
                        else
                        {
                            if (CubeSphere.GetRelativeChunkId(id, face) == (sqrtChunksPerFace * sqrtChunksPerFace - 1))    //Chunk de arriba a la derecha
                            {
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face)].reason) chunks[CubeSphere.GetChunkId(id, "right", face)].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face)].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face)].reason), borderNormals[i]);
                                }
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))         //Esquina arriba izquierda
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "up", face) - 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "up", face) - 1].reason) chunks[CubeSphere.GetChunkId(id, "up", face) - 1].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "up", face) - 1].reason), borderNormals[i]);
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals((chunkSize / chunks[id - 1].reason + 1) * (chunkSize / chunks[id - 1].reason + 1) - 1, borderNormals[i]);
                                }
                            }
                            else                                                                           //Chunk de arriba sin hacer esquina con nada
                            {
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "up", face) + 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "up", face) + 1].reason) chunks[CubeSphere.GetChunkId(id, "up", face) + 1].RebuildNormals(0, borderNormals[i]);
                                    if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals((chunkSize / chunks[id + 1].reason + 1) * (chunkSize / chunks[id + 1].reason), borderNormals[i]);
                                }
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))         //Esquina arriba izquierda
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "up", face) - 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "up", face) - 1].reason) chunks[CubeSphere.GetChunkId(id, "up", face) - 1].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "up", face) - 1].reason), borderNormals[i]);
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals((chunkSize / chunks[id - 1].reason + 1) * (chunkSize / chunks[id - 1].reason + 1) - 1, borderNormals[i]);
                                }
                            }
                        }
                    }
                    else            //No Chunk de arriba (abajo o en medio)
                    {
                        if (i >= ((chunkSize / reason + 1) * (chunkSize / reason)))    //Normales de arriba
                        {
                            if (chunks[id + sqrtChunksPerFace].isActive && reason >= chunks[id + sqrtChunksPerFace].reason) borderNormals[i] = chunks[id + sqrtChunksPerFace].mesh.normals[thisChunk.GetStartingI(id + sqrtChunksPerFace, i - (thisChunk.GetNumVertices() - 1 - chunkSize / reason))];
                        }
                        //Esquinas de arriba
                        if (CubeSphere.GetRelativeChunkId(id, face) % sqrtChunksPerFace == 0)   //Chunk de la izquierda
                        {
                            if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))  //Esquina arriba izquierda
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "left", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face)].reason) chunks[CubeSphere.GetChunkId(id, "left", face)].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason + 1) - 1, borderNormals[i]);
                                if (chunks[CubeSphere.GetChunkId(id, "left", face) + sqrtChunksPerFace].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face) + sqrtChunksPerFace].reason) chunks[CubeSphere.GetChunkId(id, "left", face) + sqrtChunksPerFace].RebuildNormals(chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face) + sqrtChunksPerFace].reason, borderNormals[i]);
                            }
                            if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                            {
                                if (chunks[id + sqrtChunksPerFace + 1].isActive && reason >= chunks[id + sqrtChunksPerFace + 1].reason) chunks[id + sqrtChunksPerFace + 1].RebuildNormals(0, borderNormals[i]);
                                if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals((chunkSize / chunks[id + 1].reason + 1) * (chunkSize / chunks[id + 1].reason), borderNormals[i]);
                            }
                        }
                        else
                        {
                            if ((CubeSphere.GetRelativeChunkId(id, face) + 1) % sqrtChunksPerFace == 0)    //Chunk de la derecha
                            {
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face)].reason) chunks[CubeSphere.GetChunkId(id, "right", face)].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face)].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face)].reason), borderNormals[i]);
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face) + sqrtChunksPerFace].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face) + sqrtChunksPerFace].reason) chunks[CubeSphere.GetChunkId(id, "right", face) + sqrtChunksPerFace].RebuildNormals(0, borderNormals[i]);
                                }
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))         //Esquina arriba izquierda
                                {
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals((chunkSize / chunks[id - 1].reason + 1) * (chunkSize / chunks[id - 1].reason + 1) - 1, borderNormals[i]);
                                    if (chunks[id + sqrtChunksPerFace - 1].isActive && reason >= chunks[id + sqrtChunksPerFace - 1].reason) chunks[id + sqrtChunksPerFace - 1].RebuildNormals(chunkSize / chunks[id + sqrtChunksPerFace - 1].reason, borderNormals[i]);
                                }
                            }
                            else                                                                //Chunk de abajo sin hacer esquina con nada
                            {
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason + 1) - 1))  //Esquina arriba derecha
                                {
                                    if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals((chunkSize / chunks[id + 1].reason + 1) * (chunkSize / chunks[id + 1].reason), borderNormals[i]);
                                    if (chunks[id + sqrtChunksPerFace + 1].isActive && reason >= chunks[id + sqrtChunksPerFace + 1].reason) chunks[id + sqrtChunksPerFace + 1].RebuildNormals(0, borderNormals[i]);
                                }
                                if (i == ((chunkSize / reason + 1) * (chunkSize / reason)))         //Esquina arriba izquierda
                                {
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals((chunkSize / chunks[id - 1].reason + 1) * (chunkSize / chunks[id - 1].reason + 1) - 1, borderNormals[i]);
                                    if (chunks[id + sqrtChunksPerFace - 1].isActive && reason >= chunks[id + sqrtChunksPerFace - 1].reason) chunks[id + sqrtChunksPerFace - 1].RebuildNormals(chunkSize / chunks[id + sqrtChunksPerFace - 1].reason, borderNormals[i]);
                                }
                            }
                        }
                    }
                    //Normales de abajo
                    if (CubeSphere.GetRelativeChunkId(id, face) < sqrtChunksPerFace)      //Chunk de abajo
                    {
                        if (i < (chunkSize / reason + 1))    //Normales de abajo deben de ser de otra cara
                        {
                            if (chunks[CubeSphere.GetChunkId(id, "down", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "down", face)].reason) borderNormals[i] = chunks[CubeSphere.GetChunkId(id, "down", face)].mesh.normals[chunkSize / chunks[CubeSphere.GetChunkId(id, "down", face)].reason - thisChunk.GetStartingI(CubeSphere.GetChunkId(id, "down", face), i)];
                        }
                        //Esquinas de abajo
                        if (CubeSphere.GetRelativeChunkId(id, face) == 0)                        //Chunk de abajo a la izquierda
                        {
                            if (i == 0)                                    //Esquina abajo izquierda
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "left", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face)].reason) chunks[CubeSphere.GetChunkId(id, "left", face)].RebuildNormals(chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason, borderNormals[i]);
                            }
                            if (i == chunkSize / reason)                     //Esquina abajo derecha
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "down", face) + 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "down", face) + 1].reason) chunks[CubeSphere.GetChunkId(id, "down", face) + 1].RebuildNormals(chunkSize / chunks[CubeSphere.GetChunkId(id, "down", face)].reason, borderNormals[i]);
                                if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals(0, borderNormals[i]);
                            }
                        }
                        else
                        {
                            if (CubeSphere.GetRelativeChunkId(id, face) == (sqrtChunksPerFace - 1))       //Chunk de abajo a la derecha
                            {
                                if (i == 0)                                    //Esquina abajo izquierda
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "down", face) - 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "down", face) - 1].reason) chunks[CubeSphere.GetChunkId(id, "down", face) - 1].RebuildNormals(0, borderNormals[i]);
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals(chunkSize / chunks[id - 1].reason, borderNormals[i]);
                                }
                                if (i == chunkSize / reason)                     //Esquina abajo derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face)].reason) chunks[CubeSphere.GetChunkId(id, "right", face)].RebuildNormals(0, borderNormals[i]);
                                }
                            }
                            else                                                    //Chunk de abajo sin hacer esquina con nada
                            {
                                if (i == 0)                                    //Esquina abajo izquierda
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "down", face) - 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "down", face) - 1].reason) chunks[CubeSphere.GetChunkId(id, "down", face) - 1].RebuildNormals(0, borderNormals[i]);
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals(chunkSize / chunks[id - 1].reason, borderNormals[i]);
                                }
                                if (i == chunkSize / reason)                     //Esquina abajo derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "down", face) + 1].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "down", face) + 1].reason) chunks[CubeSphere.GetChunkId(id, "down", face) + 1].RebuildNormals(chunkSize / chunks[CubeSphere.GetChunkId(id, "down", face)].reason, borderNormals[i]);
                                    if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals(0, borderNormals[i]);
                                }
                            }
                        }
                    }
                    else                                                //No Chunk de abajo (arriba o en medio)
                    {
                        if (i < (chunkSize / reason + 1))    //Normales de abajo
                        {
                            if (chunks[id - sqrtChunksPerFace].isActive && reason >= chunks[id - sqrtChunksPerFace].reason) borderNormals[i] = chunks[id - sqrtChunksPerFace].mesh.normals[thisChunk.GetStartingI(id - sqrtChunksPerFace, i) + chunks[id - sqrtChunksPerFace].GetNumVertices() - 1 - (chunkSize / chunks[id - sqrtChunksPerFace].reason)];
                        }
                        //Esquinas de abajo
                        if (CubeSphere.GetRelativeChunkId(id, face) % sqrtChunksPerFace == 0)   //Chunk de la izquierda
                        {
                            if (i == 0)  //Esquina abajo izquierda
                            {
                                if (chunks[CubeSphere.GetChunkId(id, "left", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face)].reason) chunks[CubeSphere.GetChunkId(id, "left", face)].RebuildNormals(chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason, borderNormals[i]);
                                if (chunks[CubeSphere.GetChunkId(id, "left", face) - sqrtChunksPerFace].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face) - sqrtChunksPerFace].reason) chunks[CubeSphere.GetChunkId(id, "left", face) - sqrtChunksPerFace].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face) - sqrtChunksPerFace].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face) - sqrtChunksPerFace].reason + 1) - 1, borderNormals[i]);
                            }
                            if (i == (chunkSize / reason))  //Esquina abajo derecha
                            {
                                if (chunks[id - sqrtChunksPerFace + 1].isActive && reason >= chunks[id - sqrtChunksPerFace + 1].reason) chunks[id - sqrtChunksPerFace + 1].RebuildNormals((chunkSize / chunks[id - sqrtChunksPerFace + 1].reason + 1) * (chunkSize / chunks[id - sqrtChunksPerFace + 1].reason), borderNormals[i]);
                                if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals(0, borderNormals[i]);
                            }
                        }
                        else
                        {
                            if ((CubeSphere.GetRelativeChunkId(id, face) + 1) % sqrtChunksPerFace == 0)    //Chunk de la derecha
                            {
                                if (i == (chunkSize / reason))  //Esquina abajo derecha
                                {
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face)].reason) chunks[CubeSphere.GetChunkId(id, "right", face)].RebuildNormals(0, borderNormals[i]);
                                    if (chunks[CubeSphere.GetChunkId(id, "right", face) - sqrtChunksPerFace].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face) - sqrtChunksPerFace].reason) chunks[CubeSphere.GetChunkId(id, "right", face) - sqrtChunksPerFace].RebuildNormals((chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face) - sqrtChunksPerFace].reason + 1) * (chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face) - sqrtChunksPerFace].reason), borderNormals[i]);
                                }
                                if (i == 0)         //Esquina abajo izquierda
                                {
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals(chunkSize / chunks[id - 1].reason, borderNormals[i]);
                                    if (chunks[id - sqrtChunksPerFace - 1].isActive && reason >= chunks[id - sqrtChunksPerFace - 1].reason) chunks[id - sqrtChunksPerFace - 1].RebuildNormals((chunkSize / chunks[id - sqrtChunksPerFace - 1].reason + 1) * (chunkSize / chunks[id - sqrtChunksPerFace - 1].reason + 1) - 1, borderNormals[i]);
                                }
                            }
                            else                                                                //Chunk de arriba sin hacer esquina con nada
                            {
                                if (i == (chunkSize / reason))  //Esquina abajo derecha
                                {
                                    if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) chunks[id + 1].RebuildNormals(0, borderNormals[i]);
                                    if (chunks[id - sqrtChunksPerFace + 1].isActive && reason >= chunks[id - sqrtChunksPerFace + 1].reason) chunks[id - sqrtChunksPerFace + 1].RebuildNormals((chunkSize / chunks[id - sqrtChunksPerFace + 1].reason + 1) * (chunkSize / chunks[id - sqrtChunksPerFace + 1].reason), borderNormals[i]);
                                }
                                if (i == 0)         //Esquina abajo izquierda
                                {
                                    if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) chunks[id - 1].RebuildNormals(chunkSize / chunks[id - 1].reason, borderNormals[i]);
                                    if (chunks[id - sqrtChunksPerFace - 1].isActive && reason >= chunks[id - sqrtChunksPerFace - 1].reason) chunks[id - sqrtChunksPerFace - 1].RebuildNormals((chunkSize / chunks[id - sqrtChunksPerFace - 1].reason + 1) * (chunkSize / chunks[id - sqrtChunksPerFace - 1].reason + 1) - 1, borderNormals[i]);
                                }
                            }
                        }
                    }
                    //Normales de la izquierda
                    if (CubeSphere.GetRelativeChunkId(id, face) % sqrtChunksPerFace == 0)
                    {
                        if (i % (chunkSize / reason + 1) == 0)    //Normales de la izquierda deben de ser de otra cara
                        {
                            if (chunks[CubeSphere.GetChunkId(id, "left", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "left", face)].reason) borderNormals[i] = chunks[CubeSphere.GetChunkId(id, "left", face)].mesh.normals[thisChunk.GetStartingI(CubeSphere.GetChunkId(id, "left", face), i) + (chunkSize / chunks[CubeSphere.GetChunkId(id, "left", face)].reason)];
                        }
                    }
                    else
                    {
                        if (i % (chunkSize / reason + 1) == 0)    //Normales de la izquierda
                        {
                            if (chunks[id - 1].isActive && reason >= chunks[id - 1].reason) borderNormals[i] = chunks[id - 1].mesh.normals[thisChunk.GetStartingI(id - 1, i) + (chunkSize / chunks[id - 1].reason)];
                        }
                    }
                    //Normales de la derecha
                    if ((CubeSphere.GetRelativeChunkId(id, face) + 1) % sqrtChunksPerFace == 0)
                    {
                        if ((i + 1) % (chunkSize / reason + 1) == 0)    //Normales de la derecha deben de ser de otra cara
                        {
                            if (chunks[CubeSphere.GetChunkId(id, "right", face)].isActive && reason >= chunks[CubeSphere.GetChunkId(id, "right", face)].reason) borderNormals[i] = chunks[CubeSphere.GetChunkId(id, "right", face)].mesh.normals[thisChunk.GetStartingI(CubeSphere.GetChunkId(id, "right", face), i) - (chunkSize / chunks[CubeSphere.GetChunkId(id, "right", face)].reason)];
                        }
                    }
                    else
                    {
                        if ((i + 1) % (chunkSize / reason + 1) == 0)    //Normales de la derecha
                        {
                            if (chunks[id + 1].isActive && reason >= chunks[id + 1].reason) borderNormals[i] = chunks[id + 1].mesh.normals[thisChunk.GetStartingI(id + 1, i - (chunkSize / reason))];
                        }
                    }
                }
            }
        }
        thisChunk.mesh.normals = borderNormals;
    }
}
