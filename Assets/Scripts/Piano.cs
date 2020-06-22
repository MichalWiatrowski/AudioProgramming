using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Code reasearched, adapted and sourced from the following sources
//https://www.youtube.com/watch?v=5xd9BMxoXqo&t=245s
//https://en.wikibooks.org/wiki/Sound_Synthesis_Theory/Oscillators_and_Wavetables
//https://docs.unity3d.com/Manual/class-AudioClip.html
//https://forum.unity.com/threads/audioclip-setdata-for-2-channel-i-e-stereo.203727/
//http://www.piano-keyboard-guide.com/middle-c.html

public class Piano : MonoBehaviour
{
   
    AudioSource audioSource;
    public InputField channelsInput;
    public InputField amplitudeInput;
    public InputField samplesInput;
    public InputField phaseOffsetInput;
    public Dropdown methodChoice;

    float[] samples;
    float m_amplitude = 500.0f;
    float m_phase = 0.0f;
    float m_time = 0.0f;
    float m_frequency = 0.0f;
    float m_finalFrequency = 0.0f;
    int m_numberOfSamples = 44100;
    int m_channels = 2;
    int m_noteID = 0;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    { 
    }

    public void NotePress(Button button)
    {
        //split the buttons name into two to get the correct octave
        string[] split = button.name.Split('|');

        //set the default frequency based on the octave
        if (System.Convert.ToInt32(split[1]) == 1) m_frequency = 110.0f;
        else if (System.Convert.ToInt32(split[1]) == 2) m_frequency = 220.0f;
        else if (System.Convert.ToInt32(split[1]) == 3) m_frequency = 440.0f;
        else if (System.Convert.ToInt32(split[1]) == 4) m_frequency = 880.0f;
        else if (System.Convert.ToInt32(split[1]) == 5) m_frequency = 1760.0f;
        else m_frequency = 3520.0f;

        //then get the correct key (12 pitches in an octave) so we can calculate the final frequency
        //lecture 6, slide 7: freq = 440 * ((2^(1/12)) ^ (semitones from A))
        //440 is replaced by default, m_frequency, for the octave
        if (split[0] == "C") m_noteID = 0;
        else if (split[0] == "D") m_noteID = 2;
        else if (split[0] == "E") m_noteID = 4;
        else if (split[0] == "F") m_noteID = 5;
        else if (split[0] == "G") m_noteID = 7;
        else if (split[0] == "A") m_noteID = 9;
        else if (split[0] == "B") m_noteID = 11;
        else if (split[0] == "_C") m_noteID = 1;
        else if (split[0] == "_D") m_noteID = 3;
        else if (split[0] == "_F") m_noteID = 6;
        else if (split[0] == "_G") m_noteID = 8;
        else if (split[0] == "_A") m_noteID = 10;

        //Check if any input in the input boxes
        if (amplitudeInput.text != "")
            m_amplitude = (float)System.Convert.ToDouble(amplitudeInput.text);
        if (phaseOffsetInput.text != "")
            m_phase = (float)System.Convert.ToDouble(phaseOffsetInput.text);
        if (samplesInput.text != "")
            m_numberOfSamples = System.Convert.ToInt32(samplesInput.text);
        if (channelsInput.text != "")
            m_channels = System.Convert.ToInt32(channelsInput.text);

        //Debug.Log(m_finalFrequency);

        //calculate the final frequency using the formula mentioned above
        m_finalFrequency = m_frequency * Mathf.Pow(Mathf.Pow(2, (m_noteID / 12.0f)), 1.0f);
        //allocate memory and prepare for sample generation
        samples = new float[m_numberOfSamples * m_channels];
        m_time = 0.0f;


        //run the chosen synthesis method
        if (methodChoice.value == 0) SineWave(true);
        else if (methodChoice.value == 1) SineWave(false);
        else if (methodChoice.value == 2) SquareWave();
        else if (methodChoice.value == 3) TriangleWave();
        else if (methodChoice.value == 4) UnrealEngineWave();
           
        //Create an audio clip, with the sample length and channels
        AudioClip temp = AudioClip.Create("Piano", samples.Length, m_channels, 44100, false);
        //Set the samples data into the audio clip
        temp.SetData(samples, 0);
        //Attach the audio clip to the piano's audio source
        audioSource.clip = temp;
        //Play the audio clip
        audioSource.Play();   
    }

