using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using Random=UnityEngine.Random;

public class Stack : MonoBehaviour
{
    private GameManager gameManager;

    [Header("Array of Materials")]
    public List<Material> colors;

    int sizeRandom;
    public GameObject[] arr = new GameObject[6]; 
    Material currMat;

    private Tile levelTile;
    public bool Dragging = false;
    [SerializeField] public bool Placed = false;
    public Vector3 originalPosition;
    
    Vector3 mousePosition;

    GameObject[] tils;
    public List<GameObject> sample;
    GameObject[] stacks;
    [HideInInspector]
    public GameObject current;
    GameObject Manager;

    public bool occupiedStack = false;

    void Awake(){
        // fetching all colored tiles, level tiles, and setting up the materials' array
        arr = GameObject.FindGameObjectsWithTag("Player");
        tils = GameObject.FindGameObjectsWithTag("Tile");
        
        Manager = GameObject.FindGameObjectWithTag("GameController");
        colors = new List<Material>(6);
        for(int i=0; i<arr.Length; i++){
            colors.Add(arr[i].GetComponent<Renderer>().material);
        }
        originalPosition = transform.position;
    }
    private Vector3 GetMousePosition(){
        return Camera.main.WorldToScreenPoint(this.gameObject.transform.position);
    }
    void Start()
    {
        // creating a random size stack and assigning materials to the materials according to the size assigned
        gameManager = Manager.GetComponent<GameManager>();
        sizeRandom = System.Convert.ToInt32(Random.Range(3, arr.Length-1));
        sample = new List<GameObject>();

        // if size if less than 4 then all tiles will have same colors
        if(sizeRandom < 4){
            currMat = colors[Random.Range(0, colors.Count-1)];
            for(int i=0; i<sizeRandom; i++){
                arr[i].GetComponent<Renderer>().material = currMat;
                sample.Add(arr[i]);
            }
        }

        else if(sizeRandom >= 4){
            for(int i=0; i<sizeRandom; i++){
                currMat = colors[Random.Range(0, colors.Count-1)];
                if(i == 0){
                    arr[i].GetComponent<Renderer>().material = currMat;
                    sample.Add(arr[i]);
                }

            // If the material matches the previous one, add to the list
                if ( i > 0 && arr[i].GetComponent<Renderer>().material == sample[i-1].GetComponent<Renderer>().material) {
                    sample.Add(arr[i]);
                } 
                else {
                // If the material does not match, find the correct position to insert
                    int insertIndex = sample.FindIndex(gO => gO.GetComponent<Renderer>().material == currMat);
                    if (insertIndex == -1) {
                        sample.Add(arr[i]);
                    } 
                    else {
                        sample.Insert(insertIndex, arr[i]);
                    }
                }
            }
        }

        for(int i=0; i<sample.Count; i++){
            GameObject tile;
            tile = Instantiate(sample[i], this.gameObject.transform.localPosition + new Vector3(0, i*5, 0), Quaternion.Euler(90,0, 90), this.gameObject.transform);
            tile.tag = "NoUse";
        }
    }

    void Update()
    {
        stacks = GameObject.FindGameObjectsWithTag("Stack");
        
        if(Placed){
            return;
        }
        if(Input.GetMouseButtonDown(0)){
            
            mousePosition = Input.mousePosition;
            if(Mathf.Abs(GetMousePosition().x - mousePosition.x) < 50f && Mathf.Abs(GetMousePosition().y - mousePosition.y) < 50){
                Dragging = true;
            }
            else Dragging = false;
        }

        //when being dragged
        if(Dragging){
            float closest = Mathf.Infinity;
            foreach(GameObject tile in tils){
                Vector3 diff = transform.position - tile.transform.position;
                float currDistance = diff.sqrMagnitude;
                if(currDistance < closest){
                    current = tile;
                    levelTile = current.GetComponent<Tile>();
                    closest = currDistance;
                }
            }
            transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 440f));
        }

        // if mouse is released
        if(Input.GetMouseButtonUp(0) && Dragging == true){
            Dragging = false;
            if(current.GetComponent<Tile>().FindDistance() && current.GetComponent<Tile>().occupied == false){
                transform.position = current.transform.position + new Vector3(0, 5f, 0f);
                Placed = true;
                gameManager.checkPlaced(this.gameObject);
                current.GetComponent<Tile>().occupied = true;
                ++gameManager.occupiedTiles;
                checkDistance();
                if(gameManager.swapping == false){

                IEnumerator keepCheck = keepChecking(1);
                StartCoroutine(keepCheck);
                }
                if(gameManager.occupiedTiles == tils.Length){
                    gameManager.GameOver();
                }
            }
            else{
                transform.position = originalPosition;
            }
        }
    }
    
    // function to check whether two stacks are close enough to swap
    public void checkDistance(){
        if(gameManager.swapping == false){
        for(int i=0; i<stacks.Length; i++){
            if(stacks[i] != this.gameObject && stacks[i].GetComponent<Stack>().Placed && this.gameObject.GetComponent<Stack>().Placed){
                if(Vector3.Distance(stacks[i].gameObject.transform.position, this.gameObject.transform.position) < 70){
                    gameManager.checkAround(stacks[i], this.gameObject);
                }
            }
        }
        }
    }
    
    // to keep checking even after the stack has swapped once after being placed
    private IEnumerator keepChecking(int seconds){
        while(true){
            yield return new WaitForSeconds(seconds);
            checkDistance();
        }
    }
}
