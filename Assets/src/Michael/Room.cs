using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/*
 * Base class for Room.
 * The Init function creates the floor and ceiling, and adds a collider trigger to each room 
 * (for detecting player entering/exiting).
 *
 * Contains a SetLighting function, for setting color and brightness of room lights;
 * OnTriggerExit/onTriggerEnter which toggles lights when player enters, and toggles boolean variable PlayerInRoom;
 * GetWalls/BuildWall, builds walls between each corner,
 * GetInnerWalls which builds the inner walls,
 * and Decorate which adds objects (including teleporters).
 * and get/set for room size and zero coordinate.
 *
 */

public class Room : MonoBehaviour
{
    public bool addPortal = true;
    public bool initialized = false;
    public bool PlayerInRoom = false;
    public bool testbuild = false;
    public int complexity = 3;
    public List<GameObject> DoorList;
    protected static GameObject Wall; 
    protected static GameObject Column;
    protected static GameObject Portal;
    protected static GameObject Door;
    protected static GameObject Console;
    protected static GameObject Panel;
    protected static GameObject FloorTile;
    protected GameObject Player;
    [SerializeField]
    protected Vector3 Zero;
    [SerializeField]
    protected Vector3 size;

    public GameObject Walls;
    public GameObject Floor;
    public GameObject Ceiling;

    protected Color lightColor;

    protected RoomGenerator RG; 
    public virtual void Awake()
    {
        RG = RoomGenerator.instance;
        Wall = Resources.Load<GameObject>("Michael/Wall_2_X4");
        Door = Resources.Load<GameObject>("Michael/WindowGlass_001");
        FloorTile = Resources.Load<GameObject>("Michael/Floor_003");
        Ceiling = Resources.Load<GameObject>("Michael/Plane");
        Console = Resources.Load<GameObject>("Michael/Console_001");
        Panel = Resources.Load<GameObject>("Michael/Panel_001");
        Portal = Resources.Load<GameObject>("Michael/Portal 1");
        Column =  Resources.Load<GameObject>("Michael/Wall_2_Column");
        this.gameObject.transform.position = Zero;
        DoorList = new List<GameObject>();
        Player = GameObject.FindWithTag("Player");
        Walls = new GameObject("Walls");
        Walls.transform.parent = this.gameObject.transform;
        RG.roomList.Add(this);
        lightColor = RG.Cyan;

        if(testbuild) {
            RoomGenerator.BuildDoors();
            RoomGenerator.BakeNavMesh();
        }
    }

    public void Init() 
    {
        if(size == Vector3.zero)
            size = RG.GetSize();

        initialized = true;

        Floor = GameObject.Instantiate(
            FloorTile,
            Zero+new Vector3(size.x/2,0,size.z/2),
            this.gameObject.transform.rotation,
            this.transform);
        Vector3 FloorSize = Floor.GetComponent<Renderer>().bounds.size;
        Floor.name = "Floor";
        Floor.transform.localScale = new Vector3(size.x / FloorSize.x, 1, size.z / FloorSize.x);
        //Floor.GetComponent<Renderer>().material.mainTextureScale = new Vector2(size.x/2, size.z/2);

        Ceiling = GameObject.Instantiate(
            Resources.Load<GameObject>("Michael/Plane"),
            Zero+new Vector3(size.x / 2, size.y, size.z / 2), 
            Quaternion.Euler(180.0f, 0, 0), 
            this.transform);
        Ceiling.name = "Ceiling";
        Ceiling.transform.localScale = new Vector3(size.x / 10.0f, 1, size.z / 10.0f);

        // Box Collider tells me when player enters or exits a room;
        // also gets bordering rooms
        gameObject.AddComponent<BoxCollider>().size = size;
        this.GetComponent<BoxCollider>().center = size/2;
        this.GetComponent<BoxCollider>().isTrigger = true;
        this.gameObject.layer = 2;

    }


