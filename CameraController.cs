using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour
{
    private string mode = "";

    public bool isDim = false;
    public bool strikeDim = false;
    private float dimMax = 0.8f;
    private float dimSpeed = 0.035f;
    private float currentDim = 0f;
    public float strikeDelay = 0f;
    public float dimDelay = 0f;

    public bool fixedCam = false;

    public float xConstraint;
    public float yConstraint;

    public float xShake;
    public float yShake;
    public float duoShake;
    public float shakeIntensity;
    private const float frameMultiplier = 60;

    private Vector2 currentPos = new Vector2(0,0);
    private Vector2 targetPos = new Vector2(0, 0);
    private Vector2 posOffset = new Vector2(0, 0);
    private Vector2 targetOffset = new Vector2(0, 0);

    private float baseCamSpeed = 1f;
    private float baseCamSmoothX = 12f;
    private float baseCamSmoothY = 16f;
    private float camSpeedScale = 1f;
    private float deltaOffset = 60f;


    public float targetZoom = 180;
    private float currentZoom = 180;
    public float maxZoom = 270;
    private float minZoom = 180f;
    private float zoomSpeedScale = 1.0f;
    private float zoomSpeedAmt = 1f;


    private float baseCamXZoom;
    private float baseCamYZoom;
    private float targetXZoom;
    private float targetYZoom;

    public float xMaxBase = 510;
    public float yMaxBase;
    public float yMinBase;
    private float xMax;
    private float yMax;
    private float yMin;

    public bool endShake = false;


    private List<Vector2> targets;



    public float getZoom() {
        return currentZoom;
    }

    private void FixedUpdate()
    {
        dim();
        if (!fixedCam)
        {
            if (mode == "player")
            {
                findAvgPosition();
            }
            else if (mode == "ball")
            {
                targetZoom = 180f;
                GameObject ball = GameObject.FindGameObjectWithTag("Ball");
                if (ball != null)
                {
                    targetPos = ball.transform.position;
                }
            }
            currentZoom -= ((currentZoom - targetZoom) / (baseCamSmoothY)) * zoomSpeedAmt * Time.deltaTime * deltaOffset * InputControl.globalTime;
            moveCamera();
            GetComponent<Camera>().orthographicSize = currentZoom;
            clamp();
              
        }
        transform.position = new Vector3(currentPos.x + posOffset.x, currentPos.y + posOffset.y, transform.position.z);
    }

    //This method handles the dimming effect for the blade/beam supers
    public void dim() {
        if (dimDelay > 0) dimDelay--;
        if (strikeDelay > 0) strikeDelay -= 1f * InputControl.globalTime;
        if (isDim && currentDim < dimMax &&!strikeDim) {
            currentDim += dimSpeed;
        }
        if (!isDim && currentDim > 0 && dimDelay <= 0 && !strikeDim)
        {
            currentDim -= dimSpeed;
        }

        if (strikeDim && currentDim < dimMax)
        {
            currentDim += dimSpeed * 5;
        }
        if (strikeDim && currentDim > 0 && strikeDelay <= 0)
        {
            currentDim -= dimSpeed;
        }

        if (currentDim >= dimMax) {
            currentDim = dimMax;
        }
        else if (currentDim <= 0) {
            currentDim = 0;
        }
        if (strikeDelay <= 0) {
            strikeDim = false;
        }
        transform.Find("Dimmer").GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, currentDim);
    }

    public void setPos(Vector3 p) {
        currentPos = p;
    }

    

    
    private void clamp() {
        //These 3 lines calculate where the camera should stop based on the current zoom level.
        //Makes sure that the blastzones are never visible
        xMax = xMaxBase - ((Mathf.Max(0, currentZoom - 180)) * 1.7f);
        yMax = yMaxBase - (Mathf.Max(0, currentZoom - 180));
        yMin = yMinBase + (Mathf.Max(0, currentZoom - 180));
        if (currentPos.x > xMax)
        {
            currentPos.x = xMax;
        }
        else if (currentPos.x < xMax * -1) {
            currentPos.x = xMax * -1;
        }
        if (currentPos.y > yMax) {
            currentPos.y = yMax;
        }
        if (currentPos.y < yMin) {
            currentPos.y = yMin;
        }

    }

    private void moveCamera() {
        //Move the camera toward its target
        Vector2 velocity = new Vector2(0, 0);
        velocity.x = baseCamSpeed * deltaOffset * ((targetPos.x - currentPos.x) / baseCamSmoothX);
        velocity.y = baseCamSpeed * deltaOffset * ((targetPos.y - currentPos.y) / (baseCamSmoothY));


        if (Mathf.Abs(currentPos.x - targetPos.x) < 1) {
            currentPos.x = targetPos.x;
        }
        if (Mathf.Abs(currentPos.y - targetPos.y) < 1)
        {
            currentPos.y = targetPos.y;
        }


        currentPos += (velocity * Time.deltaTime * InputControl.globalTime * camSpeedScale) * new Vector2(1, 1);
    
        
    }

    private void findAvgPosition() {
        //This method finds all objects in the scene with the CameraAnchor attached, and then parses through them to find
        //The middle position between the two farthest objects
        Vector2 avg = new Vector2(0, 0);
        GameObject[] players = GameObject.FindGameObjectsWithTag("CameraAnchor");
        Vector2 highest = new Vector2(0, 0);
        Vector2 lowest = new Vector2(0, 0);
        if (players.Length > 0) {
            highest = players[0].transform.position;
            lowest = players[0].transform.position;
        }

        //Iterate through all "CameraAnchor"s and update the highest and lowest positions
        for (int i = 0; i < players.Length; i++) {
            
            if (players[i].transform.position.x > highest.x) {
                highest.x = players[i].transform.position.x;
            }
            if (players[i].transform.position.y > highest.y)
            {
                highest.y = players[i].transform.position.y;
            }
            if (players[i].transform.position.x < lowest.x)
            {
                lowest.x = players[i].transform.position.x;
            }
            if (players[i].transform.position.y < lowest.y)
            {
                lowest.y = players[i].transform.position.y;
            }
            
            
        }
        if (lowest.x < xMaxBase * -1) lowest.x = xMaxBase * -1;
        if (highest.x > xMaxBase) highest.x = xMaxBase;
        avg = (highest + lowest) / 2;

        targetPos = (avg + new Vector2(0, 10f));
        
        
       

        //Formula for caluclating zoom level
        float rawZoom = (((highest.x - lowest.x) * 0.9f) + (((highest.y + 50) - lowest.y) * 2f) /1.7f) + 250;
        targetZoom = rawZoom / 3.55556f;
        if (targetZoom < minZoom) {
            targetZoom = minZoom;
        }
        if (targetZoom > maxZoom)
        {
            targetZoom = maxZoom;
        }
        


    }

    // Start is called before the first frame update
    void Start()
    {

        currentPos = transform.position;
        currentZoom = GetComponent<Camera>().orthographicSize;
        
    }

    


    //Change the camera's target position
    public void setTargetPos(Vector2 target) {
        targetPos = target;
    }

    //Force the camera to its current target
    public void forceTarget() {
        transform.position = new Vector3(targetPos.x, targetPos.y, transform.position.z);
    }

    //Change the camera mode
    public void changeMode(string m) {
        mode = m;
    }

    public void setTargetPos(float x, float y) {
        setTargetPos(new Vector2(x, y));
    }

   

    public void startShake(float magnitude, float duration, Vector2 multiplier, float decay = 0) {
        StartCoroutine(shake(magnitude, duration, multiplier, decay));
    }

    public void startShake2(float magnitude, float duration, Vector2 multiplier, float decay = 0)
    {
        StartCoroutine(shake2(magnitude, duration, multiplier, decay));
    }


    

    //Function for camera shake
    public IEnumerator shake(float magnitude, float duration, Vector2 multiplier, float decay = 0)
    {
        duration /= frameMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            magnitude -= decay;
            if (magnitude <= 0) magnitude = 0;
            float x = Random.Range(-1f, 1f) * magnitude * multiplier.x;
            float y = Random.Range(-1f, 1f) * magnitude * multiplier.y;

            posOffset = new Vector2(x, y);
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        posOffset = new Vector2(0,0);
    }

    //This one is used only for the end of game transition to the results screen
    public IEnumerator shake2(float magnitude, float duration, Vector2 multiplier, float decay = 0)
    {
        duration /= frameMultiplier;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            magnitude -= decay;
            if (magnitude <= 0) magnitude = 0;
            float x = Random.Range(-1f, 1f) * magnitude * multiplier.x;
            float y = Random.Range(-1f, 1f) * magnitude * multiplier.y;

            posOffset = new Vector2(x, y);
            GameObject.Find("Camera").transform.localPosition = new Vector3(0 + posOffset.x, 0 + posOffset.y, GameObject.Find("Camera").transform.localPosition.z);
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate();
        }
        posOffset = new Vector2(0, 0);
        GameObject.Find("Camera").transform.localPosition = new Vector3(0 + posOffset.x, 0 + posOffset.y, GameObject.Find("Camera").transform.localPosition.z);
        yield return new WaitForSeconds(0.05f);
        endShake = true;
    }
}
