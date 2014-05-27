using UnityEngine;
using System.Collections;

public class SonarController : MonoBehaviour
{
    private static int LOOKUP_TEXTURE_SIZE = 32;

    public Gradient frequencyGradient;

    public MicrophoneController microphone;
    public Material[] materials;

    private Texture2D lookupTexture;
    private Color[] lookupTexturePixels = new Color[LOOKUP_TEXTURE_SIZE];

	public void Start ()
	{
        this.lookupTexture = new Texture2D(1, LOOKUP_TEXTURE_SIZE);
        this.lookupTexture.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < materials.Length; i ++)
            materials[i].SetTexture("_WaveTex", lookupTexture);
	}

    private void UpdateLookupTexture()
    {
        float[] smoothVolumeSamples = microphone.SmoothVolumeSamples;
        int smoothIndex = microphone.SmoothVolumeSamplesCurrentIndex;

        float freq = microphone.SmoothFrequency;
        Color freqColor = frequencyGradient.Evaluate(Mathf.Clamp01(freq / 150f));

        // Wrap around circular array :)
        for (int i = 0; i < lookupTexturePixels.Length; i++)
        {
            Color c = freqColor;
            c.a = smoothVolumeSamples[(smoothIndex + i) % LOOKUP_TEXTURE_SIZE];
            lookupTexturePixels[i] = c;
        }

        lookupTexture.SetPixels(lookupTexturePixels);
        lookupTexture.Apply();
    }

	public void Update () 
	{
        UpdateLookupTexture();
	}
}