    public void SetLighting(Color c, float intensity = 4)
    {
        foreach(Transform w in this.Walls.transform) {
            Transform l = w.transform.Find("Roof_Light_003");
            l.gameObject.GetComponent<Light>().color = c;
            l.gameObject.GetComponent<Light>().intensity = intensity;
            l.GetComponent<Renderer>().material.SetColor("_EmissionColor",c);
            l.GetComponent<Renderer>().material.SetColor("_Color",c);
        }
    }

    //turn lights on or off when player enters or leaves.
    protected virtual void OnTriggerEnter(Collider other) {
        if (other.gameObject == Player)
        {
            this.SetLighting(lightColor);
            PlayerInRoom = true;
            RG.playerRoom = this;
        }
    }

    protected virtual void OnTriggerExit(Collider other) 
    {
        if (other.gameObject == Player)
        {
            this.SetLighting(RG.Cyan, 0.0f);
            lightColor = RG.Fuschia;
            PlayerInRoom = false;
        }
    }

    //call BuildWall for each outer wall.
    public void GetWalls()
    {
        //Wall Width
        float ww = Wall.GetComponent<Renderer>().bounds.size.z/8;
        BuildWall(Zero+new Vector3(0,0,ww), Zero+new Vector3(size.x, 0, ww), size.y,side:"South");
        BuildWall(Zero+new Vector3(ww,0,0), Zero+new Vector3(ww, 0, size.z), size.y,side:"West");
        BuildWall(Zero+new Vector3(size.x-ww, 0, 0), Zero+new Vector3(size.x-ww, 0, size.z), size.y,side:"East");
        BuildWall(Zero+new Vector3(0, 0, size.z-ww), Zero+new Vector3(size.x, 0, size.z-ww), size.y,side:"North");

        // Find's a random point along outer wall, and finds point on wall directly opposite.
        // Enter recursive function GetInnerWalls with these parameters....
        Transform start = Walls.transform.GetChild(Random.Range(0, Walls.transform.childCount - 1));
        Vector3 dir = start.forward;
        Vector3 startpoint = start.position + 
            (start.position.x > start.position.z ? 
             new Vector3(Random.Range(-start.GetComponent<Renderer>().bounds.size.x/3,start.GetComponent<Renderer>().bounds.size.x/3),0,0) : 
             new Vector3(0,0,Random.Range(-start.GetComponent<Renderer>().bounds.size.z/3,start.GetComponent<Renderer>().bounds.size.z/3)));

        RaycastHit hit;
        if (Physics.Raycast(startpoint + new Vector3(0, size.y / 2, 0), dir, out hit, Mathf.Infinity, RG.wallMask))
        {
            GetInnerWalls(startpoint, hit, 0);
        }
            
        foreach(Transform w in Walls.transform) {
            Light l = w.transform.Find("Roof_Light_003").GetComponent<Light>();
            l.range = Mathf.Min(size.x,size.z);
        }
        Decorate();
    }

