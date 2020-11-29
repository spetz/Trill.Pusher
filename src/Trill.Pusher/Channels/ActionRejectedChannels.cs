using System.Threading.Channels;

namespace Trill.Pusher.Channels
{
    public class ActionRejectedChannels
    {
        private readonly Channel<ActionRejected> _channel;

        public ActionRejectedChannels()
        {
            _channel = Channel.CreateUnbounded<ActionRejected>();
        }

        public ChannelWriter<ActionRejected> Writer => _channel.Writer;
        public ChannelReader<ActionRejected> Reader => _channel.Reader;
    }
}