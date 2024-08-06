using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;
using UnityEngine;
using UnityEditor.PackageManager.UI;
using TMPro;
using System;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;

public class GameManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField]GameObject pausePanel;

    public bool swapping = false;
    [SerializeField]GameObject quitPanel;
    [SerializeField]GameObject gameOverPanel;
    [SerializeField]GameObject lvlCompPanel;
    [SerializeField]TMPro.TMP_Text score;
    [SerializeField]TMPro.TMP_Text Points;
    [SerializeField]TMPro.TMP_Text collected;
    GameObject newStack;
    [SerializeField]int requiredPoints;
    int scoreBoard = 0;

    [HideInInspector]
    public int occupiedTiles = 0;
    [SerializeField] GameObject emptyStack;

    void Awake(){
    }
    void Start()
    {
        Points.text = requiredPoints.ToString();
    }
    void Update()
    {
        
    }

    public void PlayGame(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

    public void CallToMain(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex-1);
    }

    public void Pause(){
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }

    public void Resume(){
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }
    
    public void Replay(){
        Time.timeScale = 1;
        SceneManager.LoadScene("Game");
    }

    public void GameOver(){
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
    }

    public void LevelComplete(){
        Time.timeScale = 0;
        lvlCompPanel.SetActive(true);
    }

    public void NextLevel(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }

// function that is called from stack script if two stacks are placed and are close enough to swap
    public void checkAround(GameObject surround, GameObject itself){

        Stack surroundStack = surround.GetComponent<Stack>();
        Stack itselfStack = itself.GetComponent<Stack>();

        List<GameObject> surroundSample = surroundStack.sample;
        List<GameObject> itselfSample = itselfStack.sample;

        // swap to the stack which is smaller in size
        if(surround.transform.childCount > itself.transform.childCount){

            StartCoroutine(swapSurround(surround, itself, surroundSample, itselfSample));
                
        }
        else{
            StartCoroutine(swapItself(surround, itself, surroundSample, itselfSample));
        }
    
    }

    // function to instantiate new stacks to be placed
    public void checkPlaced(GameObject stack){
        newStack = Instantiate(emptyStack, stack.GetComponent<Stack>().originalPosition, Quaternion.identity);
        newStack.AddComponent<Stack>();
        newStack.GetComponent<Stack>().enabled = true;
        newStack.tag = "Stack";
    }

    // keeping a check to score points once similar materials stack the count of 10
    public void checkPoints(GameObject stack){
        
        int size = stack.transform.childCount;
        String currMaterial = stack.transform.GetChild(size-1).GetComponent<Renderer>().material.name;
        int count = 1;
        for(int i=size-2; i>=0; i--){
            if(stack.transform.GetChild(i).GetComponent<Renderer>().material.name.StartsWith(currMaterial[0])){
                count++;
            }
            else{
                break;
            }
        }
        if(count >= 10){
            scoreBoard += count;
            score.text = scoreBoard.ToString();
            if(scoreBoard >= requiredPoints){
                LevelComplete();
                collected.text = scoreBoard.ToString();
            }
            StartCoroutine(delayDestroy(stack, size, count));
        }
    }

// once a stack goes empty it needs to be destroyed from the environment
    public void checkEmpty(GameObject stack){
        if(stack.GetComponent<Stack>().sample.Count == 0){
            stack.GetComponent<Stack>().current.GetComponent<Tile>().occupied = false;
            --occupiedTiles;
            Destroy(stack);
        }
    }

// co-routines called respectively to give space to the animations and keeping the actions aligned
    private IEnumerator swapSurround(GameObject surround, GameObject itself, List<GameObject> surroundSample, List<GameObject> itselfSample){
        while(surroundSample.Count >= 1 && surround.transform.GetChild(surroundSample.Count -1).GetComponent<Renderer>().material.name.StartsWith(itself.transform.GetChild(itselfSample.Count-1).GetComponent<Renderer>().material.name[0])){
            swapping = true;
                GameObject tile;
                tile = Instantiate(surround.transform.GetChild(surroundSample.Count-1).gameObject, itself.transform.localPosition + new Vector3(0, itselfSample.Count*5, 0), Quaternion.Euler(90,0, 90), itself.transform);
                tile.GetComponent<Renderer>().material = surround.transform.GetChild(surroundSample.Count -1).GetComponent<Renderer>().material;
                tile.tag = "NoUse";
                StartCoroutine(rotateAnimation(tile));
                FindObjectOfType<AudioManager>().Play("Swap");
                itselfSample.Add(surround.transform.GetChild(surroundSample.Count-1).gameObject);
                Destroy(surround.transform.GetChild(surroundSample.Count-1).gameObject);
                surroundSample.RemoveAt(surroundSample.Count-1);
                    checkEmpty(surround);
                    yield return new WaitForSeconds(.1f);
                }
        checkPoints(itself);
        swapping = false;   
    }
    private IEnumerator swapItself(GameObject surround, GameObject itself, List<GameObject> surroundSample, List<GameObject> itselfSample){
        while(itselfSample.Count >= 1 && surround.transform.GetChild(surroundSample.Count -1).GetComponent<Renderer>().material.name.StartsWith(itself.transform.GetChild(itselfSample.Count-1).GetComponent<Renderer>().material.name[0])){
            Debug.Log(surroundSample.Count -1);
            Debug.Log(itselfSample.Count-1);
            swapping = true;
                GameObject tile;
                tile = Instantiate(itself.transform.GetChild(itselfSample.Count-1).gameObject, surround.transform.localPosition + new Vector3(0, surroundSample.Count*5, 0), Quaternion.Euler(90,0, 90), surround.transform);
                tile.GetComponent<Renderer>().material = surround.transform.GetChild(surroundSample.Count -1).GetComponent<Renderer>().material;
                tile.tag = "NoUse";
                StartCoroutine(rotateAnimation(tile));
                FindObjectOfType<AudioManager>().Play("Swap");
                surroundSample.Add(itself.transform.GetChild(itselfSample.Count-1).gameObject);
                Destroy(itself.transform.GetChild(itselfSample.Count-1).gameObject);
                itselfSample.RemoveAt(itselfSample.Count-1);
                checkEmpty(itself);
                yield return new WaitForSeconds(.1f);
            }
          checkPoints(surround);    
          swapping = false;  
    }
    
    private IEnumerator delayDestroy(GameObject stack, int size, int count){
        for(int i=size-1; i>=size-count; i--){
                FindObjectOfType<AudioManager>().Play("Destroy");
                StartCoroutine(destroyAnimation(stack.transform.GetChild(i).gameObject));
                stack.GetComponent<Stack>().sample.RemoveAt(i);
                yield return new WaitForSeconds(.1f);
        }
        checkEmpty(stack);
    }

    private IEnumerator destroyAnimation(GameObject tile){
        for(int i=5; i>0; i--){
            if(tile != null){
            tile.transform.localScale -= tile.transform.localScale/i;
            yield return new WaitForSeconds(.05f);
            }
        }
            Destroy(tile);
    }

    private IEnumerator rotateAnimation(GameObject tile){
        for(int i=1; i<=10; i++){
        if(tile != null){
            tile.transform.eulerAngles = new Vector3(27*i, 0, 90);
            yield return new WaitForSeconds(.05f);
        }
        }
    }
}
