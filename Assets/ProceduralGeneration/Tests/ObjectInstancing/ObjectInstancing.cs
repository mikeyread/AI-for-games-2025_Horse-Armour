using UnityEngine;

using Random = UnityEngine.Random;


public class ObjectInstancing : MonoBehaviour
{
    [SerializeField] public float radius;
    [SerializeField] uint maximumObjects;

    [SerializeField] public GameObject prefab;
    GameObject[] activeObjects;

    private void Awake() {
        activeObjects = new GameObject[maximumObjects];
        
        for (int i = 0; i < maximumObjects; i++) {
            activeObjects[i] = (GameObject)Instantiate(prefab, Vector3.zero, Quaternion.identity);
            activeObjects[i].SetActive(false);
        }
    }

    private void Update() {

        // Regenerates objects if they are either inactive or not within player distance.
        foreach (var obj in activeObjects) {
            if (obj.activeSelf != true) RegenerateObject(obj);
            if (!withinPlayerDistance(obj.transform.position)) RegenerateObject(obj);
        }
    }

    private void RegenerateObject(GameObject obj) {
        Vector3 flattenedCamPos = new(Camera.main.transform.position.x, 0, Camera.main.transform.position.z);
        obj.transform.position = flattenedCamPos + RandomFlatPosition() * radius;
        obj.SetActive(true);
    }

    private Vector3 RandomFlatPosition() {
        Vector3 position = new Vector3(Random.value * 2 - 1, 0, Random.value * 2 - 1);
        return position;
    }

    private bool withinPlayerDistance(Vector3 objectPosition) {
        if ((objectPosition - Camera.main.transform.position).sqrMagnitude > radius * radius) return false;
        else return true;
    }
}


// Objects Generate within a larger radius than the view
// Objects that are outside are hidden, and any objects at an extreme distance are completely regenerated.
