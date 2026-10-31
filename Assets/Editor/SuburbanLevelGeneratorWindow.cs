using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SuburbanLevelGeneratorWindow : EditorWindow
{
    const string RootName="__GENERATED_SUBURBAN_LEVEL__";
    const string Folder="Assets/GeneratedSuburban";
    [SerializeField] int seed=2026;
    [SerializeField] float mapSize=300f, roadWidth=10f, sampleSpacing=3f, houseSpacing=22f, houseOffset=17f;
    [SerializeField] int interactableCount=20, streetPropCount=25, parkPropCount=15;
    [SerializeField] bool createTerrain=true, createSidewalks=true, roadColliders=true;
    [SerializeField] Material roadMaterial, sidewalkMaterial, parkMaterial;
    [SerializeField] List<GameObject> houses=new List<GameObject>(), trees=new List<GameObject>(), streetProps=new List<GameObject>(), interactables=new List<GameObject>(), parkProps=new List<GameObject>();
    Vector2 scroll;

    [MenuItem("Tools/Who Looks After This?/Suburban Level Generator")]
    static void Open()=>GetWindow<SuburbanLevelGeneratorWindow>("Suburban Generator");

    void OnGUI()
    {
        scroll=EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.HelpBox("V2 builds a larger 300 x 300 suburban layout with six curved streets, residential rows, sidewalks and a central community park. Generated objects are isolated under one root.",MessageType.Info);
        seed=EditorGUILayout.IntField("Random Seed",seed);
        mapSize=EditorGUILayout.FloatField("Map Size (metres)",mapSize);
        roadWidth=EditorGUILayout.FloatField("Road Width",roadWidth);
        sampleSpacing=EditorGUILayout.FloatField("Road Sample Spacing",sampleSpacing);
        houseSpacing=EditorGUILayout.FloatField("House Spacing",houseSpacing);
        houseOffset=EditorGUILayout.FloatField("House Offset",houseOffset);
        createTerrain=EditorGUILayout.Toggle("Create New Flat Terrain",createTerrain);
        createSidewalks=EditorGUILayout.Toggle("Create Sidewalks",createSidewalks);
        roadColliders=EditorGUILayout.Toggle("Road Colliders",roadColliders);
        roadMaterial=(Material)EditorGUILayout.ObjectField("Road Material",roadMaterial,typeof(Material),false);
        sidewalkMaterial=(Material)EditorGUILayout.ObjectField("Sidewalk Material",sidewalkMaterial,typeof(Material),false);
        parkMaterial=(Material)EditorGUILayout.ObjectField("Park Material",parkMaterial,typeof(Material),false);
        DrawList("House Prefabs",houses); DrawList("Tree Prefabs",trees); DrawList("Street Furniture Prefabs",streetProps);
        DrawList("Interactable Prefabs",interactables); DrawList("Park Prop Prefabs",parkProps);
        interactableCount=EditorGUILayout.IntField("Interactable Count",interactableCount);
        streetPropCount=EditorGUILayout.IntField("Street Furniture Count",streetPropCount);
        parkPropCount=EditorGUILayout.IntField("Park Prop Count",parkPropCount);
        EditorGUILayout.HelpBox("Use prefab assets from Project, not scene objects. Interactable prefabs need your existing InteractableAsset, configured Asset data and colliders. Save scene first.",MessageType.Warning);
        GUI.enabled=!Application.isPlaying;
        if(GUILayout.Button("Generate / Regenerate Suburban Level V2",GUILayout.Height(36)) &&
           EditorUtility.DisplayDialog("Regenerate","Only "+RootName+" will be removed and rebuilt. Continue?","Generate","Cancel")) Generate();
        GUI.enabled=true; EditorGUILayout.EndScrollView();
    }

    void DrawList(string label,List<GameObject> list)
    {
        EditorGUILayout.LabelField(label,EditorStyles.boldLabel); int remove=-1;
        for(int i=0;i<list.Count;i++){EditorGUILayout.BeginHorizontal();list[i]=(GameObject)EditorGUILayout.ObjectField(list[i],typeof(GameObject),false);if(GUILayout.Button("-",GUILayout.Width(24)))remove=i;EditorGUILayout.EndHorizontal();if(remove==i)break;}
        if(remove>=0)list.RemoveAt(remove);if(GUILayout.Button("+ Add "+label))list.Add(null);
    }
    Transform Group(Transform p,string n){GameObject g=new GameObject(n);Undo.RegisterCreatedObjectUndo(g,"Create "+n);g.transform.SetParent(p,false);return g.transform;}
    void EnsureFolder(){if(!AssetDatabase.IsValidFolder(Folder))AssetDatabase.CreateFolder("Assets","GeneratedSuburban");}
    void Generate()
    {
        if(!SceneManager.GetActiveScene().IsValid())return;
        Random.InitState(seed);
        GameObject old=GameObject.Find(RootName);if(old)Undo.DestroyObjectImmediate(old);
        GameObject root=new GameObject(RootName);Undo.RegisterCreatedObjectUndo(root,"Generate suburban level");
        Transform ground=Group(root.transform,"Ground"),roads=Group(root.transform,"Curved Roads"),walks=Group(root.transform,"Sidewalks"),building=Group(root.transform,"Houses"),landscape=Group(root.transform,"Trees"),street=Group(root.transform,"Street Furniture"),park=Group(root.transform,"Central Community Park"),items=Group(root.transform,"Interactable Assets");
        if(createTerrain)MakeTerrain(ground);
        MakePark(park);
        List<Vector3[]> paths=RoadPaths();List<Vector3> samples=new List<Vector3>();
        foreach(var path in paths){var pts=Sample(path);samples.AddRange(pts);Ribbon(roads,pts,roadWidth,roadMaterial,0.12f,"Road",roadColliders);
            if(createSidewalks){Ribbon(walks,pts,2.2f,sidewalkMaterial,0.17f,"Sidewalk",false,-1,roadWidth/2+1.1f);Ribbon(walks,pts,2.2f,sidewalkMaterial,0.17f,"Sidewalk",false,1,roadWidth/2+1.1f);}}
        PlaceHouses(building,paths);PlaceTrees(landscape,paths);PlaceRandom(streetProps,streetPropCount,street,mapSize*.46f);PlaceParkProps(park,parkPropCount);PlaceInteractables(items,samples);
        Selection.activeGameObject=root;EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Suburban V2 generated. Inspect intersections, prefab placement, spawn and gameplay references.");
    }
    void MakeTerrain(Transform parent)
    {
        EnsureFolder();string path=AssetDatabase.GenerateUniqueAssetPath(Folder+"/SuburbanTerrainData.asset");
        TerrainData d=new TerrainData{heightmapResolution=257,size=new Vector3(mapSize,2,mapSize)};
        d.SetHeights(0,0,new float[d.heightmapResolution,d.heightmapResolution]);AssetDatabase.CreateAsset(d,path);
        GameObject t=Terrain.CreateTerrainGameObject(d);Undo.RegisterCreatedObjectUndo(t,"Create suburban terrain");t.name="SuburbanTerrain";t.transform.position=new Vector3(-mapSize/2,0,-mapSize/2);t.transform.SetParent(parent,true);
    }
    List<Vector3[]> RoadPaths()
    {
        float h=mapSize*.5f,a=mapSize*.38f;
        return new List<Vector3[]>{
        new Vector3[]{new Vector3(-a,.12f,-a*.72f),new Vector3(-a*.55f,.12f,-a*.82f),new Vector3(0,.12f,-a*.68f),new Vector3(a*.52f,.12f,-a*.78f),new Vector3(a,.12f,-a*.62f)},
        new Vector3[]{new Vector3(-a,.12f,-a*.05f),new Vector3(-a*.52f,.12f,-a*.16f),new Vector3(0,.12f,-a*.02f),new Vector3(a*.52f,.12f,-a*.14f),new Vector3(a,.12f,-a*.02f)},
        new Vector3[]{new Vector3(-a,.12f,a*.62f),new Vector3(-a*.5f,.12f,a*.72f),new Vector3(0,.12f,a*.58f),new Vector3(a*.55f,.12f,a*.72f),new Vector3(a,.12f,a*.58f)},
        new Vector3[]{new Vector3(-a*.68f,.12f,-a),new Vector3(-a*.78f,.12f,-a*.5f),new Vector3(-a*.62f,.12f,0),new Vector3(-a*.75f,.12f,a*.52f),new Vector3(-a*.62f,.12f,a)},
        new Vector3[]{new Vector3(0,.12f,-a),new Vector3(a*.10f,.12f,-a*.5f),new Vector3(-a*.02f,.12f,0),new Vector3(a*.12f,.12f,a*.5f),new Vector3(0,.12f,a)},
        new Vector3[]{new Vector3(a*.68f,.12f,-a),new Vector3(a*.78f,.12f,-a*.5f),new Vector3(a*.62f,.12f,0),new Vector3(a*.75f,.12f,a*.52f),new Vector3(a*.62f,.12f,a)}};
    }
    List<Vector3> Sample(Vector3[] p)
    {
        List<Vector3> o=new List<Vector3>();for(int s=0;s<p.Length-1;s++){Vector3 p0=p[Mathf.Max(s-1,0)],p1=p[s],p2=p[s+1],p3=p[Mathf.Min(s+2,p.Length-1)];int n=Mathf.Max(2,Mathf.CeilToInt(Vector3.Distance(p1,p2)/Mathf.Max(.5f,sampleSpacing)));
        for(int j=0;j<n;j++){float t=j/(float)n;Vector3 v=.5f*((2*p1)+(-p0+p2)*t+(2*p0-5*p1+4*p2-p3)*t*t+(-p0+3*p1-3*p2+p3)*t*t*t);v.y=.12f;o.Add(v);}}o.Add(p[p.Length-1]);return o;
    }
    void Ribbon(Transform parent,List<Vector3> p,float width,Material mat,float y,string label,bool collider,int side=0,float offset=0)
    {
        if(p.Count<2)return;GameObject g=new GameObject(label);Undo.RegisterCreatedObjectUndo(g,"Create "+label);g.transform.SetParent(parent,false);
        Vector3[] v=new Vector3[p.Count*2];Vector2[] uv=new Vector2[v.Length];int[] tri=new int[(p.Count-1)*6];
        for(int i=0;i<p.Count;i++){Vector3 tangent=i==0?p[1]-p[0]:(i==p.Count-1?p[i]-p[i-1]:p[i+1]-p[i-1]);tangent.y=0;tangent.Normalize();Vector3 right=Vector3.Cross(tangent,Vector3.up).normalized;Vector3 c=p[i]+right*(side*offset);c.y=y;Vector3 half=right*width/2;v[i*2]=c-half;v[i*2+1]=c+half;uv[i*2]=new Vector2(0,i/(float)(p.Count-1));uv[i*2+1]=new Vector2(1,i/(float)(p.Count-1));
        if(i<p.Count-1){int a=i*2,b=a+1,c1=a+2,d=a+3,t=i*6;tri[t]=a;tri[t+1]=b;tri[t+2]=c1;tri[t+3]=b;tri[t+4]=d;tri[t+5]=c1;}}
        Mesh m=new Mesh{name="Generated"+label};m.vertices=v;m.uv=uv;m.triangles=tri;m.RecalculateNormals();m.RecalculateBounds();g.AddComponent<MeshFilter>().sharedMesh=m;g.AddComponent<MeshRenderer>().sharedMaterial=mat?mat:DefaultMat(label);if(collider)g.AddComponent<MeshCollider>().sharedMesh=m;
    }
    Material DefaultMat(string label)
    {
        EnsureFolder();string path=Folder+"/"+label.Replace(" ","")+"Material.mat";Material m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;Shader s=Shader.Find("Universal Render Pipeline/Lit");if(!s)s=Shader.Find("Standard");m=new Material(s);Color c=label=="Sidewalk"?new Color(.58f,.56f,.52f):new Color(.22f,.23f,.25f);if(m.HasProperty("_BaseColor"))m.SetColor("_BaseColor",c);if(m.HasProperty("_Color"))m.SetColor("_Color",c);AssetDatabase.CreateAsset(m,AssetDatabase.GenerateUniqueAssetPath(path));AssetDatabase.SaveAssets();return m;
    }
    void MakePark(Transform parent)
    {
        GameObject g=new GameObject("Central Park Green");Undo.RegisterCreatedObjectUndo(g,"Create park");g.transform.SetParent(parent,false);g.transform.position=new Vector3(0,.04f,0);g.transform.localScale=new Vector3(mapSize*.22f,.08f,mapSize*.22f);
        Mesh m=new Mesh();m.vertices=new Vector3[]{new Vector3(-.5f,0,-.5f),new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(.5f,0,-.5f)};m.triangles=new int[]{0,1,2,0,2,3};m.RecalculateNormals();g.AddComponent<MeshFilter>().sharedMesh=m;
        Material pm=parkMaterial;if(!pm){EnsureFolder();string path=Folder+"/ParkGrassMaterial.mat";pm=AssetDatabase.LoadAssetAtPath<Material>(path);if(!pm){Shader s=Shader.Find("Universal Render Pipeline/Lit");if(!s)s=Shader.Find("Standard");pm=new Material(s);Color c=new Color(.22f,.42f,.2f);if(pm.HasProperty("_BaseColor"))pm.SetColor("_BaseColor",c);if(pm.HasProperty("_Color"))pm.SetColor("_Color",c);AssetDatabase.CreateAsset(pm,path);}}g.AddComponent<MeshRenderer>().sharedMaterial=pm;
    }
    GameObject Pick(List<GameObject> l){if(l==null||l.Count==0)return null;List<GameObject> v=l.FindAll(x=>x);return v.Count==0?null:v[Random.Range(0,v.Count)];}
    GameObject Place(GameObject prefab,Vector3 pos,Quaternion rot,Transform parent){if(!prefab)return null;GameObject g=PrefabUtility.InstantiatePrefab(prefab,parent) as GameObject;if(!g)g=Instantiate(prefab,parent);Undo.RegisterCreatedObjectUndo(g,"Place suburban prefab");g.transform.position=pos;g.transform.rotation=rot;return g;}
    bool Bounds(Vector3 p)=>Mathf.Abs(p.x)<mapSize*.46f&&Mathf.Abs(p.z)<mapSize*.46f;
    void PlaceHouses(Transform parent,List<Vector3[]> paths)
    {
        if(houses.Count==0)return;foreach(var path in paths){var pts=Sample(path);int step=Mathf.Max(1,Mathf.RoundToInt(houseSpacing/sampleSpacing));for(int i=2;i<pts.Count-2;i+=step){Vector3 tangent=(pts[i+1]-pts[i-1]).normalized,right=Vector3.Cross(tangent,Vector3.up).normalized;for(int s=-1;s<=1;s+=2){if(Random.value<.18f)continue;Vector3 pos=pts[i]+right*(roadWidth/2+houseOffset)*s;if(!Bounds(pos))continue;Vector3 forward=s>0?-tangent:tangent;forward.y=0;Place(Pick(houses),pos,Quaternion.LookRotation(forward),parent);}}}
    }
    void PlaceTrees(Transform parent,List<Vector3[]> paths)
    {
        if(trees.Count==0)return;foreach(var path in paths){var pts=Sample(path);int step=Mathf.Max(2,pts.Count/18);for(int i=1;i<pts.Count-1;i+=step){Vector3 tangent=(pts[i+1]-pts[i-1]).normalized,right=Vector3.Cross(tangent,Vector3.up).normalized;for(int s=-1;s<=1;s+=2){if(Random.value<.45f)continue;Vector3 pos=pts[i]+right*(roadWidth/2+houseOffset*.6f+Random.Range(2,5))*s;if(Bounds(pos))Place(Pick(trees),pos,Quaternion.Euler(0,Random.Range(0,360),0),parent);}}}
    }
    void PlaceRandom(List<GameObject> l,int count,Transform parent,float radius){if(l.Count==0)return;for(int i=0;i<count;i++){Vector3 p=new Vector3(Random.Range(-radius,radius),0,Random.Range(-radius,radius));Place(Pick(l),p,Quaternion.Euler(0,Random.Range(0,360),0),parent);}}
    void PlaceParkProps(Transform parent,int count){if(parkProps.Count==0)return;float r=mapSize*.10f;for(int i=0;i<count;i++){Vector3 p=new Vector3(Random.Range(-r,r),.12f,Random.Range(-r,r));Place(Pick(parkProps),p,Quaternion.Euler(0,Random.Range(0,360),0),parent);}}
    void PlaceInteractables(Transform parent,List<Vector3> samples)
    {
        if(interactables.Count==0)return;int n=0,tries=0;while(n<interactableCount&&tries<interactableCount*40){tries++;Vector3 p=samples[Random.Range(0,samples.Count)];Vector2 off=Random.insideUnitCircle*(roadWidth+houseOffset+8);Vector3 pos=new Vector3(p.x+off.x,0,p.z+off.y);if(!Bounds(pos))continue;if(Place(Pick(interactables),pos,Quaternion.Euler(0,Random.Range(0,360),0),parent))n++;}if(n<interactableCount)Debug.LogWarning("Only placed "+n+" interactables. Check assigned prefabs.");
    }
}
