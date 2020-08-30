using UnityEngine;
using UnityEditor;

public class MenuItems {

    [MenuItem("Tools/Generate Terrain")]
    private static void GenerateTerrain() {
        TerrainGenerator generator = new TerrainGenerator();
        generator.Generate();
    }

}