using UnityEngine;
using UnityEditor;

public class MenuItems {

    // 1024 tiles per chunk.
    // We may have to revisit this mechanism if it causes performance issues;
    // we could maybe combine the tiles into a single plane.
    private const int NUM_TILES_X = 32;
    private const int NUM_TILES_Z = 32;

    [MenuItem("Tools/Generate Terrain")]
    private static void GenerateTerrain() {

        // Find all Tile prefabs
        string[] tilePrefabGuids = AssetDatabase.FindAssets(
            "t:Prefab", new[] { "Assets/Prefabs/Tiles" });

        // For now, just use the first one we find
        string tilePrefabGuid = tilePrefabGuids[0];
        string tilePrefabPath = AssetDatabase.GUIDToAssetPath(tilePrefabGuid);
        GameObject tilePrefab = (GameObject) AssetDatabase.LoadAssetAtPath(
                tilePrefabPath, typeof(GameObject));

        // Create a parent object to hold the terrain
        GameObject parent = new GameObject("Terrain");

        // Instantiate our tiles
        for (int z = 0; z < NUM_TILES_Z; z++) {
            for (int x = 0; x < NUM_TILES_X; x++) {
                GameObject tile = (GameObject)
                        PrefabUtility.InstantiatePrefab(tilePrefab);
                tile.name = x + ", " + z;
                tile.transform.parent = parent.transform;
                // We add 0.5 because the quad has its origin in the centre.
                // This way, (0, 0) becomes a chunk boundary.
                tile.transform.position = new Vector3(x + 0.5f, 0, z + 0.5f);
            }
        }

        Selection.activeGameObject = parent;
    }

}