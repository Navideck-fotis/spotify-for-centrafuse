using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using SpotiFire.SpotifyLib;
using System.Threading;
using System.Windows.Forms;

namespace Spotify
{
    partial class Spotify
    {
        private const string NOW_PLAYING_FILE_NAME = "NowPlaying.xml";
        private void SaveNowPlayingToFile()
        {
            try
            {
                if (NowPlayingTable != null & nowPlayingTableLoaded)    //LK, 22-may-2016: Only save when NowPlayingTable is loaded OK
                {
                    var links = NowPlayingTable.Rows.Cast<DataRow>()
                        .Select(row => row["TrackObject"] as ITrack)
                        .Select(track =>
                        {
                            var link = track.CreateLink();
                            string linkString = link.ToString();
                            link.Dispose();
                            return linkString;
                        }).ToList();

                    var fullPath = Path.Combine(CF_params.pluginConfigPath, NOW_PLAYING_FILE_NAME);
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);

                    PersistentNowPlaying pnp = new PersistentNowPlaying();
                    pnp.List = links;

                    if (currentTrack != null)
                    {
                        var timespan = player.Position + currentTrackPositionOffset;
                        var link = currentTrack.CreateLink();
                        var currentTrackLink = link.ToString();
                        link.Dispose();
                        pnp.CurrentSong = currentTrackLink;
                        pnp.CurrentSongPosition = timespan.TotalMilliseconds;
                    }

                    WriteLog("NowPlayingList will be saved in file: " + fullPath);
                    pnp.Save(fullPath);
                    WriteLog("NowPlayingList successfully saved");
                }
            }
            catch (Exception ex)
            {
                WriteLog("Error serializing now playing list: " + ex.Message);
            }
        }

        private void UpdateNowPlaying()
        {
            var tracks = new List<ITrack>();

            foreach (DataRow row in NowPlayingTable.Rows)
            {
                ITrack track = (ITrack)row["TrackObject"];
                if (track.IsLoaded)		//LK, 11-jul-2016: Don't try to get attributes when track isn't loaded (yet)
                {
                    row["Name"] = track.Name;
                    row["Artist"] = GetArtistsString(track.Artists);
                    row["Album"] = track.Album.Name;
                }
                else
                {
                    row["Name"] = "";
                    row["Artist"] = pluginLang.ReadField("/AppLang/Spotify/TrackNotAvailableOffline");
                    row["Album"] = "";
                }
                row["Starred"] = GetStarredStatusString(track.IsStarred);
                row["Available"] = GetAvailableStatusString(track.IsAvailable, track.Equals(currentTrack));
                row["TrackObject"] = track;
                if (track.IsAvailable)
                    tracks.Add(track);
            }

            //var tracks = this.NowPlayingTable.Rows.Cast<DataRow>().Select(row => (row["TrackObject"] as ITrack)).Where(t => t.IsAvailable); //TODO: selecteert toch alle tracks

            NonShuffledTracks = new LinkedList<ITrack>(tracks);   //LK, 11-jun-2016: Only add available tracks
            ShuffledTracks = new LinkedList<ITrack>(ShuffleSongs(NonShuffledTracks));   //LK, 11-jun-2016: Only add available tracks
        }

        private void RestoreNowPlaying(bool startPlaying)
        {
            nowPlayingTableLoaded = false; //LK, 22-may-2016: Avoid saving empty table

            var fullPath = Path.Combine(CF_params.pluginConfigPath, NOW_PLAYING_FILE_NAME);
            if (File.Exists(fullPath))
            {
                CF_systemCommand(centrafuse.Plugins.CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/RestoringNowPlaylist"));
                ThreadPool.QueueUserWorkItem(delegate(object obj)
                {
                    try
                    {
                        var pnp = PersistentNowPlaying.Load(fullPath);
                        
                        var links = pnp.List.Select(l => SpotifySession.ParseLink(l)).ToList();
                        var linkToPlay = links.SingleOrDefault(l => l.ToString().Equals(pnp.CurrentSong, StringComparison.CurrentCultureIgnoreCase));

                        int trackIxToPlay = linkToPlay != null ? links.IndexOf(linkToPlay) : -1;

                        var tracks = links.Select(l => l.As<ITrack>()).ToArray();

                        NowPlayingTable = LoadTracksIntoTable(tracks);

                        NonShuffledTracks = new LinkedList<ITrack>(tracks.Where(t => (t.IsAvailable)));   //LK, 11-jun-2016: Only add available tracks
                        ShuffledTracks = new LinkedList<ITrack>(ShuffleSongs(NonShuffledTracks));   //LK, 11-jun-2016: Only add available tracks

                        var trackToPlay = trackIxToPlay != -1 ? tracks[trackIxToPlay] : null;

                        foreach (var link in links)
                            link.Dispose();

                        nowPlayingTableLoaded = true;   //LK, 22-may-2016: NowPlaying table may be saved from now on

                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                SwitchToTab(Tabs.NowPlaying, GroupingType.Songs, NowPlayingTable, "Now Playing", null, true);
                                CF_systemCommand(centrafuse.Plugins.CF_Actions.HIDEINFO);
                                if (trackToPlay != null && !trackToPlay.IsPlaceholder)
                                {
                                    PlayTrack(trackToPlay);
                                    if (pnp.CurrentSongPosition > 5000)
                                    {
                                        //subtract 5 seconds
                                        var seekPosition = pnp.CurrentSongPosition - 5000;
                                        SeekCurrentTrack((int)seekPosition);
                                    }

                                    //[Grant] Pause after restore if auto play is disabled.
                                    if (!autoPlay)
                                        Pause();
                                }
                                else
                                    currentTrack = null;
                            }));
                    }
                    catch (Exception ex)
                    {
                        nowPlayingTableLoaded = false;  //LK, 22-may-2016: Avoid saving corrupted table
                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                CF_systemDisplayDialog(centrafuse.Plugins.CF_Dialogs.OkBox, pluginLang.ReadField("/AppLang/Spotify/FailedToRestoreNowPlaylist") + ex.Message);
                                CF_systemCommand(centrafuse.Plugins.CF_Actions.HIDEINFO);
                            }));
                    }
                });
            }
        }
    }
}
