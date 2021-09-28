using UnityEngine;
using UnityEngine.SceneManagement;

public class SampleSceneManager : MonoBehaviour
{
    public GameObject AICarPrefab;
    public Transform[] spawnPositions;
    public Transform AICarsNode;
    public GameObject roadNode;

    bool isFreeZone = false;

    // Start is called before the first frame update
    void Start()
    {
        isFreeZone = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAddAICarClick()
    {
        if (AICarPrefab)
        {
            GameObject newAI = Instantiate(AICarPrefab);
            CarAIManager aiManager = newAI.GetComponent<CarAIManager>();
            aiManager.aiMode = CarAIManager.AIMode.followWaypoints;
            aiManager.skillLevel = Random.Range(0.0f, 1.0f);

            Vector3 position = spawnPositions[Random.Range(0, spawnPositions.Length - 1)].position;
            newAI.transform.position = position;

            newAI.transform.SetParent(AICarsNode);
        }
    }

    public void OnFreeZoneClick()
    {
        isFreeZone = !isFreeZone;
        EdgeCollider2D[] edgeCollider2Ds = roadNode.GetComponentsInChildren<EdgeCollider2D>();
        foreach (EdgeCollider2D collider2D in edgeCollider2Ds)
        {
            // collider diable when it is free zone
            collider2D.enabled = !isFreeZone;
        }
        
    }

    public void OnQuitClick()
    {
        SceneManager.LoadScene("Lobby");
    }
}
