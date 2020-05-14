using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class videoPlayerScript : MonoBehaviour
{
    private GameObject player;
    private bool endVideo = false;
    private bool play = false;
    private bool pause = false;
    private bool resume = false;

    // Start is called before the first frame update
    void Start()
    {
        CreateVideo();
    }

    //Create RenderTexture
    private RenderTexture createRenderTexture()
    {
        RenderTexture render = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
        render.Create();
        return render;
    }

    //Set url method
    public void PlayVideo(bool playVideo)
    {
        play = playVideo;
    }

    public void PauseVideo(bool pauseVideo)
    {
         pause = pauseVideo;
    }

    public void ResumeVideo(bool resumeVideo)
    {
        resume = resumeVideo;
    }
    public void StopVideo(bool stopVideo)
    {
        endVideo = stopVideo;
    }

    // Update is called once per frame
    void Update()
    {
        var videoPlayer = GetComponent<UnityEngine.Video.VideoPlayer>();
        if (play | Input.GetKeyDown("space"))
        {
            videoPlayer.url = "https://alexaapi.compositegrid.com/companyVideo";
            videoPlayer.frame = 100;
            videoPlayer.enabled = true;
            videoPlayer.Play();
            //Debug.Log("Playing");
            play = false;
        }
        if (pause)
        {
            videoPlayer.Pause();
            pause = false;
        }
        if (resume)
        {
            videoPlayer.Play();
            resume = false;
        }

        videoPlayer.loopPointReached += EndReached;

        void EndReached(UnityEngine.Video.VideoPlayer vp)
        {
            vp.playbackSpeed = vp.playbackSpeed / 10.0F;
            endVideo = true;
        }

        if (endVideo)
        {
            //videoPlayer.targetTexture.Release();
            Destroy(videoPlayer);
            CreateVideo();
            endVideo = false;
            
        }

    }

    void CreateVideo()
    {
        Shader shader = Shader.Find("Sprites/Default");
        player = GameObject.Find("Video Player");
        GameObject quad = GameObject.Find("Quad");
        Material videoMaterial = new Material(shader);
        RenderTexture renderTexture = createRenderTexture();
        videoMaterial.mainTexture = renderTexture;

        //// VideoPlayer added to GameObject
        var videoPlayer = player.AddComponent<UnityEngine.Video.VideoPlayer>();
        quad.GetComponent<MeshRenderer>().material = videoMaterial;

        //Disable VideoPlayer component at the start
        videoPlayer.enabled = false;

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        videoPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        //videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our Scene to be visible through the video being played.
        videoPlayer.targetCameraAlpha = 0.5F;

        //Set the video target texture
        videoPlayer.targetTexture = renderTexture;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        //videoPlayer.url = "";

        // Skip the first 100 frames.
        //videoPlayer.frame = 100;

        // Restart from beginning when done.
        videoPlayer.isLooping = false;


        // Each time we reach the end, we slow down the playback by a factor of 10.
        //videoPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        //videoPlayer.Play();


    }
}
