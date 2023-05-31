using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VoxReader.Interfaces;
using static BetterStreamingAssets;
using Random = UnityEngine.Random;

namespace VoxelDestruction
{
    public class VoxelObject : MonoBehaviour
    {
        #region Variables

        public string path;
        
        public bool buildObject = true;
        public bool exportMesh = false;
        public bool startWithPhysics = false;
        public bool createCollider = true;
        public bool useConvexCollider = false;
        public bool addVoxelCollider = false;
        public float collisionScale = 1;
        public bool buildEntire = true;
        public int buildModel = 0;

        //[Space]
        public float objectScale = 1;
        public int loadingTime = 1;
        public bool delayRecalculation = true;
        public bool isResetable = false;
        public bool use32BitInt = false;
        public GenerationType generationType = GenerationType.Safe;
        public enum GenerationType
        {
            Normal, Safe, Fast
        }

        public bool scheduleParallel = false;

        public Allocator allocator = Allocator.Persistent;

        //[Space]

        public float totalMass = 10;
        public float drag = 0;
        public float angularDrag = 0.05f;

        public RigidbodyConstraints constraints = RigidbodyConstraints.None;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
        public CollisionDetectionMode collisionMode = CollisionDetectionMode.Discrete;
        
        //[Space] 
        
        public bool destructible = false;
        
        //Destruction Settings
        
        public DestructionType destructionType = DestructionType.PreCalculatedFragments;
        public enum DestructionType
        {
            //Abbreviations: PVR, AVR, FR and PF
            PassiveVoxelRemoval, ActiveVoxelRemoval, FragmentationRemoval, PreCalculatedFragments
        }
        
        //only FR
        public bool relativeSize = false;


        //Only PVR and AVR
        public GameObject particle = null;

        //PVR, AVR, FR and PF
        public DestructionGeneralSettings destructionGS;
        //PVR, AVR, FR and PF
        public DestructionSoundSettings destructionSS;
        
        //FR not relative
        public DestructionFragmentNRSettings destructionFNRS;
        //FR relative
        public DestructionFragmentRSettings destructionFRS;
        //PF
        public DestructionPreFragmentSettings destructionPFS;

        //[Space]
        
        public bool debug;
        private MeshFilter meshFiler;
        public Material material;

        private Transform fragmentParent;
        
        private VoxelData data;
        private Vector3Int length;
        private VoxelData dataCopy;

        private bool activeRb;
        
        private int4 arrayLength;

        private List<GameObject> activateOnRecalculation;

        private int destructionCount;
        private Coroutine recalculationI;

        private FragmentBuilder fragmentBuilder;
        private Transform[] fragmentsT;
        private DestructionGoal _destructionGoal;
        private CameraShake _cameraShake;
        #endregion

        #region ObjectCreation

#if UNITY_EDITOR

        private void Awake()
                {
                    if (transform.Find("EDITOR_DEMO_MESH"))
                    {
                        DestroyImmediate(transform.Find("EDITOR_DEMO_MESH").gameObject);
                    }
                    
                    NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
                }
                    
        #endif
        
