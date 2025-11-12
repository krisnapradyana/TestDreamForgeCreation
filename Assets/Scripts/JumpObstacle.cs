using UnityEngine;

public class JumpObstacle : Obstacle
{
    [SerializeField] float minHeigth = .5f;
    [SerializeField] float maxHeigth = 2.5f;

    private void OnEnable()
    {
        RandomizeHeight();
    }

    void RandomizeHeight()
    {
        this.transform.localScale = new Vector3(this.transform.localScale.x, Random.Range(minHeigth, maxHeigth), this.transform.localScale.z);
    }

    public override void HandleCollision(Player player)
    {
        player.ReduceLife();
        Debug.Log("Collided with jump obstacle, Reducing health");
    }
}