    // this function just makes it look prettier.  and adds teleporters.
    public void Decorate() 
    {
        GameObject console,panel, portal, column;
        foreach(GameObject door in DoorList) {
            float doorX = door.GetComponent<Renderer>().bounds.size.x;
            float doorY = door.GetComponent<Renderer>().bounds.size.y;
            float doorZ = door.GetComponent<Renderer>().bounds.size.z;
            console = GameObject.Instantiate(Console,
                    door.transform.position - (doorX > doorZ ? new Vector3(doorX,doorY/2,0) : new Vector3(0,doorY/2,doorZ)),
                    door.transform.rotation, 
                    this.transform);
            console.transform.Rotate(door.transform.position.z > Zero.z+size.z/2 || door.transform.position.x > Zero.x+size.x/2 ? new Vector3(0,180,0) : Vector3.zero);
            console.transform.position += door.transform.TransformDirection(Vector3.forward)*(door.transform.position.z > Zero.z+size.z/2 || door.transform.position.x > Zero.x+size.x/2 ? -1 : 1);
            panel = GameObject.Instantiate(Panel,
                    door.transform.position + (doorX > doorZ ? new Vector3(doorX,0,0) : new Vector3(0,0,doorZ)),
                    door.transform.rotation,
                    this.transform);
            panel.transform.Rotate(door.transform.position.z > Zero.z+size.z/2 || door.transform.position.x > Zero.x+size.x/2 ? new Vector3(0,180,0) : Vector3.zero);
            panel.transform.position += door.transform.TransformDirection(Vector3.forward)*(door.transform.position.z > Zero.z+size.z/2 || door.transform.position.x > Zero.x+size.x/2 ? -0.15f : 0.15f);
        }

        if(addPortal) {
            portal = GameObject.Instantiate(Portal,Zero+new Vector3(size.x,0,size.z),Portal.transform.rotation,this.transform);
            portal.transform.position -= new Vector3(portal.GetComponent<CapsuleCollider>().radius*2,0,portal.GetComponent<CapsuleCollider>().radius*2);
            portal.GetComponent<Teleporter>().SetHeight(this.size.y);
            portal.name = "Teleporter";
            RG.teleporterList.Add(portal);
        }

    }

    // Build a wall between start and end of height height, if doors == true then search for doors and build walls between them.
    public void BuildWall(Vector3 start, Vector3 end,float height,bool doors = true, string side = "inner")
    {
        Vector3 wBounds;
        //start at wall's starting location; 
        //increase length of wall segment(segmentEnd -> segmentStart) until hits a door, or hits wallEnd
        //build wall there, start again.

        Vector3 SegmentStart = start;
        Vector3 SegmentEnd = start;
        Vector3 dir;
        GameObject w,l,c,p;
        Vector3 lastDoor = new Vector3();
        float t = 0.0f;
        if (doors)
        {
            while (Vector3.Distance(SegmentEnd, end) > 0.1f)
            {
                SegmentEnd = Vector3.Lerp(SegmentEnd, end, t);
                foreach (Collider o in Physics.OverlapBox(new Vector3((SegmentEnd.x + SegmentStart.x) / 2, Zero.y+size.y / 2, (SegmentEnd.z + SegmentStart.z) / 2), new Vector3(Mathf.Abs(SegmentEnd.x - SegmentStart.x), size.y*0.75f, Mathf.Abs(SegmentEnd.z - SegmentStart.z))/2+new Vector3(1,1,1)))
                {
                    if (o.name == "Door" && lastDoor != o.transform.position)
                    {
                        SegmentEnd = o.bounds.ClosestPoint(start);
                        SegmentEnd = new Vector3(SegmentEnd.x, 0, SegmentEnd.z);
                        w = GameObject.Instantiate(Wall, (SegmentStart + SegmentEnd) / 2, this.gameObject.transform.rotation, Walls.transform);
                        wBounds = w.GetComponent<Renderer>().bounds.size;
                        w.transform.LookAt(end);
                        w.transform.Rotate(0,90*(side == "South" || side == "East" ? -1 : 1), 0);
                        w.transform.localScale = new Vector3(Vector3.Distance(SegmentStart, SegmentEnd) / wBounds.x, height / wBounds.y, 1);
                        w.name = "Wall";
                        w.GetComponent<NavMeshObstacle>().size = wBounds;
                        w.gameObject.SetActive(false);
                        w.gameObject.SetActive(true);

                        SegmentStart = o.GetComponent<Renderer>().bounds.ClosestPoint(end);
                        SegmentStart = new Vector3(SegmentStart.x, 0, SegmentStart.z);
                        SegmentEnd = SegmentStart;
                        t = 0.0f;
                        lastDoor = o.transform.position;
                        break;

                    }
                }
                t += 0.1f;
            }
        }
        SegmentEnd = end;
        w = GameObject.Instantiate(Wall, (SegmentStart + SegmentEnd) / 2, this.gameObject.transform.rotation, Walls.transform);
        //w = GameObject.Instantiate(Wall, (SegmentStart + SegmentEnd) / 2, Quaternion.identity,Walls.transform);
        wBounds = w.GetComponent<Renderer>().bounds.size;
        w.name = "Wall";
        w.transform.LookAt(end);
        w.transform.Rotate(0, 90*(w.transform.position.z < Zero.z+size.z/2 || w.transform.position.x > Zero.x+size.x/2 ? -1 : 1), 0);
        w.transform.localScale = new Vector3(Vector3.Distance(SegmentStart,SegmentEnd)/wBounds.x, height/wBounds.y, 1);
        w.GetComponent<NavMeshObstacle>().size = wBounds;

        dir = (w.transform.TransformDirection(Vector3.forward)*((w.transform.position.x == Zero.x || w.transform.position.z == Zero.z+size.z ? 1 : -1))).normalized;

        w.gameObject.SetActive(false);
        w.gameObject.SetActive(true);

    }