        private void Start()
        {
            _destructionGoal = FindObjectOfType<DestructionGoal>();
            _cameraShake = FindObjectOfType<CameraShake>();
            if (buildObject)
            {
                destructionCount = 0;
                buildObject = false;
                string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, path);
                BetterStreamingAssets.Initialize();

                BetterStreamingAssets.ReadInfo r = new BetterStreamingAssets.ReadInfo();
#if !UNITY_EDITOR
                if (BetterStreamingAssets.LooseFilesImpl.TryGetInfoWeb(path, out r))
                {
                    print("BETTER STREAMIN ASSETS FILE EXIST MONOBEH");
                    if (debug)
                        Debug.Log("Reading Model...", this);
                    //IVoxFile file = VoxReader.VoxReader.Read(filePath, true);
                    StartCoroutine(GetBinaryData(filePath));
                    print("READING MODEL DONE");
                    //if (r.bytes.Length > 0)
                    //{
                    //    print("DATA READEN");

                    //}
                }
                else if (BetterStreamingAssets.FileExists(filePath))
                {
                    print("A TAK YA NASHEL CHETA " + filePath);
                }
                else
                {
                    // print("FILE NOT EXIST");
                    print("File with path " + path + " not found! \n Checked path: " + path);
                }
#endif

                if (BetterStreamingAssets.FileExists(path))
                {
                    print("BETTER STREAMIN ASSETS FILE EXIST MONOBEH");
                    if (debug)
                        Debug.Log("Reading Model...", this);
                    print("READING MODEL");
                    IVoxFile file = VoxReader.VoxReader.Read(path, true);

                    DrawVoxelFile(file);
                }
                else if (BetterStreamingAssets.FileExists(filePath))
                {
                    print("A TAK YA NASHEL CHETA " + filePath);
                }
                else
                {
                    // print("FILE NOT EXIST");
                    print("File with path " + path + " not found! \n Checked path: " + path);
                }
            }
        }
        private IEnumerator GetBinaryData(string filePath)
        {
            print("START LOADED BINARY DATA ROUTINE");
            WWW www = new WWW(filePath);
            //result = www.text;
            yield return www;
            //info.readPath = filePath;
            //info.bytes = www.bytes;
            if(www.bytes.Length > 0)
            {
                print("BYTES DATA NOT EMPTY");
            }
            IVoxFile file = VoxReader.VoxReader.Read(www.bytes);
            DrawVoxelFile(file);
            Debug.Log("COROUTINE IS DONE DATA LOADED");
        }
        private void DrawVoxelFile(IVoxFile file)
        {
            print("START DRAW VOXEL FILE");
            if (buildEntire)
            {
                for (int i = 0; i < file.Models.Length; i++)
                {
                    if (debug)
                        Debug.Log("Drawing Model " + i + "...", this);
                    
                    if (i == 0)
                    {
                        GameObject modelChild = new GameObject("VoxelModel " + i);
                        modelChild.transform.parent = transform;
                        modelChild.transform.localPosition = new Vector3(-0.5f, 0.5f,0.5f);
                        modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
                        modelChild.transform.localScale = Vector3.one;
                        meshFiler = modelChild.AddComponent<MeshFilter>();

                        modelChild.AddComponent<MeshRenderer>();
                        modelChild.GetComponent<MeshRenderer>().material = material;
                        modelChild.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;

                        transform.localScale = new Vector3(objectScale, objectScale, objectScale);
                        
                        DrawVoxelModel(file.Models[i], i);
                    }
                    else
                    {
                        GameObject newParent = new GameObject(gameObject.name + " " + (i + 1).ToString());
                        newParent.transform.position = transform.position;
                        newParent.transform.rotation = transform.rotation;
                        newParent.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
                        newParent.transform.parent = transform.parent;

                        VoxelObject other = newParent.AddComponent<VoxelObject>();
                        other.buildObject = false;
                        other.debug = debug;
                        other.material = material;
                        
                        other.destructible = destructible;

                        other.destructionGS = destructionGS;
                        other.destructionCount = destructionCount;
                        other.destructionSS = destructionSS;
                        other.destructionPFS = destructionPFS;
                        other.destructionFRS = destructionFRS;
                        other.destructionFNRS = destructionFNRS;
                        other.relativeSize = relativeSize;
                        other.particle = particle;
                        other.destructionType = destructionType;

                        other.totalMass = totalMass;
                        other.drag = drag;
                        other.angularDrag = angularDrag;
                        other.constraints = constraints;
                        other.interpolation = interpolation;
                        other.collisionMode = collisionMode;
                        other.useConvexCollider = useConvexCollider;
                        
                        other.use32BitInt = use32BitInt;
                        other.generationType = generationType;
                        other.loadingTime = loadingTime;

                        other.isResetable = isResetable;
                        other.delayRecalculation = delayRecalculation;
                        
                        other.objectScale = objectScale;
                        other.startWithPhysics = startWithPhysics;
                        other.path = path;
                        other.exportMesh = exportMesh;
                        other.addVoxelCollider = addVoxelCollider;
                        other.collisionScale = collisionScale;

                        GameObject modelChild = new GameObject("VoxelModel " + i);
                        modelChild.transform.parent = newParent.transform;
                        modelChild.transform.localPosition = new Vector3(-0.5f, 0.5f,0.5f);
                        modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
                        modelChild.transform.localScale = Vector3.one;
                        other.meshFiler = modelChild.AddComponent<MeshFilter>();
                        
                        modelChild.AddComponent<MeshRenderer>();
                        modelChild.GetComponent<MeshRenderer>().material = material;
                        modelChild.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;
                        
                        other.BuildModel(file.Models[i], i);
                    }
                }   
            }
            else if (buildModel < file.Models.Length)
            {
                GameObject modelChild = new GameObject("VoxelModel " + buildModel);
                modelChild.transform.parent = transform;
                modelChild.transform.localPosition = new Vector3(-0.5f, 0.5f,0.5f);
                modelChild.transform.localRotation = Quaternion.Euler(-90, 0,0);
                modelChild.transform.localScale = Vector3.one;
                meshFiler = modelChild.AddComponent<MeshFilter>();

                modelChild.AddComponent<MeshRenderer>();
                modelChild.GetComponent<MeshRenderer>().material = material;
                modelChild.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.TwoSided;

                transform.localScale = new Vector3(objectScale, objectScale, objectScale);
                    
                DrawVoxelModel(file.Models[buildModel], buildModel);
            }
        }

        public void BuildModel(IModel model, int modelIndex)
        {
            activateOnRecalculation = new List<GameObject>();
            length = Vector3Int.RoundToInt(model.Size);
            
            arrayLength = new int4(length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 2 * 6,
                length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 4 * 6);
            arrayLength.xyzw /= 2;
            
            if (debug)
                Debug.Log("Read Model Dimensions: " + $"{length.x}/{length.y}/{length.z}", this);

            data = VoxToVoxelData.GenerateVoxelData(model);
            
            if (isResetable)
            {
                dataCopy = new VoxelData(length, new Voxel[data.Blocks.Length]);

                for (int i = 0; i < dataCopy.Blocks.Length; i++)
                {
                    dataCopy.Blocks[i] = new Voxel(data.Blocks[i]);
                }   
            }

            if (destructionType == DestructionType.PreCalculatedFragments && destructible)
                PrecalculateFragments(modelIndex);
            
            if (debug)
                Debug.Log("Creating Mesh...", this);

            if (createCollider || startWithPhysics)
            {
                meshFiler.gameObject.AddComponent<MeshCollider>();
                meshFiler.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                meshFiler.GetComponent<MeshCollider>().convex = useConvexCollider;   
            }

            if (addVoxelCollider)
            {
                meshFiler.gameObject.AddComponent<VoxelCollider>();
                meshFiler.GetComponent<VoxelCollider>().collisionScale = collisionScale;
            }

            if (!startWithPhysics)
            {
                activeRb = false;
            }
            else
            {
                meshFiler.GetComponent<MeshCollider>().convex = true;

                Rigidbody rb = meshFiler.gameObject.AddComponent<Rigidbody>();
                rb.mass = totalMass;
                rb.drag = drag;
                rb.angularDrag = angularDrag;

                rb.interpolation = interpolation;
                rb.collisionDetectionMode = collisionMode;
                rb.constraints = constraints;

                activeRb = true;
            }
                
            StartCoroutine(CreateVoxelMesh(meshFiler, true));

    #if UNITY_EDITOR
                
            if (exportMesh)
            {
                string filePath = 
                    EditorUtility.SaveFilePanelInProject("Save Mesh", "Mesh", "asset", "");
                if (filePath != "")
                {
                    AssetDatabase.CreateAsset(meshFiler.mesh, filePath);   
                }
            }
                
    #endif
        }

