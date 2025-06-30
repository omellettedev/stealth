using UnityEngine;

public class Manager : MonoBehaviour
{
    private static Manager instance;
    public static Manager Instance => instance;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private LoadedPrefabs loadedPrefabs;
    public LoadedPrefabs LoadedPrefabs => loadedPrefabs;
}
