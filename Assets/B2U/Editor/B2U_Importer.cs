// Blender To Unity Importer v1.1
// Cogumelo Softworks 2017

using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using System.IO;

// B2U Inspector UI

public class B2U_Importer : EditorWindow {

    // Global parameters
    public static string metaDataPath = "B2U/Editor/Data/";
    public static string prefixPrefab = "_";
    Object metaDataFolder;

    // Scene Importer parameters
    Object sceneFile;

    // ------------------------------------------------------------------------------------------
    // Importer Panel UI
    [MenuItem("Window/B2U Importer")]
    static void Init() {

        B2U_Importer window = (B2U_Importer)EditorWindow.GetWindow(typeof(B2U_Importer));
        GUIContent title = new GUIContent();
        title.text = "B2U Importer";
        window.titleContent = title;
        window.Show();
    }

    [MenuItem("Assets/B2U/Import B2U Scene")]
    private static void ImportB2UScene() {
        var selected = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(selected);
        B2U_Utils.CreateScene(path, true, true, true);
    }


    [MenuItem("Assets/B2U/Import B2U Scene", true)]
    private static bool ImportB2USceneValidation() {
        var selected = Selection.activeObject;
        var path = Path.GetExtension(AssetDatabase.GetAssetPath(selected));
        return path == ".b2us";
    }

    [MenuItem("CONTEXT/B2U_Scene/Reimport B2U Scene")]
    private static void ReimportScene() {
        GameObject selected = Selection.activeObject as GameObject;
        var path = selected.GetComponent<B2U_Scene>().dataPath;
        B2U_Utils.CreateScene(path, true, true, true);
    }

    void Awake() {
        prefixPrefab = EditorPrefs.GetString("B2U_prefixPrefab", prefixPrefab);
    }

    void OnGUI() {

        GUILayout.Label(" B2U Importer:", EditorStyles.boldLabel);
        prefixPrefab = EditorGUILayout.TextField("Prefix Identification", prefixPrefab);

        // Save in EditorPlayerPrefs
        EditorPrefs.SetString("B2U_prefixPrefab", prefixPrefab);
    }
}

// B2U Importer System

class B2U_Postprocessor : AssetPostprocessor {

    public static List<B2U_MatPath> MatList = new List<B2U_MatPath>();

    //Setting Objects and Material Paths --------------------
    void OnPreprocessModel() {
        if (assetPath.Contains(B2U_Importer.prefixPrefab)) {

            ModelImporter modelImporter = assetImporter as ModelImporter;

            modelImporter.materialName = ModelImporterMaterialName.BasedOnMaterialName;
            modelImporter.materialSearch = ModelImporterMaterialSearch.Everywhere;

            string b2upref = Path.ChangeExtension(assetPath, "b2up");
            string correct_nameprefab = Path.GetFileName(b2upref);
            string prefab_path = "Assets/" + B2U_Importer.metaDataPath + "Prefabs/" + correct_nameprefab;

            // Keep Materials and correct paths in a list
            XmlDocument doc = new XmlDocument();
            doc.Load(prefab_path);
            XmlNode root = doc.DocumentElement;
            XmlNodeList Materials = root.SelectSingleNode("Materials").ChildNodes;

            // UV2 Generation
            string uv2 = root.SelectSingleNode("UV2").InnerText;
            if (uv2 == "True") {
                modelImporter.generateSecondaryUV = true;
            }
            else {
                modelImporter.generateSecondaryUV = false;
            }

            for (int i = 0; i < Materials.Count; i++) {
                string path = Materials[i].SelectSingleNode("Path").InnerText;
                string name = Materials[i].SelectSingleNode("Name").InnerText;

                // Creates a Generic material
                Material mat = new Material(Shader.Find("Standard"));
                mat.name = name;

                B2U_MatPath matpath = new B2U_MatPath(mat, path);
                MatList.Add(matpath);
            }
        }
    }