        private void DrawVoxelModel(IModel model, int modelIndex)
        {
            activateOnRecalculation = new List<GameObject>();
            length = Vector3Int.RoundToInt(model.Size); 
            
            arrayLength = new int4(length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 2 * 6,
                length.x * length.y * length.z * 4 * 6, length.x * length.y * length.z * 4 * 6);

            arrayLength.xyzw /= 2;

            if (debug)
                Debug.Log("Read Model Dimensions: " + $"{length.x}/{length.y}/{length.z}", this);

            data = VoxToVoxelData.GenerateVoxelData(model);
            if (isResetable)
            {
                dataCopy = new VoxelData(length, new Voxel[data.Blocks.Length]);

                for (int i = 0; i < dataCopy.Blocks.Length; i++)
                {
                    dataCopy.Blocks[i] = new Voxel(data.Blocks[i]);
                }   
            }

            if (destructionType == DestructionType.PreCalculatedFragments && destructible)
                PrecalculateFragments(modelIndex);
            
            if (debug)
                Debug.Log("Creating Mesh...", this);

            if (createCollider || startWithPhysics)
            {
                meshFiler.gameObject.AddComponent<MeshCollider>();
                meshFiler.GetComponent<MeshCollider>().cookingOptions = MeshColliderCookingOptions.UseFastMidphase;
                meshFiler.GetComponent<MeshCollider>().convex = useConvexCollider;       
            }

            if (addVoxelCollider)
            {
                meshFiler.gameObject.AddComponent<VoxelCollider>();
                meshFiler.GetComponent<VoxelCollider>().collisionScale = collisionScale;
            }
            
            if (!startWithPhysics)
            {
                activeRb = false;
            }
            else
            {
                meshFiler.gameObject.AddComponent<MeshCollider>();
                meshFiler.GetComponent<MeshCollider>().convex = true;

                Rigidbody rb = meshFiler.gameObject.AddComponent<Rigidbody>();
                rb.mass = totalMass;
                rb.drag = drag;
                rb.angularDrag = angularDrag;

                rb.interpolation = interpolation;
                rb.collisionDetectionMode = collisionMode;
                rb.constraints = constraints;

                activeRb = true;
            }
                
            StartCoroutine(CreateVoxelMesh(meshFiler, true));

    #if UNITY_EDITOR
                
            if (exportMesh)
            {
                string filePath = 
                    EditorUtility.SaveFilePanelInProject("Save Procedural Mesh", "Procedural Mesh", "asset", "");
                if (filePath != "")
                {
                    AssetDatabase.CreateAsset(meshFiler.mesh, filePath);   
                }
            }
                
    #endif
        }

        #endregion

        #region Recalculation

        private MeshBuilder builder;
        private MeshBuilderParallel parallelBuilder;
        private MeshBuilderSafe safeBuilder;
        private AudioSource _audioSource;
        [SerializeField] private AudioClip _destructionSound;

