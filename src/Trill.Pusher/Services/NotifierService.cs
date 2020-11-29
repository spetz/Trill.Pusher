using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Trill.Pusher.Channels;

namespace Trill.Pusher.Services
{
    public class NotifierService : Notifier.NotifierBase
    {
        private readonly StorySentChannels _storySentChannels;
        private readonly ActionRejectedChannels _actionRejectedChannels;
        private readonly ILogger<NotifierService> _logger;

        public NotifierService(StorySentChannels storySentChannels, ActionRejectedChannels actionRejectedChannels,
            ILogger<NotifierService> logger)
        {
            _storySentChannels = storySentChannels;
            _actionRejectedChannels = actionRejectedChannels;
            _logger = logger;
        }

        public override async Task StreamStories(SubscribeStories request, IServerStreamWriter<Story> responseStream,
            ServerCallContext context)
        {
            await foreach (var storyCreated in _storySentChannels.Reader.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _logger.LogInformation($"Channel has received story created {storyCreated.StoryId}");
                await responseStream.WriteAsync(new Story
                {
                    Id = storyCreated.StoryId,
                    Title = storyCreated.Title,
                    CreatedAt = $"{storyCreated.CreatedAt:u}",
                    Author = new StoryAuthor
                    {
                        Id = storyCreated.Author.Id.ToString(),
                        Name = storyCreated.Author.Name
                    },
                    Tags = {storyCreated.Tags}
                });
            }

            _logger.LogInformation("Closing the stories stream.");
        }

        public override async Task StreamRejectedActions(SubscribeRejectedActions request,
            IServerStreamWriter<ActionRejected> responseStream,
            ServerCallContext context)
        {
            await foreach (var actionRejected in _actionRejectedChannels.Reader.ReadAllAsync())
            {
                if (context.CancellationToken.IsCancellationRequested)
                {
                    break;
                }

                _logger.LogInformation("Channel has received action rejected");
                await responseStream.WriteAsync(new ActionRejected
                {
                    Code = actionRejected.Code,
                    Reason = actionRejected.Reason
                });
            }

            _logger.LogInformation("Closing the action rejected stream.");
        }
    }
}