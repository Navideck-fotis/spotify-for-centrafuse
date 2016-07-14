using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Spotify.JSON;
using SpotiFire.SpotifyLib;

namespace Spotify
{
    public partial class Spotify
    {
        private const string PANDORA_PLUGIN_NAME = "OpenPandora";
        public override string CF_pluginData(string command, string param1)
        {
            switch (command)
            {
                  
                case "VISIMAGEPATH":    //LK, 22-may-2016: For pre CF V4.5.x support only
                    if (string.IsNullOrEmpty(currentVisImagePath))
                        return "VISOFF";
                    else
                        return currentVisImagePath;

                case "Ping": 
                    return true.ToString();

                case "Search":
                    var serializer = new JavaScriptSerializer();
                    try
                    {
                        var request = serializer.Deserialize<SearchRequest>(param1);
                        if (request == null)
                            throw new Exception();

                        PerformAsyncSearch(request);
                        return true.ToString();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }

                case "Star": 
                    serializer = new JavaScriptSerializer();
                    try
                    {
                        var song = serializer.Deserialize<Song>(param1);
                        if (song == null)
                            throw new Exception();

                        var track = SpotifySession.ParseLink(song.Id).As<ITrack>();
                        track.IsStarred = true;
                        track.Dispose();
                        return true.ToString();
                    }
                    catch (Exception ex)
                    {
                        return ex.Message;
                    }
                default: return "Unrecognized";
            }
        }

        private void PerformAsyncSearch(SearchRequest request)
        {
            InitSpotifyClient();
            if (SpotifySession == null)
            {
                var response = new SearchResponse()
                {
                    Success = false,
                    Error = "Spotify failed to initialize"
                };

                var serializer = new JavaScriptSerializer();
                var serializedResponse = serializer.Serialize(response);

                CF_getPluginData(PANDORA_PLUGIN_NAME, "SearchResponse", serializedResponse);

                return;
            }

            Task.Factory.StartNew(() =>
                {
                    var search = SpotifySession.SearchTracks(string.Format("{0} {1}", request.Artist, request.Title), 0, 5);

                    if (search != null) //LK, 22-may-2016: When no search text supplied
                    {
                        SleepUntilTrue(() => search.IsComplete);

                        List<ITrack> tracks = new List<ITrack>();
                        foreach (var track in search.Tracks)
                        {
                            if (track.IsAvailable)
                            {
                                tracks.Add(track);
                            }
                        }
                        search.Dispose();

                        var perfectMatch = tracks.FirstOrDefault(t =>
                            t.Album.Name.Equals(request.Album, StringComparison.CurrentCultureIgnoreCase) &&
                            t.Name.Equals(request.Title, StringComparison.CurrentCultureIgnoreCase) &&
                            t.Artists.Any(a => a.Name.Equals(request.Artist, StringComparison.CurrentCultureIgnoreCase)));


                        var serializer = new JavaScriptSerializer();
                        SearchResponse response;
                        if (perfectMatch != null)
                        {
                            perfectMatch.IsStarred = true;
                            response = new SearchResponse()
                            {
                                Success = true,
                                PerfectMatchBookmark = true
                            };
                        }
                        else
                        {
                            response = new SearchResponse()
                            {
                                Success = true,
                                Results = tracks.Select(t =>
                                    {
                                        var s = new Song()
                                        {
                                            Name = t.Name,
                                            Album = t.Album.Name,
                                            Artist = string.Join(", ", t.Artists.Select(a => a.Name).ToArray()),
                                        };

                                        var link = t.CreateLink();
                                        s.Id = link.ToString();
                                        link.Dispose();
                                        return s;
                                    }).ToArray()
                            };
                        }
                        var serializedResponse = serializer.Serialize(response);

                        CF_getPluginData(PANDORA_PLUGIN_NAME, "SearchResponse", serializedResponse);

                        tracks.ForEach(t => t.Dispose());
                    }
                });
        }

    }
}
