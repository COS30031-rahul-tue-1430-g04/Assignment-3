using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SuburbanLevelGeneratorWindow : EditorWindow
{
    const string RootName = "__GENERATED_SUBURBAN_LEVEL__";
    const string AssetFolder = "Assets/GeneratedSuburban";
    const string TerrainPath = AssetFolder + "/SuburbanTerrainData.asset";

    [SerializeField] int seed = 2026;
    [SerializeField, Min(200)] float mapSize = 450f;
    [SerializeField, Min(4)] float roadWidth = 7f;
    [SerializeField, Min(1)] float roadSampleSpacing = 3f;
    [SerializeField, Min(5)] float houseSpacing = 14f;
    [SerializeField, Min(0)] int houseCount = 55;
    [SerializeField, Min(4)] float houseOffset = 15f;
    [SerializeField, Min(0)] float houseRoadClearance = 3f;
    [SerializeField, Min(1)] float houseMinSeparation = 12f;
    [SerializeField, Min(1)] float treeHouseClearance = 7f;
    [SerializeField, Range(6, 20)] int roadCount = 14;
    [SerializeField, Min(0)] int roadsideTreeCount = 45;
    [SerializeField, Min(0)] int treeCount = 35;
    [SerializeField, Min(0)] int gardenTreeCount = 20;
    [SerializeField] bool createTerrain = true;
    [SerializeField] bool addRoadColliders = true;
    [SerializeField] Material roadMaterial;
    [SerializeField] List<GameObject> housePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> treePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> streetPropPrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> interactablePrefabs = new List<GameObject>();
    [SerializeField] List<GameObject> parkPropPrefabs = new List<GameObject>();
    [SerializeField, Min(0)] int interactableCount = 35;
    [SerializeField, Min(0)] int streetPropCount = 60;
    [SerializeField, Min(0)] int parkPropCount = 25;
    Vector2 scroll;

    [MenuItem("Tools/Who Looks After This?/Suburban Level Generator")]
    static void OpenWindow() => GetWindow<SuburbanLevelGeneratorWindow>("Suburban Generator");

    void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.HelpBox("V3 creates a larger, denser suburban layout with extra curved streets and adjustable tree placement. Regeneration removes only the generated root, not your player, managers, Canvas, timer, or minimap.", MessageType.Info);
        EditorGUILayout.LabelField("Layout", EditorStyles.boldLabel);
        seed = EditorGUILayout.IntField("Random Seed", seed);
        mapSize = EditorGUILayout.FloatField("Map Size", mapSize);
        roadCount = EditorGUILayout.IntSlider("Road Count", roadCount, 6, 20);
        roadWidth = EditorGUILayout.FloatField("Road Width", roadWidth);
        roadSampleSpacing = EditorGUILayout.FloatField("Road Sample Spacing", roadSampleSpacing);
        houseCount = EditorGUILayout.IntField("Number of Houses", houseCount);
        houseSpacing = EditorGUILayout.FloatField("House Spacing", houseSpacing);
        houseOffset = EditorGUILayout.FloatField("House Offset", houseOffset);
        houseRoadClearance = EditorGUILayout.FloatField("House Road Clearance", houseRoadClearance);
        houseMinSeparation = EditorGUILayout.FloatField("Minimum House Separation", houseMinSeparation);
        treeHouseClearance = EditorGUILayout.FloatField("Tree-to-House Clearance", treeHouseClearance);
        roadsideTreeCount = EditorGUILayout.IntField("Roadside Trees", roadsideTreeCount);
        treeCount = EditorGUILayout.IntField("Extra Random Trees", treeCount);
        gardenTreeCount = EditorGUILayout.IntField("Garden / Park Trees", gardenTreeCount);
        createTerrain = EditorGUILayout.Toggle("Create New Flat Terrain", createTerrain);
        addRoadColliders = EditorGUILayout.Toggle("Add Road Colliders", addRoadColliders);
        roadMaterial = (Material)EditorGUILayout.ObjectField("Road Material", roadMaterial, typeof(Material), false);

        EditorGUILayout.Space();
        DrawList("House Prefabs", housePrefabs);
        DrawList("Tree Prefabs", treePrefabs);
        DrawList("Street Furniture Prefabs", streetPropPrefabs);
        DrawList("Interactable Prefabs", interactablePrefabs);
        DrawList("Park Prop Prefabs", parkPropPrefabs);

        interactableCount = EditorGUILayout.IntField("Interactable Count", interactableCount);
        streetPropCount = EditorGUILayout.IntField("Street Prop Count", streetPropCount);
        parkPropCount = EditorGUILayout.IntField("Park Prop Count", parkPropCount);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Roadside trees are placed in the verge beside roads, alternating sides. Tree-to-House Clearance controls spacing from houses. Tree Prefabs are also used for extra garden and park trees. Assign multiple varieties for a natural mix. Assign prefab assets from the Project window. Interactable prefabs should already have InteractableAsset and valid Asset data. Save the scene before generating.", MessageType.Warning);
        GUI.enabled = !Application.isPlaying;
        if (GUILayout.Button("Generate / Regenerate Suburban Level", GUILayout.Height(34)))
        {
            if (EditorUtility.DisplayDialog("Generate Suburban Level",
                "Only the existing " + RootName + " root will be removed. Continue?", "Generate", "Cancel"))
                Generate();
        }
        GUI.enabled = true;
        EditorGUILayout.EndScrollView();
    }

    void DrawList(string label, List<GameObject> list)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        int remove = -1;
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            list[i] = (GameObject)EditorGUILayout.ObjectField(list[i], typeof(GameObject), false);
            if (GUILayout.Button("-", GUILayout.Width(24))) remove = i;
            EditorGUILayout.EndHorizontal();
            if (remove == i) break;
        }
        if (remove >= 0) list.RemoveAt(remove);
        if (GUILayout.Button("+ Add " + label)) list.Add(null);
        EditorGUILayout.Space(3);
    }

    void Generate()
    {
        Scene scene = SceneManager.GetActiveScene();
        if (!scene.IsValid()) { EditorUtility.DisplayDialog("No scene", "Open your SuburbanLevel scene first.", "OK"); return; }
        Random.InitState(seed);

        GameObject old = GameObject.Find(RootName);
        if (old != null) Undo.DestroyObjectImmediate(old);

        GameObject root = new GameObject(RootName);
        Undo.RegisterCreatedObjectUndo(root, "Generate suburban level");
        Transform groundRoot = Child(root.transform, "Ground");
        Transform roadRoot = Child(root.transform, "Curved Roads");
        Transform houseRoot = Child(root.transform, "Houses");
        Transform treeRoot = Child(root.transform, "Trees");
        Transform propRoot = Child(root.transform, "Street Props");
        Transform interactRoot = Child(root.transform, "Interactable Assets");
        Transform parkRoot = Child(root.transform, "Community Green");

        if (createTerrain) MakeTerrain(groundRoot);
        List<Vector3[]> paths = MakePaths();
        List<Vector3> samplesAll = new List<Vector3>();
        foreach (var path in paths)
        {
            List<Vector3> samples = Sample(path, roadSampleSpacing);
            samplesAll.AddRange(samples);
            MakeRoad(roadRoot, samples);
        }
        List<Vector3> housePositions = PlaceHouses(houseRoot, paths);
        PlaceTrees(treeRoot, paths, roadsideTreeCount, housePositions);
        PlaceRandomTrees(treePrefabs, treeCount, treeRoot, mapSize * .44f, paths, housePositions);
        PlaceRandomTrees(treePrefabs, gardenTreeCount, treeRoot, mapSize * .30f, paths, housePositions);
        PlaceRandom(streetPropPrefabs, streetPropCount, propRoot, mapSize * .42f);
        PlaceRandom(parkPropPrefabs, parkPropCount, parkRoot, mapSize * .30f);
        PlaceInteractables(interactRoot, samplesAll);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("Generated suburban layout under " + RootName + ". Check prefab placement and road junctions before playtesting.");
    }

    Transform Child(Transform parent, string name)
    {
        GameObject go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    void EnsureFolder(string path)
    {
        string current = "Assets";
        foreach (string part in path.Substring("Assets/".Length).Split('/'))
        {
            string next = current + "/" + part;
            if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, part);
            current = next;
        }
    }

    void MakeTerrain(Transform parent)
    {
        EnsureFolder(AssetFolder);
        TerrainData data = AssetDatabase.LoadAssetAtPath<TerrainData>(TerrainPath);
        if (data == null)
        {
            data = new TerrainData();
            AssetDatabase.CreateAsset(data, AssetDatabase.GenerateUniqueAssetPath(TerrainPath));
        }
        Undo.RecordObject(data, "Configure suburban terrain");
        data.heightmapResolution = 257;
        data.size = new Vector3(mapSize, 2f, mapSize);
        data.SetHeights(0, 0, new float[data.heightmapResolution, data.heightmapResolution]);
        EditorUtility.SetDirty(data);
        GameObject terrainObject = Terrain.CreateTerrainGameObject(data);
        Undo.RegisterCreatedObjectUndo(terrainObject, "Create suburban terrain");
        terrainObject.name = "SuburbanTerrain";
        terrainObject.transform.position = new Vector3(-mapSize / 2f, 0f, -mapSize / 2f);
        terrainObject.transform.SetParent(parent, true);
    }

    List<Vector3[]> MakePaths()
    {
        float h = mapSize * .5f;
        float e = mapSize * .39f;
        List<Vector3[]> paths = new List<Vector3[]>();
        // Two long curved arterial roads.
        paths.Add(new Vector3[] { new Vector3(-e,.10f,-h*.92f), new Vector3(-e*.72f,.10f,-h*.35f), new Vector3(-e*.48f,.10f,h*.05f), new Vector3(-e*.68f,.10f,h*.52f), new Vector3(-e*.35f,.10f,h*.92f) });
        paths.Add(new Vector3[] { new Vector3(-h*.92f,.10f,-h*.22f), new Vector3(-h*.35f,.10f,-h*.32f), new Vector3(h*.18f,.10f,-h*.12f), new Vector3(h*.58f,.10f,h*.12f), new Vector3(h*.92f,.10f,h*.22f) });
        // Add curved residential streets in alternating directions to form connected blocks.
        int extras = Mathf.Max(0, roadCount - 2);
        for (int i = 0; i < extras; i++)
        {
            float t = (i + 1f) / (extras + 1f);
            float offset = Mathf.Lerp(-h*.70f, h*.70f, t);
            if (i % 2 == 0)
                paths.Add(new Vector3[] { new Vector3(-h*.92f,.10f,offset-12f), new Vector3(-h*.35f,.10f,offset+8f), new Vector3(h*.18f,.10f,offset-7f), new Vector3(h*.92f,.10f,offset+10f) });
            else
                paths.Add(new Vector3[] { new Vector3(offset-10f,.10f,-h*.92f), new Vector3(offset+8f,.10f,-h*.30f), new Vector3(offset-8f,.10f,h*.28f), new Vector3(offset+10f,.10f,h*.92f) });
        }
        return paths;
    }

    List<Vector3> Sample(Vector3[] p, float spacing)
    {
        List<Vector3> output = new List<Vector3>();
        for (int seg = 0; seg < p.Length - 1; seg++)
        {
            Vector3 p0 = p[Mathf.Max(seg - 1, 0)], p1 = p[seg];
            Vector3 p2 = p[seg + 1], p3 = p[Mathf.Min(seg + 2, p.Length - 1)];
            int steps = Mathf.Max(2, Mathf.CeilToInt(Vector3.Distance(p1, p2) / Mathf.Max(.5f, spacing)));
            for (int j = 0; j < steps; j++)
            {
                float t = j / (float)steps;
                Vector3 v = .5f * ((2f*p1) + (-p0+p2)*t + (2f*p0-5f*p1+4f*p2-p3)*t*t + (-p0+3f*p1-3f*p2+p3)*t*t*t);
                v.y = .10f;
                output.Add(v);
            }
        }
        output.Add(p[p.Length - 1]);
        return output;
    }

    void MakeRoad(Transform parent, List<Vector3> points)
    {
        if (points.Count < 2) return;
        GameObject road = new GameObject("Curved Road");
        Undo.RegisterCreatedObjectUndo(road, "Create curved road");
        road.transform.SetParent(parent, false);
        Vector3[] vertices = new Vector3[points.Count * 2];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[(points.Count - 1) * 6];
        for (int i = 0; i < points.Count; i++)
        {
            Vector3 tangent = i == 0 ? points[1]-points[0] :
                (i == points.Count-1 ? points[i]-points[i-1] : points[i+1]-points[i-1]);
            tangent.y = 0; tangent.Normalize();
            Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized * roadWidth * .5f;
            vertices[i*2] = points[i] - side;
            vertices[i*2+1] = points[i] + side;
            uv[i*2] = new Vector2(0, i/(float)(points.Count-1));
            uv[i*2+1] = new Vector2(1, i/(float)(points.Count-1));
            if (i < points.Count-1)
            {
                int a=i*2, b=a+1, c=a+2, d=a+3, t=i*6;
                triangles[t]=a; triangles[t+1]=b; triangles[t+2]=c;
                triangles[t+3]=b; triangles[t+4]=d; triangles[t+5]=c;
            }
        }
        Mesh mesh = new Mesh { name = "GeneratedCurvedRoadMesh" };
        mesh.vertices=vertices; mesh.uv=uv; mesh.triangles=triangles;
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        road.AddComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = road.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = roadMaterial != null ? roadMaterial : GetRoadMaterial();
        if (addRoadColliders) road.AddComponent<MeshCollider>().sharedMesh = mesh;
    }

    Material GetRoadMaterial()
    {
        const string path = "Assets/GeneratedSuburban/GeneratedRoadMaterial.mat";
        EnsureFolder(AssetFolder);
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        material = new Material(shader) { name = "GeneratedRoadMaterial" };
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", new Color(.22f,.23f,.25f));
        if (material.HasProperty("_Color")) material.SetColor("_Color", new Color(.22f,.23f,.25f));
        AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(path));
        AssetDatabase.SaveAssets();
        return material;
    }

    GameObject Pick(List<GameObject> list)
    {
        if (list == null || list.Count == 0) return null;
        List<GameObject> valid = list.FindAll(x => x != null);
        return valid.Count == 0 ? null : valid[Random.Range(0, valid.Count)];
    }

    GameObject Place(GameObject prefab, Vector3 pos, Quaternion rot, Transform parent)
    {
        if (prefab == null) return null;
        GameObject go = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
        if (go == null) go = Instantiate(prefab, parent);
        Undo.RegisterCreatedObjectUndo(go, "Place suburban prefab");
        go.transform.position = pos;

        // Preserve the prefab's authored X and Z rotation. Only apply the
        // generator's requested Y rotation (house alignment / random yaw).
        Vector3 prefabEuler = prefab.transform.eulerAngles;
        float requestedYaw = rot.eulerAngles.y;
        go.transform.rotation = Quaternion.Euler(
            prefabEuler.x,
            requestedYaw,
            prefabEuler.z
        );
        return go;
    }

    bool InBounds(Vector3 p) => Mathf.Abs(p.x) < mapSize*.46f && Mathf.Abs(p.z) < mapSize*.46f;

    List<Vector3> PlaceHouses(Transform parent, List<Vector3[]> paths)
    {
        // Return house positions so tree placement can keep clear of every building.
        List<Vector3> placedHousePositions = new List<Vector3>();
        if (housePrefabs.Count == 0) return placedHousePositions;
        int step = Mathf.Max(1, Mathf.RoundToInt(houseSpacing / Mathf.Max(1f, roadSampleSpacing)));

        foreach (var path in paths)
        {
            if (placedHousePositions.Count >= houseCount) break;
            List<Vector3> pts = Sample(path, Mathf.Max(2f, roadSampleSpacing));
            for (int i = 2; i < pts.Count - 2; i += step)
            {
                if (placedHousePositions.Count >= houseCount) break;
                Vector3 tangent = (pts[i + 1] - pts[i - 1]).normalized;
                tangent.y = 0f;
                if (tangent.sqrMagnitude < 0.001f) continue;
                tangent.Normalize();

                // Side vector is perpendicular to the road direction.
                Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized;

                for (int sign = -1; sign <= 1; sign += 2)
                {
                    if (placedHousePositions.Count >= houseCount) break;

                    // Place the house beyond the road edge, with an adjustable setback.
                    Vector3 pos = pts[i] + side * (roadWidth * .5f + houseOffset) * sign;
                    pos.y = 0f;

                    if (!InBounds(pos)) continue;

                    // Keep the whole house frontage safely outside the road corridor.
                    if (!IsRoadClear(pos, paths, houseRoadClearance)) continue;

                    // Prevent duplicate/overlapping houses near junctions and parallel roads.
                    float minSeparationSqr = houseMinSeparation * houseMinSeparation;
                    bool overlapsAnotherHouse = false;
                    foreach (Vector3 existing in placedHousePositions)
                    {
                        Vector2 a = new Vector2(pos.x, pos.z);
                        Vector2 b = new Vector2(existing.x, existing.z);
                        if ((a - b).sqrMagnitude < minSeparationSqr)
                        {
                            overlapsAnotherHouse = true;
                            break;
                        }
                    }
                    if (overlapsAnotherHouse) continue;

                    // House front faces the road; its front wall remains parallel to the road.
                    Vector3 facingRoad = -side * sign;
                    facingRoad.y = 0f;
                    GameObject house = Place(
                        Pick(housePrefabs),
                        pos,
                        Quaternion.LookRotation(facingRoad, Vector3.up),
                        parent
                    );

                    if (house != null)
                        placedHousePositions.Add(pos);
                }
            }
        }
        Debug.Log("House placement: placed " + placedHousePositions.Count + " of " + houseCount +
                  " requested. If fewer were placed, road clearance, map boundaries or minimum separation limited valid positions.");
        return placedHousePositions;
    }

    void PlaceTrees(Transform parent, List<Vector3[]> paths, int maxTrees, List<Vector3> housePositions)
    {
        if (treePrefabs.Count == 0 || maxTrees <= 0) return;

        int placed = 0;
        // Roadside trees are deliberately placed in the verge just outside the road.
        // Use a moderate sample interval and alternate sides to avoid a continuous wall.
        float sampleInterval = Mathf.Max(16f, houseSpacing + 2f);

        foreach (var path in paths)
        {
            if (placed >= maxTrees) break;

            List<Vector3> pts = Sample(path, sampleInterval);
            for (int i = 1; i < pts.Count - 1; i++)
            {
                if (placed >= maxTrees) break;

                Vector3 tangent = (pts[i + 1] - pts[i - 1]).normalized;
                tangent.y = 0f;
                if (tangent.sqrMagnitude < 0.001f) continue;
                tangent.Normalize();

                Vector3 side = Vector3.Cross(tangent, Vector3.up).normalized;

                // Alternate sides along each road so trees are distributed naturally.
                int sign = (i % 2 == 0) ? 1 : -1;
                float vergeOffset = roadWidth * 0.5f + Random.Range(3.5f, 5.5f);
                Vector3 pos = pts[i] + side * vergeOffset * sign;
                pos.y = 0f;

                // Keep a small safety margin from the road edge, not a large exclusion zone.
                if (!InBounds(pos) ||
                    !IsRoadClear(pos, paths, 2.5f) ||
                    !IsClearOfHouses(pos, housePositions))
                    continue;

                GameObject tree = Place(
                    Pick(treePrefabs),
                    pos,
                    Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                    parent
                );

                if (tree != null) placed++;
            }
        }

        Debug.Log("Roadside tree placement: placed " + placed + " of " + maxTrees +
                  " requested. If fewer were placed, road junctions, map boundaries or house-clearance checks limited the available verge space.");
    }

    bool IsClearOfHouses(Vector3 point, List<Vector3> housePositions)
    {
        float minDistanceSqr = treeHouseClearance * treeHouseClearance;
        Vector2 treePoint = new Vector2(point.x, point.z);

        foreach (Vector3 housePosition in housePositions)
        {
            Vector2 housePoint = new Vector2(housePosition.x, housePosition.z);
            if ((treePoint - housePoint).sqrMagnitude < minDistanceSqr)
                return false;
        }
        return true;
    }

    // Returns false when a point is too close to any road centreline.
    // Uses XZ distance to every road segment, not just the sampled tree-placement path.
    bool IsRoadClear(Vector3 point, List<Vector3[]> paths, float clearance)
    {
        float minimumDistance = roadWidth * 0.5f + clearance;
        float minSqr = minimumDistance * minimumDistance;

        foreach (var path in paths)
        {
            List<Vector3> samples = Sample(path, Mathf.Max(2f, roadSampleSpacing));
            for (int i = 0; i < samples.Count - 1; i++)
            {
                Vector2 a = new Vector2(samples[i].x, samples[i].z);
                Vector2 b = new Vector2(samples[i + 1].x, samples[i + 1].z);
                Vector2 p2 = new Vector2(point.x, point.z);
                Vector2 ab = b - a;
                float lengthSqr = ab.sqrMagnitude;
                float t = lengthSqr > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p2 - a, ab) / lengthSqr) : 0f;
                Vector2 closest = a + ab * t;
                if ((p2 - closest).sqrMagnitude < minSqr) return false;
            }
        }
        return true;
    }

    void PlaceRandomTrees(List<GameObject> prefabs, int count, Transform parent, float radius, List<Vector3[]> paths, List<Vector3> housePositions)
    {
        if (prefabs == null || prefabs.Count == 0 || count <= 0) return;

        int placed = 0;
        int attempts = 0;
        int maxAttempts = count * 40;

        while (placed < count && attempts < maxAttempts)
        {
            attempts++;
            Vector3 pos = new Vector3(
                Random.Range(-radius, radius),
                0f,
                Random.Range(-radius, radius));

            if (!InBounds(pos) ||
                !IsRoadClear(pos, paths, 13f) ||
                !IsClearOfHouses(pos, housePositions))
                continue;

            if (Place(Pick(prefabs), pos, Quaternion.Euler(0, Random.Range(0, 360), 0), parent) != null)
                placed++;
        }

        if (placed < count)
            Debug.LogWarning("Placed " + placed + " of " + count +
                " extra trees because road-clearance checks rejected some positions.");
    }

    void PlaceRandom(List<GameObject> prefabs, int count, Transform parent, float radius)
    {
        if (prefabs.Count == 0) return;
        for (int i=0; i<count; i++)
        {
            Vector3 pos=new Vector3(Random.Range(-radius,radius),0,Random.Range(-radius,radius));
            Place(Pick(prefabs),pos,Quaternion.Euler(0,Random.Range(0,360),0),parent);
        }
    }

    void PlaceInteractables(Transform parent, List<Vector3> roadSamples)
    {
        if (interactablePrefabs.Count == 0) return;
        int placed=0, attempts=0;
        while (placed < interactableCount && attempts < interactableCount*30)
        {
            attempts++;
            Vector3 pos;
            if (roadSamples.Count > 0)
            {
                Vector3 rp=roadSamples[Random.Range(0,roadSamples.Count)];
                Vector2 offset=Random.insideUnitCircle*(roadWidth+houseOffset+5f);
                pos=new Vector3(rp.x+offset.x,0,rp.z+offset.y);
            }
            else pos=new Vector3(Random.Range(-mapSize*.4f,mapSize*.4f),0,Random.Range(-mapSize*.4f,mapSize*.4f));
            if (!InBounds(pos)) continue;
            GameObject prefab=Pick(interactablePrefabs);
            if (prefab == null) break;
            if (Place(prefab,pos,Quaternion.Euler(0,Random.Range(0,360),0),parent) != null) placed++;
        }
        if (placed < interactableCount) Debug.LogWarning("Placed " + placed + " of " + interactableCount + " interactables. Check prefab list and map size.");
    }
}
