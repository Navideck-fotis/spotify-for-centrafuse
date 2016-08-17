using System;
namespace SpotiFire.SpotifyLib
{
    public interface ITrack : ISpotifyObject, IAsyncLoaded, IDisposable
    {
        IAlbum Album { get; }
        IArray<IArtist> Artists { get; }
        int Disc { get; }
        TimeSpan Duration { get; }
        sp_error Error { get; }
        int Index { get; }
        bool IsAvailable { get; }
        bool IsOfflineAvailable { get; }
        //LK, 11-jun-2016: Inherrited from IAsyncLoaded: //bool IsLoaded { get; }
        bool IsStarred { get; set; }
        bool IsPlaceholder { get; }
        string Name { get; }
        int Popularity { get; }
        ITrack Clone(ISession session);
    }
}