        private IEnumerator CreateVoxelMesh(MeshFilter filter, bool forceSafe)
        {
            if (generationType == GenerationType.Normal && !forceSafe)
            {
                builder = new MeshBuilder();
                builder.StartMeshDrawing(data, arrayLength, allocator, scheduleParallel);
                
                for (int i = 0; i < loadingTime; i++)
                {
                    if (!builder.IsCompleted())
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
                
                Mesh mesh = builder.GetVoxelObject(use32BitInt);
            
                builder.Dispose();
                builder = null;
            
                if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
                {
                    ForceJobDispose();
                    Destroy(gameObject);
                    yield break;
                }
                
                filter.mesh = mesh;

                if (createCollider)
                    filter.GetComponent<MeshCollider>().sharedMesh = mesh;
                
                if (debug)
                    Debug.Log($"Finished Building Mesh: verts {mesh.vertices.Length}, triangles {mesh.triangles.Length}, uvs {mesh.uv.Length}, color {mesh.colors.Length}", this);
            }
            else if (generationType == GenerationType.Safe || forceSafe)
            {
                safeBuilder = new MeshBuilderSafe();
                safeBuilder.StartMeshDrawing(data, allocator);
                
                for (int i = 0; i < loadingTime; i++)
                {
                    if (!safeBuilder.IsCompleted())
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
                
                Mesh mesh = safeBuilder.GetVoxelObject(use32BitInt);
            
                safeBuilder.Dispose();
                safeBuilder = null;
            
                if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
                {
                    ForceJobDispose();
                    Destroy(gameObject);
                    yield break;
                }
                
                filter.mesh = mesh;
                
                if (createCollider)
                    filter.GetComponent<MeshCollider>().sharedMesh = mesh;
                
                if (debug)
                    Debug.Log($"Finished Building Mesh: verts {mesh.vertices.Length}, triangles {mesh.triangles.Length}, uvs {mesh.uv.Length}, color {mesh.colors.Length}", this);
            }
            else if (generationType == GenerationType.Fast)
            {
                parallelBuilder = new MeshBuilderParallel();
                parallelBuilder.StartMeshDrawing(data, arrayLength, allocator, scheduleParallel);
                
                for (int i = 0; i < loadingTime; i++)
                {
                    if (!parallelBuilder.IsCompleted())
                    {
                        yield return null;
                    }
                    else
                    {
                        break;
                    }
                }
                
                Mesh mesh = parallelBuilder.GetVoxelObject(use32BitInt);
            
                parallelBuilder.Dispose();
                parallelBuilder = null;
            
                if (mesh.vertices.Length == 0 || mesh.triangles.Length == 0)
                {
                    ForceJobDispose();
                    Destroy(gameObject);
                    yield break;
                }
                
                filter.mesh = mesh;

                if (createCollider)
                    filter.GetComponent<MeshCollider>().sharedMesh = mesh;
                
                if (debug)
                    Debug.Log($"Finished Building Mesh: verts {mesh.vertices.Length}, triangles {mesh.triangles.Length}, uvs {mesh.uv.Length}, color {mesh.colors.Length}", this);
            }
            
            if (delayRecalculation && activateOnRecalculation.Count > 0)
            {
                for (int i = 0; i < activateOnRecalculation.Count; i++)
                {
                    activateOnRecalculation[i].transform.root.gameObject.SetActive(true);
                }
                
                activateOnRecalculation.Clear();
            }
        }
        
        private void QuitRecalculation()
        {
            if (recalculationI != null)
            {
                StopCoroutine(recalculationI);
                
                ForceJobDispose();
            }
        }

        #endregion

        #region Collision

        //Collision between Voxel Objects
        public void OnVoxelCollision(Collision collision, float _collisionScale)
        {
            if (!destructible)
                return;

            ComputeCollision(collision.relativeVelocity.magnitude * _collisionScale, collision.contacts[0].point, collision.contacts[0].normal, -1f);
        }

        public void AddDestruction(float strength, Vector3 point, Vector3 normal, float overrideMax)
        {
            if (!destructible)
                return;
            _cameraShake.ShakeCamera();
            ComputeCollision(strength, point, normal, overrideMax);
        }
        
        public void AddDestruction(float strength, Vector3 point, Vector3 normal)
        {
            if (!destructible)
                return;
            _cameraShake.ShakeCamera();
            ComputeCollision(strength, point, normal, -1f);
        }


        #endregion

        #region Destruction
        
        private void ComputeCollision(float relativeVel, Vector3 point, Vector3 normal, float overrideMax)
        {
            if (debug)
                Debug.Log($"Destruction entering with strength: {relativeVel}, point: {point}, normal: {normal}, overrideMax: {overrideMax}");
            
            if (relativeVel > destructionGS.minCollisionMag)
                StartCoroutine(CreateDestruction(relativeVel, point, normal, overrideMax));
        }

        private IEnumerator CreateDestruction(float relativeVel, Vector3 point, Vector3 normal, float overrideMax)
        {
            destructionCount++;

            //Sound
            if (destructionSS.collisionClip != null && destructionSS.collisionClip.Length > 0)
            {
                string clip = destructionSS
                    .collisionClip[Random.Range(0, destructionSS.collisionClip.Length)];
                SoundManager.instance.Play(clip, "Game", true, point, true, Mathf.Clamp(relativeVel / destructionSS.soundVolumeScale, 0.1f, 2f));
            }
            //_audioSource.PlayOneShot(_destructionSound);
            if (destructionType == DestructionType.PassiveVoxelRemoval || 
                destructionType == DestructionType.ActiveVoxelRemoval)
            {
                //Get point voxel

                Vector3 localPoint = meshFiler.transform.InverseTransformPoint(point);

                int removalCount = Mathf.FloorToInt(relativeVel * destructionGS.collisionStrength);
                
                Color col = Color.magenta;
                for (int j = 0; j < removalCount; j++)
                {
                    int voxel = GetNearestVoxel(localPoint, overrideMax == -1f ? destructionGS.maxVoxelDistance : overrideMax);

                    if (voxel == -1)
                        break;
                    
                    col = data.Blocks[voxel].color;

                    if (destructionType ==
                        DestructionType.ActiveVoxelRemoval)
                    {
                        GameObject newCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        newCube.transform.position =
                            transform.GetChild(0).TransformPoint(To3D(voxel, data.Size.x, data.Size.y));

                        Rigidbody cube = newCube.AddComponent<Rigidbody>();
                        newCube.transform.localScale = new Vector3(objectScale, objectScale, objectScale);

                        float cubeMass = totalMass / (length.x * length.y * length.z);

                        cube.mass = cubeMass;
                        cube.drag = 0;
                        cube.angularDrag = 0.05f;
                        cube.interpolation = interpolation;

                        cube.AddForce(normal * destructionGS.collisionForce, ForceMode.Impulse);
                        
                        Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
                        Color[] colors = new Color[mesh.vertices.Length];

                        for (int k = 0; k < mesh.vertices.Length; k++)
                        {
                            colors[k] = col;
                        }

                        mesh.colors = colors;

                        cube.GetComponent<MeshFilter>().mesh = mesh;

                        cube.GetComponent<MeshRenderer>().material = material;

                        if (delayRecalculation)
                        {
                            newCube.SetActive(false);
                            activateOnRecalculation.Add(newCube);
                        }
                    }
                    data.Blocks[voxel] = new Voxel(Color.black, false);
                }

                if (delayRecalculation)
                    yield return null;
                
                if (!Exists())
                {
                    ForceJobDispose();
                    Destroy(gameObject);
                    yield break;
                }

                if (particle && removalCount > 0)
                {
                    Quaternion rot = Quaternion.FromToRotation(Vector3.up, normal);
                    GameObject par = Instantiate(particle, point, rot);
                    ParticleSystem.MainModule mod = par.GetComponentInChildren<ParticleSystem>().main;
                    mod.startColor = col;
                    Destroy(par, 1f);
                }
            }
            else if (destructionType == DestructionType.FragmentationRemoval)
            {
                //Get point voxel
                Vector3 localPoint = meshFiler.transform.InverseTransformPoint(point);

                //Fragmentation

                int removalCount = Mathf.FloorToInt(relativeVel * destructionGS.collisionStrength);

                for (int j = 0; j < removalCount; j++)
                {
                    int fragmentSize = relativeSize
                        ? Mathf.RoundToInt(relativeVel * destructionFRS.relativeFragmentScale)
                        : Random.Range(destructionFNRS.minFragmentSize, destructionFNRS.maxFragmentSize); 
                    
                    VoxReader.Voxel[] fragmentArray = new VoxReader.Voxel[fragmentSize];
                    
                    int start = GetNearestVoxel(localPoint,   overrideMax == -1f ? destructionGS.maxVoxelDistance : overrideMax);
                    
                    if (start == -1)
                        break;
                    
                    for (int k = 0; k < fragmentArray.Length; k++)
                    {
                        if (k == 0)
                        {
                            fragmentArray[k] = new VoxReader.Voxel(To3D(start, length.x, length.y),
                                new Color(data.Blocks[start].color.r * 255, data.Blocks[start].color.g * 255,
                                    data.Blocks[start].color.b * 255, 255));
                        
                            data.Blocks[start] = new Voxel(Color.black, false);   
                        }
                        else
                        {
                            int current = GetRandomNeighboar(start);

                            if (current == -1)
                            {
                                VoxReader.Voxel[] temp = fragmentArray;
                                fragmentArray = new VoxReader.Voxel[k];

                                for (int l = 0; l < fragmentArray.Length; l++)
                                {
                                    fragmentArray[l] = temp[l];
                                }
                                
                                break;
                            }
                            else
                            {
                                fragmentArray[k] = new VoxReader.Voxel(To3D(current, length.x, length.y),
                                    new Color(data.Blocks[current].color.r * 255, data.Blocks[current].color.g * 255,
                                        data.Blocks[current].color.b * 255, 255));
                        
                                data.Blocks[current] = new Voxel(Color.black, false);   
                            
                                start = current;
                            }
                        }
                    }
                    
                    if (delayRecalculation)
                        yield return null;

                    if (fragmentArray.Length > 0)
                    {
                        GameObject newCube = new GameObject();
                        newCube.transform.position =
                            transform.GetChild(0).TransformPoint(GetMinV3(fragmentArray));
                   
                        newCube.transform.name = transform.name + " Fragment";
                    
                        newCube.transform.localScale = new Vector3(objectScale, objectScale, objectScale);
                    
                        newCube.AddComponent<VoxelStruct>();
                        newCube.GetComponent<VoxelStruct>().BuildObject(fragmentArray, material, allocator, delayRecalculation, isResetable ? this : null);
                    
                        Rigidbody cube = newCube.AddComponent<Rigidbody>();
                    
                        float cubeMass = (totalMass / (length.x * length.y * length.z)) * fragmentArray.Length;

                        cube.mass = cubeMass;
                        cube.drag = drag;
                        cube.angularDrag = angularDrag;
                        cube.interpolation = interpolation;
                    
                        cube.AddForce(normal * destructionGS.collisionForce, ForceMode.Impulse);

                        j += fragmentArray.Length;

                        if (delayRecalculation)
                        {
                            activateOnRecalculation.Add(newCube);
                        }
                        //накопление монет, монеты, деньги
                        Destroy(newCube, 3f);
                    }
                    _destructionGoal.ChangeProgress(removalCount);
                }
            }
            else if (destructionType == DestructionType.PreCalculatedFragments)
            {
                if (destructionPFS.useJobFragmentation)
                {
                    int removalCount = Mathf.FloorToInt((relativeVel * destructionGS.collisionStrength) / ((destructionPFS.fragSphereRadiusMax + destructionPFS.fragSphereRadiusMin) / 2f));

                    if (removalCount > 0)
                    {
                        if (fragmentBuilder != null)
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                yield return null;
                                
                                if (fragmentBuilder == null)
                                    break;

                                if (i == 9)
                                    yield break;
                            }
                        }
                        
                        fragmentBuilder = new FragmentBuilder(fragmentsT, removalCount, point);

                        if (delayRecalculation)
                            yield return null;

                        int[] fragments = fragmentBuilder.GetFragments();
                        fragmentBuilder.Dispose();
                        
                        for (int i = 0; i < fragments.Length; i++)
                        {
                            if (fragments[i] == -1)
                            {
                                Debug.Log("Stopping array at index " + i);
                                break;   
                            }

                            if (fragments[i] >= fragmentsT.Length)
                                break;
                            
                            if ((fragmentsT[fragments[i]].position - point).sqrMagnitude >  Mathf.Pow(overrideMax == -1f ? destructionGS.maxVoxelDistance : overrideMax, 2))
                                continue;
                        
                            int[] toRemove = fragmentsT[fragments[i]].GetComponent<VoxelFragment>().fragment;

                            for (int j = 0; j < toRemove.Length; j++)
                            {
                                data.Blocks[toRemove[j]].active = false;
                            }
                        
                            fragmentsT[fragments[i]].gameObject.SetActive(true);

                            fragmentsT[fragments[i]].parent = null;
                            fragmentsT[fragments[i]].GetComponent<Rigidbody>().AddForce(normal * destructionGS.collisionForce, ForceMode.Impulse);
                            Destroy(fragmentsT[fragments[i]].gameObject, 5f);   
                        }
                        fragmentBuilder = null;
                    }
                }
                else
                {
                    int removalCount = Mathf.FloorToInt(relativeVel * destructionGS.collisionStrength);
                    for (int i = 0; i < removalCount; )
                    {
                        float minDistance = -1;
                        int selected = -1;
                    
                        for (int j = 0; j < fragmentsT.Length; j++)
                        {
                            if (!fragmentsT[j].gameObject.activeInHierarchy)
                            {
                                if (minDistance == -1)
                                {
                                    selected = j;
                                    minDistance = (point - fragmentsT[j].position).sqrMagnitude;
                                }
                                else if ((point - fragmentsT[j].position).sqrMagnitude < minDistance)
                                {
                                    selected = j;
                                    minDistance = (point - fragmentsT[j].position).sqrMagnitude;
                                }
                            }
                        }
                    
                        if (selected == -1)
                            break;
                        
                        if ((fragmentsT[selected].position - point).sqrMagnitude > Mathf.Pow(overrideMax == -1f ? destructionGS.maxVoxelDistance : overrideMax, 2))
                            break;

                        int[] toRemove = fragmentsT[selected].GetComponent<VoxelFragment>().fragment;

                        for (int j = 0; j < toRemove.Length; j++)
                        {
                            data.Blocks[toRemove[j]].active = false;
                        }
                        print("ZALUPA");
                        fragmentsT[selected].gameObject.SetActive(true);
                        fragmentsT[selected].GetComponent<Rigidbody>().AddForce(normal * destructionGS.collisionForce, ForceMode.Impulse);
                        fragmentsT[selected].parent = null;

                        i += toRemove.Length;
                    }
                }
            }
            
            if (!activeRb && destructionGS.physicsOnCollision)
            {
                activeRb = true;
                    
                meshFiler.GetComponent<MeshCollider>().convex = true;
                            
                Rigidbody myRb = meshFiler.AddComponent<Rigidbody>();
                            
                myRb.mass = totalMass;
                myRb.drag = drag;
                myRb.angularDrag = angularDrag;
                myRb.constraints = constraints;
                myRb.interpolation = interpolation;
                myRb.collisionDetectionMode = collisionMode;
            }
                
            if (destructionCount == 1)
            {
                if (recalculationI != null)
                    yield return recalculationI;
                
                recalculationI = StartCoroutine(CreateVoxelMesh(meshFiler, false));   
            } 

            destructionCount--;
        }