    //Build wall between start and endhit,
    // then find a random point between the two, increase recursion depth variable,
    // and start again.
    public void GetInnerWalls(Vector3 start, RaycastHit endHit, int depth)
    {
        bool built = false;
        if (depth > complexity) return;
        //Debug.DrawRay(start+new Vector3(0,size.y/2,0), endHit.point-start-new Vector3(0,size.y/2,0), Color.red, 10);
        Vector3 end = endHit.point - new Vector3(0, size.y / 2, 0);
        if (Vector3.Distance(start, end) < 4)   return;
        RaycastHit hit,hit2;
        Vector3 dir;
        // if end point is a Door, build a wall 2/3 of the way from start towards end.
        foreach(Collider o in Physics.OverlapBox(endHit.point, new Vector3(2,2,2))) {
            if(o.transform.name == "Door") {
                BuildWall(start,Vector3.Lerp(start,end,0.7f),size.y/2,false);
                built = true;
            }
        }

        // if end point is not a door, build a wall 1/3 of the way out from start, and 1/3 of the way back from end.
        if(!built) {
            BuildWall(start, Vector3.Lerp(start, end, 0.3f), size.y / 2,false);
            BuildWall(end, Vector3.Lerp(end, start, 0.3f), size.y / 2,false);
        }

        Vector3 newStart = Vector3.Lerp(start, end, Random.Range(0.2f, 0.8f));
        dir = Vector3.Cross(start+new Vector3(0,1,0), end+new Vector3(0,1,0));
        dir = new Vector3(dir.x, 0, dir.z).normalized;
        if (Physics.Raycast(newStart+new Vector3(0,size.y/2,0), dir, out hit, Mathf.Infinity, RG.wallMask))
        {
            if (Physics.Raycast(newStart+new Vector3(0,size.y/2,0), -dir, out hit2, Mathf.Infinity, RG.wallMask))
            {
                if (Vector3.Distance(newStart, hit2.point) >= Vector3.Distance(newStart, hit.point))
                    GetInnerWalls(newStart, hit2, depth + 1);
                else if(Vector3.Distance(newStart,hit.point) >= Vector3.Distance(newStart,hit2.point))
                    GetInnerWalls(newStart, hit, depth + 1);
            }
            else 
                GetInnerWalls(newStart, hit, depth + 1);
        }
        else if (Physics.Raycast(newStart+new Vector3(0,size.y/2,0), -dir, out hit2, Mathf.Infinity, RG.wallMask))
            GetInnerWalls(newStart, hit2, depth + 1);



    }

    public void SetSize(Vector3 size) { this.size = size; }

    public Vector3 GetSize() { return size; }

    public void SetZero(Vector3 Zero) 
    {
        this.Zero = Zero;
        this.transform.position = Zero;
    }
    public Vector3 GetZero() { return Zero; }

}
