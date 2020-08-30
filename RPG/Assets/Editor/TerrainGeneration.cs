using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TerrainGenerator {

    private const int NUM_TILES_X = 32;
    private const int NUM_TILES_Z = 32;
    private const int NUM_TILES = NUM_TILES_X * NUM_TILES_Z;
    private const int VERTS_PER_QUAD = 4;
    private const int INDICES_PER_QUAD = 6;
    private const int NUM_VERTICES = NUM_TILES * VERTS_PER_QUAD;
    private const int NUM_INDICES = NUM_TILES * INDICES_PER_QUAD;

    private Vector3[] newVertices = new Vector3[NUM_VERTICES];
    private Vector3[] newNormals = new Vector3[NUM_VERTICES];
    private Vector2[] newUV = new Vector2[NUM_VERTICES];
    private int[] newTriangles = new int[NUM_INDICES];

    public void Generate() {

        // Create our vertices
        for (int z = 0; z < NUM_TILES_Z; z++) {
            for (int x = 0; x < NUM_TILES_X; x++) {
                CreateTile(x, z);
            }
        }

        // Create our terrain
        Mesh mesh = CreateMesh();
        GameObject terrainObj = CreateTerrainObject(mesh);

        // Select the new terrain object in the editor
        Selection.activeGameObject = terrainObj;
    }

    private void CreateTile(int x, int z) {

        int nextVertex = (z * NUM_TILES_X + x) * VERTS_PER_QUAD;
        int nextIndex = (z * NUM_TILES_X + x) * INDICES_PER_QUAD;

        /*
            * Create our vertices as follows:
            *
            *  1  _____  3
            *    |\    |       z
            *    |  \  |       ^
            *    |____\|       + > x
            *  0         2
            */

        newVertices[nextVertex]     = new Vector3(x, 0, z);
        newVertices[nextVertex + 1] = new Vector3(x, 0, z + 1);
        newVertices[nextVertex + 2] = new Vector3(x + 1, 0, z);
        newVertices[nextVertex + 3] = new Vector3(x + 1, 0, z + 1);

        // For now, the normals always point up as the terrain is flat
        newNormals[nextVertex]     = Vector3.up;
        newNormals[nextVertex + 1] = Vector3.up;
        newNormals[nextVertex + 2] = Vector3.up;
        newNormals[nextVertex + 3] = Vector3.up;

        // For now, use the entire texture for each tile
        newUV[nextVertex]     = new Vector3(0, 0);
        newUV[nextVertex + 1] = new Vector3(0, 1);
        newUV[nextVertex + 2] = new Vector3(1, 0);
        newUV[nextVertex + 3] = new Vector3(1, 1);

        // First triangle: 0-1-2
        newTriangles[nextIndex]     = nextVertex;
        newTriangles[nextIndex + 1] = nextVertex + 1;
        newTriangles[nextIndex + 2] = nextVertex + 2;

        // Second triangle: 1-3-2
        newTriangles[nextIndex + 3] = nextVertex + 1;
        newTriangles[nextIndex + 4] = nextVertex + 3;
        newTriangles[nextIndex + 5] = nextVertex + 2;
    }

    private Mesh CreateMesh() {
        Mesh mesh = new Mesh();
        mesh.vertices = newVertices;
        mesh.normals = newNormals;
        mesh.uv = newUV;
        mesh.triangles = newTriangles;
        return mesh;
    }

    private GameObject CreateTerrainObject(Mesh mesh) {

        // Instantiate our terrain prefab
        GameObject terrainPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(
                "Assets/Prefabs/Terrain.prefab", typeof(GameObject));
        GameObject terrainObj = (GameObject)
                PrefabUtility.InstantiatePrefab(terrainPrefab);

        // Attach our mesh to the new terrain object
        MeshFilter meshFilter = (MeshFilter)
                terrainObj.AddComponent(typeof(MeshFilter));
        meshFilter.mesh = mesh;

        // Attach our mesh to the mesh collider
        MeshCollider collider = terrainObj.GetComponent<MeshCollider>();
        collider.sharedMesh = mesh;

        return terrainObj;
    }

}
