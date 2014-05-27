using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof(AudioSource))]
public class MicrophoneController : MonoBehaviour 
{
    private static Material lineMaterial = null;

    private static int SAMPLE_RATE = 8192;
    private static int SAMPLE_SIZE = 256;
    private static int SMOOTH_SAMPLE_SIZE = 32;
    
    private float[] samples;
    private float[] spectrumSamples;

    private float[] smoothVolumeSamples;
    private int smoothVolumeSamplesCurrentIndex;

    public float[] SmoothVolumeSamples
    {
        get { return smoothVolumeSamples; }
    }

    public int SmoothVolumeSamplesCurrentIndex
    {
        get { return smoothVolumeSamplesCurrentIndex; }
    }

    private float smoothVolume;
    private float smoothVolumeVelocity;

    private float smoothFrequency;
    private float smoothFrequencyVelocity;

    public float SmoothFrequency
    {
        get { return smoothFrequency; }
    }

    private AudioSource audioSource;
    private AudioClip micClip;

	public void Start ()
    {
        this.samples = new float[SAMPLE_SIZE];
        this.smoothVolumeSamples = new float[SMOOTH_SAMPLE_SIZE];
        this.spectrumSamples = new float[SAMPLE_RATE / 2];

        this.micClip = Microphone.Start(Microphone.devices[0], true, 15, SAMPLE_RATE);
        while (!(Microphone.GetPosition(null) > 0)) { } // Wait until the recording has started

        this.audioSource = this.audio;
        this.audioSource.clip = micClip;
        this.audioSource.loop = true;
        this.audioSource.mute = true;
        this.audioSource.Play();
	}

    public void OnDestroy()
    {
        Microphone.End(null);
    }

    private void UpdateVolume()
    {
        audioSource.GetOutputData(samples, 0);

        float a = 0;
        for (int i = 0; i < SAMPLE_SIZE; i++)
            a += Mathf.Max(Mathf.Abs(Mathf.Sqrt(samples[i]) * .5f) - .2f, 0f) * .75f;

        smoothVolume = Mathf.SmoothDamp(smoothVolume, a, ref smoothVolumeVelocity, .1f);

        // Circular array
        smoothVolumeSamples[smoothVolumeSamplesCurrentIndex] = smoothVolume;
        smoothVolumeSamplesCurrentIndex = smoothVolumeSamplesCurrentIndex + 1 >= SMOOTH_SAMPLE_SIZE ? 0 : smoothVolumeSamplesCurrentIndex + 1;
    }

    private void UpdateMaxFrequency()
    {
        audioSource.GetSpectrumData(spectrumSamples, 0, FFTWindow.Blackman);
        
        float maxBin = 0.0f;
        int maxBinIndex = 0;

        for (int i = 0; i < spectrumSamples.Length; i++)
        {
            if (maxBin < spectrumSamples[i])
            {
                maxBin = spectrumSamples[i];
                maxBinIndex = i;
            }
        }

        smoothFrequency = Mathf.SmoothDamp(smoothFrequency, maxBinIndex * SAMPLE_RATE / (float)spectrumSamples.Length, ref smoothFrequencyVelocity, .25f);
    }

    public void Update()
    {
        UpdateVolume();
        UpdateMaxFrequency();
    }

    public void OnGUI()
    {
        DrawGraph(new Rect(10f, 25f, 150f, 15f), samples, Color.white);
        DrawGraph(new Rect(170f, 25f, 150f, 15f), smoothVolumeSamples, Color.yellow);
        DrawGraph(new Rect(330f, 25f, 150f, 15f), spectrumSamples, Color.red);
    }

    private static void InitializeLineMaterial()
    {
        if(!lineMaterial)
        {
            var shaderText =
                "Shader \"Line Material\" {" +
                "   Properties { _Color (\"Line Color\", Color) = (1,1,1,0) }" +
                "   SubShader {" +
                "       BindChannels { " +
                "           Bind \"Color\", color " +
                "           Bind \"Vertex\", vertex " +
                "       }" +
                "       Pass {}" +
                "   }" +
                "}";
            lineMaterial = new Material(shaderText);
        }
    }

    public static void DrawGraph(Rect container, float[] keyframes, Color color)
    {
        InitializeLineMaterial();

        GL.PushMatrix();
        lineMaterial.SetPass(0);
        GL.LoadOrtho();
        GL.Begin(GL.LINES);

        // Transform container
        container.x /= Screen.width;
        container.width /= Screen.width;
        container.y /= Screen.height;
        container.height /= Screen.height;

        // Normalize graph
        float maxValue = 0f;

        for (int i = 1; i < keyframes.Length; i++)
            if (Mathf.Abs(keyframes[i]) > maxValue)
                maxValue = .5f;// Mathf.Abs(k);

        for (int i = 1; i < keyframes.Length; i++)
        {
            float prevX = ((i - 1) / (float)keyframes.Length) * container.width + container.x;
            float x = (i / (float)keyframes.Length) * container.width + container.x;

            GL.Color(color);

            GL.Vertex(new Vector3(prevX, keyframes[i - 1] * .5f * container.height / maxValue + container.y, 0f));
            GL.Vertex(new Vector3(x, keyframes[i] * .5f * container.height / maxValue + container.y, 0f));
        }

        GL.End();
        GL.PopMatrix();
    }
}
