using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using NativeWebSocket;
using OpenCvSharp.Demo;


public class player : MonoBehaviour
{
    public Animator animator;
    public AnimationClip[] animations;
    private float idleAnimation = 0f;
    private float goneIdle = 5f;
    private bool isTalking = false;
    private bool stopTalking = false;
    private float current = 0;
    private float delayWeight = 0;
    private string line = "";
    private string response = "";
    public GameObject unityChan;
    private int y_rotation = 0;
    private int prev_rotation = 0;
    public GameObject camera;
    public GameObject videoPlayer;
    WebSocket websocket;


    // Start is called before the first frame update
    async void Start()
    {
        animator = GetComponent<Animator>();
        camera = GameObject.Find("RawImage");
        videoPlayer = GameObject.Find("Video Player");
        unityChan = GameObject.Find("unitychan");
        SetIdleTime();

        GameObject main_camera = GameObject.Find("Main Camera");

        websocket = new WebSocket("wss://alexaapi.compositegrid.com/ws");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            //Debug.Log("OnMessage!");
            //Debug.Log(bytes);

            // getting the message as a string
            line = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("SocketMessage! " + line);
        };

        // waiting for messages
        await websocket.Connect();
    }

    // Update is called once per frame
    void Update()
    {
        //Face tracking
        y_rotation = camera.GetComponent<FaceTracker>().rotation;
        Debug.Log("Received: " + y_rotation);

        //Rotate once
        if (y_rotation != null && y_rotation != prev_rotation)
        {
            float turning = y_rotation - unityChan.transform.rotation.eulerAngles.y;
            unityChan.transform.Rotate(0.0f, turning, 0.0f, Space.Self);
            prev_rotation = y_rotation;
        }

        //Checks for a change of state
        watcher();
        
        //This cycles through the idle animations to be played after a certain amount of idle time
        idleAnimation += Time.deltaTime;
        if (isTalking)
        {
            animator.Play("WAIT00", -1, 0f);
        }

        if (idleAnimation > goneIdle)
        {
            idleAnimation = 0;
            SetIdleTime();
            int num = Random.Range(1, 3);
            if (num == 1)
            {
                animator.Play("WAIT01", -1, 0f);
                idleAnimation = -5 - (this.animator.GetCurrentAnimatorClipInfo(0))[0].clip.length;
            }
            else
            {
                animator.Play("WAIT02", -1, 0f);
                idleAnimation = -7 - (this.animator.GetCurrentAnimatorClipInfo(0))[0].clip.length;
            }

        }

        //This is the temporary toggle for the mouth animations to start
        if (isTalking)
        {
            Debug.Log("Talking");
            animator.SetBool("isTalking", isTalking);
            animator.CrossFade("MTH_A", 0);
            idleAnimation = -5;
            current = 1;
            

        }
        //Stop talking when state is IDLE
        if (stopTalking)
        {
            animator.SetBool("isTalking", isTalking);
            current = 0;
            //animator.CrossFade("default@unitychan", 0);
        }

        //fix this to default layer if you're not talking
        animator.SetLayerWeight(1, current);

    }

    private void watcher()
    {
        switch (line)
        {
            case "Speaking":
                isTalking = true;
                stopTalking = false;
                break;
            case "Playing video":
                //Play video hosted on /companyVideo
                videoPlayer.GetComponent<videoPlayerScript>().PlayVideo(true);
                break;
            case "Pause":
                //Pause video
                videoPlayer.GetComponent<videoPlayerScript>().PauseVideo(true);
                break;
            case "Resume":
                //Resume video
                videoPlayer.GetComponent<videoPlayerScript>().ResumeVideo(true);
                break;
            case "Stop":
                //Stop video
                videoPlayer.GetComponent<videoPlayerScript>().StopVideo(true);
                break;
            case "Idle":
                isTalking = false;
                stopTalking = true;
                break;
            default:
                isTalking = false;
                stopTalking = false;
                break;
        }

        line = "";

    }

 /*   IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            string[] pages = uri.Split('/');
            int page = pages.Length - 1;

            *//*if (webRequest.isNetworkError)
            {
                Debug.Log(pages[page] + ": Error: " + webRequest.error);
            }
            else
            {
                Debug.Log(pages[page] + ":\nReceived: " + webRequest.downloadHandler.text);
            }*//*
            response = webRequest.downloadHandler.text;

            
        }
    }*/

    void SetIdleTime()
    {
        goneIdle = Random.Range(5, 8);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}
