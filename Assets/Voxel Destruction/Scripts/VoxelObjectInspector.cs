using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VoxReader.Interfaces;
using Random = UnityEngine.Random;

namespace VoxelDestruction
{
    #if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(VoxelObject))]
    public class VoxelObjectInspector : Editor
    {
        private SerializedProperty path;
        private SerializedProperty buildObject;
        
        private SerializedProperty exportMesh;
        private SerializedProperty startWithPhysics;
        private SerializedProperty createCollider;
        private SerializedProperty useConvexCollider;
        private SerializedProperty addVoxelCollider;
        private SerializedProperty collisionScale;
        private SerializedProperty material;
        public SerializedProperty buildEntire;
        public SerializedProperty buildModel;
        
        private SerializedProperty objectScale;
        private SerializedProperty loadingTime;
        private SerializedProperty delayRecalculation;
        private SerializedProperty isResetable;
        private SerializedProperty use32BitInt;
        private SerializedProperty generationType;
        private SerializedProperty allocator;
        public SerializedProperty useParallel;

        private SerializedProperty destructible;
        private SerializedProperty destructionType;
        private SerializedProperty relativeSize;
        private SerializedProperty particle;
        private SerializedProperty destructionGS;
        private SerializedProperty destructionSS;
        private SerializedProperty destructionFNRS;
        private SerializedProperty destructionFRS;
        private SerializedProperty destructionPFS;

        private SerializedProperty totalMass;
        private SerializedProperty totalDrag;
        private SerializedProperty totalAngularDrag;
        private SerializedProperty contraints;
        private SerializedProperty interpolation;
        private SerializedProperty collisionMode;

        private SerializedProperty debug;
        
        private bool generalSettings;
        private bool performanceSettings;
        private bool physicsSettings;
        
