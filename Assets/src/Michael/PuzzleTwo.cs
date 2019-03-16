using UnityEngine;

public class PuzzleTwo : MonoBehaviour {

    private GameObject Player;
    private Inventory inventory;
    private bool solved;
    private GameObject box;
    private GameObject TargetTile;
    private Vector3 Zero, size;
    private PuzzleRoom R;

    public void Start()
    {
        Player = GameObject.FindWithTag("Player");
        R = this.GetComponent<PuzzleRoom>();
        Zero = R.GetZero();
        size = R.GetSize();
        //inventory = GameObject.Find("GameManager").GetComponent<Inventory>();

        box = GameObject.Instantiate(
            Resources.Load<GameObject>("Prefabs/Crate_003"),
            new Vector3(Zero.x + Random.Range(2, size.x-3), size.y / 2, Zero.z + Random.Range(2, size.z-3)),
            Quaternion.Euler(-90,0,0),
            this.transform);
        TargetTile = R.FloorTiles[Random.Range(0,R.FloorTiles.Count)];
        TargetTile.GetComponent<Renderer>().materials[0].color = new Color(0.31f, 0.98f, 0.16f);
        TargetTile.name = "Target";
		
	}

	void FixedUpdate () {
        if (!R.solved) 
        {
            if (Vector3.Distance(box.transform.position, TargetTile.transform.position) < 1.0f)
            {
                R.solved = true;
                R.PlaySolvedSound();
                box.GetComponent<Renderer>().materials[0].color = new Color(0.6f, 0.8f, 0.2f);
                //box.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                Vector3 dir = (box.transform.position-TargetTile.transform.position).normalized;
                box.GetComponent<Rigidbody>().MovePosition(box.transform.position+(dir*Time.deltaTime));
            }
        }

	}
    public void Solve(bool s) { solved = s; }
    public bool isSolved() { return solved; }

}