        #endregion

        #region Other

        private int GetNearestVoxel(Vector3 point, float maxDistance)
        {
            int current = 0;
            float minDistance = 999;
            
            for (int i = 0; i < data.Blocks.Length; i++)
            {
                if (!data.Blocks[i].active)
                    continue;
                
                float distance = (point - To3D(i, data.Size.x, data.Size.y)).sqrMagnitude;

                if (distance < minDistance)
                {
                    current = i;
                    minDistance = distance;
                }
            }

            if (Mathf.Sqrt(minDistance) > maxDistance || minDistance == 999f)
                return -1;

            return current;
        }
        
        private int GetRandomNeighboar (int voxel)
        {
            int target = Random.Range(0, 6);
            for (int i = 0; i < 6; i++)
            {
                int3 dir = new int3(0, 0, 0);
                dir[target % 3] = 1;
                if (target > 2)
                    dir = -dir;

                Vector3 pos = To3D(voxel, length.x, length.y) + new Vector3(dir.x, dir.y, dir.z);
                
                if (pos.x >= length.x || pos.x < 0 || pos.y >= length.y || pos.y < 0 || pos.z >= length.z || pos.z < 0)
                    continue;
                
                int index = To1D(pos);

                if (index >= 0 && index < data.Blocks.Length && data.Blocks[index].active)
                {
                    return index;
                }

                target += 1;
                if (target > 5)
                    target = 0;
            }

            return -1;
        }

