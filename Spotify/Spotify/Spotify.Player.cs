using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotiFire.SpotifyLib;
using System.Data;
using System.Windows.Forms;

namespace Spotify
{
    public partial class Spotify
    {
        private BASSPlayer player = new BASSPlayer();

        public bool ShuffleOn
        {
            get
            {
                string field = this.pluginConfig.ReadField("/APPCONFIG/SHUFFLE");
                bool val;
                if (bool.TryParse(field, out val))
                    return val;
                else
                    return false;
            }
            set
            {
                this.pluginConfig.WriteField("/APPCONFIG/SHUFFLE", value.ToString(), true);
            }
        }

        private LinkedList<ITrack> ShuffledTracks = new LinkedList<ITrack>();
        private LinkedList<ITrack> NonShuffledTracks = new LinkedList<ITrack>();    //LK, 11-jun-2016: Add non-shuffled linked track list

        private void SubscribePlayerEvents(ISession session)
        {
            session.StreamingError += new SessionEventHandler(session_StreamingError);
            session.MusicDeliver += new MusicDeliveryEventHandler(session_MusicDeliver);
        }

        void PlaybackMonitor_Tick(object sender, EventArgs e)
        {
            //LK, 11-jun-2016: Throwing an exception here, will crash Centrafuse and we don't want to cause that here
            if (currentTrack == null)
            {
                //throw new Exception("Playback monitor started, but no track playing");
                WriteLog("Playback monitor started, but no track playing (stopped)");
                PlaybackMonitor.Enabled = false;
            }
            else
            {

                var position = player.Position + currentTrackPositionOffset;
                var duration = currentTrack.Duration;

                if (position >= duration) //:LK add cross fade functionallity - TimeSpan.FromSeconds(4)) //TODO: use CF_configParam
                {
                    this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                    {
                        try
                        {

                            if (!PlayNextTrack(autoLoop))
                            {
                                var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
                                trackRow["Available"] = GetAvailableStatusString(currentTrack.IsAvailable, false);

                                currentTrack = null;
                                SpotifySession.PlayerUnload();
                                PlaybackMonitor.Stop();
                                player.Stop();
                                isPaused = false;
                                currentTrackPositionOffset = new TimeSpan(0);
                            }
                        }
                        catch (Exception ex) { CF_displayMessage(ex.Message); PlaybackMonitor.Enabled = false; }     
                    }));
                }
            }
        }

        /// <summary>
        /// Finds next available track
        /// </summary>
        /// <param name="currentTrack">The current track where the search starts</param>
        /// <param name="loopAround">If true, if the current track is the last track in the table, start playing the first track</param>
        /// <returns>The next track or null when no tracks are available</returns>
        ITrack findAvailableTrack(ITrack currentTrack, bool loopAround)
        {
            ITrack nextTrack = null;
            if (currentTrack != null)
            {
                if (ShuffleOn)
                {
                    if (!ShuffledTracks.Any(t => t.IsAvailable))
                        throw new Exception(pluginLang.ReadField("/AppLang/Spotify/NoOfflineTracksAvailable"));

                    var nextNode = ShuffledTracks.Find(currentTrack);

                    //In case the current node doesn't exist anymore, start on the top
                    if (nextNode == null)
                        nextNode = ShuffledTracks.First;

                    while (nextNode != null && !nextNode.Value.IsAvailable)
                    {
                        nextNode = nextNode.Next;

                        if (nextNode == null && loopAround)
                            nextNode = ShuffledTracks.First;
                    }

                    if (nextNode != null)
                        nextTrack = nextNode.Value;
                }
                else
                {
                    if (!NonShuffledTracks.Any(t => t.IsAvailable))
                        throw new Exception(pluginLang.ReadField("/AppLang/Spotify/NoOfflineTracksAvailable"));

                    var nextNode = NonShuffledTracks.Find(currentTrack);

                    if (nextNode == null)
                        nextNode = NonShuffledTracks.First;

                    while (nextNode != null && !nextNode.Value.IsAvailable)
                    {
                        nextNode = nextNode.Next;

                        if (nextNode == null && loopAround)
                            nextNode = NonShuffledTracks.First;
                    }

                    if (nextNode != null)
                        nextTrack = nextNode.Value;
                }
            }
            return nextTrack;
        }


        /// <summary>
        /// Plays next track
        /// </summary>
        /// <param name="loopAround">If true, if the current track is the last track in the table, start playing the first track</param>
        /// <returns>True if there's a track to play. Always true if loopAround is true</returns>
        private bool PlayNextTrack(bool loopAround)
        {
            ITrack nextTrack = null;
            if (currentTrack != null)
            {
                if (ShuffleOn)
                {
                    var currentNode = ShuffledTracks.Find(currentTrack);
                    var nextNode = currentNode.Next;

                    if (nextNode == null && loopAround)
                        nextNode = ShuffledTracks.First;

                    if (nextNode != null)
                        nextTrack = nextNode.Value;
                }
                else
                {
                    var currentNode = NonShuffledTracks.Find(currentTrack);
                    var nextNode = currentNode.Next;

                    if (nextNode == null && loopAround)
                        nextNode = NonShuffledTracks.First;

                    if (nextNode != null)
                        nextTrack = nextNode.Value;

                    #region Old code
                    //var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
                    //var trackRowIx = this.NowPlayingTable.Rows.IndexOf(trackRow);
                    //var trackCurrentRowIx = trackRowIx;

                    //trackRow["Available"] = GetAvailableStatusString(currentTrack.IsOfflineAvailable, false);

                    ////LK, 11-jun-2016: Skip unavailable tracks
                    //while (nextTrack == null)
                    //{
                    //    trackRowIx++;
                    //    if (this.NowPlayingTable.Rows.Count > trackRowIx)
                    //    {
                    //        nextTrack = this.NowPlayingTable.Rows[trackRowIx]["TrackObject"] as ITrack;
                    //    }
                    //    else if (loopAround)
                    //    {
                    //        nextTrack = this.NowPlayingTable.Rows[0]["TrackObject"] as ITrack;
                    //    }
                    //    else
                    //        break;

                    //    if (trackRowIx == trackCurrentRowIx)    //No other tracks available?
                    //        break;

                    //    if (!nextTrack.IsAvailable)
                    //        nextTrack = null; //Skip when not available
                    //}
                    #endregion Old code
                }
            }

            if (nextTrack != null)
            {
                PlayTrack(nextTrack);
                return true;
            }
            else
                return false;
        }

        private bool PlayPreviousTrack(bool loopAround)
        {
            ITrack previousTrack = null;
            if (currentTrack != null)
            {
                if (ShuffleOn)
                {
                    var currentNode = ShuffledTracks.Find(currentTrack);
                    var previousNode = currentNode.Previous;

                    if (previousNode == null && loopAround)
                        previousNode = ShuffledTracks.Last;

                    if (previousNode != null)
                        previousTrack = previousNode.Value;
                }
                else
                {
                    var currentNode = NonShuffledTracks.Find(currentTrack);
                    var previousNode = currentNode.Previous;

                    if (previousNode == null && loopAround)
                        previousNode = NonShuffledTracks.Last;

                    if (previousNode != null)
                        previousTrack = previousNode.Value;

                    #region Old code
                    //var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
                    //var trackRowIx = this.NowPlayingTable.Rows.IndexOf(trackRow);
                    //var trackCurrentRowIx = trackRowIx;

                    //trackRow["Available"] = GetAvailableStatusString(currentTrack.IsOfflineAvailable, false);

                    ////LK, 11-jun-2016: Skip unavailable tracks
                    //while (previousTrack == null)
                    //{
                    //    trackRowIx--;
                    //    if (this.NowPlayingTable.Rows.Count > trackRowIx && trackRowIx >= 0)
                    //    {
                    //        previousTrack = this.NowPlayingTable.Rows[trackRowIx]["TrackObject"] as ITrack;
                    //    }
                    //    else if (loopAround)
                    //    {
                    //        previousTrack = this.NowPlayingTable.Rows[this.NowPlayingTable.Rows.Count - 1]["TrackObject"] as ITrack;
                    //    }
                    //    else
                    //        break;

                    //    if (trackRowIx == trackCurrentRowIx)    //No other tracks available?
                    //        break;

                    //    if (!previousTrack.IsAvailable)
                    //        previousTrack = null; //Skip when not available
                    //}
                    #endregion
                }
            }
            if (previousTrack != null)
            {
                PlayTrack(previousTrack);
                return true;
            }
            else
                return false;
        }

        private ITrack currentTrack = null;

        //LK, 22-may-2016: Be sure that the CF parameter Media.mediaPlaying is following isPause
        private bool _isPaused = true;
        private bool isPaused
        {
            get 
            {
                return _isPaused;
            }
            set
            {
                _isPaused = value;
                this.CF_params.Media.mediaPlaying = !(_isPaused || player.Paused);
            }
        }

        private void PlayTrack(ITrack track)
        {
            if (currentTrack != null)
            {
                var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
                trackRow["Available"] = GetAvailableStatusString(currentTrack.IsAvailable, false);

                SpotifySession.PlayerUnload();
                player.Stop();
                PlaybackMonitor.Stop();
                currentTrack = null;
            }

            if (track.IsAvailable)    //LK, 11-jun-2016: When not logged in, the track must be off-line available
            {
                currentTrackPositionOffset = new TimeSpan(0);
                var result = SpotifySession.PlayerLoad(track);
                Play();
                currentTrack = track;
                SyncMainTableWithView();
                PlaybackMonitor.Start();

                var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
                trackRow["Available"] = GetAvailableStatusString(currentTrack.IsAvailable, true);
            }
            else
            {
                string message = pluginLang.ReadField("/AppLang/Spotify/TrackNotAvailableOffline");
                WriteError(message);
                CF_displayMessage(message);
                CF_setPlayPauseButton(true, _zone);
            }
        }

        private void SeekCurrentTrack(int milliseconds)
        {
            player.Stop();
            currentTrackPositionOffset = new TimeSpan(0, 0, 0, 0, milliseconds);
            player.ReadyPlay();
            SpotifySession.PlayerSeek(milliseconds);
        }

        public void StopAllPlayback()
        {
            var trackRow = this.NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack).Equals(currentTrack));
            trackRow["Available"] = GetAvailableStatusString(currentTrack.IsAvailable, false);

            currentTrack = null;
            SpotifySession.PlayerUnload();
            currentTrackPositionOffset = new TimeSpan(0);
            player.Stop();
            PlaybackMonitor.Stop();
            isPaused = false;
            CF_setPlayPauseButton(true, _zone);
        }

        void session_MusicDeliver(ISession sender, MusicDeliveryEventArgs e)
        {
            e.ConsumedFrames = player.EnqueueSamples(e.Channels, e.Rate, e.Samples, e.Frames);
        }

        void session_StreamingError(ISession sender, SessionEventArgs e)
        {
            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    WriteError(e.Message);
                    CF_displayMessage("Streaming Error:" + Environment.NewLine + e.Message);
                }));
        }

        private void PlayPause()
        {
            if (currentTrack != null)
            {
                if (isPaused)
                    Play();
                else
                    Pause();
            }
        }

        private void Play()
        {
            player.ReadyPlay();
            player.Paused = false;
            SpotifySession.PlayerPlay();
            isPaused = false;
            CF_setPlayPauseButton(false, _zone);
        }

        private void Pause()
        {
            SpotifySession.PlayerPause();
            player.Paused = true;
            isPaused = true;
            CF_setPlayPauseButton(true, _zone);
        }
    }
}
