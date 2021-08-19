using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Pool types
    public enum PoolType
    {
       MarkCell,
       BuildLine
    }

    #region Variables

    //Singleton
    private static SpawnManager _instance;
    public static SpawnManager GetInstance()
    {
        if (_instance == null)
        {
            Debug.LogWarning("Instance of " + nameof(SpawnManager) + " is null referenced!");
            throw new System.Exception("Instance of " + nameof(SpawnManager) + " is null referenced!");
        }
        return _instance;
    }
    // Pools
    private List<List<GameObject>> _pools;

    #endregion

    #region Unity

    // Start is called before the first frame update
    private void Start()
    {
        // Set instance to this
        _instance = this;
        // Generate pools
        _pools = new List<List<GameObject>>();
        for (int i = 0; i < PoolType.GetNames(typeof(PoolType)).Length; i++)
        {
            _pools.Add(new List<GameObject>());
        }
    }

    #endregion

    #region Methods

    // Disables all objects
    public void DisableAllObjects()
    {
        // Cancel all invoke methods in this class
        CancelInvoke();
        // Iterate through each pool
        for (int poolIteration = 0; poolIteration < _pools.Count; poolIteration++)
        {
            // Iterate through each element in list
            for (int listIteration = 0; listIteration < _pools[poolIteration].Count; listIteration++)
            {
                _pools[poolIteration][listIteration].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Spawns object
    /// </summary>
    /// <param name="poolType">Pool type in which object should be spawned</param>
    /// <param name="gameObject">Object to spawn</param>
    public GameObject SpawnObject(PoolType poolType, GameObject gameObject) 
    {
        List<GameObject> pool = _pools[(int)poolType];
        int objInd = IsObjectInPool(pool, gameObject); // If object is indeed in pool, then reactivate it
        if (objInd != -1)
        {
            pool[objInd].SetActive(true);
            return pool[objInd];
        }
        else
        {
            // Instantiate object
            GameObject toInstantiate = Instantiate(gameObject);
            // Putting this object inside spawn manager
            toInstantiate.transform.parent = transform;
            // Add object to pool
            pool.Add(toInstantiate);
            return toInstantiate;
        }
    }

    // Checking if the object is in the pool
    private int IsObjectInPool(List<GameObject> pool, GameObject obj)
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (pool[i].activeSelf == false)
            {
                return i;
            }
        }
        return -1;
    }

    #endregion
}
