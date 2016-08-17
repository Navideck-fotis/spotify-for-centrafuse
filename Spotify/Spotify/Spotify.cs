using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using centrafuse.Plugins;
using SpotiFire.SpotifyLib;
using System.Data;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Drawing;

namespace Spotify
{
    public partial class Spotify : CFPlugin
    {
        #region Constants

        //LK, 22-may-2016: Add standard plugin constants for module log support
        private const string PluginName = "Spotify";
        private const string PluginPath = @"\Plugins\" + PluginName + @"\";
        private const string PluginPathSkins = PluginPath + @"Skins\";
        private const string PluginPathLanguages = PluginPath + @"Languages\";
        private const string ConfigurationFileName = "config.xml";
        private const string LogFileName = "Spotify.log";

        private const string userAgent = "SpotiFire";
        #endregion Constants

        #region Variables
        public static string LogFilePath = CFTools.AppDataPath + PluginPath + LogFileName;  //LK, 22-may-2016: add support for event logging in module log

        internal string currentVisImagePath = "";   //LK, 22-may-2016: Added support for full function visuals and album art
        #endregion

        #region settings

        public static bool LogEvents;
        private string username;
        private string password;
        private string tempPath;
        private Boolean autoPlay;
        private Boolean autoLoop;   //LK, 22-may-2016: add support for auto loop (playlist)
        private sp_bitrate preferredBitrate;
        private int powerResumeDelay = 5000;
        #endregion

        #region key

        // SpotiFire.

        static internal readonly byte[] applicationKey = new Byte[]
            {
                0x01, 0x16, 0xA3, 0xB3, 0x8D, 0x4C, 0x38, 0x69, 0xBE, 0xE2, 0x65, 0xD3, 0x4B, 0x57, 0x70, 0x12,
                0x6D, 0x4B, 0x28, 0x2A, 0x89, 0x7B, 0x87, 0x07, 0xC7, 0xAE, 0x68, 0xBC, 0x01, 0x20, 0xBE, 0xF9,
                0x0B, 0x65, 0xC1, 0x11, 0xA9, 0x07, 0x94, 0xEB, 0x9A, 0xCE, 0x29, 0xAF, 0x9B, 0x11, 0xE3, 0xE8,
                0x01, 0x22, 0x9B, 0x5C, 0xF5, 0x05, 0xA8, 0x91, 0xE9, 0x9F, 0xE1, 0xA9, 0x4D, 0x8C, 0x21, 0xAB,
                0x53, 0x44, 0x9E, 0xAE, 0x65, 0xA3, 0x36, 0x37, 0x6E, 0xAC, 0xFA, 0x63, 0x43, 0xEB, 0xA1, 0x89,
                0xC6, 0xAC, 0xF0, 0x79, 0x61, 0xF9, 0xB0, 0x15, 0x3B, 0x88, 0xFA, 0xA8, 0x5C, 0xC3, 0xA0, 0x74,
                0xE1, 0xC3, 0x42, 0xF3, 0x72, 0xD2, 0xD1, 0xC3, 0xD5, 0x8E, 0xE7, 0xD4, 0x8D, 0x97, 0x36, 0x54,
                0x7C, 0x58, 0x92, 0xF2, 0xE1, 0x0C, 0x40, 0x37, 0x5F, 0x6D, 0x80, 0x95, 0x58, 0x2B, 0x20, 0xC2,
                0x13, 0x5F, 0x76, 0xDB, 0x6B, 0x55, 0x8B, 0xD0, 0x0E, 0x9F, 0x3D, 0x80, 0x02, 0xC4, 0xCE, 0xF8,
                0x22, 0xE3, 0x14, 0x46, 0x4E, 0xBF, 0x37, 0xD3, 0x51, 0x9D, 0xD2, 0x42, 0x7D, 0x5A, 0xEE, 0xEC,
                0xE3, 0xA3, 0xF3, 0xBD, 0x7B, 0x77, 0x59, 0x8E, 0xD5, 0xD9, 0x7D, 0xE3, 0xCE, 0x6D, 0x15, 0x05,
                0x88, 0x3F, 0xC2, 0x27, 0x54, 0x09, 0x8C, 0x2D, 0x4D, 0x94, 0x86, 0xF0, 0x14, 0xAB, 0xA2, 0x9E,
                0xC8, 0xEF, 0xCE, 0x48, 0x48, 0xDD, 0x63, 0xAD, 0xEF, 0x40, 0x0A, 0x31, 0x81, 0xCA, 0x70, 0x89,
                0x01, 0x1A, 0x4D, 0x2B, 0xF8, 0x9F, 0x8D, 0x42, 0xB4, 0x31, 0xF1, 0xBA, 0x8A, 0x49, 0xF4, 0xFA,
                0xCD, 0x75, 0x30, 0x5F, 0x85, 0xC0, 0x0B, 0xF4, 0x27, 0x83, 0x1B, 0x34, 0x53, 0x37, 0x39, 0x35,
                0xED, 0x82, 0x73, 0xE7, 0x91, 0xA6, 0x5C, 0x85, 0x58, 0x5C, 0xC7, 0x34, 0x18, 0x3F, 0x07, 0x8C,
                0x5E, 0xF3, 0xA0, 0xC6, 0xB7, 0xC7, 0x8B, 0xF8, 0x41, 0x0D, 0x2D, 0xFD, 0x63, 0x0C, 0x6C, 0xA9,
                0xD0, 0xE7, 0x12, 0x18, 0x02, 0xB7, 0x1C, 0xFB, 0x98, 0x0D, 0xFA, 0x71, 0x98, 0xAA, 0x71, 0xDB,
                0xC8, 0x4E, 0xCB, 0x1A, 0xB2, 0xC7, 0xA1, 0x91, 0xB8, 0xD2, 0x38, 0xA7, 0x11, 0x25, 0xC6, 0xF8,
                0x3F, 0x04, 0xC4, 0x41, 0x3A, 0x40, 0x2A, 0x7D, 0xCA, 0x6C, 0xD5, 0xC1, 0x67, 0x5D, 0xA3, 0x94,
                0x1C
            };


        #endregion

        #region buttons
        private const string BUTTON_NOW_PLAYING = "Spotify.nowPlayingButton";
        private const string BUTTON_PLAYLISTS = "Spotify.playlistsButton";
        private const string BUTTON_INBOX = "Spotify.inboxButton";
        private const string BUTTON_POPULAR = "Spotify.popularButton";
        private const string BUTTON_SEARCH = "Spotify.search";
        #endregion

        #region list templates
        private const string TEMPLATE_SONGS = "default";
        private const string TEMPLATE_ARTISTS = "spotifyArtists";
        private const string TEMPLATE_ALBUMS = "spotifyAlbums";
        private const string TEMPLATE_PLAYLISTS = "spotifyPlaylists";
        #endregion

        private const string PLUGIN_NAME = "Spotify";

        private bool pluginNonBufferedVisSupport = false;   //LK, 22-may-2016
        private bool nowPlayingTableLoaded = false;   //LK, 22-may-2016

        private CFControls.CFVis miniPicVis;

        private Stack<TableState> TableStates = new Stack<TableState>();

        private BindingSource MainTableBindingSource;
        private DataTable NowPlayingTable;
        private System.Windows.Forms.Timer PlaybackMonitor = new System.Windows.Forms.Timer();

        public Spotify()
        {
            MainTableBindingSource = new BindingSource();
            NowPlayingTable = LoadTracksIntoTable(new ITrack[] { });
            PlaybackMonitor.Interval = 300;
            PlaybackMonitor.Tick += PlaybackMonitor_Tick;
        }
        