    // Import and Set Materials -----------------------------
    Material OnAssignMaterialModel(Material material, Renderer renderer) {
        // Find this material
        string path_mat = "";
        string path_mat_xml = "";
        for (int i = 0; i < MatList.Count; i++) {
            B2U_MatPath Mat = MatList[i];
            if (Mat == null) { // Previne erros quando a variável possui lixo ou foi removido pela Garbage Collector
                MatList.RemoveAt(i);
            }
            else {
                if (Mat._Mat.name == material.name) {
                    path_mat = Mat._Path + "/" + material.name + ".mat";
                    path_mat_xml = "Assets/" + B2U_Importer.metaDataPath + "Materials/" + material.name + ".b2mat";
                }
            }
        }
        // Create/Reimport B2U Material
        if (path_mat != "") {
            // Configure the Material based on XML file
            XmlDocument mat_xml = new XmlDocument();
            mat_xml.Load(path_mat_xml);
            XmlNode mat_xml_root = mat_xml.DocumentElement;
            Shader shader_name = Shader.Find(mat_xml_root.SelectSingleNode("Shader").InnerText);

            // If it's a not valid shader use the B2U error material
            if (shader_name == null) {
                Debug.LogWarning("[B2U Log] The material " + path_mat + " has not a valid shader name. The Default Error Material will be used to your convenience");
                Material errorMat = (Material)AssetDatabase.LoadAssetAtPath("Assets/B2U/Editor/UI/Error.mat", typeof(Material));
                return errorMat;
            }


            bool rewrite = ((mat_xml_root.SelectSingleNode("Rewrite").InnerText == "True") ? true : false);
            Material mat;

            bool updateProperties = false;

            // Se tiver um material com esse nome
            if (AssetDatabase.LoadAssetAtPath(path_mat, typeof(Material))) {

                // Se estiver marcado para ser atualizado
                if (rewrite) {
                    // Marca para atualizar
                    updateProperties = true;
                    mat = AssetDatabase.LoadAssetAtPath(path_mat, typeof(Material)) as Material;
                    mat.shader = shader_name;
                }

                // Tem o material, mas não está marcado para atualizar, só usa o mesmo
                else {
                    return AssetDatabase.LoadAssetAtPath(path_mat, typeof(Material)) as Material;
                }

            }

            // O material ainda não existe
            else {
                updateProperties = true;
                material.shader = shader_name;
                AssetDatabase.CreateAsset(material, path_mat);
                mat = material;
            }

            // Atualiza Propriedades
            if (updateProperties) {
                // Reseta as propriedades
                for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++) {
                    string name = ShaderUtil.GetPropertyName(mat.shader, i);
                    if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.Color) {
                        mat.SetColor(name, new Color(1, 1, 1, 1));
                    }
                    if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.Float) {
                        mat.SetFloat(name, 0.0f);
                    }
                    if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.Range) {
                        mat.SetFloat(name, 0.0f);
                    }
                    if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv) {
                        mat.SetTexture(name, null);
                    }
                }

                // Coloca as propriedades configuradas
                XmlNodeList channels = mat_xml_root.SelectSingleNode("Channels").ChildNodes;
                // Para Cada Canal
                for (int i = 0; i < channels.Count; i++) {

                    string name = channels[i].Name;
                    string data = channels[i].InnerText;
                    // Verifica o tipo
                    string[] words = data.Split(',');
                    int wordType = words.Length;

                    // Color Object
                    if (words[0] == "Color") {
                        Color outColor = new Color();
                        outColor.r = float.Parse(words[1]);
                        outColor.g = float.Parse(words[2]);
                        outColor.b = float.Parse(words[3]);
                        outColor.a = float.Parse(words[4]);
                        mat.SetColor(name, outColor);
                    }

                    // Float Object
                    if (words[0] == "Float") {
                        try { mat.SetFloat(name, float.Parse(words[1])); } catch { }
                    }

                    // Bool Object
                    if (words[0] == "Bool") {
                        if (words[1] == "True") {
                            mat.SetFloat(name, 1.0f);
                        }
                        else {
                            mat.SetFloat(name, 0.0f);
                        }
                    }

                    mat.EnableKeyword("_EMISSION");

                    // Key Object
                    if (words[0] == "Key") {
                        if (words[1] == "True") {                          
                            mat.EnableKeyword(name);
                        }
                        else {
                            mat.DisableKeyword(name);
                        }
                    }

                    // Texture Object
                    if (words[0] == "Texture") {
                        string type = words[1];
                        string path = words[2];
                        if (path != null) {
                            // Default importer
                            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
                            if (textureImporter != null) {
                                textureImporter.textureType = TextureImporterType.Default;
                                if (type == "NORMAL") {
                                    textureImporter.textureType = TextureImporterType.NormalMap;
                                }
                                AssetDatabase.ImportAsset(path);
                                Texture tempTex = AssetDatabase.LoadAssetAtPath(path, typeof(Texture)) as Texture;
                                mat.SetTexture(name, tempTex);
                            }
                        }
                    }
                }
            }
            return mat;
        }
        else {
            // Default Importer
            return null;
        }
    }

    // Add Properties and Correct the Object Transform ------
    void OnPostprocessModel(GameObject obj) {
        if (obj.name.Contains(B2U_Importer.prefixPrefab)) {
            obj.transform.rotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            string b2upref = Path.ChangeExtension(assetPath, "b2up");
            string correct_nameprefab = Path.GetFileName(b2upref);
            string prefab_path = "Assets/" + B2U_Importer.metaDataPath + "Prefabs/" + correct_nameprefab;

            XmlDocument doc = new XmlDocument();
            doc.Load(prefab_path);
            XmlNode root = doc.DocumentElement;
            string Tag = root.SelectSingleNode("Tag").InnerText;
            string Layer = root.SelectSingleNode("Layer").InnerText;
            string Static = root.SelectSingleNode("Static").InnerText;
            string _Collider = root.SelectSingleNode("Collider").InnerText;

            // Check if Layer is Valid and Set
            int layeridx = LayerMask.NameToLayer(Layer);
            obj.layer = ((layeridx >= 0) ? layeridx : 0);

            // Check if Tag is Valid and Set
            for (int i = 0; i < UnityEditorInternal.InternalEditorUtility.tags.Length; i++) {
                if (UnityEditorInternal.InternalEditorUtility.tags[i].Contains(Tag)) {
                    obj.tag = Tag;
                    break;
                }
            }

            // Set Object to Static or Dynamic
            obj.isStatic = ((Static == "Static") ? true : false);

            // Set Collider
            switch (_Collider) {
                case "None":
                    break;
                case "Box":
                    obj.AddComponent<BoxCollider>();
                    break;
                case "Sphere":
                    obj.AddComponent<SphereCollider>();
                    break;
                case "Mesh":
                    obj.AddComponent<MeshCollider>();
                    break;
                case "Capsule":
                    obj.AddComponent<CapsuleCollider>();
                    break;
                default:
                    return;
            }
        }
    }

    // Handle all Group to Prefab imports, also will handle MetaData cleanups for models in future
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        foreach (string str in importedAssets) {
            // Auto Import Groups
            if (str.Contains(".b2ug")) {
                B2U_Utils.GroupToPrefab(str);
            }
        }
    }
}

