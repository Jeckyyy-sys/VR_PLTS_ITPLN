using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class RefinePVModules
{
    private const string AssetFolder = "Assets/PLTS/RefinedPVModules";
    private const string GeometryRootName = "Physically sized JKM545 modules";
    private const float ModuleWidth = 1.134f;
    private const float ModuleLength = 2.274f;
    private const float ModuleThickness = 0.035f;
    private const float ModuleGap = 0.018f;

    private static readonly HashSet<string> OriginalVisualNames = new HashSet<string>
    {
        "Aluminium Module Frame", "Solar Module", "Cell Grid", "Cream photovoltaic backsheet"
    };

    private sealed class MeshBuilder
    {
        private readonly List<Vector3> vertices = new List<Vector3>();
        private readonly List<int> triangles = new List<int>();

        public void Quad(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            int start = vertices.Count;
            vertices.AddRange(new[] { a, b, c, d });
            triangles.AddRange(new[] { start, start + 1, start + 2, start, start + 2, start + 3 });
        }

        public void TopRectangle(float centerX, float centerZ, float width, float length, float y)
        {
            float left = centerX - width / 2f;
            float right = centerX + width / 2f;
            float near = centerZ - length / 2f;
            float far = centerZ + length / 2f;
            Quad(new Vector3(left, y, near), new Vector3(left, y, far),
                new Vector3(right, y, far), new Vector3(right, y, near));
        }

        public void Box(Vector3 center, Vector3 size)
        {
            Vector3 min = center - size / 2f;
            Vector3 max = center + size / 2f;
            Quad(new Vector3(min.x, max.y, min.z), new Vector3(min.x, max.y, max.z),
                new Vector3(max.x, max.y, max.z), new Vector3(max.x, max.y, min.z));
            Quad(new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z),
                new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z));
            Quad(new Vector3(min.x, min.y, min.z), new Vector3(min.x, max.y, min.z),
                new Vector3(max.x, max.y, min.z), new Vector3(max.x, min.y, min.z));
            Quad(new Vector3(max.x, min.y, max.z), new Vector3(max.x, max.y, max.z),
                new Vector3(min.x, max.y, max.z), new Vector3(min.x, min.y, max.z));
            Quad(new Vector3(min.x, min.y, max.z), new Vector3(min.x, max.y, max.z),
                new Vector3(min.x, max.y, min.z), new Vector3(min.x, min.y, min.z));
            Quad(new Vector3(max.x, min.y, min.z), new Vector3(max.x, max.y, min.z),
                new Vector3(max.x, max.y, max.z), new Vector3(max.x, min.y, max.z));
        }

        public void Cell(float centerX, float centerZ, float width, float length)
        {
            const float corner = 0.004f;
            const float y = 0.0164f;
            float left = centerX - width / 2f;
            float right = centerX + width / 2f;
            float near = centerZ - length / 2f;
            float far = centerZ + length / 2f;
            int start = vertices.Count;
            vertices.AddRange(new[]
            {
                new Vector3(left + corner, y, near), new Vector3(left, y, near + corner),
                new Vector3(left, y, far - corner), new Vector3(left + corner, y, far),
                new Vector3(right - corner, y, far), new Vector3(right, y, far - corner),
                new Vector3(right, y, near + corner), new Vector3(right - corner, y, near)
            });
            for (int i = 1; i < 7; i++)
                triangles.AddRange(new[] { start, start + i, start + i + 1 });
        }

        public Mesh Save(string name)
        {
            string path = AssetFolder + "/" + name + ".asset";
            Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (existing) return existing;

            var mesh = new Mesh { name = name };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
            return mesh;
        }
    }

    private static Material GetMaterial(string name, Color color, float metallic, float smoothness)
    {
        string path = AssetFolder + "/" + name + ".mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material) return material;

        material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
        material.SetColor("_BaseColor", color);
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Smoothness", smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static void Surface(Transform parent, string name, Mesh mesh, Material material)
    {
        var surface = new GameObject(name);
        surface.transform.SetParent(parent, false);
        surface.AddComponent<MeshFilter>().sharedMesh = mesh;
        surface.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    public static string Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "ITPLNCampusPLTS" || EditorApplication.isPlaying)
            throw new InvalidOperationException("ITPLNCampusPLTS must be open in Edit Mode.");

        Transform[] containers = scene.GetRootGameObjects()
            .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
            .Where(t => t.gameObject.activeInHierarchy &&
                (t.name == "Inclined Modules" || t.name == "Low inclined photovoltaic modules"))
            .ToArray();

        if (containers.Length == 0)
            throw new InvalidOperationException("No photovoltaic module containers found.");

        Transform[] pending = containers.Where(t => !t.Find(GeometryRootName)).ToArray();
        if (pending.Length == 0) return "All photovoltaic racks already use physically sized modules.";

        foreach (Transform container in pending)
        {
            Vector3 scale = container.lossyScale;
            if (scale.x <= 0f || scale.y <= 0f || scale.z <= 0f)
                throw new InvalidOperationException("Module container requires positive scale: " + container.name);
        }

        if (!AssetDatabase.IsValidFolder(AssetFolder))
            AssetDatabase.CreateFolder("Assets/PLTS", "RefinedPVModules");

        var frameBuilder = new MeshBuilder();
        const float frameWidth = 0.018f;
        foreach (float side in new[] { -1f, 1f })
        {
            frameBuilder.Box(new Vector3(side * (ModuleWidth - frameWidth) / 2f, 0f, 0f),
                new Vector3(frameWidth, ModuleThickness, ModuleLength));
            frameBuilder.Box(new Vector3(0f, 0f, side * (ModuleLength - frameWidth) / 2f),
                new Vector3(ModuleWidth - frameWidth * 2f, ModuleThickness, frameWidth));
        }

        var backingBuilder = new MeshBuilder();
        backingBuilder.Box(new Vector3(0f, -0.013f, 0f),
            new Vector3(ModuleWidth - frameWidth * 2f, 0.006f, ModuleLength - frameWidth * 2f));

        var substrateBuilder = new MeshBuilder();
        substrateBuilder.TopRectangle(0f, 0f, ModuleWidth - frameWidth * 2f,
            ModuleLength - frameWidth * 2f, 0.015f);

        var cellBuilder = new MeshBuilder();
        var busbarBuilder = new MeshBuilder();
        const int columns = 6;
        const int rows = 24;
        const float margin = 0.025f;
        const float cellGap = 0.0025f;
        const float centerGap = 0.008f;
        float cellWidth = (ModuleWidth - margin * 2f - cellGap * (columns - 1)) / columns;
        float cellLength = (ModuleLength - margin * 2f - cellGap * (rows - 1) - centerGap) / rows;
        for (int row = 0; row < rows; row++)
        {
            float z = -ModuleLength / 2f + margin + cellLength / 2f + row * (cellLength + cellGap);
            if (row >= rows / 2) z += centerGap;
            for (int column = 0; column < columns; column++)
            {
                float x = -ModuleWidth / 2f + margin + cellWidth / 2f + column * (cellWidth + cellGap);
                cellBuilder.Cell(x, z, cellWidth, cellLength);
                busbarBuilder.TopRectangle(x - cellWidth / 6f, z, 0.0006f, cellLength, 0.0166f);
                busbarBuilder.TopRectangle(x + cellWidth / 6f, z, 0.0006f, cellLength, 0.0166f);
            }
        }

        Mesh frameMesh = frameBuilder.Save("JKM545 35mm frame");
        Mesh backingMesh = backingBuilder.Save("JKM545 cream backsheet");
        Mesh substrateMesh = substrateBuilder.Save("JKM545 dark cell substrate");
        Mesh cellMesh = cellBuilder.Save("JKM545 144 half cells");
        Mesh busbarMesh = busbarBuilder.Save("JKM545 fine cell conductors");
        Material frameMaterial = GetMaterial("Matte aluminium", new Color(0.56f, 0.59f, 0.59f), 0.65f, 0.24f);
        Material backingMaterial = GetMaterial("Cream backsheet", new Color(0.75f, 0.74f, 0.68f), 0f, 0.12f);
        Material substrateMaterial = GetMaterial("Cell gap charcoal", new Color(0.055f, 0.062f, 0.066f), 0f, 0.18f);
        Material cellMaterial = GetMaterial("Blue black photovoltaic cells", new Color(0.022f, 0.038f, 0.061f), 0.12f, 0.3f);
        Material conductorMaterial = GetMaterial("Fine silver conductors", new Color(0.22f, 0.27f, 0.29f), 0.3f, 0.2f);

        int modulesCreated = 0;
        foreach (Transform container in pending)
        {
            var geometry = new GameObject(GeometryRootName);
            geometry.transform.SetParent(container, false);
            Vector3 inheritedScale = container.lossyScale;
            geometry.transform.localScale = new Vector3(1f / inheritedScale.x, 1f / inheritedScale.y, 1f / inheritedScale.z);
            int moduleCount = container.name == "Low inclined photovoltaic modules" ? 2 : 4;
            float totalWidth = moduleCount * ModuleWidth + (moduleCount - 1) * ModuleGap;

            for (int index = 0; index < moduleCount; index++)
            {
                var module = new GameObject("Jinko JKM545M-72HL4-V module " + (index + 1));
                module.transform.SetParent(geometry.transform, false);
                module.transform.localPosition = new Vector3(
                    -totalWidth / 2f + ModuleWidth / 2f + index * (ModuleWidth + ModuleGap), 0f, 0f);
                Surface(module.transform, "35 mm aluminium frame", frameMesh, frameMaterial);
                Surface(module.transform, "Cream backsheet", backingMesh, backingMaterial);
                Surface(module.transform, "Cell substrate", substrateMesh, substrateMaterial);
                Surface(module.transform, "6 by 24 half cells", cellMesh, cellMaterial);
                Surface(module.transform, "Cell conductors", busbarMesh, conductorMaterial);
                module.AddComponent<BoxCollider>().size = new Vector3(ModuleWidth, ModuleThickness, ModuleLength);
                modulesCreated++;
            }

            foreach (Transform child in container.Cast<Transform>().Where(t => OriginalVisualNames.Contains(t.name)))
                child.gameObject.SetActive(false);
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Rebuilt " + pending.Length + " racks with " + modulesCreated +
            " modules, each 2.274 x 1.134 x 0.035 m. Existing support transforms and interactive equipment retained.";
    }
}
