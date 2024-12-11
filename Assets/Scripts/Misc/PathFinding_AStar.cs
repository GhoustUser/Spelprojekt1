using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFindingAStar : MonoBehaviour
{
    /* ---- variables ---- */
    //reference to tilemap
    public Tilemap tilemap;
    //reference to the transform of the origin object
    public Transform originObject;
    //stores the previous positions of start and target,
    //used for updating path when either is changed
    private Vector2Int prevStart, prevTarget;
    //start position, is set to 'originObject's position
    private Vector2Int start;
    //target position
    public Vector2Int target;

    //line renderer for visualizing path
    private LineRenderer lr;

    //list of nodes that have not yet been evaluated
    private List<Node> openSet = new List<Node>();
    //list of nodes that have been evaluated
    private List<Node> closedSet = new List<Node>();
    //list of nodes for the completed path
    private List<Node> path = new List<Node>();
    
    public List<Node> Path => path;

    //list of directions the pathfinder can move in
    private static readonly Vector2Int[] Directions =
    {
        new(0, 1),
        new(1, 0),
        new(0, -1),
        new(-1, 0)
    };


    /* ---- start ---- */
    void Start()
    {
        //create line renderer
        lr = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;

        //set start position
        start.x = tilemap.WorldToCell(originObject.position).x;
        start.y = tilemap.WorldToCell(originObject.position).y;
        //store positions
        prevStart = start;
        prevTarget = target;

        //generate initial path
        FindPath();
    }

    /* ---- update ---- */
    void Update()
    {
        //update start position
        start.x = tilemap.WorldToCell(originObject.position).x;
        start.y = tilemap.WorldToCell(originObject.position).y;
        
        //check if start position has changed
        if (prevStart != start)
        {
            //generate new path
            prevStart = start;
            FindPath();
        }

        //check if target position has changed
        if (prevTarget != target)
        {
            //generate new path
            prevTarget = target;
            FindPath();
        }
    }

    /* ---- functions ---- */
    //adds a segment to the line renderer
    private void AddLRSegment(Vector2Int position)
    {
        lr.positionCount++;
        lr.SetPosition(lr.positionCount - 1, new Vector3(position.x + 0.5f, position.y + 0.5f, 0f));
    }

    //generates a path from 'start' to 'target'
    public void FindPath()
    {
        //reset lists
        openSet = new List<Node>();
        closedSet = new List<Node>();
        path = new List<Node>();

        //add open node on starting position
        openSet.Add(new Node(start, -1, target));

        //maximum amount of steps to be evaluated
        const int maxSteps = 100;
        //path finding loop
        for (int i = 0; i < maxSteps; i++)
        {
            //select first open node
            List<int> currentIndices = new List<int>() { 0 };

            //loop throught open nodes to find optimal one
            for (int j = 1; j < openSet.Count; j++)
            {
                //if this node is closer to target, select only it
                if (openSet[j].distance < openSet[currentIndices[0]].distance)
                {
                    currentIndices.Clear();
                    currentIndices.Add(j);
                }
                //if this node is of equal distance, add to selected
                else if (openSet[j].distance <= openSet[currentIndices[0]].distance)
                {
                    currentIndices.Add(j);
                }
            }
            
            //select one node index from selected
            int currentIndex = currentIndices[Random.Range(0, currentIndices.Count)];

            //close node
            closedSet.Add(openSet[currentIndex]);
            openSet.RemoveAt(currentIndex);
            //create reference to closed node
            Node currentNode = closedSet[closedSet.Count - 1];

            //check if node has reached target
            if (currentNode.distance < 1.0f)
            {
                //generate path
                path.Add(currentNode);
                while (path[path.Count - 1].parentIndex != -1)
                {
                    path.Add(closedSet[path[path.Count - 1].parentIndex]);
                }

                //generate line renderer visualization
                lr.positionCount = 0;
                foreach (var node in path)
                {
                    AddLRSegment(node.position);
                }

                //end pathfinding
                return;
            }

            //add neighbours
            foreach (var direction in Directions)
            {
                Vector2Int newPosition = currentNode.position + direction;
                //if tile is not valid for path, ignore
                if(tilemap.GetTile(new(newPosition.x, newPosition.y)) != null) continue;
                
                bool isValid = true;
                //check for duplicate in open set
                foreach (var node in openSet)
                {
                    if (node.position == newPosition)
                    {
                        isValid = false;
                        break;
                    }
                }
                //skip node if a duplicate exists
                if(!isValid) continue;
                
                //check for duplicate in closed set
                foreach (var node in closedSet)
                {
                    if (node.position == newPosition)
                    {
                        isValid = false;
                        break;
                    }
                }
                //skip node if a duplicate exists
                if(!isValid) continue;

                //add new node to open set
                Node newNode = new(newPosition, closedSet.Count - 1, target);
                openSet.Add(newNode);
            }
        }
        Debug.Log("could not find path");
        Debug.Log(closedSet[closedSet.Count - 1].position);
    }

    /* ---- node class ---- */
    public class Node
    {
        public Vector2Int position;
        public int parentIndex;
        public float distance;

        public Node(Vector2Int position, int parentIndex, Vector2Int target)
        {
            this.position = position;
            this.parentIndex = parentIndex;
            this.distance = Vector2Int.Distance(position, target);
        }
    }
}