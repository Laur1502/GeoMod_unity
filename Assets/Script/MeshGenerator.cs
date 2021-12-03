using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]

public class MeshGenerator : MonoBehaviour
{
    delegate Vector3 ComputeVector3FromKxKz(float kX, float kZ);

    MeshFilter m_Mf;
    // Start is called before the first frame update

    private void Awake()
    {
        m_Mf = GetComponent<MeshFilter>();
        //m_Mf.sharedMesh = CreateTriangle();
        //m_Mf.sharedMesh = CreateQuadXZ(new Vector3(4, 0, 2));
        //m_Mf.sharedMesh = CreateStripXZ(new Vector3(4, 0, 2), 10);
        //m_Mf.sharedMesh = CreatePlaneXZ(new Vector3(4, 0, 2), 20, 10);
        m_Mf.sharedMesh = WrapNormalizePlane(20, 10, (kX, kZ)=> new Vector3((kX-.5f)*4,0,(kZ-.5f)*2));
        /*m_Mf.sharedMesh = WrapNormalizePlane(20, 10, (kX, kZ)=>
        {
            float theta = kX * 2 * Mathf.PI;
            float z = 4 * kZ;
            float rho = 2;
            return new Vector3(rho * Mathf.Cos(theta), z, rho * Mathf.Sin(theta));
        });*/
    }
    
    Mesh CreateTriangle()
    {
        Mesh newMesh = new Mesh();
        newMesh.name = "triangle";

        Vector3[] vertices = new Vector3[3];
        int[] triangles = new int[3];

        vertices[0] = Vector3.right;
        vertices[1] = Vector3.up;
        vertices[2] = Vector3.forward;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();

        return newMesh;
    }

    Mesh CreateQuadXZ(Vector3 size)
    {
        Vector3 halfsize = size *.5f;

        Mesh newMesh = new Mesh();
        newMesh.name = "quad";

        Vector3[] vertices = new Vector3[4];
        int[] triangles = new int[2*3]; //un quad est composé de 2 triangles

        vertices[0] = new Vector3(-halfsize.x, 0, -halfsize.z);
        vertices[1] = new Vector3(-halfsize.x, 0, halfsize.z);
        vertices[2] = new Vector3(halfsize.x, 0, halfsize.z);
        vertices[3] = new Vector3(halfsize.x, 0, -halfsize.z);
  

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();

        return newMesh;
    }

    Mesh CreateStripXZ(Vector3 size, int nSegments)
    {
        Vector3 halfsize = size * .5f;

        Mesh newMesh = new Mesh();
        newMesh.name = "strip";

        Vector3[] vertices = new Vector3[(nSegments+1)*2];
        int[] triangles = new int[nSegments*2*3]; 

        //vertices
        for(int i = 0; i<nSegments+1; i++)
        {
            float k = (float)i / nSegments;
            vertices[i] = new Vector3(Mathf.Lerp(-halfsize.x, halfsize.x, k), 0, -halfsize.z); // point en bas du strip
            vertices[nSegments+1+i] = new Vector3(Mathf.Lerp(-halfsize.x, halfsize.x, k), 0, halfsize.z); //vertices[point opposé se trouvant sur le haut du strip]
        }

        //triangles
        int index = 0;
        for (int i = 0; i < nSegments; i++)
        {
            triangles[index++] = i;
            triangles[index++] = i+nSegments+1;
            triangles[index++] = i+nSegments+2;

            triangles[index++] = i;
            triangles[index++] = i + nSegments +2;
            triangles[index++] = i + 1;
        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();

        return newMesh;
    }

    Mesh CreatePlaneXZ(Vector3 size, int nSegmentsX, int nSegmentsZ)
    {
        Vector3 halfsize = size * .5f;

        Mesh newMesh = new Mesh();
        newMesh.name = "plane";

        Vector3[] vertices = new Vector3[(nSegmentsX+1)*(nSegmentsZ+1)];
        int[] triangles = new int[nSegmentsX*nSegmentsZ*2*3];

        //vertices
        int index = 0;
        for (int i = 0; i < nSegmentsX + 1; i++)
        {
            float kX = (float)i / nSegmentsX;
            for(int j = 0; j<nSegmentsZ+1; j++)
            {
                float kZ = (float)j / nSegmentsZ;
                vertices[index++] = new Vector3(Mathf.Lerp(-halfsize.x, halfsize.x, kX), 0, Mathf.Lerp(-halfsize.z, halfsize.z, kZ));
            }

        }

        //triangles
        index = 0;
        for (int i = 0; i < nSegmentsX; i++)
        {
            int indexOffset = i*(nSegmentsZ +1);
            for (int j = 0; j < nSegmentsZ; j++)
            {
                triangles[index++] = indexOffset+j;
                triangles[index++] = indexOffset + j + 1;
                triangles[index++] = indexOffset + j+1 + nSegmentsZ + 1;

                triangles[index++] = indexOffset + j;
                triangles[index++] = indexOffset + j + 1 + nSegmentsZ +1;
                triangles[index++] = indexOffset + j + nSegmentsZ +1;
            }

        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();

        return newMesh;
    }

    Mesh WrapNormalizePlane(int nSegmentsX, int nSegmentsZ, ComputeVector3FromKxKz computePosition)
    {
        Vector3 halfsize = nSegmentsX*nSegmentsZ * .5f;

        Mesh newMesh = new Mesh();
        newMesh.name = "plane";

        Vector3[] vertices = new Vector3[(nSegmentsX + 1) * (nSegmentsZ + 1)];
        int[] triangles = new int[nSegmentsX * nSegmentsZ * 2 * 3];

        //vertices
        int index = 0;
        for (int i = 0; i < nSegmentsX + 1; i++)
        {
            float kX = (float)i / nSegmentsX;
            for (int j = 0; j < nSegmentsZ + 1; j++)
            {
                float kZ = (float)j / nSegmentsZ;
                vertices[index++] = computePosition(kX, kZ);
            }

        }

        //triangles
        index = 0;
        for (int i = 0; i < nSegmentsX; i++)
        {
            int indexOffset = i * (nSegmentsZ + 1);
            for (int j = 0; j < nSegmentsZ; j++)
            {
                triangles[index++] = indexOffset + j;
                triangles[index++] = indexOffset + j + 1;
                triangles[index++] = indexOffset + j + 1 + nSegmentsZ + 1;

                triangles[index++] = indexOffset + j;
                triangles[index++] = indexOffset + j + 1 + nSegmentsZ + 1;
                triangles[index++] = indexOffset + j + nSegmentsZ + 1;
            }

        }

        newMesh.vertices = vertices;
        newMesh.triangles = triangles;
        newMesh.RecalculateBounds();

        return newMesh;
    }
}