// Utility Functions

public class B2U_Utils {
    // Parse Functions ---------------------------------------
    public static Vector3 parseVector3(string sourceString) {

        string outString;
        Vector3 outVector3;
        string[] splitString = new string[3];

        outString = sourceString.Substring(1, sourceString.Length - 2);
        splitString = outString.Split(","[0]);

        outVector3.x = float.Parse(splitString[0]);
        outVector3.y = float.Parse(splitString[1]);
        outVector3.z = float.Parse(splitString[2]);

        return outVector3;
    }

    public static Quaternion parseQuart(string sourceString) {

        string outString;
        Quaternion outQuart;
        string[] splitString = new string[3];

        outString = sourceString.Substring(1, sourceString.Length - 2);
        splitString = outString.Split(","[0]);

        outQuart.x = float.Parse(splitString[0]);
        outQuart.y = float.Parse(splitString[1]);
        outQuart.z = float.Parse(splitString[2]);
        outQuart.w = float.Parse(splitString[3]);

        return outQuart;
    }

    public static void CreateScene(string path, bool iObject, bool iCamera, bool iLamp) {
        XmlDocument doc = new XmlDocument();
        try {
            doc.Load(path);
        }
        catch { }
        XmlNode root = doc.DocumentElement;

        XmlNode Parameters = root.SelectSingleNode("Parameters");
        // Cria a cena
        string SceneName = Parameters.SelectSingleNode("Name").InnerText;
        B2U_Scene[] ScenePivotList = GameObject.FindObjectsOfType<B2U_Scene>();
        GameObject ScenePivot = null;
        foreach (B2U_Scene scene in ScenePivotList) {
            // So this is the scene to handle
            if (scene.dataPath == path) {
                ScenePivot = scene.gameObject;
            }
        }

        // If there's no pivot with that path
        if (ScenePivot == null) {
            ScenePivot = new GameObject();
            B2U_Scene temp = ScenePivot.AddComponent<B2U_Scene>();
            temp.dataPath = path;
        }

        ScenePivot.name = Parameters.SelectSingleNode("Name").InnerText;

        // Objetos da Cena e seu parentesco
        List<B2U_ParentList> pList = new List<B2U_ParentList>();

        if (iObject) {
            // Cria os Objetos
            XmlNodeList Objects = root.SelectSingleNode("Objects").ChildNodes;
            for (int i = 0; i < Objects.Count; i++) {
                string Name = Objects[i].SelectSingleNode("Name").InnerText;
                string Prefab = Objects[i].SelectSingleNode("Prefab").InnerText;
                string Loc = Objects[i].SelectSingleNode("Position").InnerText;
                string Rot = Objects[i].SelectSingleNode("Rotation").InnerText;
                string Sca = Objects[i].SelectSingleNode("Scale").InnerText;

                // Overwrited per Object Settings		
                string Sta = Objects[i].SelectSingleNode("Static").InnerText;
                string Tag = Objects[i].SelectSingleNode("Tag").InnerText;
                string Layer = Objects[i].SelectSingleNode("Layer").InnerText;

                string ObjName = B2U_Importer.prefixPrefab + Name;
                GameObject Obj = null;
                if (ScenePivot != null) {
                    Transform tempTrans = ScenePivot.transform.Find(ObjName);
                    if (tempTrans != null) {
                        Obj = tempTrans.gameObject;
                    }
                }
                if (Obj == null) {
                    Prefab = Prefab.Replace("/", "\\");
                    Object PrefabObj = AssetDatabase.LoadMainAssetAtPath(Prefab);
                    Obj = UnityEditor.PrefabUtility.InstantiatePrefab(PrefabObj) as GameObject;
                    Obj.name = ObjName;
                }

                B2U_ParentList TempDataObj = new B2U_ParentList();
                TempDataObj.obj = Obj;
                TempDataObj.parentName = Objects[i].SelectSingleNode("Parent").InnerText;
                pList.Add(TempDataObj);
                Obj.transform.parent = null;
                Obj.transform.position = parseVector3(Loc);
                Obj.transform.localScale = parseVector3(Sca);
                Vector3 FRot = parseVector3(Rot);

                Obj.transform.rotation = new Quaternion(); //new Quaternion().EulerRotation(0, 0, 0);

                Obj.transform.Rotate(new Vector3(FRot[0] * -1, 0, 0), Space.World);
                Obj.transform.Rotate(new Vector3(0, FRot[2] * -1, 0), Space.World);
                Obj.transform.Rotate(new Vector3(0, 0, FRot[1] * -1), Space.World);
                Obj.transform.parent = ScenePivot.transform;
                if (Sta != "Keep") {
                    if (Sta == "Static")
                        Obj.isStatic = true;
                    else
                        Obj.isStatic = false;
                }

                // Check if Layer is Valid and Set
                if (Layer != "") {
                    int layeridx = LayerMask.NameToLayer(Layer);
                    Obj.layer = ((layeridx >= 0) ? layeridx : 0);

                    // Check if Tag is Valid and Set
                    if (Tag != "") {
                        for (int j = 0; j < UnityEditorInternal.InternalEditorUtility.tags.Length; j++) {
                            if (UnityEditorInternal.InternalEditorUtility.tags[j].Contains(Tag)) {
                                Obj.tag = Tag;
                                break;
                            }
                        }
                    }
                }
            }
        }

        if (iLamp) {
            // Reconstruct Lamps
            XmlNodeList Lamps = root.SelectSingleNode("Lamps").ChildNodes;
            for (int l = 0; l < Lamps.Count; l++) {
                string NameLamp = Lamps[l].SelectSingleNode("Name").InnerText;
                string LocLamp = Lamps[l].SelectSingleNode("Position").InnerText;
                string RotLamp = Lamps[l].SelectSingleNode("Rotation").InnerText;
                string ColorLamp = Lamps[l].SelectSingleNode("Color").InnerText;
                string PowerLamp = Lamps[l].SelectSingleNode("Power").InnerText;
                string TypeLamp = Lamps[l].SelectSingleNode("Type").InnerText;
                //string DistanceLamp = Lamps[l].SelectSingleNode("Distance").InnerText;

                //string ObjNameLamp = B2U_Importer.prefixPrefab + NameLamp;
                //GameObject ObjLamp = GameObject.Find(ObjNameLamp);

                string ObjNameLamp = B2U_Importer.prefixPrefab + NameLamp;
                GameObject ObjLamp = null;
                if (ScenePivot != null) {
                    Transform tempTrans = ScenePivot.transform.Find(ObjNameLamp);
                    if (tempTrans != null) {
                        ObjLamp = tempTrans.gameObject;
                    }
                }
                if (ObjLamp == null) {
                    ObjLamp = new GameObject();
                    ObjLamp.AddComponent<Light>();
                    ObjLamp.name = ObjNameLamp;
                }

                B2U_ParentList TempDataLamp = new B2U_ParentList();
                TempDataLamp.obj = ObjLamp;
                TempDataLamp.parentName = Lamps[l].SelectSingleNode("Parent").InnerText;
                pList.Add(TempDataLamp);

                ObjLamp.transform.position = parseVector3(LocLamp);
                Vector3 FRotLamp = parseVector3(RotLamp);

                ObjLamp.transform.rotation = Quaternion.identity;
                //Fix Light Default Rotation in Unity 
                ObjLamp.transform.Rotate(new Vector3(90, 0, 0), Space.World);

                ObjLamp.transform.Rotate(new Vector3(FRotLamp[0] * -1, 0, 0), Space.World);
                ObjLamp.transform.Rotate(new Vector3(0, FRotLamp[2] * -1, 0), Space.World);
                ObjLamp.transform.Rotate(new Vector3(0, 0, FRotLamp[1] * -1), Space.World);

                Vector3 Col = parseVector3(ColorLamp);
                ObjLamp.GetComponent<Light>().color = new Color(Col[0], Col[1], Col[2]);
                ObjLamp.GetComponent<Light>().intensity = 2.0f;
                ObjLamp.GetComponent<Light>().range = float.Parse(PowerLamp);

                if (TypeLamp == "POINT") {
                    ObjLamp.GetComponent<Light>().type = LightType.Point;
                }
                if (TypeLamp == "SPOT") {
                    string SpotSize = Lamps[l].SelectSingleNode("SpotSize").InnerText;
                    ObjLamp.GetComponent<Light>().spotAngle = float.Parse(SpotSize);
                    ObjLamp.GetComponent<Light>().type = LightType.Spot;
                }
                if (TypeLamp == "SUN") {
                    ObjLamp.GetComponent<Light>().type = LightType.Directional;
                }

                ObjLamp.transform.parent = ScenePivot.transform;
            }
        }

        if (iCamera) {
            // Reconstruct Cameras
            XmlNodeList Cams = root.SelectSingleNode("Cameras").ChildNodes;
            for (int c = 0; c < Cams.Count; c++) {
                string NameCam = Cams[c].SelectSingleNode("Name").InnerText;
                string LocCam = Cams[c].SelectSingleNode("Position").InnerText;
                string RotCam = Cams[c].SelectSingleNode("Rotation").InnerText;
                string ProjCam = Cams[c].SelectSingleNode("Projection").InnerText;
                string FovCam = Cams[c].SelectSingleNode("Fov").InnerText;
                string NearCam = Cams[c].SelectSingleNode("Near").InnerText;
                string FarCam = Cams[c].SelectSingleNode("Far").InnerText;
                string SizeCam = Cams[c].SelectSingleNode("Size").InnerText;

                string ObjNameCam = B2U_Importer.prefixPrefab + NameCam;
                GameObject ObjCam = null;
                if (ScenePivot != null) {
                    Transform tempTrans = ScenePivot.transform.Find(ObjNameCam);
                    if (tempTrans != null) {
                        ObjCam = tempTrans.gameObject;
                    }
                }
                if (ObjCam == null) {
                    ObjCam = new GameObject();
                    ObjCam.AddComponent<Camera>();
                    ObjCam.name = ObjNameCam;
                }

                B2U_ParentList TempDataCam = new B2U_ParentList();
                TempDataCam.obj = ObjCam;
                TempDataCam.parentName = Cams[c].SelectSingleNode("Parent").InnerText;
                pList.Add(TempDataCam);

                ObjCam.transform.position = parseVector3(LocCam);
                Vector3 FRotCam = parseVector3(RotCam);

                ObjCam.transform.rotation = new Quaternion(); //Quaternion.EulerRotation(0, 0, 0);
                ObjCam.transform.rotation = Quaternion.Euler(90 - FRotCam[0], FRotCam[2] * -1, FRotCam[1] * -1);

                float vFov = Mathf.Atan(Mathf.Tan(Mathf.Deg2Rad * float.Parse(FovCam) / 2) / ObjCam.GetComponent<Camera>().aspect) * 2;
                vFov *= Mathf.Rad2Deg;

                ObjCam.GetComponent<Camera>().fieldOfView = vFov;
                ObjCam.GetComponent<Camera>().nearClipPlane = float.Parse(NearCam);
                ObjCam.GetComponent<Camera>().farClipPlane = float.Parse(FarCam);
                ObjCam.GetComponent<Camera>().orthographicSize = float.Parse(SizeCam) * 0.28f;

                if (ProjCam == "PERSP") {
                    ObjCam.GetComponent<Camera>().orthographic = false;
                }
                else {
                    ObjCam.GetComponent<Camera>().orthographic = true;
                }

                ObjCam.transform.parent = ScenePivot.transform;
            }
        }

        // Configura Parents
        for (int k = 0; k < pList.Count; k++) {
            B2U_ParentList Data = pList[k];
            GameObject ObjSource = Data.obj;
            string ObjDest = Data.parentName;

            if (ObjDest != "None") {
                GameObject FatherObj = GameObject.Find(B2U_Importer.prefixPrefab + ObjDest);
                ObjSource.transform.parent = FatherObj.transform;
            }
        }
    }