        public override void CF_pluginInit()
        {
            this.CF3_initPlugin(PLUGIN_NAME, true);

            try { LogEvents = Boolean.Parse(pluginConfig.ReadField("/APPCONFIG/LOGEVENTS")); }
            catch { LogEvents = false; };

            WriteLog("startup");
            WriteLog("startup, Time of day: " + DateTime.Now + ", Version: " + CF_params.pluginVersion);
            WriteLog("Start");


            this.CF_localskinsetup();

            LoadSettings(); //LK, 22-may-2016: Moved from initSpotifyClient()

            this.CF_params.Media.isAudioPlugin = true;
            this.CF_params.Media.useCorePlaybackControl = true;
            this.CF_params.Media.mediaPlaying = false;

            //LK,22-may-2016: Added support for new mixer function (mute single mixer input channel for render (output) devices is available from cfMixer V3.7.1)
            //      and check for Centrafuse V4.4.9-; the next release will support Vis for non-buffered audio apps (like this one)
            try
            {
                string cfMixerVersion = CFTools.GetVersion("cfMixer.dll");
                string[] cfMixerVersionSplit = cfMixerVersion.Split('.');
                int majorVersion = int.Parse(cfMixerVersionSplit[0]);
                int minorVersion = int.Parse(cfMixerVersionSplit[1]);
                int updateVersion = int.Parse(cfMixerVersionSplit[2]);
                if (majorVersion > 3 || (majorVersion == 3 && minorVersion > 7) || (majorVersion == 3 && minorVersion == 7 && updateVersion >= 1))
                    if (Environment.OSVersion.Version.Major >= 6)
                        CF_params.Media.playbackLine = "Centrafuse";

                string cfVersion = CFTools.GetVersion("Centrafuse.exe");
                string[] cfVersionSplit = cfVersion.Split('.');
                majorVersion = int.Parse(cfVersionSplit[0]);
                minorVersion = int.Parse(cfVersionSplit[1]);
                updateVersion = int.Parse(cfVersionSplit[2]);
                if (majorVersion > 4 || (majorVersion == 4 && minorVersion > 4) || (majorVersion == 4 && minorVersion == 4 && updateVersion > 9))
                    pluginNonBufferedVisSupport = true;
            }
            catch { CF_params.Media.playbackLine = string.Empty; }

            CF_events.powerModeChanged += new Microsoft.Win32.PowerModeChangedEventHandler(Spotify_CF_Event_powerModeChanged);
            this.CF_events.trackPositionChanged += new CFTrackPositionChangedEventHandler(CF_events_trackPositionChanged);
            this.CF_events.connectionEstablished += new EventHandler(CF_events_connectionChanged);
            this.CF_events.connectionLost += new EventHandler(CF_events_connectionChanged);

            this.player.ChannelChangedEvent += new BASSPlayer.ChannelChangedDelegate(channelChangedHandler);

            WriteLog("Stop");
        }

