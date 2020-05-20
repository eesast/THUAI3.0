using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logic.Constant;
using static Logic.Constant.Constant;
using Communication.Proto;
using Google.Protobuf;
using UnityEngine.UI;
using System.Net;
using static THUnity2D.Tools;
using Debug = UnityEngine.Debug;
using static Tools;
using System.Collections.Concurrent;

public class UserInterface : MonoBehaviour
{
    public Text scoreText;
    //public Text timeText;

    public Text taskText;
    public string taskString;

    public int[] scores= { -1, -1, -1, -1 };
    //private float time;
    public delegate void ChangeTimer(int interval);
    public ChangeTimer change;
    public int playMode = 1;
    public bool isPlaybackHidden = false;
    public bool isMethodBinded = false;

    public Slider speedSlider;
    public Button playButton;
    public Button pauseButton;
    public Button defaultButton;

    private int interval;
    

    private string[] colors = { "red", "blue", "yellow", "green" };

    // Start is called before the first frame update
    void Start()
    {
        taskString = "Tasks: ";
    }

    // Update is called once per frame
    void Update()
    {
        if(playMode!=1)
        {
            if (!isPlaybackHidden)
            {
                speedSlider.gameObject.SetActive(false);
                playButton.gameObject.SetActive(false);
                pauseButton.gameObject.SetActive(false);
                defaultButton.gameObject.SetActive(false);
                isPlaybackHidden = true;
            }
        }
        else
        {
            if(!isMethodBinded)
            {
                speedSlider.onValueChanged.AddListener((float value)=> { OnSpeedChanged(); });
                playButton.GetComponent<Button>().onClick.AddListener(OnPlay);
                pauseButton.GetComponent<Button>().onClick.AddListener(OnPause);
                defaultButton.GetComponent<Button>().onClick.AddListener(OnTimeScaleSetDefault);
                isMethodBinded = true;
                interval = (int)(50 / Mathf.Exp(6 * (speedSlider.value - 0.5f)));
            }
        }
        string sText = "Scores:";
        for(int i=0;i<4;i++)
        {
            if(scores[i]>=0)
            {
                sText += " "+colors[i] + "[" + scores[i] + "]";
            }
        }
        scoreText.text = sText;
        taskText.text = taskString;

    }

    public void Quit()
    {
        Application.Quit();
    }

    public void OnPlay()
    {
        change(interval);
    }
    public void OnPause()
    {
        change(-1);
    }
    public void OnSpeedChanged()
    {
        float value = speedSlider.value;
        interval = (int)(50/Mathf.Exp(6 * (value - 0.5f)));
        change(interval);
    }
    public void OnTimeScaleSetDefault()
    {
        speedSlider.value = 0.5f;
    }
}
