
namespace SpotiFire.SpotifyLib
{
    public class TrackMessageEventArgs : TrackEventArgs
    {
        string message;
        int position;

        public TrackMessageEventArgs(ITrack track, int position, string message) 
            : base(track)
        {
            this.position = position;
            this.message = message;
        }

        public int Position
        {
            get { return this.position; }
        }

        public string Message
        {
            get { return this.message; }
        }

    }
}
