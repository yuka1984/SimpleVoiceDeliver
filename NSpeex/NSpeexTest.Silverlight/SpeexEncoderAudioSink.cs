using System;
using System.ServiceModel;
using System.Windows.Media;
using NSpeex;
using NSpeexTest.Silverlight.ServiceContract;

namespace NSpeexTest.Silverlight
{
    public class SpeexEncoderAudioSink : AudioSink
    {
        private readonly SpeexEncoder encoder = new SpeexEncoder(BandMode.Narrow);
        private readonly ISpeexStreamerUpChannel sender;

        public SpeexEncoderAudioSink(Uri endpoint)
        {
            sender = new ChannelFactory<ISpeexStreamerUpChannel>(new BasicHttpBinding(), new EndpointAddress(endpoint)).CreateChannel();
        }

        protected override void OnCaptureStarted()
        {
            sender.Open();
        }

        protected override void OnCaptureStopped()
        {
            sender.Close();
        }

        protected override void OnFormatChange(AudioFormat audioFormat)
        {}

        protected override void OnSamples(long sampleTimeInHundredNanoseconds, long sampleDurationInHundredNanoseconds, byte[] sampleData)
        {
            if (sampleDurationInHundredNanoseconds < 400000)
                return;

            // convert to short
            short[] data = new short[320];
            int sampleIndex = 0;
            for (int index = 0; sampleIndex < data.Length; index += 2, sampleIndex++)
            {
                data[sampleIndex] = BitConverter.ToInt16(sampleData, index);
            }

            var encodedData = new byte[data.Length];
            var encodedBytes = encoder.Encode(data, 0, sampleIndex, encodedData, 0, data.Length);
            if (encodedBytes != 0)
            {
                var upstreamFrame = new byte[encodedBytes];
                Array.Copy(encodedData, upstreamFrame, encodedBytes);

                //Debug.WriteLine("Publishing: " + encodedBytesSample + " bytes");));)
                sender.Begin_Publish(upstreamFrame, null, null);
            }
        }
    }
}