        private Vector3 GetMinV3(VoxReader.Voxel[] array)
        {
            Vector3 min = array[0].Position;
            
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i].Position.x < min.x)
                    min = new Vector3(array[i].Position.x, min.y, min.z);
                
                if (array[i].Position.y < min.y)
                    min = new Vector3(min.x, array[i].Position.y, min.z);
                
                if (array[i].Position.z < min.z)
                    min = new Vector3(min.x, min.y, array[i].Position.z);
            }

            return min;
        }
        
        private bool Exists()
        {
            for (int i = 0; i < data.Blocks.Length; i++)
            {
                if (data.Blocks[i].active)
                    return true;
            }

            return false;
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
        
        private int To1D(Vector3 index)
        {
            return (int)(index.x + length.x * (index.y + length.y * index.z));
        }
        
        #endregion

        #region FragmentCalculation

        public void PrecalculateFragments(int modelIndex)
        {
            Debug.Log("PRECALC FRAGMENTS");
            //ебнуть путь сюда
            if (BetterStreamingAssets.FileExists(path.Replace(".vox", "_" + modelIndex.ToString() + ".txt")))
            {
                string output = BetterStreamingAssets.ReadAllText(path.Replace(".vox", "_" + modelIndex.ToString() + ".txt"));

                string[] arrayStrings = output.Split(';');

                fragmentParent = new GameObject("FragmentParent").transform;

                fragmentParent.parent = transform;
                fragmentParent.localScale = Vector3.one;
                fragmentParent.localPosition = Vector3.zero;
                fragmentParent.localRotation = Quaternion.identity;
                fragmentsT = new Transform[arrayStrings.Length];
                
                for (int i = 0; i < arrayStrings.Length; i++)
                { 
                    if (arrayStrings[i] == "" || arrayStrings[i] == " ")
                        continue;
                    
                    int[] currFrag = Array.ConvertAll(arrayStrings[i].Split(','), s => int.Parse(s));;
                    
                    if (debug)
                    {
                        Color c = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f),
                            1);
                    
                        for (int j = 0; j < currFrag.Length; j++)
                        {
                            data.Blocks[currFrag[j]].color = c;
                        }   
                    }

                    VoxReader.Voxel[] fragmentArray = new VoxReader.Voxel[currFrag.Length];

                    for (int j = 0; j < fragmentArray.Length; j++)
                    {
                        fragmentArray[j] = new VoxReader.Voxel(To3D(currFrag[j], length.x, length.y),
                            new Color(data.Blocks[currFrag[j]].color.r * 255, data.Blocks[currFrag[j]].color.g * 255, data.Blocks[currFrag[j]].color.b * 255, 255));
                    }
                    
                    GameObject newCube = new GameObject();
                    newCube.transform.position =
                        transform.GetChild(0).TransformPoint(GetMinV3(fragmentArray));

                    newCube.transform.parent = fragmentParent;
                    newCube.transform.name = transform.name + " Fragment " + i;
                    
                    newCube.transform.localScale = Vector3.one;
                    
                    newCube.AddComponent<VoxelFragment>();
                    newCube.GetComponent<VoxelFragment>().BuildObject(fragmentArray, currFrag, material, this);
                    
                    fragmentsT[i] = newCube.transform;
                    
                    Rigidbody cube = newCube.AddComponent<Rigidbody>();
                    
                    float cubeMass = (totalMass / (length.x * length.y * length.z)) * fragmentArray.Length;
                    
                    cube.mass = cubeMass;
                    cube.drag = drag;
                    cube.angularDrag = angularDrag;
                    cube.interpolation = interpolation;
                }
            }
            else
            {
                Debug.LogWarning("No Fragment File found, consider precaulculating Fragments in the Editor");
                
                 List<int[]> fragments = new List<int[]>();
                List<int> currentFrag = new List<int>();
                VoxelData tempData;

                tempData.Size = data.Size;
                tempData.Blocks = new Voxel[data.Blocks.Length];
                for (int i = 0; i < data.Blocks.Length; i++)
                {
                    tempData.Blocks[i] = new Voxel(data.Blocks[i]);
                }

                int so = 0;
                int current = GetNext(tempData);
                while (current != -1 && so < data.Blocks.Length)
                {
                    //Frag
                    SimpleSphere fragmentSphere = new SimpleSphere(To3D(current, length.x, length.y), Random.Range(
                        destructionPFS.fragSphereRadiusMin,
                        destructionPFS.fragSphereRadiusMax));
                    
                    for (int i = 0; i < tempData.Blocks.Length; i++)
                    {
                        if (tempData.Blocks[i].active && fragmentSphere.IsInsideSphere(To3D(i, length.x, length.y)))
                        {
                            currentFrag.Add(i);
                            tempData.Blocks[i] = new Voxel(Color.black, false);
                        }
                    }
                    
                    fragments.Add(currentFrag.ToArray());
                    currentFrag.Clear();
                    
                    current = GetNext(tempData);
                    so++;
                }
                
                if (so >= data.Blocks.Length)
                    Debug.LogWarning("Calculating fragments caused Stackoverflow, make sure values are correct!", this);
                else
                {
                    fragmentParent = new GameObject("FragmentParent").transform;

                    fragmentParent.parent = transform;
                    fragmentParent.localScale = Vector3.one;
                    fragmentParent.localPosition = Vector3.zero;
                    fragmentParent.localRotation = Quaternion.identity;
                    fragmentsT = new Transform[fragments.Count];
                    
                    for (int i = 0; i < fragments.Count; i++)
                    {
                        //Gives every Fragment his own color (debugging)
                        if (debug)
                        {
                            Color c = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f),
                                1);
                        
                            for (int j = 0; j < fragments[i].Length; j++)
                            {
                                data.Blocks[fragments[i][j]].color = c;
                            }   
                        }

                        VoxReader.Voxel[] fragmentArray = new VoxReader.Voxel[fragments[i].Length];

                        for (int j = 0; j < fragmentArray.Length; j++)
                        {
                            fragmentArray[j] = new VoxReader.Voxel(To3D(fragments[i][j], length.x, length.y),
                                new Color(data.Blocks[fragments[i][j]].color.r * 255, data.Blocks[fragments[i][j]].color.g * 255, data.Blocks[fragments[i][j]].color.b * 255, 255));
                        }
                        
                        GameObject newCube = new GameObject();
                        newCube.transform.position =
                            transform.GetChild(0).TransformPoint(GetMinV3(fragmentArray));

                        newCube.transform.parent = fragmentParent;
                        newCube.transform.name = transform.name + " Fragment " + i;
                        
                        newCube.transform.localScale = Vector3.one;
                        
                        newCube.AddComponent<VoxelFragment>();
                        newCube.GetComponent<VoxelFragment>().BuildObject(fragmentArray, fragments[i], material, this);
                        
                        fragmentsT[i] = newCube.transform;
                        
                        Rigidbody cube = newCube.AddComponent<Rigidbody>();
                        
                        float cubeMass = (totalMass / (length.x * length.y * length.z)) * fragmentArray.Length;
                        
                        cube.mass = cubeMass;
                        cube.drag = drag;
                        cube.angularDrag = angularDrag;
                        cube.interpolation = interpolation;
                    }
                }   
            }
        }
        
        private int GetNext (VoxelData temp)
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
            
            return To1D(nearest);
        }

        #endregion
        
        #region Events

        public void ResetModel(bool removeFragments)
        {
            if (!isResetable)
            {
                Debug.LogWarning("You can not reset a Voxel Object that is not set to be resetable. Make sure to check the resetable bool!", this);
                return;
            }
            
            ForceJobDispose();
            StopAllCoroutines();
            destructionCount = 0;

            data = dataCopy;
            
            dataCopy = new VoxelData(length, new Voxel[data.Blocks.Length]);

            for (int i = 0; i < dataCopy.Blocks.Length; i++)
            {
                dataCopy.Blocks[i] = new Voxel(data.Blocks[i]);
            }

            if (removeFragments)
            {
                if (destructionType == DestructionType.FragmentationRemoval)
                {
                    VoxelStruct[] frag = FindObjectsOfType<VoxelStruct>();

                    for (int i = 0; i < frag.Length; i++)
                    {
                        if (frag[i].voxOrigin == this)
                            Destroy(frag[i].transform.root.gameObject);
                    }   
                }
                else if (destructionType == DestructionType.PreCalculatedFragments)
                {
                    VoxelFragment[] frag = FindObjectsOfType<VoxelFragment>();

                    for (int i = 0; i < frag.Length; i++)
                    {
                        if (frag[i].voxOrigin == this)
                        {
                            frag[i].transform.parent = fragmentParent;
                            frag[i].ResetFrag();
                        }
                    }
                }
            }
            
            StartCoroutine(CreateVoxelMesh(meshFiler, false));
        }
        
        private void Reset()
        {
            destructionGS = new DestructionGeneralSettings();
            destructionSS = new DestructionSoundSettings();
            destructionPFS = new DestructionPreFragmentSettings();
            destructionFRS = new DestructionFragmentRSettings();
            destructionFNRS = new DestructionFragmentNRSettings();
        }

        private void OnApplicationQuit()
        {
            ForceJobDispose();
        }

        private void ForceJobDispose()
        {
            if (builder != null)
                builder.Dispose();
            else if (safeBuilder != null)
                safeBuilder.Dispose();
            else if (parallelBuilder != null)
                parallelBuilder.Dispose();
            
            if (fragmentBuilder != null)
                fragmentBuilder.Dispose();
        }

        #endregion
    }
    
    [Serializable]
    public class DestructionGeneralSettings
    {
        [Tooltip("The mimimun relative velocity magnitude between the colliding objects to start destruction")]
        public float minCollisionMag = 0.35f;

        [Tooltip("The mimimun relative velocity magnitude between the colliding objects to start destruction")]
        public float collisionStrength = 30;

        [Tooltip("The Force applied to the Fragment rigidbody on destruction")]
        public float collisionForce = 0.1f;

        [Tooltip("Makes the model a physic object on collision")]
        public bool physicsOnCollision = false;
        
        [Tooltip("The maximum distance between collision point and the Voxel/Fragment")]
        public float maxVoxelDistance = 10;
    }
    
    [Serializable]
    public class DestructionSoundSettings
    {
        [Tooltip("The collision clips for collisions, a random one gets selected, make sure it is setup correctly")]
        public string[] collisionClip = new string[] {"Destruction1"};

        [Tooltip("The volume scale")]
        public float soundVolumeScale = 100;
    }

    [Serializable]
    public class DestructionFragmentNRSettings
    {
        [Tooltip("Minimum fragment size in Voxels")]
        public int minFragmentSize = 2;

        [Tooltip("Maximum fragment size in Voxel")]
        public int maxFragmentSize = 10;
    }
    
    [Serializable]
    public class DestructionFragmentRSettings
    {
        [Tooltip("The relative fragment scale")]
        public float relativeFragmentScale = 0.5f;
    }

    [Serializable]
    public class DestructionPreFragmentSettings
    {
        [Tooltip("Defines if a Job should be used to calculate the fragments to remove")]
        public bool useJobFragmentation = true;
        
        [Space]
        
        [Tooltip("The minimum sphere radius used for fragment calculation")]
        public float fragSphereRadiusMin = 1;
        [Tooltip("The maxmimum sphere radius used for fragment calculation")]
        public float fragSphereRadiusMax = 3;
    }
}
