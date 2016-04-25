using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;
using NSpeexTest.Silverlight.Commands;

namespace NSpeexTest.Silverlight
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private CaptureSource captureSource;
        private SpeexEncoderAudioSink audioSink;
        public DelegateCommand<object> StartCommand { get; set; }
        public DelegateCommand<object> ListenCommand { get; set; }

        public string ListenAddress { get; set; }

        public IEnumerable<AudioCaptureDevice> AudioDevices
        {
            get
            {
                return CaptureDeviceConfiguration.GetAvailableAudioCaptureDevices();
            }
        }

        private AudioCaptureDevice selectedAudioDevice;
        public AudioCaptureDevice SelectedAudioDevice
        {
            get
            {
                return selectedAudioDevice;
            }
            set
            {
                if(value != selectedAudioDevice)
                {
                    selectedAudioDevice = value;
                    FirePropertyChanged("SelectedAudioDevice");
                }
            }
        }

        public MainViewModel()
        {
            ListenAddress = "localhost:8002/SpeexStreamer";
            StartCommand = new DelegateCommand<object>(o => SelectedAudioDevice != null, DoStartPlay);
            ListenCommand = new DelegateCommand<object>(o => !string.IsNullOrWhiteSpace(ListenAddress), DoStartListen);
            SelectedAudioDevice = CaptureDeviceConfiguration.GetDefaultAudioCaptureDevice();
        }
        
        private void DoStartPlay(object obj)
        {
            CaptureDeviceConfiguration.RequestDeviceAccess();
            if (captureSource != null)
            {
                captureSource.Stop();
                captureSource = null;
            }
            // Desired format is 16kHz 16bit
            var queriedAudioFormats = from format in SelectedAudioDevice.SupportedFormats
                                      where format.SamplesPerSecond == 8000 && format.BitsPerSample == 16 && format.Channels == 1
                                      select format;
            SelectedAudioDevice.DesiredFormat = queriedAudioFormats.FirstOrDefault();
            SelectedAudioDevice.AudioFrameSize = 40;

            captureSource = new CaptureSource
                                  {
                                      AudioCaptureDevice = SelectedAudioDevice
                                  };

            audioSink = new SpeexEncoderAudioSink (new Uri(@"http://" + ListenAddress))
                            {
                                CaptureSource = captureSource
                            };

            captureSource.Start();
        }

        private void DoStartListen(object obj)
        {
            throw new NotImplementedException();
        }

        void FirePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