    public static void GroupToPrefab(string path) {
        XmlDocument doc = new XmlDocument();
        try {
            doc.Load(path);
        }
        catch { }

        GameObject tempPivot = new GameObject();
        List<B2U_ParentList> pList = new List<B2U_ParentList>();

        XmlNode root = doc.DocumentElement;
        XmlNodeList Objects = root.SelectNodes("Object");
        string savePrefab = root.SelectSingleNode("Path").InnerText;
        string prefabPath = Path.ChangeExtension(savePrefab, "prefab");
        foreach (XmlNode Obj in Objects) {
            string prefab_path = Obj.SelectSingleNode("Prefab").InnerText;
            string loc = Obj.SelectSingleNode("Position").InnerText;
            string rot = Obj.SelectSingleNode("Rotation").InnerText;
            string sca = Obj.SelectSingleNode("Scale").InnerText;
            string name = Obj.SelectSingleNode("Name").InnerText;
            GameObject element = AssetDatabase.LoadAssetAtPath<GameObject>(prefab_path);
            GameObject objTemp = GameObject.Instantiate(element);
            objTemp.name = name;
            objTemp.transform.parent = tempPivot.transform;

            objTemp.transform.position = parseVector3(loc);
            objTemp.transform.localScale = parseVector3(sca);
            Vector3 FRot = parseVector3(rot);

            objTemp.transform.rotation = new Quaternion();
            objTemp.transform.Rotate(new Vector3(FRot[0] * -1, 0, 0), Space.World);
            objTemp.transform.Rotate(new Vector3(0, FRot[2] * -1, 0), Space.World);
            objTemp.transform.Rotate(new Vector3(0, 0, FRot[1] * -1), Space.World);

            B2U_ParentList TempDataObj = new B2U_ParentList();
            TempDataObj.obj = objTemp;
            TempDataObj.parentName = Obj.SelectSingleNode("Parent").InnerText;
            pList.Add(TempDataObj);
        }

        // Configure Parents
        for (int k = 0; k < pList.Count; k++) {
            B2U_ParentList Data = pList[k];
            GameObject ObjSource = Data.obj;
            string dest = Data.parentName;
            List<GameObject> staticList = new List<GameObject>();

            foreach(B2U_ParentList obj in pList) {
                if (Data.parentName == obj.obj.name) {
                    ObjSource.transform.parent = obj.obj.transform;
                }
            }
        }
        

        // Save in Project
        GameObject oldPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (oldPrefab != null) {
            PrefabUtility.ReplacePrefab(tempPivot, oldPrefab, ReplacePrefabOptions.ReplaceNameBased);
        }
        else {
            PrefabUtility.CreatePrefab(prefabPath, tempPivot, ReplacePrefabOptions.ConnectToPrefab);
        }
        GameObject.DestroyImmediate(tempPivot);
    }

}

public class B2U_MatPath {
    public Material _Mat;
    public string _Path;

    public B2U_MatPath(Material _mat, string _path) {
        _Mat = _mat;
        _Path = _path;
    }
}

public class B2U_ParentList {
    public GameObject obj;
    public string parentName;
}