        void channelChangedHandler(int newChannel)
        {
            try
            {
                this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                {
                    try
                    {
                        if (newChannel == -1)
                            CF_bassSetVis(newChannel, true, -1);  //LK, 22-may-2016: Add support for visualizations
                        else
                            CF_bassSetVis(newChannel, false, -1);  //LK, 22-may-2016: Add support for visualizations
                    }
                    catch (Exception ex) { CF_displayMessage(ex.Message); }
                }));
            }
            catch (Exception ex) { CF_displayMessage(ex.Message); }
        }

        void CF_events_trackPositionChanged(object sender, TrackPositionChangedArgs e)
        {
            try
            {
                if (currentTrack != null)
                {
                    var percentage = e.Percentage();

                    if (percentage < 0)
                        return;

                    double fraction = (double)percentage / 100;
                    var offset = (int)(currentTrack.Duration.TotalMilliseconds * fraction);
                    SeekCurrentTrack(offset);
                }
            }
            catch (Exception ex) { CF_displayMessage(ex.Message); }
        }

        void CF_events_connectionChanged(object sender, EventArgs e)
        {
            try
            {
                if (SpotifySession != null)
                {
                    if (CF_getConnectionStatus())
                    {
                        //Internet connection reastablished
                        SpotifySession.SetConnectionType(ConnectionType.Wired);
                        WriteLog("Internet connection become available: Session connection type is set to " + ConnectionType.Wired);
                    }
                    else
                    {
                        //Internet connection lost
                        SpotifySession.SetConnectionType(ConnectionType.None);
                        WriteLog("Internet connection lost: Session connection type is set to " + ConnectionType.None);
                    }

                    if (CF_params.Media.mediaPlaying)       //LK, 04-aug-2016: Don't do anything when not playing 
                    {
                        this.CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));

                        ThreadPool.QueueUserWorkItem(delegate(object obj)
                        {
                            try
                            {
                                this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                                {
                                    try
                                    {
                                        if (CF_getConnectionStatus())
                                        {
                                            //Internet connection reastablished
                                            SleepUntilTrue(() => SpotifySession.ConnectionState == sp_connectionstate.LOGGED_IN, 30000);
                                            UpdateNowPlaying();   //Restore NowPlaying playlist but leave player in the current state
                                        }
                                        else
                                        {
                                            //Internet connection lost
                                            SleepUntilTrue(() => SpotifySession.ConnectionState == sp_connectionstate.DISCONNECTED, 60000);
                                            if (currentTrack != null)
                                            {
                                                if (!currentTrack.IsAvailable)
                                                {
                                                    string message = pluginLang.ReadField("/AppLang/Spotify/TrackNotAvailableOffline");
                                                    WriteLog(message);  //LK, 22-jul-2016: Write message to module log file i.s.o. Error.log file
                                                    CF_displayMessage(message);

                                                    this.CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"), "AUTOHIDE");
                                                    ITrack nextAvailableTrack = findAvailableTrack(currentTrack, autoLoop);
                                                    StopAllPlayback();
                                                    nextAvailableTrack = findAvailableTrack(nextAvailableTrack, autoLoop);  //Be sure, the track is still in the list
                                                    if (nextAvailableTrack != null)    //LK, 22-jul-2016: Only start playing when a valid track is found
                                                        PlayTrack(nextAvailableTrack);
                                                    else
                                                    {
                                                        //LK, 22-jul-2016: Display a message when no valid off-line tracks are found
                                                        message = pluginLang.ReadField("/AppLang/Spotify/NoOfflineTracksAvailable");
                                                        WriteLog(message);
                                                        CF_displayMessage(message);
                                                    }
                                                }
                                            }

                                            //LK, 22-jul-2016: Always update the now play list (even when the current track isn't available any more)
                                            UpdateNowPlaying();  //Restore NowPlaying playlist but leave player in the current state
                                        }
                                        CF_systemCommand(CF_Actions.HIDEINFO);
                                    }
                                    catch (Exception ex)
                                    {
                                        CF_systemCommand(CF_Actions.HIDEINFO);
                                        WriteLog("Error serializing now playing list: " + ex.Message);  //LK, 22-jul-2016: Don't border user with this:  //  CF_displayMessage(ex.Message);
                                    }
                                }));
                            }
                            catch (Exception ex)
                            {
                                CF_systemCommand(CF_Actions.HIDEINFO);
                                CF_displayMessage(ex.Message);
                            }
                        });
                    }
                    else
                    {
                        //LK, 04-aug-2016: Always update the now play list (even when we are not playing or even the current audio source)
                        UpdateNowPlaying();  //Restore NowPlaying playlist but leave player in the current state
                    }
                }
            }
            catch (Exception ex)
            {
                CF_systemCommand(CF_Actions.HIDEINFO);
                CF_displayMessage(ex.Message);
            }
        }

        public override void CF_localskinsetup()
        {
            WriteLog("Start");

            this.CF3_initSection("Spotify");    //lk, 22-may-2016: Don't get confused with Centrafuse MAIN section
            var list = advancedlistArray[CF_getAdvancedListID("mainList")];
            list.DataBinding = MainTableBindingSource;
            list.DoubleClickListTiming = true;
            list.DoubleClick += new EventHandler<CFControlsExtender.Listview.ItemArgs>(list_DoubleClick);
            list.LongClick += new EventHandler<CFControlsExtender.Listview.ItemArgs>(list_LongClick);
            list.LinkedItemClick += new EventHandler<CFControlsExtender.Listview.LinkedItemArgs>(list_LinkedItemClick);
            SwitchToTab(Tabs.NowPlaying, GroupingType.Songs, NowPlayingTable, "Now Playing", null, false);

            //LK, 22-may-2016: Add support for Album Art and Visualizations
            miniPicVis = this.visArray[CF_getVisID("AlbumArt")];

            WriteLog("Stop");
        }
        
        void list_LinkedItemClick(object sender, CFControlsExtender.Listview.LinkedItemArgs e)
        {
            WriteLog("Start");

            switch (e.LinkId)
            {
                case "starred":
                    var table = MainTableBindingSource.DataSource as DataTable;
                    if (e.ItemId < table.Rows.Count)
                    {
                        var row = table.Rows[e.ItemId];
                        var track = row["TrackObject"] as ITrack;

                        bool newValue = !track.IsStarred;
                        track.IsStarred = newValue;

                        this.CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/PleaseWait"));
                        ThreadPool.QueueUserWorkItem((o) =>
                            {
                                try
                                {
                                    SleepUntilTrue(() => track.IsStarred == newValue);

                                    this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                                    {
                                        this.CF_systemCommand(CF_Actions.HIDEINFO);

                                        row["Starred"] = GetStarredStatusString(newValue);

                                        if (CurrentTab != Tabs.NowPlaying)
                                        {
                                            var nowPlayingRow = NowPlayingTable.Rows.Cast<DataRow>().SingleOrDefault(r => (r["TrackObject"] as ITrack).Equals(track));
                                            if (nowPlayingRow != null)
                                                nowPlayingRow["Starred"] = GetStarredStatusString(newValue);
                                        }
                                    }));
                                }
                                catch (Exception ex)
                                {
                                    this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                                        {
                                            CF_systemCommand(CF_Actions.HIDEINFO);
                                            WriteError(ex);
                                            CF_displayMessage(ex.Message);
                                        }));
                                }
                            });
                    }
                    break;
            }
            WriteLog("Stop");
        }

        void list_DoubleClick(object sender, CFControlsExtender.Listview.ItemArgs e)
        {
            WriteLog("Start");

            if (CurrentTab == Tabs.NowPlaying)
            {
                if (e.ItemId < NowPlayingTable.Rows.Count)
                {
                    PlayTrack(NowPlayingTable.Rows[e.ItemId]["TrackObject"] as ITrack);
                }
            }
            else
            {
                var table = MainTableBindingSource.DataSource as DataTable;
                if (e.ItemId < table.Rows.Count)
                {
                    var row = table.Rows[e.ItemId];
                    switch (CurrentGroupingType)
                    {
                        case GroupingType.Songs:
                            {
                                AppendTracks(new ITrack[] { row["TrackObject"] as ITrack });
                            }
                            break;
                        case GroupingType.Albums:
                            {
                                var album = row["AlbumObject"] as IAlbum;
                                using (var albumBrowser = album.Browse())
                                {
                                    if (!albumBrowser.IsComplete)
                                    {
                                        CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));
                                        SleepUntilTrue(() => albumBrowser.IsComplete);
                                        CF_systemCommand(CF_Actions.HIDEINFO);
                                    }

                                    List<ITrack> tracks = new List<ITrack>();
                                    foreach (var track in albumBrowser.Tracks)
                                    {
                                        if (track.IsAvailable)
                                        {
                                            tracks.Add(track);
                                        }
                                    }

                                    var resultTable = LoadTracksIntoTable(tracks);
                                    TableStates.Peek().Position = table.Rows.IndexOf(row);
                                    TableStates.Peek().ImageID = currentImageId;
                                    SwitchToTab(CurrentTab, GroupingType.Songs, resultTable, album.Name, album.CoverId, false);
                                }
                            }
                            break;
                        case GroupingType.Artists:
                            {
                                var artist = row["ArtistObject"] as IArtist;
                                using (var artistBrowser = artist.Browse(sp_artistbrowse_type.NO_TRACKS))
                                {
                                    if (!artistBrowser.IsComplete)
                                    {
                                        CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));
                                        artistBrowser.WaitForCompletion();
                                        CF_systemCommand(CF_Actions.HIDEINFO);
                                    }

                                    var albums = GetAlbumsIncludingTopHits(artistBrowser);
                                    var resultTable = LoadAlbumsIntoTable(albums, false);
                                    TableStates.Peek().Position = table.Rows.IndexOf(row);
                                    TableStates.Peek().ImageID = currentImageId;
                                    SwitchToTab(CurrentTab, GroupingType.Albums, resultTable, artist.Name, artistBrowser.PortraitIds.FirstOrDefault(), false);
                                }
                            }
                            break;
                        case GroupingType.Playlists:
                            {
                                var playlist = row["PlaylistObject"] as IPlaylist;
                                List<ITrack> tracks = new List<ITrack>();
                                foreach (var track in playlist.Tracks)
                                {
                                    if (track.IsAvailable)
                                    {
                                        tracks.Add(track);
                                    }
                                }
                                var resultTable = LoadTracksIntoTable(tracks);
                                TableStates.Peek().Position = table.Rows.IndexOf(row);
                                TableStates.Peek().ImageID = currentImageId;
                                SwitchToTab(CurrentTab, GroupingType.Songs, resultTable, playlist.Name, playlist.ImageId, false);
                                
                                if(CurrentTab == Tabs.Playlists)
                                    SetupDynamicButton3(playlist);
                            }
                            break;
                    }
                }
            }
            WriteLog("Stop");
        }

        void list_LongClick(object sender, CFControlsExtender.Listview.ItemArgs e)
        {
            WriteLog("Start");

            if (CurrentTab == Tabs.NowPlaying)
            {
                if (e.ItemId < NowPlayingTable.Rows.Count)
                {
                    var currentTrack = NowPlayingTable.Rows[e.ItemId]["TrackObject"] as ITrack;
                    if (currentTrack != null)
                    {
                        var choices = new string[] { "Album", "Artist" };
                        var choiceDialog = new MultipleChoiceDialog(this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen, pluginLang.ReadField("/AppLang/Spotify/SearchFor"), choices);
                        choiceDialog.MainForm = base.MainForm;
                        choiceDialog.CF_pluginInit();
                        if (choiceDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            int choice = choiceDialog.Choice;
                            switch (choice)
                            {
                                case 0:
                                    using (var albumBrowser = currentTrack.Album.Browse())
                                    {
                                        if (!albumBrowser.IsComplete)
                                        {
                                            CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));
                                            albumBrowser.WaitForCompletion();
                                            CF_systemCommand(CF_Actions.HIDEINFO);
                                        }

                                        List<ITrack> tracks = new List<ITrack>();
                                        foreach (var track in albumBrowser.Tracks)
                                        {
                                            if (track.IsAvailable)
                                            {
                                                tracks.Add(track);
                                            }
                                        }

                                        var resultTable = LoadTracksIntoTable(tracks);
                                        SwitchToTab(Tabs.Search, GroupingType.Songs, resultTable, currentTrack.Album.Name, currentTrack.Album.CoverId, true);
                                    }

                                    break;
                                case 1:
                                    if (currentTrack.Artists.Count > 1)
                                    {
                                        choices = currentTrack.Artists.Select(a => a.Name).Take(5).ToArray();
                                        choiceDialog = new MultipleChoiceDialog(this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen, pluginLang.ReadField("/AppLang/Spotify/SearchFor"), choices);
                                        choiceDialog.MainForm = base.MainForm;
                                        choiceDialog.CF_pluginInit();
                                        if (choiceDialog.ShowDialog(this) == DialogResult.OK)
                                        {
                                            choice = choiceDialog.Choice;
                                        }
                                        else
                                            return;
                                    }
                                    else
                                    {
                                        choice = 0;
                                    }
                                    var artist = currentTrack.Artists.ElementAt(choice);
                                    using (var artistBrowser = artist.Browse(sp_artistbrowse_type.NO_TRACKS))
                                    {
                                        if (!artistBrowser.IsComplete)
                                        {
                                            CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));
                                            artistBrowser.WaitForCompletion();
                                            CF_systemCommand(CF_Actions.HIDEINFO);
                                        }

                                        var albums = GetAlbumsIncludingTopHits(artistBrowser);
                                        var resultTable = LoadAlbumsIntoTable(albums, false);
                                        SwitchToTab(Tabs.Search, GroupingType.Albums, resultTable, artist.Name, artistBrowser.PortraitIds.FirstOrDefault(), true);
                                    }
                                    break;
                            }
                        }
                    }
                }
            }
            WriteLog("Stop");
        }

        private IEnumerable<IAlbum> GetAlbumsIncludingTopHits(IArtistBrowse browser)
        {
            var topHitTracks = browser.TopHitTracks;
            var albums = browser.Albums.ToList();
            albums.Insert(0, new TopHitsAlbum(browser.Artist, topHitTracks, SpotifySession));
            return albums;
        }

        private void SetupDynamicButton3(IPlaylist playlist)
        {
            var button3 = buttonArray[CF_getButtonID("DynamicButton3")];
            button3.buttonEnabled = true;
            if (playlist.OfflineStatus == PlaylistOfflineStatus.No)
            {
                button3.offimage = "offline";
                button3.downimage = "offlineDown";
            }
            else
            {
                button3.offimage = "online";
                button3.downimage = "onlineDown";
            }
        }

        private void SwitchToTab(Tabs tab, GroupingType groupingType, DataTable table, string display, string imageID, bool purgeStateStack)
        {
            SwitchToTab(tab, groupingType, table, display, imageID, purgeStateStack, null);
        }

        private Tabs CurrentTab = Tabs.NowPlaying;
        private GroupingType CurrentGroupingType = GroupingType.Songs;
        private void SwitchToTab(Tabs tab, GroupingType groupingType, DataTable table, string display, string imageId, bool purgeStateStack, int? scrollState)
        {
            CF_setButtonOff(BUTTON_NOW_PLAYING);
            CF_setButtonOff(BUTTON_PLAYLISTS);
            CF_setButtonOff(BUTTON_INBOX);
            CF_setButtonOff(BUTTON_POPULAR);
            CF_setButtonOff(BUTTON_SEARCH);

            if(currentPlaylistworker != null)
            {
                currentPlaylistworker.CancelAsync();
                currentPlaylistworker = null;
            }

            SetupDynamicButtons(tab);
            
            var list = advancedlistArray[CF_getAdvancedListID("mainList")];
            string templateID = GetTemplateIDForGroupingType(groupingType);
            list.TemplateID = templateID;

            if(purgeStateStack)
            {
                //purge the table states stack
                while (TableStates.Count > 0)
                {
                    var state = TableStates.Pop();
                    if (state.Table != NowPlayingTable)
                    {
                        state.Dispose();
                    }
                }
            }

            TableStates.Push(new TableState(display, table, groupingType));
            CF_updateText("LocationLabel", GetCurrentStateStackText());

            MainTableBindingSource.DataSource = table;

            CurrentTab = tab;
            CurrentGroupingType = groupingType;
            switch (CurrentTab)
            {
                case Tabs.NowPlaying:
                    CF_setButtonOn(BUTTON_NOW_PLAYING);
                    break;
                case Tabs.Playlists:
                    CF_setButtonOn(BUTTON_PLAYLISTS);
                    break;
                case Tabs.Inbox:
                    CF_setButtonOn(BUTTON_INBOX);
                    break;
                case Tabs.Popular:
                    CF_setButtonOn(BUTTON_POPULAR);
                    break;
                case Tabs.Search:
                    CF_setButtonOn(BUTTON_SEARCH);
                    break;
            }

            list.Refresh();

            if (scrollState.HasValue)
            {
                if(CurrentTab != Tabs.NowPlaying) //it will be taken care of by SyncMainTableWithView if it is a NowPlaying Tab
                    this.MainTableBindingSource.Position = scrollState.Value;

                list.SelectedIndex = scrollState.Value;
            }

            if (CurrentTab == Tabs.NowPlaying)
                SyncMainTableWithView();
            else
                LoadImage(imageId);

            if (CurrentGroupingType == GroupingType.Playlists)
            {
                CheckAndStartPlaylistTimer();
            }
        }

        BackgroundWorker currentPlaylistworker;
        private void CheckAndStartPlaylistTimer()
        {
            //assume that we are in playlist grouping
            var table = MainTableBindingSource.DataSource as DataTable;
            bool needsStarting = table.Rows.Cast<DataRow>().Any(row =>
                {
                    var playlist = row["PlaylistObject"] as IPlaylist;
                    return playlist.OfflineStatus == PlaylistOfflineStatus.Downloading || playlist.OfflineStatus == PlaylistOfflineStatus.Waiting;
                });
            
            if (needsStarting)
            {
                if (currentPlaylistworker != null)
                {
                    currentPlaylistworker.CancelAsync();
                    currentPlaylistworker = null;
                }

                currentPlaylistworker = new BackgroundWorker();
                currentPlaylistworker.WorkerSupportsCancellation = true;
                currentPlaylistworker.DoWork += new DoWorkEventHandler(currentPlaylistworker_DoWork);
                currentPlaylistworker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(currentPlaylistworker_RunWorkerCompleted);
                currentPlaylistworker.RunWorkerAsync();
            }
        }

        void currentPlaylistworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).Dispose();
        }

        void currentPlaylistworker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                while (true)
                {
                    bool keepGoing = false;
                    this.Invoke(new MethodInvoker(delegate()
                        {
                            if (CurrentGroupingType == GroupingType.Playlists)
                            {
                                int position = MainTableBindingSource.Position;
                                var table = MainTableBindingSource.DataSource as DataTable;
                                foreach (DataRow row in table.Rows)
                                {
                                    var playlist = row["PlaylistObject"] as IPlaylist;
                                    row["OfflineStatus"] = GetPlaylistStatusString(playlist);
                                    row["DownloadingStatus"] = GetDownloadingStatusString(playlist);
                                    if (playlist.OfflineStatus == PlaylistOfflineStatus.Waiting || playlist.OfflineStatus == PlaylistOfflineStatus.Downloading)
                                    {
                                        keepGoing = true;
                                    }
                                }
                            }
                        }));
                    if (!keepGoing || e.Cancel)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                    }
                }
            }
            catch 
            {
                //there's a potential for synchronicity errors. nobody cares though, just abort
            }
            
        }

        private string GetCurrentStateStackText()
        {
            if (TableStates.Count == 0)
                return string.Empty;
            else
            {
                return string.Join(@"\", TableStates.Select(state => state.Display).Reverse().ToArray());
            }
        }

        private void SyncMainTableWithView()
        {
            if (MainTableBindingSource.DataSource == NowPlayingTable && currentTrack != null)
            {
                var currentRow = NowPlayingTable.Rows.Cast<DataRow>().Single(row => (row["TrackObject"] as ITrack) == currentTrack);
                int currentRowIx = NowPlayingTable.Rows.IndexOf(currentRow);
                MainTableBindingSource.Position = currentRowIx;
                LoadImage(currentTrack.Album.CoverId);
            }
        }

        private void SetupDynamicButtons(Tabs tab)
        {
            var button1 = buttonArray[CF_getButtonID("DynamicButton1")];
            var button2 = buttonArray[CF_getButtonID("DynamicButton2")];
            var button3 = buttonArray[CF_getButtonID("DynamicButton3")];
            button3.buttonEnabled = false; //the button will be set up by the calling method

            if (tab == Tabs.NowPlaying)
            {
                button1.buttonEnabled = true;
                button1.offimage = "clearAll";
                button1.downimage = "clearAllDown";

                button2.buttonEnabled = true;
                button3.buttonEnabled = false;

                button2.offimage = ShuffleOn ? "shuffle" : "straight";
                button2.downimage = ShuffleOn ? "shuffleDown" : "straightDown";
            }
            else if (tab == Tabs.Inbox || tab == Tabs.Playlists || tab == Tabs.Popular || tab == Tabs.Search)
            {
                button1.buttonEnabled = button2.buttonEnabled = true;

                button1.offimage = "addSelected";
                button1.downimage = "addSelectedDown";

                button2.offimage = "addAll";
                button2.downimage = "addAllDown";
            }
            this.Invalidate();
        }

        private string GetTemplateIDForGroupingType(GroupingType groupingType)
        {
            switch (groupingType)
            {
                case GroupingType.Songs:
                    return TEMPLATE_SONGS;
                case GroupingType.Albums:
                    return TEMPLATE_ALBUMS;
                case GroupingType.Artists:
                    return TEMPLATE_ARTISTS;
                case GroupingType.Playlists:
                    return TEMPLATE_PLAYLISTS;
                default:
                    throw new Exception("Unrecognized grouping type");
            }
        }


        private ISession SpotifySession;
        
        private int _zone = 0;
        bool _hasControl = false;
        public override void CF_pluginShow()
        {
            WriteLog("Start");

            base.CF_pluginShow();
            _hasControl = true;
            InitSpotifyClient();

            //[GrantA] Auto play music on show.
            if (currentTrack != null && isPaused && autoPlay)
                Play();

            miniPicVis.picVis.Visible = true;

            WriteLog("Stop");
        }


        //LK, 22-may-2016: Added to hide visualizations
        public override void CF_pluginHide()
        {
            WriteLog("Start");

            SaveNowPlayingToFile();     //LK,22-may-2016: Also save when Spotify screen is closed
            miniPicVis.picVis.Visible = false;
            base.CF_pluginHide();

            WriteLog("Stop");
        }


        private void InitSpotifyClient()
        {
            WriteLog("Start");

            if (SpotifySession == null)
            {
                if (!EnsureTempPathOK())
                {
                    return;
                }

                if (string.IsNullOrEmpty(username))
                {
                    CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/UserNameNotSpecified"));
                    return;
                }

                CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/Connecting"));
                try
                {
                    SpotifySession = SpotiFire.SpotifyLib.Spotify.CreateSession(applicationKey, tempPath, tempPath, userAgent);
                    WriteLog("Session created" + SpotifySession.ToString() + ", Connection state = " + SpotifySession.ConnectionState.ToString());
                }
                catch (Exception ex)
                {
                    CF_displayMessage(ex.Message);
                    WriteError(ex);
                    return;
                }
                finally
                {
                    CF_systemCommand(CF_Actions.HIDEINFO);
                }

                if (CF_getConnectionStatus())
                {
                    SpotifySession.SetConnectionType(ConnectionType.Wired);
                    WriteLog("Session connection type is set to " + ConnectionType.Wired);
                }
                else
                {
                    SpotifySession.SetConnectionType(ConnectionType.None);
                    WriteLog("Session connection type is set to " + ConnectionType.None);
                }
                SpotifySession.SetPrefferedBitrate(preferredBitrate);
                DistributeSessionForSubscription(SpotifySession);
            }
            else
                WriteLog("Session already exists: " + SpotifySession.ToString() + ", Connection state = " + SpotifySession.ConnectionState.ToString() + ", Login completed = " + loginComplete);

            if (!loginComplete)
            {
                WriteLog("Loading settings");
                LoadSettings();
                CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/loggingIn"));
                if (SpotifySession.ConnectionState != sp_connectionstate.LOGGED_IN)	//LK, 11-jul-2016: Don't try to login when already logged in; just wait for completion
                {
                    WriteLog("Logging in");
                    SpotifySession.Login(username, password, true);
                }

                //LK, 22-may-2016, Begin: Give the server some time to response
                int retryCount = 5;
                while ((SpotifySession.ConnectionState != sp_connectionstate.LOGGED_IN) && retryCount-- > 0)   //---   || !loginComplete
                {
                    Thread.Sleep(1000);
                    WriteLog("Waiting for login to complete, connection status = " + SpotifySession.ConnectionState.ToString() + ", loginComplete = " + loginComplete);
                }
                //LK, 22-may-2016, End: Give the server some time to response
            }
            WriteLog("Stop, Connection state = " + SpotifySession.ConnectionState.ToString());
        }

        private bool EnsureTempPathOK()
        {
            try
            {
                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                //LK, 22-may-2016: Also check if the image sub cache folder is present
                if (!Directory.Exists(tempPath + Path.DirectorySeparatorChar + "Images"))
                    Directory.CreateDirectory(tempPath + Path.DirectorySeparatorChar + "Images");
                return true;
            }
            catch (Exception ex)
            {
                CF_displayMessage(ex.Message);
                return false;
            }
        }

        private void DistributeSessionForSubscription(ISession session)
        {
            SubscribeSessionEvents(session);
            SubscribePlayerEvents(session);
        }

        public override void CF_pluginPause()
        {
            WriteLog("Start");

            _hasControl = false;

            if (currentTrack != null && !isPaused)
                Pause();

            Thread.Sleep(100);  //LK, 22-may-2016: Avoid plop

            base.CF_pluginPause();

            WriteLog("Stop");
        }

        public override void CF_pluginResume()
        {
            WriteLog("Start");

            _hasControl = true;

            if (autoPlay)
                InitSpotifyClient();    //LK, 22-may-2016: When autoPlay is set Initialize when not done already

            if (currentTrack == null)
            {
                var list = advancedlistArray[CF_getAdvancedListID("mainList")];
                if (list != null && list.Count > 0)
                {
                    if (list.SelectedIndex >= 0)
                        currentTrack = NowPlayingTable.Rows[list.SelectedIndex]["TrackObject"] as ITrack;   //Start at the selected item in the list
                    else
                        currentTrack = NowPlayingTable.Rows[0]["TrackObject"] as ITrack;    //Start at the top of the list
                }

                if (currentTrack != null)
                    PlayTrack(currentTrack);
            }
            else if (isPaused)
                Play();

            if (currentTrack == null && !autoPlay)
                CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/NotLoggedIn"), "AUTOHIDE");

            CF_setPlayPauseButton(!CF_params.Media.mediaPlaying, _zone); //LK, 22-may-2016: Set to actual state (in case Spotify didn't Autostart)
            base.CF_pluginResume();

            WriteLog("Stop");
        }

        public override void CF_pluginClose()
        {
            WriteLog("Start");


            //LK, 22-may-2016: Unsubscribe event handlers
            CF_events.powerModeChanged -= Spotify_CF_Event_powerModeChanged;
            this.CF_events.trackPositionChanged -= CF_events_trackPositionChanged;
            this.player.ChannelChangedEvent -= channelChangedHandler;

            if (SpotifySession != null)
            {
                SaveNowPlayingToFile();
                nowPlayingTableLoaded = false; //22-may-2016: Now playing list isn't valid any more

                StopAllPlayback();              //22-may-2016: Moved after saving playlist, so currentTrack is stil valid

                if (SpotifySession.ConnectionState == sp_connectionstate.LOGGED_IN)
                {
                    //LK, 15-jul-2016: Wait for log off to complete before exiting to avoid half opened sessions
                    SpotifySession.Logout();

                    WriteLog("Waiting for log off to complete: Session state = " + SpotifySession.ConnectionState.ToString());
                    SleepUntilTrue(() => SpotifySession.ConnectionState != sp_connectionstate.LOGGED_IN);
                    WriteLog("Log off completed: Session state = " + SpotifySession.ConnectionState.ToString());
                }
                try
                {
                    SpotifySession.Dispose();
                    SpotifySession = null;
                    WriteLog("Session disposed");
                }
                catch { }
            }

            WriteLog("Stop");
            base.CF_pluginClose();
        }

        //LK, 22-may-2016, Begin: Add support for power mode change evnts
        private void Spotify_CF_Event_powerModeChanged(object sender, Microsoft.Win32.PowerModeChangedEventArgs e)
        {
            WriteLog("Start, Mode = " + e.Mode.ToString());

            try
            {
                if (SpotifySession != null)
                {
                    WriteLog("Session state = " + SpotifySession.ConnectionState.ToString() + ", MediaPlaying = " + this.CF_params.Media.mediaPlaying);

                    if (e.Mode.Equals(Microsoft.Win32.PowerModes.Resume))
                    {
                        if (this.CF_params.Media.mediaPlaying)
                        {
                            this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                            {
                                Thread.Sleep(powerResumeDelay);
                                if (SpotifySession.ConnectionState == sp_connectionstate.LOGGED_OUT)
                                    SpotifySession.Login(username, password, true);

                                RestoreNowPlaying(true);
                            }));
                        }
                    }
                    else
                    {
                        StopAllPlayback();
                        if (SpotifySession.ConnectionState == sp_connectionstate.LOGGED_IN)
                            SpotifySession.Logout();
                        nowPlayingTableLoaded = false;  //22-may-2016: Now playing list isn't valid any more
                    }
                }
            }
            catch (Exception errmsg) { WriteError(errmsg); }

            WriteLog("Stop");
        }
        //LK, 22-may-2016, End: Add support for power mode change evnts

        //LK, 22-may-2016, Begin: Add support event logging in the module specific file
        private void WriteLog(string msg)
        {
            try
            {
                if (LogEvents)
                    if (msg.Equals("startup"))
                        CFTools.writeModuleLog(msg, LogFilePath);
                    else
                        CFTools.writeModuleLog((new StackTrace(true)).GetFrame(1).GetMethod().Name + ": " + msg, LogFilePath, CFTools.StackFramesToSkip.THREE);
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }
        //LK, 22-may-2016, End: Add support event logging in the module specific file


        private void WriteError(Exception ex)
        {
            try
            {
                CFTools.writeError(ex.Message, ex.StackTrace);
            }
            finally { }
        }

        private void WriteError(string message)
        {
            try
            {
                CFTools.writeError(message);
            }
            finally { }
        }

        public override bool CF_pluginCMLCommand(string command, string[] strparams, CF_ButtonState state, int zone)
        {
            WriteLog("Start, Command = " + command + ", button state = " + state);

            //LK, 22-jul-2016: Threre is no use to not accepting commands when paused
            //if (!_hasControl)
            //    return false;

            if (state < CF_ButtonState.Click) //LK, 01-aug-2016: Only click and hold click
                return false;

            string currMediaSource = base.MainForm
            string currLocationParam;
            CF_Actions currLocation = base.MainForm.CF_Main_getCurrentLocation(out currLocationParam);
            if (currLocation != CF_Actions.PLUGIN || currLocationParam != CF_params.pluginName)
                return false;

            _zone = zone;
            switch (command)
            {
                case "Spotify.NowPlaying":
                    {
                        LoadNowPlaying();
                        return true;
                    }
                case "Spotify.Search":
                    {
                        LoadTrackSearch();
                        return true;
                    }
                case "Spotify.SearchHold":
                    if (state == CF_ButtonState.HoldClick)
                    {
                        List<string> choices = new List<string>();
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Songs"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Albums"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Artists"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Playlists"));
                        var choiceDialog = new MultipleChoiceDialog(this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen, pluginLang.ReadField("/AppLang/Spotify/SearchFor"), choices);
                        choiceDialog.MainForm = base.MainForm;
                        choiceDialog.CF_pluginInit();
                        if (choiceDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            int choice = choiceDialog.Choice;
                            switch (choice)
                            {
                                case 0:
                                    LoadTrackSearch();
                                    break;
                                case 1:
                                    LoadAlbumSearch();
                                    break;
                                case 2:
                                    LoadArtistSearch();
                                    break;
                                case 3:
                                    LoadPlaylistSearch();
                                    break;
                            }
                            return true;
                        }
                        else
                            return false;
                    }
                    else
                        return false;
                case "Spotify.Inbox":
                    {
                        List<string> choices = new List<string>();
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Songs"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Albums"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Playlists"));
                        var choiceDialog = new MultipleChoiceDialog(this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen, pluginLang.ReadField("/AppLang/Spotify/RetrieveInboxOf"), choices);
                        choiceDialog.MainForm = base.MainForm;
                        choiceDialog.CF_pluginInit();
                        if (choiceDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            int choice = choiceDialog.Choice;
                            switch (choice)
                            {
                                case 0:
                                    LoadInboxTracks();
                                    break;
                                case 1:
                                    LoadInboxAlbums();
                                    break;
                                case 2:
                                    LoadInboxPlaylists();
                                    break;
                            }
                        }
                        return true;
                    }
                case "Spotify.Playlists":
                    {
                        LoadPlaylists();
                        return true;
                    }
                case "Spotify.Popular":
                    {
                        List<string> choices = new List<string>();
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Songs"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Albums"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/Artists"));
                        choices.Add(pluginLang.ReadField("/AppLang/Spotify/StarredSongs"));
                        var choiceDialog = new MultipleChoiceDialog(this.CF_displayHooks.displayNumber, this.CF_displayHooks.rearScreen, pluginLang.ReadField("/AppLang/Spotify/RetrieveTopListOf"), choices);
                        choiceDialog.MainForm = base.MainForm;
                        choiceDialog.CF_pluginInit();
                        if (choiceDialog.ShowDialog(this) == DialogResult.OK)
                        {
                            int choice = choiceDialog.Choice;
                            switch (choice)
                            {
                                case 0:
                                    LoadPopularTracks();
                                    break;
                                case 1:
                                    LoadPopularAlbums();
                                    break;
                                case 2:
                                    LoadPopularArtists();
                                    break;
                                case 3:
                                    LoadStarredTracks();
                                    break;
                            }
                        }
                        else
                            return false;
                        return true;
                    }
                case "Spotify.ScrollUp":
                    {
                        var list = advancedlistArray[CF_getAdvancedListID("mainList")];
                        list.PageUp();
                        return true;
                    }
                case "Spotify.ScrollDown":
                    {
                        var list = advancedlistArray[CF_getAdvancedListID("mainList")];
                        list.PageDown();
                        return true;
                    }
                case "Spotify.DynamicButton1":
                    {
                        OnDynamic1Clicked();
                        return true;
                    }
                case "Spotify.DynamicButton2":
                    {
                        OnDynamic2Clicked();
                        return true;
                    }
                case "Spotify.DynamicButton1Hold":
                    if (state == CF_ButtonState.HoldClick)
                    {
                        OnDynamic1Hold();
                        return true;
                    }
                    else
                        return false;
                case "Spotify.DynamicButton2Hold":
                    if (state == CF_ButtonState.HoldClick)
                    {
                        OnDynamic2Hold();
                        return true;
                    }
                    else
                        return false;
                case "Spotify.Back":
                    {
                        OnBackClicked();
                        return true;
                    }
                case "Spotify.DynamicButton3":
                    {
                        OnDynamic3Clicked();
                        return true;
                    }

                //LK, 22-may-2016: Leave handling of this to CF
                //case "Centrafuse.Main.PlayPause":
                //{
                //    PlayPause();
                //    return true;
                //}

                //LK, 22-may-2016: Add support for more hotkeys
                case "centrafuse.cfactions.PREVSONG":
                case "Centrafuse.Plugin.Rewind":
                case "Centrafuse.Main.Rewind":
                    if (this.CF_params.Media.mediaPlaying) //LK, 01-aug-2016: Only when this app is playing
                    {
                        PlayPreviousTrack(true);
                        return true;
                    }
                    else
                        return false;
                //LK, 22-may-2016: Add support for more hotkeys
                case "Plugin.Spotify.NextSong":
                case "centrafuse.cfactions.NEXTSONG":
                case "Centrafuse.Plugin.FastForward":
                case "Centrafuse.Main.FastForward":
                    if (this.CF_params.Media.mediaPlaying) //LK, 01-aug-2016: Only when this app is playing
                    {
                        PlayNextTrack(true);
                        return true;
                    }
                    else
                        return false;

                default:
                    return false;   //LK, 01-aug-2016: Leave this to the core:  //base.CF_pluginCMLCommand(command, strparams, state, zone);
            }
        }

        private void OnBackClicked()
        {
            if (TableStates.Count > 1)
            {
                if(CurrentTab == Tabs.NowPlaying)
                {
                    throw new Exception("Invalid state encountered");
                }

                var currentState = TableStates.Pop();
                var stateToApply = TableStates.Pop();
                SwitchToTab(CurrentTab, stateToApply.GroupingType, stateToApply.Table, stateToApply.Display, stateToApply.ImageID, false, stateToApply.Position);
                currentState.Dispose();
            }
        }

        public override string CF_pluginCMLData(CF_CMLTextItems textItem)
        {
            //WriteLog("Start");

            if (currentTrack != null)   //LK, 22-may-2016: Only when track is valid
                switch (textItem)
                {
                    case CF_CMLTextItems.MainTitle:
                        return currentTrack == null ? String.Empty : currentTrack.Name;
                    case CF_CMLTextItems.MediaArtist:
                        return currentTrack == null ? String.Empty : GetArtistsString(currentTrack.Artists);
                    case CF_CMLTextItems.MediaTitle:
                        return currentTrack == null ? String.Empty : currentTrack.Name;
                    case CF_CMLTextItems.MediaAlbum:
                        return currentTrack == null ? String.Empty : currentTrack.Album.Name;
                    case CF_CMLTextItems.MediaSource:
                    case CF_CMLTextItems.MediaStation:
                        return "Spotify";
                    case CF_CMLTextItems.MediaFileName:
                        if (string.IsNullOrEmpty(currentVisImagePath))
                            return "VISOFF";
                        else
                            return currentVisImagePath;
                    case CF_CMLTextItems.MediaDuration:
                        return GetCurrentTrackDuration();
                    case CF_CMLTextItems.MediaPosition:
                        return GetCurrentTrackPosition();
                    case CF_CMLTextItems.MediaSliderPosition:
                        return GetCurrentTrackScrubberPosition();

                    default:
                        return base.CF_pluginCMLData(textItem);
                }
            else
                return base.CF_pluginCMLData(textItem);
        }

        private string GetCurrentTrackScrubberPosition()
        {
            if (currentTrack == null)
                return 0.ToString();

            var currentPosition = player.Position + currentTrackPositionOffset;
            var totalLength = currentTrack.Duration;
            var positionPercentage = Math.Floor(currentPosition.TotalSeconds / totalLength.TotalSeconds * 100);
            if (positionPercentage > 100)
                positionPercentage = 100;

            return ((int)positionPercentage).ToString();
        }

        private TimeSpan currentTrackPositionOffset = new TimeSpan(0);
        private string GetCurrentTrackPosition()
        {
            //LK, 22-may-2016: Add support for position count down (tine left to end of track)
            bool positionCountDown = CF_getConfigFlag(CF_ConfigFlags.PositionCountDown);
            var timespan = positionCountDown ? (currentTrack != null ? currentTrack.Duration : TimeSpan.Zero) - (player.Position + currentTrackPositionOffset): player.Position + currentTrackPositionOffset;
            return (positionCountDown ? "-" : "") + string.Format(timeFormat, timespan.Minutes, timespan.Seconds);
        }

        private const string timeFormat = "{0}:{1:00}";
        private string GetCurrentTrackDuration()
        {
            TimeSpan trackLength = currentTrack != null ? currentTrack.Duration : TimeSpan.Zero;
            return string.Format(timeFormat, trackLength.Minutes, trackLength.Seconds);
        }

        private void OnDynamic2Clicked()
        {
            var table = MainTableBindingSource.DataSource as DataTable;

            if (table == null)
                return;

            if (CurrentTab == Tabs.NowPlaying)
            {
                ShuffleOn = !ShuffleOn;
                SetupDynamicButtons(CurrentTab);
            }
            else
            {
                switch (CurrentGroupingType)
                {
                    case GroupingType.Songs:
                        {
                            var tracks = table.Rows.Cast<DataRow>().Select(row => row["TrackObject"] as ITrack);
                            AppendTracks(tracks);
                            break;
                        }
                    case GroupingType.Albums:
                        {
                            CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/CantAddMultipleAlbums"));
                            break;
                        }
                    case GroupingType.Artists:
                        {
                            CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/CantAddMultipleArtists"));
                            break;
                        }
                    case GroupingType.Playlists:
                        {
                            CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/CantAddMultiplePlayLists"));
                            break;
                        }
                }
            }
        }

        private void OnDynamic1Clicked()
        {
            var list = advancedlistArray[CF_getAdvancedListID("mainList")];

            if (CurrentTab == Tabs.NowPlaying)
            {
                var selectedIx = list.SelectedIndex;
                if (selectedIx < NowPlayingTable.Rows.Count)
                {
                    var row = NowPlayingTable.Rows[selectedIx];
                    var track = row["TrackObject"] as ITrack;
                    if (track == currentTrack)
                    {
                        if (!PlayNextTrack(autoLoop))   //LK, 11-jun-2016: Also alow loop when enabled here
                            StopAllPlayback();
                    }
                    ShuffledTracks.Remove(track);
                    NonShuffledTracks.Remove(track);
                    track.Dispose();
                    NowPlayingTable.Rows.Remove(row);
                    list.Refresh();
                    //LK, 15-jul-2016: Save the now-playlist when changed
                    if (nowPlayingTableLoaded)
                        SaveNowPlayingToFile();

                }
            }
            else
            {
                
                var selectedIx = list.SelectedIndex;
                if (selectedIx < 0)
                    return;

                var table = MainTableBindingSource.DataSource as DataTable;

                if (table == null)
                    return;

                if (selectedIx >= table.Rows.Count)
                    return;

                var selectedRow = table.Rows[selectedIx];

                switch (CurrentGroupingType)
                {
                    case GroupingType.Songs:
                        {
                            var track = selectedRow["TrackObject"] as ITrack;
                            AppendTracks(new ITrack[] { track });
                            break;
                        }
                    case GroupingType.Albums:
                        {
                            var album = selectedRow["AlbumObject"] as IAlbum;
                            CF_systemCommand(CF_Actions.SHOWINFO, pluginLang.ReadField("/AppLang/Spotify/WaitForConnection"));
                            ThreadPool.QueueUserWorkItem(delegate(object obj)
                            {
                                try
                                {
                                    using (var albumBrowser = album.Browse())
                                    {
                                        SleepUntilTrue(() => albumBrowser.IsComplete);
                                        SleepUntilTrue(() => albumBrowser.Tracks.All(t => t.IsLoaded));

                                        List<ITrack> tracks = new List<ITrack>();
                                        foreach (var track in albumBrowser.Tracks)
                                        {
                                            if (track.IsAvailable)
                                                tracks.Add(track);
                                        }
                                        this.ParentForm.BeginInvoke(new MethodInvoker(delegate()
                                            {
                                                CF_systemCommand(CF_Actions.HIDEINFO);
                                                AppendTracks(tracks);
                                            }));
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.ParentForm.BeginInvoke(new MethodInvoker(() =>
                                        {
                                            CF_systemCommand(CF_Actions.HIDEINFO);
                                            WriteError(ex);
                                            CF_displayMessage(ex.Message);
                                        }));
                                }
                            });
                            break;
                        }
                    case GroupingType.Playlists:
                        {
                            var playlist = selectedRow["PlaylistObject"] as IPlaylist;
                            List<ITrack> tracks = new List<ITrack>();
                            foreach (var track in playlist.Tracks)
                            {
                                if (track.IsAvailable)
                                {
                                    tracks.Add(track);
                                }
                            }
                            AppendTracks(tracks);
                            break;
                        }

                }
            }
        }

        private void OnDynamic1Hold()
        {
            if (CurrentTab == Tabs.NowPlaying)
            {
                var list = advancedlistArray[CF_getAdvancedListID("mainList")];

                var dialogResult = CF_systemDisplayDialog(CF_Dialogs.YesNo, pluginLang.ReadField("/AppLang/Spotify/ConfirmClearPlaylist"));
                if (dialogResult == System.Windows.Forms.DialogResult.OK || dialogResult == System.Windows.Forms.DialogResult.Yes)
                {
                    StopAllPlayback();
                    Utility.DisposeTableDisposables(NowPlayingTable);
                    NowPlayingTable.Clear();
                    ShuffledTracks.Clear();
                    NonShuffledTracks.Clear();
                    list.Refresh();

                    //LK, 15-jul-2016: Save the now-playlist when changed and valid
                    if (nowPlayingTableLoaded)
                        SaveNowPlayingToFile();
                }
            }
        }

        private void OnDynamic2Hold()
        {
        }

        private void OnDynamic3Clicked()
        {
            if (TableStates.Count >= 2)
            {
                var state = TableStates.ElementAt(1); //stack enumerator is reversed
                var row = state.Table.Rows[state.Position];
                var playlist = row["PlaylistObject"] as IPlaylist;
                if (playlist.OfflineStatus == PlaylistOfflineStatus.No)
                {
                    var result = CF_systemDisplayDialog(CF_Dialogs.YesNo, pluginLang.ReadField("/AppLang/Spotify/ConfirmOfflinePlaylist"));
                    if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                    {
                        playlist.SetOfflineMode(true);
                    }
                }
                else
                {
                    var result = CF_systemDisplayDialog(CF_Dialogs.YesNo, pluginLang.ReadField("/AppLang/Spotify/ConfirmUnavailOfflinePlaylist"));
                    if (result == System.Windows.Forms.DialogResult.OK || result == System.Windows.Forms.DialogResult.Yes)
                    {
                        playlist.SetOfflineMode(false);
                    }
                }
                SetupDynamicButton3(playlist);
            }
        }

        private void AppendTracks(IEnumerable<ITrack> tracks)
        {
            //Ignore tracks that are already in the list
            var allNowPlayingTracks = NowPlayingTable.Rows.Cast<DataRow>().Select(row => row["TrackObject"] as ITrack);
            tracks = tracks.Where(t => !allNowPlayingTracks.Contains(t));

            if (tracks.Count() > 0)
            {
                if (!nowPlayingTableLoaded)
                    RestoreNowPlaying(false);

                //LK, 15-jul-2016: Warn user when overwriting an unloaded playlist
                if (!nowPlayingTableLoaded)
                {
                    var result = CF_systemDisplayDialog(CF_Dialogs.YesNo, pluginLang.ReadField("/AppLang/Spotify/ConfirmNowPlayingListNotLoaded"));
                    if (result != System.Windows.Forms.DialogResult.OK && result != System.Windows.Forms.DialogResult.Yes)
                    {
                        return;
                    }
                }

                if (tracks.Count() > 1)
                {
                    var result = CF_systemDisplayDialog(CF_Dialogs.YesNo, string.Format(pluginLang.ReadField("/AppLang/Spotify/ConfirmAddMultipleTracks"), tracks.Count(), Environment.NewLine));
                    if (result != System.Windows.Forms.DialogResult.OK && result != System.Windows.Forms.DialogResult.Yes)
                    {
                        return;
                    }
                }

                List<ITrack> tracksToShuffle = new List<ITrack>();

                foreach (var track in tracks)
                {
                    var clonedTrack = track.Clone(SpotifySession);
                    var newRow = this.NowPlayingTable.NewRow();
                    newRow["Name"] = clonedTrack.Name;
                    newRow["Artist"] = GetArtistsString(clonedTrack.Artists);
                    newRow["Album"] = clonedTrack.Album.Name;
                    newRow["Starred"] = GetStarredStatusString(clonedTrack.IsStarred);
                    newRow["Available"] = GetAvailableStatusString(clonedTrack.IsAvailable, false);
                    newRow["TrackObject"] = clonedTrack;

                    this.NowPlayingTable.Rows.Add(newRow);
                    nowPlayingTableLoaded = true;       //LK, 15-jul-2015: Now-Playinglist is now valid (when it wasn't already)

                    if (clonedTrack.IsAvailable)    //LK, 11-jun-2016: Only add available tracks to shuffled and non-shuffled lists
                    {
                        tracksToShuffle.Add(clonedTrack);
                        NonShuffledTracks.AddLast(clonedTrack);
                    }
                }

                IEnumerable<ITrack> songsToAppend = null;
                if (currentTrack != null)
                {
                    var songNode = ShuffledTracks.Find(currentTrack);
                    if (songNode!=null && songNode.Next != null)
                    {
                        //there are songs after the ones we're playing right now
                        var trimmedPlaylist = ShuffledTracks.TakeWhile(s => s != songNode.Next.Value).ToArray();
                        var remainder = ShuffledTracks.Where(s => !trimmedPlaylist.Contains(s)).ToArray();

                        ShuffledTracks = new LinkedList<ITrack>(trimmedPlaylist);
                        songsToAppend = tracksToShuffle.Concat(remainder).ToArray();
                    }
                    else
                        songsToAppend = tracksToShuffle;
                }
                else
                    songsToAppend = tracksToShuffle;

                songsToAppend = ShuffleSongs(songsToAppend);

                ShuffledTracks = new LinkedList<ITrack>(ShuffledTracks.Concat(songsToAppend));

                CF_systemCommand(CF_Actions.CLICKSOUND);

                if (currentTrack == null)
                {
                    if (ShuffleOn)
                    {
                        PlayTrack(ShuffledTracks.First.Value);
                    }
                    else
                    {
                        PlayTrack(NowPlayingTable.Rows[0]["TrackObject"] as ITrack);
                    }
                }

                //LK, 15-jul-2016: Save the now-playlist when changed and valid
                if (nowPlayingTableLoaded)
                    SaveNowPlayingToFile();
            }
        }

        private IEnumerable<ITrack> ShuffleSongs(IEnumerable<ITrack> songsLeftToPlay)
        {
            var r = new Random();
            return songsLeftToPlay.OrderBy(x => r.Next()).ToArray();
        }

        private string GetArtistsString(IArray<IArtist> artists)
        {
            return string.Join(", ", artists.Select(a => a.Name).ToArray());
        }

        private bool CheckLoggedIn()
        {
            if (SpotifySession == null || !loginComplete)
            {
                WriteLog("Login NOT complete");
                CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/NotLoggedIn"));
                return false;
            }
            WriteLog("Login complete");
            return true;
        }

        private bool CheckLoggedInAndOnline()
        {
            if (SpotifySession == null || !loginComplete)
            {
                WriteLog("Login NOT complete");
                CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/NotLoggedIn"));
                return false;
            }
            if (!this.CF_getConnectionStatus())
            {
                WriteLog("Login NOT on-line");
                CF_displayMessage(pluginLang.ReadField("/AppLang/Spotify/NeedToBeOnline"));
                return false;
            }

            WriteLog("On-line and login complete");
            return true;
        }

        public override DialogResult CF_pluginShowSetup()
        {
            WriteLog("Start");

            // Return DialogResult.OK for the main application to update from plugin changes.
            DialogResult returnvalue = DialogResult.Cancel;

            try
            {
                // Creates a new plugin setup instance. If you create a CFDialog or CFSetup you must
                // set its MainForm property to the main plugins MainForm property.
                Setup setup = new Setup(this.MainForm, this.pluginConfig, this.pluginLang);
                returnvalue = setup.ShowDialog();

                if (returnvalue == DialogResult.OK)
                {
                    if(LoadSettings() && SpotifySession != null && (SpotifySession.ConnectionState & sp_connectionstate.LOGGED_IN) > 0)
                    {
                        //settings changed, reconnect
                        SpotifySession.Logout();
                    }
                    if (SpotifySession != null)
                        SpotifySession.SetPrefferedBitrate(preferredBitrate);
                }

                setup.Close();
                setup = null;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.ToString()); }


            WriteLog("Stop");
            return returnvalue;
        }

        private bool LoadSettings()
        {
            bool retValue = false;

            // Note: this is function is also called when there where changes made during setup
            //  So, the display name may be changed
            if (CF_params.displayName != pluginLang.ReadField("/AppLang/Spotify/DisplayName"))
                CF_params.displayName = pluginLang.ReadField("/AppLang/Spotify/DisplayName");

            if (CF_params.settingsDisplayName != pluginLang.ReadField("/AppLang/Spotify/DisplayName"))
                CF_params.settingsDisplayName = pluginLang.ReadField("/AppLang/Spotify/DisplayName");

            if (CF_params.settingsDisplayDesc != pluginLang.ReadField("/AppLang/Spotify/Description"))
                CF_params.settingsDisplayDesc = pluginLang.ReadField("/AppLang/Spotify/Description");

            string newUsername = this.pluginConfig.ReadField("/APPCONFIG/USERNAME");
            if (newUsername != username)
            {
                retValue = true;
            }
            username = newUsername;

            string _encryptedPassword = this.pluginConfig.ReadField("/APPCONFIG/PASSWORD");

            if (!String.IsNullOrEmpty(_encryptedPassword))
            {
                try
                {
                    password = EncryptionHelper.DecryptString(_encryptedPassword, Setup.PASSWORD);
                }
                catch (Exception ex)
                {
                    CF_displayMessage(ex.Message);
                    WriteError(ex);
                }
            }

            string tempPathString = this.pluginConfig.ReadField("/APPCONFIG/LOCATION");
            if (!string.IsNullOrEmpty(tempPathString))
                tempPath = tempPathString;
            else
                tempPath = Utility.GetDefaultLocationPath();

            string bitrateString = this.pluginConfig.ReadField("/APPCONFIG/BITRATE");
            if (string.IsNullOrEmpty(bitrateString))
            {
                preferredBitrate = sp_bitrate.BITRATE_160k;
            }
            else
            {
                try
                {
                    object bitrate = Enum.Parse(typeof(sp_bitrate), bitrateString);
                    preferredBitrate = (sp_bitrate)bitrate;
                }
                catch
                {
                    preferredBitrate = sp_bitrate.BITRATE_160k;
                }
            }

            //LK, 22-may-2016: Add powerResumeDelay parameter to allow the computer to reconnect to the Internet
            string powerResumeDelayString = this.pluginConfig.ReadField("/APPCONFIG/POWERRESUMEDELAY");
            if (string.IsNullOrEmpty(powerResumeDelayString))
            {
                powerResumeDelay = 5000;
            }
            else
            {
                try
                {
                    powerResumeDelay = int.Parse(powerResumeDelayString);
                }
                catch
                {
                    powerResumeDelay = 5000;
                }
            }

            //[GrantA] Load music auto play setting.
            string newAutoPlay = this.pluginConfig.ReadField("/APPCONFIG/AUTOPLAY");
            if (newAutoPlay.Equals("True"))
            {
                autoPlay = true;
            }
            else
            {
                autoPlay = false;
            }

            //LK, 22-may-2016: Add support for auto loop playlists
            string newAutoLoop = this.pluginConfig.ReadField("/APPCONFIG/AUTOLOOP");
            if (newAutoLoop.Equals("True"))
            {
                autoLoop = true;
            }
            else
            {
                autoLoop = false;
            }

            //LK, 22-may-2016: Add support for module event logging
            string newLogEvents = this.pluginConfig.ReadField("/APPCONFIG/LOGEVENTS");
            if (newLogEvents.Equals("True"))
            {
                LogEvents = true;
            }
            else
            {
                LogEvents = false;
            }

            return retValue;
        }


        //LK, 22-may-2016, Begin: Add support for album art and visualizations
        internal void SetVisImage(Image visimage, string imageId)
        {
            WriteLog("Start");

            try
            {
                if (miniPicVis != null)
                {
                    if (visimage != null)
                    {
                        Image tempImage = ResizeToFitBox((Image)visimage.Clone(), new Rectangle(new Point(), visimage.Size));
                        string visImagePath = tempPath + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + "VisImage_" + imageId + ".png";
                        if (!File.Exists(visImagePath))
                            tempImage.Save(visImagePath, System.Drawing.Imaging.ImageFormat.Png);
                        currentVisImagePath = visImagePath;

                        if (!pluginNonBufferedVisSupport)
                        {
                            miniPicVis.picVis.Image = tempImage;
                            if (miniPicVis.picVis.Image != null)
                            {
                                miniPicVis.picVis.SizeMode = PictureBoxSizeMode.StretchImage;
                            }
                            else
                            {
                                currentVisImagePath = "NOVIS";
                            }
                        }
                        miniPicVis.picVis.Invalidate();
                    }
                    else
                        currentVisImagePath = "NOVIS";
                }
            }
            catch (Exception ex) { CFTools.writeError("Spotify:" + ex.Message, ex.StackTrace); }

            WriteLog("Stop");
        }


        public override IntPtr CF_pluginVisPicHandle()
        {
            try
            {
                if (miniPicVis != null)
                    return miniPicVis.picVis.Handle;
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
            return IntPtr.Zero;
        }


        public override void CF_pluginResetVis()
        {
            try
            {
                if (miniPicVis != null)
                    miniPicVis.ResetVis();
            }
            catch (Exception errmsg) { CFTools.writeError(errmsg.Message, errmsg.StackTrace); }
        }


        public override void CF_pluginSetVisImage(Image visimage)
        {
            WriteLog("Start");

            try
            {
                if (miniPicVis != null)
                {
                    miniPicVis.picVis.Image = visimage;
                    miniPicVis.picVis.SizeMode = PictureBoxSizeMode.StretchImage;
                    miniPicVis.picVis.Invalidate();
                }
            }
            catch (Exception ex) { CFTools.writeError("Spotify:" + ex.Message, ex.StackTrace); }

            WriteLog("Stop");
        }


        public override Rectangle CF_pluginGetVisBounds()
        {
            WriteLog("Start");

            try
            {
                if (miniPicVis != null)
                    return miniPicVis.picVis.Bounds;
            }
            catch (Exception ex) { CFTools.writeError("Spotify:" + ex.Message, ex.StackTrace); }
            return new Rectangle(0, 0, 0, 0);
        }
        //LK, 22-may-2016, End: Add support for album art and visualizations



        public Form ParentForm
        {
            get
            {
                return this.MainForm as Form;
            }
        }
    }

    internal enum Tabs
    {
        NowPlaying,
        Search,
        Playlists,
        Inbox,
        Popular
    }

    internal enum GroupingType
    {
        Songs,
        Artists,
        Albums,
        Playlists
    }
}
