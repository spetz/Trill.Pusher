using System.Threading.Channels;
using Trill.Pusher.Events.External;

namespace Trill.Pusher.Channels
{
    public class StorySentChannels
    {
        private readonly Channel<StorySent> _channel;

        public StorySentChannels()
        {
            _channel = Channel.CreateUnbounded<StorySent>();
        }

        public ChannelWriter<StorySent> Writer => _channel.Writer;
        public ChannelReader<StorySent> Reader => _channel.Reader;
    }
}