        void OnEnable()
        {
            loading = false;
            VoxelObject script = (VoxelObject) target;
            if (script.transform.Find("EDITOR_DEMO_MESH"))
            {
                Transform p = script.transform.Find("EDITOR_DEMO_MESH");
                childs = new MeshFilter[p.childCount];

                try
                {
                    for (int i = 0; i < childs.Length; i++)
                    {
                        childs[i] = p.GetChild(i).GetComponent<MeshFilter>();
                    }
                }
                catch (Exception)
                {
                    Debug.LogWarning("Error loading demo Mesh! Make sure to not add any childs to the EDITOR_DEMO_MESH Child!");
                    
                    if (p.childCount > 0)
                    {
                        for (int i = p.childCount - 1; i <= 0; i--)
                        {
                            DestroyImmediate(p.GetChild(i).gameObject);
                        }
                    }

                    DestroyImmediate(p);
                }
            }
            
            path = serializedObject.FindProperty("path");
            buildObject = serializedObject.FindProperty("buildObject");
            
            exportMesh = serializedObject.FindProperty("exportMesh");
            startWithPhysics = serializedObject.FindProperty("startWithPhysics");
            addVoxelCollider = serializedObject.FindProperty("addVoxelCollider");
            collisionScale = serializedObject.FindProperty("collisionScale");
            material = serializedObject.FindProperty("material");
            createCollider = serializedObject.FindProperty("createCollider");
            buildEntire = serializedObject.FindProperty("buildEntire");
            buildModel = serializedObject.FindProperty("buildModel");
            useConvexCollider = serializedObject.FindProperty("useConvexCollider");
            
            objectScale = serializedObject.FindProperty("objectScale");
            loadingTime = serializedObject.FindProperty("loadingTime");
            delayRecalculation = serializedObject.FindProperty("delayRecalculation");
            isResetable = serializedObject.FindProperty("isResetable");
            use32BitInt = serializedObject.FindProperty("use32BitInt");
            generationType = serializedObject.FindProperty("generationType");
            allocator = serializedObject.FindProperty("allocator");
            useParallel = serializedObject.FindProperty("scheduleParallel");
            
            destructible = serializedObject.FindProperty("destructible");
            
            destructionType = serializedObject.FindProperty("destructionType");
            relativeSize = serializedObject.FindProperty("relativeSize");
            particle = serializedObject.FindProperty("particle");
            destructionGS = serializedObject.FindProperty("destructionGS");
            destructionSS = serializedObject.FindProperty("destructionSS");
            destructionFNRS = serializedObject.FindProperty("destructionFNRS");
            destructionFRS = serializedObject.FindProperty("destructionFRS");
            destructionPFS = serializedObject.FindProperty("destructionPFS");
            
            totalMass = serializedObject.FindProperty("totalMass");
            totalDrag = serializedObject.FindProperty("drag");
            totalAngularDrag = serializedObject.FindProperty("angularDrag");

            contraints = serializedObject.FindProperty("constraints");
            interpolation = serializedObject.FindProperty("interpolation");
            collisionMode = serializedObject.FindProperty("collisionMode");

            debug = serializedObject.FindProperty("debug");
            
            generalSettings = true;
            performanceSettings = true;
            physicsSettings = true;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(path);
            EditorGUILayout.LabelField("Object Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(buildObject);

            if (buildObject.boolValue)
            {
                if (GUILayout.Button("Build Mesh"))
                {
                    if (childs == null)
                        childs = new MeshFilter[0];

                    UpdateMesh();
                }

                if (GUILayout.Button("Clear Mesh"))
                {
                    GameObject par = null;
                    if (childs.Length > 0)
                    {
                        for (int i = 0; i < childs.Length; i++)
                        {
                            if (childs[i] != null)
                            {
                                par = childs[i].transform.parent.gameObject;
                                DestroyImmediate(childs[i].gameObject);
                            }
                        }
                    }
                    
                    if (par != null)
                        DestroyImmediate(par);
                }
                
                EditorGUILayout.Space();
                generalSettings = EditorGUILayout.Foldout(generalSettings, "General", true);

                if (generalSettings)
                {
                    EditorGUILayout.PropertyField(material, new GUIContent("Material" ,"The Mesh Material, make sure it supports Mesh Colors"));
                    EditorGUILayout.PropertyField(exportMesh, new GUIContent( "Export mesh","If you want to export the Mesh as an Asset within the Unity Editor"));
                    EditorGUILayout.PropertyField(startWithPhysics, new GUIContent("Start with physics", "Adds a rigidbody to the mesh when it is loaded for the first time"));
                    if (!startWithPhysics.boolValue)
                    {
                        EditorGUILayout.PropertyField(createCollider,
                            new GUIContent("Create collider", "Defines if you want the Mesh to have a mesh Collider"));
                        if (createCollider.boolValue)
                        {
                            EditorGUILayout.PropertyField(useConvexCollider,
                                new GUIContent("Use convex Collider", "Creates a convex Mesh Collider"));
                        }   
                    }
                    EditorGUILayout.PropertyField(addVoxelCollider, new GUIContent("Voxel collider", "Adds a Voxel Collider to the Object, which allows it to collider with other Voxel Objects"));
                    EditorGUILayout.PropertyField(buildEntire,
                        new GUIContent("Build all childs", "Defines if all models are build or only one defined model"));
                    if (!buildEntire.boolValue)
                        EditorGUILayout.PropertyField(buildModel,
                            new GUIContent("Build model", "The Index of the model that should be build"));
                    
                    if (addVoxelCollider.boolValue)
                    {
                        EditorGUILayout.PropertyField(collisionScale, new GUIContent("Collision scale", "The Collision Scale of the Voxel Collider"));
                        collisionScale.floatValue = Mathf.Clamp(collisionScale.floatValue, 0f, 100000f);
                    }
                }

                EditorGUILayout.Space();
                performanceSettings = EditorGUILayout.Foldout(performanceSettings, "Performance", true);

                if (performanceSettings)
                {
                    EditorGUILayout.PropertyField(objectScale, new GUIContent("Voxel scale", "Defines the scale of the Voxels"));
                    objectScale.floatValue = Mathf.Clamp(objectScale.floatValue, 0.001f, 10000f);
                    EditorGUILayout.PropertyField(loadingTime, new GUIContent("Max loading time", "The maximum loading time in frames when building the Mesh"));
                    loadingTime.intValue = (int)Mathf.Clamp(loadingTime.intValue, 0f, 100f);
                    EditorGUILayout.PropertyField(isResetable,
                        new GUIContent("Is Resetable",
                            "Defines if the voxel object can be reseted by calling the ResetModel() Methode"));
                    EditorGUILayout.PropertyField(use32BitInt, new GUIContent("Use 32 Bit mesh", "Uses a 32 Bit Integer for the Mesh Building, bad performance but allows bigger Meshes"));
                    EditorGUILayout.PropertyField(delayRecalculation,
                        new GUIContent("Delay recalculation",
                            "Delays the Mesh recalculation for some frames to save perfromance"));
                    EditorGUILayout.PropertyField(allocator, new GUIContent("Allocator", "Allocator used for Mesh Job, use Persistant or TempJob"));
                    EditorGUILayout.PropertyField(generationType, new GUIContent("Generation type", "Choose between a Fast, Safe and Normal Generation Type (Fast = IJobParallelFor, Safe = IJob, Normal = IJobFor)"));
                    if (generationType.enumValueIndex == 0 || generationType.enumValueIndex == 2)
                    {
                        EditorGUILayout.PropertyField(useParallel,
                            new GUIContent("Schedule parallel", "Runs the Mesh Job in parallel on multiple threads"));
                    }
                }

                EditorGUILayout.Space();

                physicsSettings = EditorGUILayout.Foldout(physicsSettings, "Physics", true);

                if (physicsSettings)
                {
                    EditorGUILayout.PropertyField(totalMass, new GUIContent("Total mass","The Total Mass of the Voxel Objects"));
                    totalMass.floatValue = Mathf.Clamp(totalMass.floatValue, 0.001f, 10000000f);
                    EditorGUILayout.PropertyField(totalDrag, new GUIContent("Drag", "The drag of the object and fragments"));
                    totalDrag.floatValue = Mathf.Clamp(totalDrag.floatValue, 0f, 10000000f);
                    EditorGUILayout.PropertyField(totalAngularDrag, new GUIContent("Angular drag", "The angular drag of the object and fragments"));
                    totalAngularDrag.floatValue = Mathf.Clamp(totalAngularDrag.floatValue, 0f, 10000000f);
                    
                    EditorGUILayout.PropertyField(contraints, new GUIContent("Constraints", "The Rigidbody constraints of the object and fragments"));
                    EditorGUILayout.PropertyField(interpolation, new GUIContent("Interpolation", "The Rigidbody interpolation of the object and fragments"));
                    EditorGUILayout.PropertyField(collisionMode, new GUIContent("Collision mode", "The Rigidbody collision mode of the object and fragments"));   
                }

                EditorGUILayout.Space();
                
                EditorGUILayout.PropertyField(destructible);
                
                if (destructible.boolValue)
                {
                    EditorGUILayout.PropertyField(destructionType, new GUIContent("Destruction type"));

                    EditorGUILayout.PropertyField(destructionGS, new GUIContent("Destruction settings"));
                    EditorGUILayout.PropertyField(destructionSS, new GUIContent("Sound settings"));
                    
                    if (destructionType.enumValueIndex != (int)VoxelObject.DestructionType.PreCalculatedFragments)
                    {
                        if (destructionType.enumValueIndex == (int)VoxelObject.DestructionType.FragmentationRemoval)
                        {
                            EditorGUILayout.PropertyField(relativeSize,
                                new GUIContent("Relative size", "Calculates the fragment size from the collision force"));

                            if (relativeSize.boolValue)
                                EditorGUILayout.PropertyField(destructionFRS, new GUIContent("Relative settings"));
                            else
                                EditorGUILayout.PropertyField(destructionFNRS, new GUIContent("Fragment settings"));
                        }
                        else
                        {
                            EditorGUILayout.PropertyField(particle, new GUIContent("Particle", "Particle spawned when destructing, note that it needs to have the ParticleSystem as the first child"));
                        }
                    }
                    else
                    {
                        EditorGUILayout.HelpBox("Make sure to recalculate Fragments after making changes to your model!", MessageType.Info);
                        
                        if (GUILayout.Button("Calculate Fragment"))
                        {
                            CalculateFragment();
                        }
                        
                        if (GUILayout.Button("Delete Fragments"))
                        {
                            DeleteFragments();
                        }

                        EditorGUILayout.PropertyField(destructionPFS, new GUIContent("Precalculation settings"));
                    }
                }
                
                EditorGUILayout.Space();

                EditorGUILayout.PropertyField(debug);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        public void CalculateFragment()
        {
            if (loading || Application.isPlaying)
                return;
            
            VoxelObject script = (VoxelObject) target;
            if (script.gameObject.activeInHierarchy && path.stringValue != "")
            {
                loading = true;

                if (File.Exists(Path.Combine(Application.streamingAssetsPath, path.stringValue)))
                {
                    IVoxFile file = VoxReader.VoxReader.Read(Path.Combine(Application.streamingAssetsPath, path.stringValue), false);

                    for (int i = 0; i < file.Models.Length; i++)
                    {
                        float progress = 0.0f;
                        float minProgress = i / (float)file.Models.Length;
                        float maxProgress = (i + 1) / (float)file.Models.Length;
                        float progressIncrement = Mathf.Clamp01(file.Models[i].Size.magnitude * 0.0000069f);
                        
                        EditorUtility.DisplayProgressBar("Calculting Fragments", $"Creating Fragments for Voxel Model {i}...", minProgress);
                        string targetPath = Path.Combine(Application.streamingAssetsPath, path.stringValue.Replace(".vox", "_" + i.ToString() + ".txt"));

                        if (File.Exists(targetPath))
                            File.Delete(targetPath);

                        VoxelData data = VoxToVoxelData.GenerateVoxelData(file.Models[i]);
                        
                        List<int[]> fragments = new List<int[]>();
                        List<int> currentFrag = new List<int>();
                        VoxelData tempData;

                        tempData.Size = data.Size;
                        tempData.Blocks = new Voxel[data.Blocks.Length];
                        for (int j = 0; j < data.Blocks.Length; j++)
                        {
                            tempData.Blocks[j] = new Voxel(data.Blocks[j]);
                        }

                        float fragMin = destructionPFS.FindPropertyRelative("fragSphereRadiusMin").floatValue;
                        float fragMax = destructionPFS.FindPropertyRelative("fragSphereRadiusMax").floatValue;

                        progress += 0.1f;
                        EditorUtility.DisplayProgressBar("Calculting Fragments", $"Creating Fragments for Voxel Model {i}...", Mathf.Lerp(minProgress, maxProgress, progress));
                        int so = 0;
                        int current = GetNext(tempData, data.Size);
                        while (current != -1 && so < data.Blocks.Length)
                        {
                            //Frag
                            SimpleSphere fragmentSphere = new SimpleSphere(To3D(current, data.Size.x, data.Size.y), 
                                 Random.Range(
                                     fragMin,
                                     fragMax));
                
                            for (int j = 0; j < tempData.Blocks.Length; j++)
                            {
                                if (tempData.Blocks[j].active && fragmentSphere.IsInsideSphere(To3D(j, data.Size.x, data.Size.y)))
                                {
                                    currentFrag.Add(j);
                                    tempData.Blocks[j] = new Voxel(Color.black, false);
                                }
                            }
                
                            fragments.Add(currentFrag.ToArray());
                            currentFrag.Clear();
                
                            current = GetNext(tempData, data.Size);
                            so++;

                            if (progress < 0.8f)
                                progress += progressIncrement;
                            EditorUtility.DisplayProgressBar("Calculting Fragments", $"Creating Fragments for Voxel Model {i}...", Mathf.Lerp(minProgress, maxProgress, progress));
                        }

                        progress = 0.9f;
                        EditorUtility.DisplayProgressBar("Calculting Fragments", $"Creating Fragments for Voxel Model {i}...", Mathf.Lerp(minProgress, maxProgress, progress));
                        
                        if (so >= data.Blocks.Length)
                            Debug.LogWarning("Calculating fragments caused Stackoverflow, make sure values are correct!", this);
                        else
                        {
                            string output = "";

                            for (int j = 0; j < fragments.Count; j++)
                            {
                                for (int k = 0; k < fragments[j].Length; k++)
                                {
                                    if (k == 0)
                                        output += fragments[j][k].ToString();
                                    else
                                        output += "," + fragments[j][k].ToString();
                                }

                                if (j < fragments.Count - 1)
                                    output += ";";
                            }
                            
                            File.WriteAllText(targetPath, output);
                            Debug.Log("Successfully created Fragment file on path: " + targetPath);
                        }
                    }
                    
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    Debug.LogWarning("Error creating Voxel Fragments: Vox File does not exist, make sure that path is correct");
                }
            }
              
            loading = false;
        }
        
        public void DeleteFragments()
        {
            if (loading || Application.isPlaying)
                return;
            
            VoxelObject script = (VoxelObject) target;
            if (script.gameObject.activeInHierarchy && path.stringValue != "")
            {
                loading = true;
                
                if (File.Exists(Path.Combine(Application.streamingAssetsPath, path.stringValue)))
                {
                    IVoxFile file = VoxReader.VoxReader.Read(Path.Combine(Application.streamingAssetsPath, path.stringValue), false);

                    for (int i = 0; i < file.Models.Length; i++)
                    {
                        string targetPath = Path.Combine(Application.streamingAssetsPath, path.stringValue.Replace(".vox", "_" + i.ToString() + ".txt"));

                        if (File.Exists(targetPath))
                        {
                            File.Delete(targetPath);
                            
                            if (File.Exists(targetPath + ".meta"))
                                File.Delete(targetPath + ".meta");
                        }
                    }
                    
                    Debug.Log("Successfully deleted Fragment file");
                }

                loading = false;
            }
        }
        
        private int GetNext (VoxelData temp, Vector3Int length)
        {
            Vector3Int nearest = new Vector3Int(-1, -1, -1);
            for (int i = 0; i < temp.Blocks.Length; i++)
            {
                if (!temp.Blocks[i].active)
                    continue;

                if (nearest.x == -1)
                {
                    nearest = Vector3Int.RoundToInt(To3D(i, length.x, length.y));
                    continue;
                }

                if (Vector3.Min(nearest, To3D(i, length.x, length.y)) != nearest)
                    nearest = Vector3Int.RoundToInt(To3D(i, length.x, length.y));
            }

            if (nearest.x == -1)
                return -1;
            
            return To1D(nearest, length);
        }
        
        //Index Stuff
        private Vector3 To3D(long index, int xMax, int yMax)
        {
            int z = (int)index / (xMax * yMax);
            int idx = (int)index - (z * xMax * yMax);
            int y = idx / xMax;
            int x = idx % xMax;
            return new Vector3(x, y, z);
        }
        
        private bool loading;
        private MeshFilter[] childs;
        
        private void UpdateMesh()
        {
            if (loading || Application.isPlaying)
                return;
            
            VoxelObject script = (VoxelObject) target;
            if (script.gameObject.activeInHierarchy && path.stringValue != "")
            {
                Debug.Log("Updating Mesh!");

                loading = true;
                
                if (File.Exists(Path.Combine(Application.streamingAssetsPath, path.stringValue)))
                {
                    try
                    {
                        IVoxFile file = VoxReader.VoxReader.Read(Path.Combine(Application.streamingAssetsPath, path.stringValue), false);

                        GameObject par = null;
                        if (childs.Length > 0)
                        {
                            for (int i = 0; i < childs.Length; i++)
                            {
                                if (childs[i] != null)
                                {
                                    par = childs[i].transform.parent.gameObject;
                                    DestroyImmediate(childs[i].gameObject);
                                }
                            }
                        }
                        
                        if (par != null)
                            DestroyImmediate(par);

                        GameObject childParent = new GameObject("EDITOR_DEMO_MESH");
                        childParent.transform.parent = script.transform;
                        childParent.transform.localPosition = new Vector3(0f, 0f,0f);
                        childParent.transform.localRotation = Quaternion.identity;
                        childParent.transform.localScale = Vector3.one;
                        childParent.tag = "EditorOnly";

                        if (buildEntire.boolValue)
                        {
                            childs = new MeshFilter[file.Models.Length];
                            for (int i = 0; i < file.Models.Length; i++)
                            {
                                GameObject modelChild = new GameObject("Editor VoxelModel " + i);
                                modelChild.transform.parent = childParent.transform;
                                modelChild.transform.localPosition = new Vector3(-0.5f, 0.5f,0.5f);
                                modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
                                modelChild.transform.localScale = Vector3.one;
                                modelChild.tag = "EditorOnly";
                                childs[i] = modelChild.AddComponent<MeshFilter>();
                                
                                modelChild.AddComponent<MeshRenderer>();
                                modelChild.GetComponent<MeshRenderer>().material = material.objectReferenceValue as Material;
                                modelChild.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;

                                script.transform.localScale = new Vector3(objectScale.floatValue, objectScale.floatValue,
                                    objectScale.floatValue);

                                Vector3Int length = Vector3Int.RoundToInt(file.Models[i].Size);
                                Voxel[,,] voxels = new Voxel[length.x, length.y, length.z];
                                for (int x = 0; x < length.x; x++)
                                {
                                    for (int y = 0; y < length.y; y++)
                                    {
                                        for (int z = 0; z < length.z; z++)
                                        {
                                            voxels[x, y, z] = new Voxel(Color.black, false);
                                        }
                                    }
                                }
                                for (int f = 0; f < file.Models[i].Voxels.Length; f++)
                                {
                                    try
                                    {
                                        voxels[(int)file.Models[i].Voxels[f].Position.x, (int)file.Models[i].Voxels[f].Position.y, (int)file.Models[i].Voxels[f].Position.z] =
                                            new Voxel(file.Models[i].Voxels[f].Color, true);
                                    }
                                    catch (Exception e)
                                    {
                                        Debug.LogWarning($"Error sorting Voxel Array! \n Voxel: {f}, Array Position: {(int)file.Models[i].Voxels[f].Position.x}/{(int)file.Models[i].Voxels[f].Position.y}/{(int)file.Models[i].Voxels[f].Position.z}" + e.Message + e.StackTrace, this);
                                        return;
                                    }
                                }
                                Voxel[] final = new Voxel[length.x * length.y * length.z];
                                for (int x = 0; x < length.x; x++)
                                {
                                    for (int y = 0; y < length.y; y++)
                                    {
                                        for (int z = 0; z < length.z; z++)
                                        {
                                            final[To1D(new Vector3(x, y, z), length)] = voxels[x, y, z];
                                        }
                                    }
                                }

                                VoxelData data = new VoxelData(length, final);
                                MeshBuilderSafe builder = new MeshBuilderSafe();
                                builder.StartMeshDrawing(data, Allocator.Persistent);
                                
                                Mesh mesh = builder.GetVoxelObject(use32BitInt.boolValue);
                    
                                builder.Dispose();
                    
                                childs[i].mesh = mesh;
                            }
                        }
                        else if (buildModel.intValue < file.Models.Length)
                        {
                            childs = new MeshFilter[1];

                            int i = buildModel.intValue;

                            GameObject modelChild = new GameObject("Editor VoxelModel " + i);
                            modelChild.transform.parent = childParent.transform;
                            modelChild.transform.localPosition = new Vector3(-0.5f, 0.5f,0.5f);
                            modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0); modelChild.transform.localScale = Vector3.one;
                            modelChild.tag = "EditorOnly";
                            childs[0] = modelChild.AddComponent<MeshFilter>();
                            
                            modelChild.AddComponent<MeshRenderer>();
                            modelChild.GetComponent<MeshRenderer>().material = material.objectReferenceValue as Material;
                            modelChild.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;

                            script.transform.localScale = new Vector3(objectScale.floatValue, objectScale.floatValue,
                                objectScale.floatValue);

                            Vector3Int length = Vector3Int.RoundToInt(file.Models[i].Size);
                            Voxel[,,] voxels = new Voxel[length.x, length.y, length.z];
                            for (int x = 0; x < length.x; x++)
                            {
                                for (int y = 0; y < length.y; y++)
                                {
                                    for (int z = 0; z < length.z; z++)
                                    {
                                        voxels[x, y, z] = new Voxel(Color.black, false);
                                    }
                                }
                            }
                            for (int f = 0; f < file.Models[i].Voxels.Length; f++)
                            {
                                try
                                {
                                    voxels[(int)file.Models[i].Voxels[f].Position.x, (int)file.Models[i].Voxels[f].Position.y, (int)file.Models[i].Voxels[f].Position.z] =
                                        new Voxel(file.Models[i].Voxels[f].Color, true);
                                }
                                catch (Exception e)
                                {
                                    Debug.LogWarning($"Error sorting Voxel Array! \n Voxel: {f}, Array Position: {(int)file.Models[i].Voxels[f].Position.x}/{(int)file.Models[i].Voxels[f].Position.y}/{(int)file.Models[i].Voxels[f].Position.z}" + e.Message + e.StackTrace, this);
                                    return;
                                }
                            }
                            Voxel[] final = new Voxel[length.x * length.y * length.z];
                            for (int x = 0; x < length.x; x++)
                            {
                                for (int y = 0; y < length.y; y++)
                                {
                                    for (int z = 0; z < length.z; z++)
                                    {
                                        final[To1D(new Vector3(x, y, z), length)] = voxels[x, y, z];
                                    }
                                }
                            }

                            VoxelData data = new VoxelData(length, final);
                            MeshBuilderSafe builder = new MeshBuilderSafe();
                            builder.StartMeshDrawing(data, Allocator.Persistent);
                            
                            Mesh mesh = builder.GetVoxelObject(use32BitInt.boolValue);
                
                            builder.Dispose();
                
                            childs[0].mesh = mesh;
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning("Error building Mesh: " + e.Message + "\n" + e.StackTrace);
                    }
                }
                else
                {
                    Debug.LogWarning("Error creating Voxel Object: Vox File does not exist, make sure that path is correct");
                }
            }

            loading = false;
        }

        private int To1D(Vector3 index, Vector3Int length)
        {
            return (int)(index.x + length.x * (index.y + length.y * index.z));
        }
    }
#endif
}