    void SetSample(float sampleValue, ref int sample)
    {
        //the float array will be interleaved if more than one channel e.g. LeftStereo[sample[0]], RightStereo[sample[0]], LeftStereo[sample[1]], RightStereo[sample[1]]
        //so we need to increment the samples by 2 and assign sample value to both channels
        //otherwise just assign sample value to current sample
        if (m_channels > 1)
        {
            //Modify the sample value based on phase
            //We dont want negative values so we design it to min at 0
            if (m_phase > 0.0f) samples[sample] = sampleValue * (1 - m_phase);
            else samples[sample] = sampleValue;

            if (m_phase < 0.0f) samples[sample + 1] = sampleValue * (1 + m_phase);
            else samples[sample + 1] = sampleValue;

            //increment sample by 2
            sample += 2;
        }
        else
        {
            //if it is just mono, simply set the sample value to the sample list and increment sample
            samples[sample] = sampleValue;
            sample++;
        }
    }
    void SquareWave()
    {
        float m_sampleValue = 0.0f;

        for (int sample = 0; sample < m_numberOfSamples;)
        {
            //ticks per cycle (44100 by pressed note)
            //so for example 44100 / 440 = ~100.22 ticks per cycle;
            float ticksPerCycle = (float)m_numberOfSamples / m_finalFrequency;
            //cycle number during the m_time during sinwave
            float currentCycle = m_time % ticksPerCycle;
            //since its a square wave, half the time we want no sound
            float halfCycle = ticksPerCycle / 2;

            //so if in the current cycle we are supposed to produce sound
            //otherwise set the sample to 0 (no sound)
            if (currentCycle < halfCycle) m_sampleValue = m_finalFrequency * m_amplitude;
            else m_sampleValue = 0.0f;

            //Call the function to set the sample value to the sample list
            SetSample(m_sampleValue, ref sample);
            //increment time
            m_time += 1.0f;
        }
    }

    void SineWave(bool sin)
    {
        float m_sampleValue = 0.0f;
        for (int sample = 0; sample < m_numberOfSamples;)
        {
            //ticks per cycle (44100 by pressed note)
            //so for example 44100 / 440 = ~100.22 ticks per cycle;
            int ticksPerCycle = (int)((float)m_numberOfSamples / m_finalFrequency);
            //cycle number during the m_time during sinwave
            float cycles = m_time / ticksPerCycle;
            //calculate the sin wave input
            float rad = 2 * 3.14159265359f * cycles;
            //calculate sample (amplitude * sinwave + offset)
            if (sin) m_sampleValue = m_amplitude * Mathf.Sin(rad);
            else m_sampleValue = m_amplitude * Mathf.Cos(rad);

            //Call the function to set the sample value to the sample list
            SetSample(m_sampleValue, ref sample);

            //increment time
            m_time += 1.0f;
        }
    }
    void TriangleWave()
    {
        //value for the sample in the cycle
        float m_sampleValue = 0.0f;
        for (int sample = 0; sample < m_numberOfSamples;)
        {
            //ticks per cycle (44100 by pressed note)
            //so for example 44100 / 440 = ~100.22 ticks per cycle;
            float ticksPerCycle = (float)m_numberOfSamples / m_finalFrequency;
            //cycle number during the m_time during the triangle wave
            float currentCycle = m_time % ticksPerCycle;
            //halfway for the "triangle to go up"
            float halfCycle = ticksPerCycle / 2;

            if (currentCycle < halfCycle) m_sampleValue = 0.0f;
            else m_sampleValue = (3 * m_finalFrequency * m_amplitude) - ((2 * m_amplitude) / halfCycle) * currentCycle;

            //Call the function to set the sample value to the sample list
            SetSample(m_sampleValue, ref sample);

            //increment time
            m_time += 1.0f;
        }
       
    }
    //Unreal engine compile error keyboard
    //Unreal makes this noise when it fails to compile, funny effect
    void UnrealEngineWave()
    {
        //value for the sample in the cycle
        float m_sampleValue = 0.0f;
        for (int sample = 0; sample < m_numberOfSamples;)
        {
            //generate sin wave, using amplitude, frequency and phase offset
            m_sampleValue = m_amplitude * Mathf.Sin(m_time * m_finalFrequency * (2 * 3.14159265359f) + m_phase);

            //Call the function to set the sample value to the sample list
            SetSample(m_sampleValue, ref sample);

            //using delta time is risky due to differences every frame
            //this is perfect for this
            m_time += Time.deltaTime;
        }
    }
}
