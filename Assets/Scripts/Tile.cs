using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool occupied = false;
    private Stack stack1;
    GameObject[] stacks;

    GameObject closest;

    void Update(){
        stacks = GameObject.FindGameObjectsWithTag("Stack");
    }
    public void printSacks(){
        foreach(GameObject stack in stacks){
            Debug.Log(stack);
        }   
    }

    // Finding which stack is closest to the specific tile
    public bool FindDistance(){
        float currDistance = Mathf.Infinity;
        foreach(GameObject stack in stacks){
           Vector3 diff = transform.position - stack.transform.position; 
           float distance = diff.magnitude;
           if(distance < currDistance){
            closest = stack;
            stack1 = closest.GetComponent<Stack>();
            currDistance = distance;
           }
        }
        if(stack1 != null){
            if(Vector3.Distance(transform.position, stack1.gameObject.transform.position) < 20){
                return true;
            }
            else {
                return false;
            }
    }
        else{ 
            return false;
        }
    }
}
