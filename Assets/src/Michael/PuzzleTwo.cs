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
            Resources.Load<GameObject>("Michael/Crate_003"),
            new Vector3(Zero.x + Random.Range(2, size.x-3), size.y / 2, Zero.z + Random.Range(2, size.z-3)),
            Quaternion.Euler(-90,0,0),
            this.transform);
        Collider[] boxCollisions = Physics.OverlapBox(box.GetComponent<Collider>().bounds.center,box.GetComponent<Collider>().bounds.size/2);
        for(int i = 0; i < boxCollisions.Length; i++) {
            Transform w = boxCollisions[i].transform;
            Vector3 dir = w.TransformDirection(Vector3.forward);
            box.transform.position += dir*box.GetComponent<Collider>().bounds.size.magnitude;
                //boxCollisions = Physics.OverlapBox(box.GetComponent<Collider>().bounds.center,box.GetComponent<Collider>().bounds.size/2);
        }

        TargetTile = R.FloorTiles[Random.Range(0,R.FloorTiles.Count)];
        while(Mathf.Abs(TargetTile.transform.position.x-Zero.x) < 3.0f || 
                Mathf.Abs(TargetTile.transform.position.x-(Zero.x+size.x)) < 3.0f ||
                Mathf.Abs(TargetTile.transform.position.z-Zero.z) < 3.0f || 
                Mathf.Abs(TargetTile.transform.position.z-(Zero.z+size.z)) < 3.0f)
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
            }
            if (Mathf.Abs(box.transform.position.x - R.Zero.x) < 2.0f || 
                Mathf.Abs(box.transform.position.x - (R.Zero.x+R.size.x)) < 2.0f ||
                Mathf.Abs(box.transform.position.z - R.Zero.z) < 2.0f ||
                Mathf.Abs(box.transform.position.z - (R.Zero.z+R.size.z)) < 2.0f)
            {
                Vector3 dir = (R.Zero + R.size / 2 - box.transform.position).normalized;
                box.GetComponent<Rigidbody>().MovePosition(box.transform.position + (dir * Time.deltaTime));
            }
        }

        if(R.solved && Vector3.Distance(box.transform.position,TargetTile.transform.position) > 0.1f)
        {
            Vector3 dir = (TargetTile.transform.position-box.transform.position).normalized/2.0f;
            box.GetComponent<Rigidbody>().MovePosition(box.transform.position+(dir*Time.deltaTime));
        }

	}
    public void Solve(bool s) { solved = s; }
    public bool isSolved() { return solved; }

}