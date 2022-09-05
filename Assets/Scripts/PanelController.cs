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
    protected float time;
    protected float timeDelay = 3.0f; //wait in seconds for pausing
    protected string stringTime;
    protected SerialPort triggerBox;
    protected Byte[] data = { (Byte)0 };
    protected bool serialSent;
    protected float rate = 90.0f;
    protected float currentFrameTime;
    



    // Start is called before the first frame update
    void Start()
    {
        //Unity.XR.Oculus.Performance.TrySetDisplayRefreshRate(120);

        render = gameObject.GetComponent<Renderer>();
        state = 1;
        interState = 1;
        totalLoops = 2;
        colors[0] = white;
        colors[1] = black;
        serialSent = false;
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 9999;
        //Application.targetFrameRate = 100;
        currentFrameTime = Time.realtimeSinceStartup;

        openTriggerBoxPort();
        

    }

    // Update is called once per frame
    void Update()
    {

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

            if (serialSent == false)
            {
                writeWithThread();
                serialSent = true;
            }
            
            for (int interState = 1; interState <= totalLoops; interState++)
            {
                if((interState % 2) == 0)
                {
                    //render.material.color = colors[1]; // black
                    render.enabled = true;
                }
                else
                {
                    //render.material.color = colors[0]; // white
                    render.enabled = true;
                }

                yield return null;
                

            }

            state = 2;
            time = Time.realtimeSinceStartup;

        }

        if(state == 2)
        {
            //render.material.color = colors[1]; // black
            render.enabled = false;
            float theTime = Time.realtimeSinceStartup;

            //Debug.Log("The game is paused. Please wait............");

            if (theTime >= time + timeDelay)
            {
                //Debug.Log("### THE GAME IS BACK ###");

                //Rsume the game and restart timer again
                state = 1;
                serialSent = false;

            }

            yield return null;

        }

        

    }

    void slowDown()
    {
        currentFrameTime += 1.0f / rate;
        var t = Time.realtimeSinceStartup;
        var sleepTime = currentFrameTime - t - 0.01f;
        if (sleepTime > 0)
        {
            Thread.Sleep((int)(sleepTime * 1000));
        }   
        while (t < currentFrameTime)
        {
            t = Time.realtimeSinceStartup;
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
