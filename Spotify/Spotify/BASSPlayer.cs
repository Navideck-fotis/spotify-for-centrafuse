using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Un4seen.Bass;

namespace Spotify
{
    public class BASSPlayer
    {
        public BASSPlayer()
        {
            Paused = true;
            Stopped = true; //LK, 22-jul-2016: Initial state is stopped
        }

        //LK, 22-may-2016, Begin: Add callback to handle changed channel
        public event ChannelChangedDelegate ChannelChangedEvent;
        public delegate void ChannelChangedDelegate (int newChannel);

        private int _channel = -1;
        private int channel
        {
            get { return _channel; }
            set 
            {
                _channel = value;

                    if (ChannelChangedEvent != null)
                        ChannelChangedEvent (_channel);
            }
        }
        //LK, 22-may-2016, End: Add callback to handle changed channel

        public int EnqueueSamples(int channels, int rate, byte[] samples, int frames)
        {
            if (_stopped)
            {
                return frames; //should we return 0? this means frames will be actively dropped
            }

            if (channel == -1)
            {
                channel = Bass.BASS_StreamCreate(rate, channels, BASSFlag.BASS_DEFAULT, BASSStreamProc.STREAMPROC_PUSH);
                Bass.BASS_ChannelPlay(channel, false);
            }

            if (channel != -1)
                Bass.BASS_StreamPutData(channel, samples, samples.Length); //data will always be queued up, never dropped

            return frames;
        }

        //LK, 22-jul-2016: Make stopped status externally available
        private bool _stopped = true;
        public bool Stopped
        {
            get
            {
                return _stopped;
            }
            set
            {
                _stopped = value;
            }
        }

        public void Stop()
        {
            if (channel != -1)
            {
                //TODO: LK: Add cross-over function
                Bass.BASS_ChannelStop(channel);
                Bass.BASS_StreamFree(channel);
                channel = -1;
                _stopped = true;
            }
        }

        public void ReadyPlay()
        {
            _stopped = false;
        }

        private bool _paused = false;
        public bool Paused
        {
            get
            {
                return _paused;
            }
            set
            {
                _paused = value;
                if (_paused)
                {
                    Bass.BASS_ChannelPause(channel);
                }
                else
                {
                    Bass.BASS_ChannelPlay(channel, false);
                }
            }
        }

        public TimeSpan Position
        {
            get
            {
                if (channel == 0) return TimeSpan.Zero;
                // length in bytes
                long len = Bass.BASS_ChannelGetPosition(channel, BASSMode.BASS_POS_BYTES);
                if (len <= 0)
                {
                    return TimeSpan.Zero;
                }
                // the time length
                int seconds = (int)Bass.BASS_ChannelBytes2Seconds(channel, len);
                return TimeSpan.FromSeconds(seconds);
            }
        }
    }
}
