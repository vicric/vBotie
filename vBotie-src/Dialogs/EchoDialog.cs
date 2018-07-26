using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;
using SimpleEchoBot;

namespace Microsoft.Bot.Sample.SimpleEchoBot
{
    [Serializable]
    public class EchoDialog : IDialog<object>
    {
        protected int count = 1;

        private MemeAPI _api = new MemeAPI();
        private Meme _meme;
        private string _topText;
        private string _bottomText;

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
            else if (message.Text.ToLower() == "memes")
            {
                var memes = new Meme[] { Meme.Brian, Meme.Fry, Meme.GrumpyCat, Meme.OMG, Meme.Spiderman, Meme.Troll, Meme.Wonka };
                var descriptions = new string[] { "Bad luck Brian", "Fry", "Grumpy Cat", "OMG", "Spiderman", "Troll", "Wonka" };
                PromptDialog.Choice<Meme>(context, ResumeAfterMemeSelectionClarification,
                    memes, "Hi, I'm MemeBot. Which meme should I generate?", descriptions: descriptions);
            }
            else
            {
                await context.PostAsync($"{this.count++}: You said {message.Text}");
                context.Wait(MessageReceivedAsync);
            }
        }

        #region Counter

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

        #endregion

        #region MEME

        private async Task ResumeAfterMemeSelectionClarification(IDialogContext context, IAwaitable<Meme> result)
        {
            _meme = await result;
            PromptDialog.Text(context, ResumeAfterTopTextClarification, "Awesome, I can work with that. What will the *top text* be?");
        }

        private async Task ResumeAfterTopTextClarification(IDialogContext context, IAwaitable<string> result)
        {
            _topText = await result;
            PromptDialog.Text(context, ResumeAfterBottomTextClarification, "Good, now what will the *bottom text* be?");
        }

        private async Task ResumeAfterBottomTextClarification(IDialogContext context, IAwaitable<string> result)
        {
            _bottomText = await result;

            // setup a message that can hold the generated image
            var replyMessage = context.MakeMessage();
            var image = new Attachment();
            image.ContentType = "image/jpeg";
            image.ContentUrl = $"data:image/jpeg;base64,{ await _api.GenerateMeme(_meme, _topText, _bottomText) }";
            replyMessage.Attachments = new List<Attachment> { image };

            // return our reply to the user
            await context.PostAsync(replyMessage);
            await context.PostAsync("I hope you like your meme. If not, generate a new one!");

            context.Wait(MessageReceivedAsync);
        }

        #endregion

    }
}