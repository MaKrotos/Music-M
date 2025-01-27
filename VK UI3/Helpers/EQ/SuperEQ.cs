using System;
using System.Collections.Generic;
using System.IO;
using Windows.Foundation.Collections;
using Windows.Foundation;
using Windows.Media;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace Aurora.Music.Effects
{
    public class SuperEQ : IBasicAudioEffect
    {
        private class BiQuadFilter
        {
            // BiQuad filter coefficients
            private float a0, a1, a2, b0, b1, b2;
            private float x1, x2, y1, y2;

            public BiQuadFilter(float sampleRate, float frequency, float bandwidth, float gain)
            {
                SetPeakingEq(sampleRate, frequency, bandwidth, gain);
            }

            public void SetPeakingEq(float sampleRate, float frequency, float bandwidth, float gain)
            {
                float A = MathF.Pow(10, gain / 40);
                float omega = 2 * MathF.PI * frequency / sampleRate;
                float alpha = MathF.Sin(omega) * MathF.Sinh(MathF.Log(2) / 2 * bandwidth * omega / MathF.Sin(omega));
                float cosw = MathF.Cos(omega);

                b0 = 1 + alpha * A;
                b1 = -2 * cosw;
                b2 = 1 - alpha * A;
                a0 = 1 + alpha / A;
                a1 = -2 * cosw;
                a2 = 1 - alpha / A;
            }

            public float Transform(float input)
            {
                float output = (b0 / a0) * input + (b1 / a0) * x1 + (b2 / a0) * x2 - (a1 / a0) * y1 - (a2 / a0) * y2;

                x2 = x1;
                x1 = input;
                y2 = y1;
                y1 = output;

                return output;
            }
        }

        private static SuperEQ current;
        public static SuperEQ Current => current;

        private List<BiQuadFilter[]> filters;
        private AudioEncodingProperties currentEncodingProperties;
        private int channels;

        public SuperEQ()
        {
            current = this;
        }

        public void SetEncodingProperties(AudioEncodingProperties encodingProperties)
        {
            currentEncodingProperties = encodingProperties;
            channels = (int)currentEncodingProperties.ChannelCount;

            // Initialize filters for each channel
            filters = new List<BiQuadFilter[]>(channels);
            for (int i = 0; i < channels; i++)
            {
                filters.Add(new BiQuadFilter[]
                {
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 30, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 75, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 150, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 300, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 600, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 1250, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 2500, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 5000, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 10000, 0.8f, 0),
                    new BiQuadFilter(currentEncodingProperties.SampleRate, 20000, 0.8f, 0)
                });
            }
        }

        public void ProcessFrame(ProcessAudioFrameContext context)
        {
            unsafe
            {
                AudioFrame inputFrame = context.InputFrame;

                using (AudioBuffer inputBuffer = inputFrame.LockBuffer(AudioBufferAccessMode.ReadWrite))
                using (IMemoryBufferReference inputReference = inputBuffer.CreateReference())
                {
                    ((IMemoryBufferByteAccess)inputReference).GetBuffer(out byte* inputDataInBytes, out uint inputCapacity);

                    float* inputDataInFloat = (float*)inputDataInBytes;
                    int dataInFloatLength = (int)inputBuffer.Length / sizeof(float);

                    for (int n = 0; n < dataInFloatLength; n++)
                    {
                        int ch = n % channels;

                        foreach (var filter in filters[ch])
                        {
                            inputDataInFloat[n] = filter.Transform(inputDataInFloat[n]);
                        }
                    }
                }
            }
        }

        public void Close(MediaEffectClosedReason reason) { }
        public void DiscardQueuedFrames() { }
        public void SetProperties(IPropertySet configuration) { }
        public bool UseInputFrameForOutput => true;
        public IReadOnlyList<AudioEncodingProperties> SupportedEncodingProperties => new List<AudioEncodingProperties>();
    }
}
