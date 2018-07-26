using System;
using System.Threading.Tasks;
using System.Collections.Generic;

using Microsoft.Bot.Connector;
using Microsoft.Bot.Builder.Dialogs;
using System.Net.Http;


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

            if (message.Text == "reset")
            {
                PromptDialog.Confirm(
                    context,
                    AfterResetAsync,
                    "Are you sure you want to reset the count?",
                    "Didn't get that!",
                    promptStyle: PromptStyle.Auto);
            }
            else if (message.Text == "set")
            {
                PromptDialog.Number(
                    context,
                    AfterSetAsync,
                    "What number do you want to set the count to?");
            }
            else if (message.Text == "set choice")
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
            else if (message.Text == "attachment")
            {
                PromptDialog.Attachment(
                    context,
                    AfterAttachmentAsync,
                    "Okey, send me an attachment");
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

    }
}