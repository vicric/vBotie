using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using GiphyDotNet.Manager;
using GiphyDotNet.Model.Parameters;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            if (message.Text.ToLower() == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Text.ToLower() == "set")
            {
                PromptDialog.Number(
                    context,
                    AfterSetAsync,
                    "What number do you want to set the count to?");
            }
            else if (message.Text.ToLower() == "set choice")
            {
                var options = new int[] {10, 100, 1000, 3000};
                var descriptions = new string[]{"Set count to 10", "Set count to 100", "Set count to 1000", "Set count to 3000"};
                PromptDialog.Choice<int>(
                    context,
                    AfterSetChoiceAsync,
                    options,
                    "What number do you want to set the count to?",
                    descriptions: descriptions );
            }
            else if (message.Text.ToLower() == "attachment")
            {
                PromptDialog.Attachment(
                    context,
                    AfterAttachmentAsync,
                    "Okey, send me an attachment");
            }
            else if (message.Text.ToLower() == "giphy")
            {
                PromptDialog.Text(
                    context, 
                    ResumeAfterTagClarification, 
                    "What tag?");
            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }

        public async Task AfterResetAsync(IDialogContext context, IAwaitable<bool> argument)
        {
            var confirm = await argument;
            if (confirm)
            {
                this.count = 1;
                await context.PostAsync("Reset count.");
            }
            else
            {
                await context.PostAsync("Did not reset count.");
            }
            context.Wait(MessageReceivedAsync);
        }
        
        public async Task AfterSetAsync(IDialogContext context, IAwaitable<long> result)
        {
            var amount = await result;
            this.count = (int)amount;
            await context.PostAsync($"Count set to {this.count}.");
            
            context.Wait(MessageReceivedAsync);
        }
        
        public async Task AfterSetChoiceAsync(IDialogContext context, IAwaitable<int> result)
        {
            var amount = await result;
            this.count = amount;
            await context.PostAsync($"Count set to {this.count}.");
            
            context.Wait(MessageReceivedAsync);
        }
        
        public async Task AfterAttachmentAsync(IDialogContext context, IAwaitable<IEnumerable<Attachment>> result)
        {
            var atts = await result;
            foreach(var att in atts)
            {
                await context.PostAsync($"You've sent me a file with type '{att.ContentType}', I'll see what I can do!");
            }
            
            context.Wait(MessageReceivedAsync);
        }

        public async Task ResumeAfterTagClarification(IDialogContext context, IAwaitable<string> result)
        {
            var tag = await result;
            var giphy = new Giphy("fdUMs7mLyMOCTmDNSnPJpBuQyPuxgUz7");

            var stickerresult = await giphy.RandomSticker(new RandomParameter()
            {
                Tag = tag
            });

            // setup a message that can hold the generated image
            var replyMessage = context.MakeMessage();

            var animationCard = new AnimationCard
            {
                Image = new ThumbnailUrl
                {
                    Url = "https://docs.microsoft.com/en-us/bot-framework/media/how-it-works/architecture-resize.png"
                },
                Media = new List<MediaUrl>
                {
                    new MediaUrl()
                    {
                        Url = stickerresult.Data.FixedHeightSmallUrl
                    }
                }
            };

            replyMessage.Attachments = new List<Attachment> { animationCard.ToAttachment() };

            // return our reply to the user
            await context.PostAsync(replyMessage);

            context.Wait(MessageReceivedAsync);
        }

    }
}