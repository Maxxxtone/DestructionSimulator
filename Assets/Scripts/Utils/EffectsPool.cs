using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EffectsPool : MonoBehaviour
{
    [SerializeField] private GameObject _pooledObject;
    [SerializeField] private int _objectsToSpawn = 10;
    private Queue<GameObject> _pool;
    private void Start()
    {
        _pool = new Queue<GameObject>();
        for (int i = 0; i < _objectsToSpawn; i++)
        {
            var obj = Instantiate(_pooledObject);
            obj.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
    public GameObject GetEffect(Vector3 spawnPosition, Quaternion rotation)
    {
        var effect = _pool.Where(t => !t.activeSelf).First();
        effect.transform.position = spawnPosition;
        effect.transform.localRotation = rotation;
        effect.SetActive(true);
        return effect;
    }
}
