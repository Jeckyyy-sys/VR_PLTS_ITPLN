using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class BuildReferenceVegetation
{
    private const string AssetFolder = "Assets/PLTS/ReferenceVegetation";
    private const string RootName = "Reference vegetation";

    private sealed class MeshParts
    {
        public readonly List<Vector3> Vertices = new List<Vector3>();
        public readonly List<Vector3> Normals = new List<Vector3>();
        public readonly List<int>[] Triangles =
        {
            new List<int>(), new List<int>(), new List<int>()
        };

        public Mesh Finish(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(Vertices);
            mesh.SetNormals(Normals);
            mesh.subMeshCount = Triangles.Length;
            for (int i = 0; i < Triangles.Length; i++)
                mesh.SetTriangles(Triangles[i], i);
            mesh.RecalculateBounds();
            return mesh;
        }
    }

    private static float Between(System.Random random, float minimum, float maximum)
    {
        return minimum + (float)random.NextDouble() * (maximum - minimum);
    }

    private static Vector3 Direction(System.Random random)
    {
        float vertical = Between(random, -1f, 1f);
        float angle = Between(random, 0f, Mathf.PI * 2f);
        float horizontal = Mathf.Sqrt(1f - vertical * vertical);
        return new Vector3(horizontal * Mathf.Cos(angle), vertical, horizontal * Mathf.Sin(angle));
    }

    private static Material FoliageMaterial(string name, Color color)
    {
        string path = AssetFolder + "/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null)
            return material;

        material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.name = name;
        material.color = color;
        material.SetFloat("_Smoothness", 0.12f);
        material.SetFloat("_Cull", (float)CullMode.Off);
        material.enableInstancing = true;
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static Mesh SaveMesh(string name, Func<Mesh> create)
    {
        string path = AssetFolder + "/" + name + ".asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
            return existing;
        var mesh = create();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }

    private static Mesh Crown(string name, Vector3 radius, int leafCount, int seed, bool clipped)
    {
        var random = new System.Random(seed);
        var parts = new MeshParts();
        for (int i = 0; i < leafCount; i++)
        {
            Vector3 outward = Direction(random);
            float depth = Mathf.Pow(Between(random, 0.25f, 1f), 0.3f);
            Vector3 center = Vector3.Scale(outward * depth, radius);
            // Offset crown lobes break the outline without introducing spherical surfaces.
            float lobe = 1f + 0.13f * Mathf.Sin(outward.x * 8f + seed) * Mathf.Cos(outward.z * 7f);
            center.x *= lobe;
            center.z *= lobe;
            if (clipped)
                center.y = Mathf.Clamp(center.y, -radius.y * 0.7f, radius.y * 0.7f);

            Vector3 normal = (outward + Direction(random) * 0.7f).normalized;
            Vector3 lengthAxis = Vector3.Cross(normal, Vector3.up);
            if (lengthAxis.sqrMagnitude < 0.01f)
                lengthAxis = Vector3.Cross(normal, Vector3.forward);
            lengthAxis.Normalize();
            lengthAxis = Quaternion.AngleAxis(Between(random, 0f, 360f), normal) * lengthAxis;
            Vector3 widthAxis = Vector3.Cross(normal, lengthAxis).normalized;
            float length = Between(random, 0.11f, 0.25f);
            float width = length * Between(random, 0.34f, 0.55f);
            int first = parts.Vertices.Count;
            parts.Vertices.Add(center - lengthAxis * length * 0.5f);
            parts.Vertices.Add(center + widthAxis * width * 0.5f);
            parts.Vertices.Add(center + lengthAxis * length * 0.5f);
            parts.Vertices.Add(center - widthAxis * width * 0.5f);
            for (int vertex = 0; vertex < 4; vertex++)
                parts.Normals.Add(normal);
            int color = random.Next(3);
            parts.Triangles[color].AddRange(new[] { first, first + 1, first + 2, first, first + 2, first + 3 });
        }
        return parts.Finish(name);
    }

    private static Mesh Grass(string name)
    {
        var random = new System.Random(7484);
        var parts = new MeshParts();
        for (int blade = 0; blade < 110; blade++)
        {
            float angle = Between(random, 0f, Mathf.PI * 2f);
            float distance = Between(random, 0f, 0.22f);
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 across = new Vector3(-direction.z, 0f, direction.x);
            Vector3 bottom = direction * distance;
            float height = Between(random, 0.16f, 0.47f);
            Vector3 middle = bottom + Vector3.up * height * 0.65f + direction * 0.09f;
            Vector3 tip = bottom + Vector3.up * height + direction * 0.28f;
            float width = Between(random, 0.012f, 0.025f);
            int first = parts.Vertices.Count;
            parts.Vertices.Add(bottom - across * width);
            parts.Vertices.Add(bottom + across * width);
            parts.Vertices.Add(middle - across * width * 0.55f);
            parts.Vertices.Add(middle + across * width * 0.55f);
            parts.Vertices.Add(tip);
            Vector3 normal = Vector3.Cross(across, middle - bottom).normalized;
            for (int vertex = 0; vertex < 5; vertex++)
                parts.Normals.Add(normal);
            parts.Triangles[blade % 3].AddRange(new[]
            {
                first, first + 1, first + 2, first + 1, first + 3, first + 2,
                first + 2, first + 3, first + 4
            });
        }
        return parts.Finish(name);
    }

    private static void AddBranch(MeshParts parts, Vector3 start, Vector3 end, float radius)
    {
        const int sides = 6;
        Vector3 direction = (end - start).normalized;
        Vector3 axis = Vector3.Cross(direction, Vector3.forward).normalized;
        if (axis.sqrMagnitude < 0.01f)
            axis = Vector3.right;
        Vector3 secondAxis = Vector3.Cross(direction, axis).normalized;
        int first = parts.Vertices.Count;
        for (int ring = 0; ring < 2; ring++)
        {
            for (int side = 0; side < sides; side++)
            {
                float angle = side * Mathf.PI * 2f / sides;
                Vector3 normal = axis * Mathf.Cos(angle) + secondAxis * Mathf.Sin(angle);
                Vector3 center = ring == 0 ? start : end;
                float ringRadius = ring == 0 ? radius : radius * 0.38f;
                parts.Vertices.Add(center + normal * ringRadius);
                parts.Normals.Add(normal);
            }
        }
        for (int side = 0; side < sides; side++)
        {
            int next = (side + 1) % sides;
            parts.Triangles[0].AddRange(new[]
            {
                first + side, first + next, first + sides + side,
                first + next, first + sides + next, first + sides + side
            });
        }
    }

    private static Mesh Branches(string name)
    {
        var parts = new MeshParts();
        var random = new System.Random(1217);
        for (int branch = 0; branch < 9; branch++)
        {
            float angle = branch * Mathf.PI * 2f / 9f;
            Vector3 end = new Vector3(Mathf.Cos(angle) * 1.5f, Between(random, 0.8f, 1.9f), Mathf.Sin(angle) * 1.5f);
            Vector3 start = new Vector3(0f, Between(random, -0.7f, 0.1f), 0f);
            Vector3 fork = Vector3.Lerp(start, end, 0.65f);
            AddBranch(parts, start, end, 0.075f);
            AddBranch(parts, fork, end + new Vector3(0.25f, 0.35f, -0.2f), 0.035f);
        }
        return parts.Finish(name);
    }

    private static GameObject Instance(Transform parent, string name, Mesh mesh, Material[] materials, Vector3 position, float yaw, Vector3 scale)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        obj.transform.position = position;
        obj.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
        obj.transform.localScale = scale;
        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.sharedMaterials = materials;
        renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        renderer.receiveShadows = true;
        return obj;
    }

    public static string Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "ITPLNCampusPLTS" || EditorApplication.isPlaying)
            throw new InvalidOperationException("ITPLNCampusPLTS must be open in Edit Mode.");
        if (scene.GetRootGameObjects().Any(obj => obj.name == RootName))
            throw new InvalidOperationException("Reference vegetation is already present.");
        if (!AssetDatabase.IsValidFolder(AssetFolder))
            AssetDatabase.CreateFolder("Assets/PLTS", "ReferenceVegetation");

        var foliage = new[]
        {
            FoliageMaterial("Leaf shadow green", new Color(0.105f, 0.19f, 0.055f)),
            FoliageMaterial("Leaf medium green", new Color(0.20f, 0.30f, 0.085f)),
            FoliageMaterial("Leaf sunlight green", new Color(0.33f, 0.40f, 0.12f))
        };
        var bark = FoliageMaterial("Branch weathered brown", new Color(0.24f, 0.20f, 0.15f));
        var branchMaterials = new[] { bark, bark, bark };
        var crowns = new[]
        {
            SaveMesh("Tree crown A", () => Crown("Tree crown A", new Vector3(2.35f, 1.95f, 2.5f), 2400, 101, false)),
            SaveMesh("Tree crown B", () => Crown("Tree crown B", new Vector3(2.55f, 1.8f, 2.25f), 2400, 202, false)),
            SaveMesh("Tree crown C", () => Crown("Tree crown C", new Vector3(2.15f, 2.1f, 2.6f), 2400, 303, false))
        };
        var bush = SaveMesh("Clipped hedge foliage", () => Crown("Clipped hedge foliage", new Vector3(0.57f, 0.43f, 0.53f), 560, 404, true));
        var grass = SaveMesh("Arching ornamental grass", () => Grass("Arching ornamental grass"));
        var branches = SaveMesh("Tree branch structure", () => Branches("Tree branch structure"));
        var shrubStem = SaveMesh("Topiary stem", () =>
        {
            var parts = new MeshParts();
            AddBranch(parts, Vector3.zero, new Vector3(0f, 1.8f, 0f), 0.045f);
            return parts.Finish("Topiary stem");
        });

        var existing = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<Transform>(true)).ToArray();
        var trunks = existing.Where(t => t.name.StartsWith("Tree Trunk", StringComparison.Ordinal) && t.gameObject.activeInHierarchy).ToArray();
        var root = new GameObject(RootName).transform;
        Undo.RegisterCreatedObjectUndo(root.gameObject, "Build photo reference vegetation");

        for (int i = 0; i < trunks.Length; i++)
        {
            var trunk = trunks[i];
            var trunkRenderer = trunk.GetComponent<Renderer>();
            float top = trunkRenderer != null ? trunkRenderer.bounds.max.y : 4f;
            Vector3 origin = new Vector3(trunk.position.x, top, trunk.position.z);
            Instance(root, "Branching tree crown " + (i + 1), branches, branchMaterials, origin, i * 47f, Vector3.one);
            Instance(root, "Leaf canopy " + (i + 1), crowns[i % crowns.Length], foliage, origin + Vector3.up * 1.2f, i * 47f, Vector3.one);
        }

        foreach (var old in existing.Where(t => t.name.StartsWith("Tree Canopy", StringComparison.Ordinal) || t.name.StartsWith("Layered Tree Foliage", StringComparison.Ordinal)))
        {
            Undo.RecordObject(old.gameObject, "Replace rounded foliage with leaves");
            old.gameObject.SetActive(false);
        }

        for (int i = 0; i < 35; i++)
        {
            float z = 3f + i * 0.5f;
            float x = 11.35f + 0.12f * Mathf.Sin(i * 1.9f);
            Instance(root, "Fence verge grass " + (i + 1), grass, foliage, new Vector3(x, 0.04f, z), i * 31f, Vector3.one);
        }
        for (int i = 0; i < 5; i++)
        {
            float x = 0.9f + i * 0.43f;
            Instance(root, "Entrance hedge " + (i + 1), bush, foliage, new Vector3(x, 0.52f, 0.45f), i * 29f, new Vector3(0.82f, 1f, 0.92f));
        }
        for (int i = 0; i < 2; i++)
        {
            float x = -i * 2f;
            Instance(root, "Topiary trunk " + (i + 1), shrubStem, branchMaterials, new Vector3(x, 0f, 0.3f), 0f, Vector3.one);
            for (int tier = 0; tier < 3; tier++)
            {
                float scale = 1.25f - tier * 0.2f;
                Instance(root, "Tiered garden shrub " + (i + 1) + " level " + (tier + 1), bush, foliage,
                    new Vector3(x, 0.55f + tier * 0.52f, 0.3f), i * 71f + tier * 23f, new Vector3(scale, 0.75f, scale));
            }
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Replaced " + trunks.Length + " spherical tree crowns with mesh leaves, and added 35 grass tufts, 5 hedge sections and 2 tiered shrubs.";
    }

    public static string Complete()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.name != "ITPLNCampusPLTS" || EditorApplication.isPlaying)
            throw new InvalidOperationException("ITPLNCampusPLTS must be open in Edit Mode.");

        var vegetation = scene.GetRootGameObjects().FirstOrDefault(obj => obj.name == RootName);
        if (vegetation == null)
            throw new InvalidOperationException("Run BuildReferenceVegetation.Build before Complete.");

        var crowns = new[]
        {
            AssetDatabase.LoadAssetAtPath<Mesh>(AssetFolder + "/Tree crown A.asset"),
            AssetDatabase.LoadAssetAtPath<Mesh>(AssetFolder + "/Tree crown B.asset"),
            AssetDatabase.LoadAssetAtPath<Mesh>(AssetFolder + "/Tree crown C.asset")
        };
        var branches = AssetDatabase.LoadAssetAtPath<Mesh>(AssetFolder + "/Tree branch structure.asset");
        if (crowns.Any(mesh => mesh == null) || branches == null)
            throw new InvalidOperationException("The previously generated crown and branch assets are required.");

        var foliage = new[]
        {
            FoliageMaterial("Leaf shadow green", new Color(0.105f, 0.19f, 0.055f)),
            FoliageMaterial("Leaf medium green", new Color(0.20f, 0.30f, 0.085f)),
            FoliageMaterial("Leaf sunlight green", new Color(0.33f, 0.40f, 0.12f))
        };
        var bark = FoliageMaterial("Branch weathered brown", new Color(0.24f, 0.20f, 0.15f));
        var branchMaterials = new[] { bark, bark, bark };
        var existing = scene.GetRootGameObjects().SelectMany(obj => obj.GetComponentsInChildren<Transform>(true)).ToArray();
        var trunks = existing.Where(t => t.name.StartsWith("Tree Trunk", StringComparison.Ordinal) && t.gameObject.activeInHierarchy).ToArray();
        var canopies = vegetation.GetComponentsInChildren<Transform>(true)
            .Where(t => t.name.StartsWith("Leaf canopy ", StringComparison.Ordinal)).ToList();
        int addedTrees = 0;

        foreach (var trunk in trunks)
        {
            bool alreadyCovered = canopies.Any(canopy =>
                Mathf.Abs(canopy.position.x - trunk.position.x) < 0.2f &&
                Mathf.Abs(canopy.position.z - trunk.position.z) < 0.2f);
            if (alreadyCovered)
                continue;

            var trunkRenderer = trunk.GetComponent<Renderer>();
            float top = trunkRenderer != null ? trunkRenderer.bounds.max.y : 4f;
            Vector3 origin = new Vector3(trunk.position.x, top, trunk.position.z);
            int index = canopies.Count;
            Instance(vegetation.transform, "Branching tree crown " + (index + 1), branches, branchMaterials,
                origin, index * 47f, Vector3.one);
            var createdCanopy = Instance(vegetation.transform, "Leaf canopy " + (index + 1), crowns[index % crowns.Length], foliage,
                origin + Vector3.up * 1.2f, index * 47f, Vector3.one);
            canopies.Add(createdCanopy.transform);
            addedTrees++;
        }

        var offsets = new[]
        {
            new Vector3(0.12f, 0.05f, -0.14f),
            new Vector3(-0.13f, -0.08f, 0.12f),
            new Vector3(0f, 0.12f, 0.08f)
        };
        var rotations = new[] { 67f, 137f, 233f };
        var scales = new[] { 0.95f, 0.98f, 0.93f };
        int addedLayers = 0;
        foreach (var canopy in canopies)
        {
            for (int layer = 0; layer < 3; layer++)
            {
                string name = "Dense leaf layer " + (layer + 1);
                if (canopy.Find(name) != null)
                    continue;
                Instance(canopy, name, crowns[layer], foliage, canopy.TransformPoint(offsets[layer]),
                    rotations[layer], Vector3.one * scales[layer]);
                addedLayers++;
            }
        }

        int hiddenSpheres = 0;
        foreach (var old in existing.Where(t => t.name.StartsWith("Tree Canopy", StringComparison.Ordinal) || t.name.StartsWith("Layered Tree Foliage", StringComparison.Ordinal)))
        {
            if (!old.gameObject.activeSelf)
                continue;
            Undo.RecordObject(old.gameObject, "Replace cloned rounded foliage with leaves");
            old.gameObject.SetActive(false);
            hiddenSpheres++;
        }

        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        return "Completed " + addedTrees + " missing tree crowns, added " + addedLayers +
            " leaf layers across " + canopies.Count + " trees, and hid " + hiddenSpheres + " rounded foliage objects.";
    }
}

