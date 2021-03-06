using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class PanelController : MonoBehaviour
{
    protected Renderer render;
    protected Color black = new Color(0, 0, 0, 1);
    protected Color white = new Color(255, 255, 255, 1);
    protected Color[] colors = new Color[2];
    protected int state;
    protected int interState;
    protected int totalLoops;
    protected int time;
    protected int timeDelay = 5; //wait in seconds for pausing
    protected int timeLeft; //count to this time
    protected string stringTime;
    protected SerialPort triggerBox;
    protected Byte[] data = { (Byte)0 };
    protected bool serialSent;
    



    // Start is called before the first frame update
    void Start()
    {
        //Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(120);

        render = gameObject.GetComponent<Renderer>();
        state = 1;
        interState = 1;
        totalLoops = 40;
        colors[0] = white;
        colors[1] = black;
        stringTime = DateTime.Now.ToString("mmss");
        time = Int16.Parse(stringTime);
        timeLeft = time + timeDelay;
        serialSent = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 100;

        openTriggerBoxPort();
        

    }

    // Update is called once per frame
    void Update()
    {

        
        stringTime = DateTime.Now.ToString("mmss");
        time = Int16.Parse(stringTime);

        StartCoroutine(colorSwitch());


    }

    

    void OnDestroy()
    {
        closeTriggerBoxPort();
    }

    IEnumerator colorSwitch()
    {
        if( state == 1)
        {
            if(serialSent == false)
            {
                writeWithThread();
                serialSent = true;
            }
            
            for (int interState = 1; interState <= totalLoops; interState++)
            {
                if((interState % 2) == 0)
                {
                    render.material.color = colors[0]; // black
                }
                else
                {
                    render.material.color = colors[0]; // white
                }
                
                yield return null;

            }

            state = 2;

        }

        if(state == 2)
        {
            render.material.color = colors[1]; // black
            //Time.timeScale = 0;

            //Debug.Log("The game is paused. Please wait............");

            if (time >= timeLeft - 1)
            {
                //Debug.Log("### THE GAME IS BACK ###");

                //Rsume the game and restart timer again
                state = 1;
                serialSent = false;
                stringTime = DateTime.Now.ToString("mmss");
                timeLeft = (Int16.Parse(stringTime) + timeDelay);
                //Time.timeScale = 0;

            }

            yield return null;

        }

        

    }

    void openTriggerBoxPort()
    {
        triggerBox = new SerialPort("COM3");
        triggerBox.Open();
        triggerBox.ReadTimeout = 5000;

        // Set the port to an initial state
        data[0] = 0x00;
        triggerBox.Write(data, 0, 1);
        Thread.Sleep(10);

    }

    void closeTriggerBoxPort()
    {
        // Reset the port to its default state
        data[0] = 0xFF;
        triggerBox.Write(data, 0, 1);
        Thread.Sleep(10);

        triggerBox.Close();
    }

    void writeToPort()
    {


        // Set Bit 0, Pin 2 of the Output(to Amp) connector
        data[0] = 0x01;
        triggerBox.Write(data, 0, 1);
        Thread.Sleep(1);

        // Reset Bit 0, Pin 2 of the Output(to Amp) connector
        data[0] = 0x00;
        triggerBox.Write(data, 0, 1);
        Thread.Sleep(1);
        //Debug.Log("writing with thread");


    }

    void writeWithThread()
    {
        Thread triggerBoxThread = new Thread(writeToPort);
        triggerBoxThread.Start();

    }



}
