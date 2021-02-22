using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public GameObject[] SystemPrefabs;

    private List<GameObject> _instancedSystemPrefabs;

    private void Start() 
    {
        DontDestroyOnLoad(gameObject);

        _instancedSystemPrefabs = new List<GameObject>();
        foreach (GameObject o in SystemPrefabs) {
            _instancedSystemPrefabs.Add(Instantiate(o));
        }
        

        LoadLevel("");
    }

    public void LoadLevel(string level) 
    {
        SceneManager.LoadSceneAsync(level, LoadSceneMode.Additive);
    }

    public void UnloadLevel() 
    {

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        foreach (GameObject o in _instancedSystemPrefabs) {
            Destroy(o);
        }
        _instancedSystemPrefabs.Clear();
    }
}
