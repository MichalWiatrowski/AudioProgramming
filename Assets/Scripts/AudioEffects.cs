using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEffects : MonoBehaviour
{
    //Wild hold the sample values
    float[] samples;

    // Start is called before the first frame update
    void Start()
    { 
    }

    // Update is called once per frame
    void Update()
    {
    }

    public AudioClip Loop(AudioClip clipToModify, int timesToLoop)
    {
        //Create an array of samples based on the sample length * how many loops * channles
        samples = new float[(clipToModify.samples * timesToLoop) * clipToModify.channels];
        //Populate the samples array with the data from the audioclip
        clipToModify.GetData(samples, 0);
        //Counter will be used to track looping
        int counter = 0;
        //start after the sample from the original clip
        for (int i = clipToModify.samples + 1; i < samples.Length; i++)
        {
            //the current sample will be set with a sample from the original clip by using counter
            samples[i] = samples[counter];
            //increment counter
            counter++;
            //if counter is greater than original clip, reset counter
            if (counter > clipToModify.samples) counter = 0;
        }
        //Create a new audioclip, based on the length of the samples and channels
        AudioClip temp = AudioClip.Create("TemporaryAudioClip", samples.Length, clipToModify.channels, 44100, false);
        //Populate the audioclip with the samples
        temp.SetData(samples, 0);
        //Return the new audioclip
        return temp;  
    }
    //Reverses the audioclip and returns it
    public AudioClip ReverseAudio(AudioClip clipToModify)
    {
        //Calculate the sample size based on the original audio clip and its channels ammount
        samples = new float[clipToModify.samples * clipToModify.channels];
        //Populate the samples array with the data from the audioclip
        clipToModify.GetData(samples, 0);

        //Reverse the order of samples in the array
        System.Array.Reverse(samples);

        //Create a new audioclip, based on the length of the samples and channels
        AudioClip temp = AudioClip.Create("TemporaryAudioClip", samples.Length, clipToModify.channels, 44100, false);
        //Populate the audioclip with the samples
        temp.SetData(samples, 0);
        //Return the new audioclip
        return temp;
    }

    public AudioClip ModifyAmplitude(float amplifyAmount, AudioClip clipToModify)
    {
        //Calculate the sample size based on the original audio clip and its channels ammount
        samples = new float[clipToModify.samples * clipToModify.channels];
        //Populate the samples array with the data from the audioclip
        clipToModify.GetData(samples, 0);

        //Loop through all the samples and apply amplification
        for (int i = 0; i < samples.Length; ++i)
            samples[i] = samples[i] * amplifyAmount;

        //Create a new audioclip, based on the length of the samples and channels
        AudioClip temp = AudioClip.Create("TemporaryAudioClip", samples.Length, clipToModify.channels, 44100, false);
        //Populate the audioclip with the samples
        temp.SetData(samples, 0);
        //Return the new audioclip
        return temp;
    }

    public AudioClip Fade(AudioClip clipToModify, float fadeOutDuration)
    {
        //Calculate the sample size based on the original audio clip and its channels ammount
        samples = new float[clipToModify.samples * clipToModify.channels];
        //Populate the samples array with the data from the audioclip
        clipToModify.GetData(samples, 0);

        //Value represents volume/gain  0 -> 1
        float value = 1.0f;
        //Calculate when the fade out should begin based on the amount of samples
        float fadeOutStart = samples.Length  - (fadeOutDuration * samples.Length);
        //Calculate volume loss per sample
        float gainRegressionPerSample = 1.0f / (fadeOutDuration * samples.Length);
       
        //Loop through all samples
        for (int i = 0; i < samples.Length; i++)
        {
            //When the sample is greater than the start fade out sample
            if (i > fadeOutStart)
            {
                //begin to decrease the volume based on the calculated value
                value -= gainRegressionPerSample;
                //We dont want negatives so cap out at 0
                if (value < 0) value = 0.0f;
            }
            //Set the sample value
            samples[i] = samples[i] * value;
        }
        //Create a new audioclip, based on the length of the samples and channels
        AudioClip temp = AudioClip.Create("TemporaryAudioClip", samples.Length, clipToModify.channels, 44100, false);
        //Populate the audioclip with the samples
        temp.SetData(samples, 0);
        //Return the new audioclip
        return temp;
    }
}
