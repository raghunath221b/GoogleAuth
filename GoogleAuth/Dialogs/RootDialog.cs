using GoogleAuth.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace GoogleAuth.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<string>
    {
        private static Dictionary<string, string> endpoints = new Dictionary<string, string>()
        {
              { "Google", "https://www.googleapis.com/userinfo/v2/me" },
              { "Facebook", "https://graph.facebook.com/v2.5/me?fields=id,name,picture" }
        };

        [NonSerialized]
        private static string[] welcomeList = new string[] { "hello", "hi", "hey", "start", "/start", "\\start", "yo", "lanuch", "how is it going", "how's going" };

        public async Task StartAsync(IDialogContext context)
        {      
            context.Wait(MessageReceivedAsync);
        }
        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            var message = await item as Activity;

            bool isStartMessage = welcomeList.Any(s => message.Text.ToLower().Contains(s));
            if (!isStartMessage)
            {
                await context.PostAsync("This skill can help you find, create and delete events on your Google Calendar.");
            }
            else
            {
                // Save the message for later
                context.ConversationData.SetValue<Activity>("OriginalMessage", (Activity)message);

                var provider = "Google";
                context.ConversationData.SetValue<string>("AuthProvider", provider);

                // Prepare the AuthenticationOptions and then forward to the AuthDialog
                AuthenticationOptions options = new AuthenticationOptions()
                {
                    ClientType = provider
                };
                await context.Forward(new AuthDialog(new GenericOAuth2Provider($"GenericOAuth2Provider{provider}"), options), async (IDialogContext authContext, IAwaitable<AuthResult> authResult) =>
                {
                    var result = await authResult;

                    // Use token to call into service
                    var prov = authContext.ConversationData.GetValue<string>("AuthProvider");
                    var endpoint = endpoints[prov];
                    var json = await new HttpClient().GetWithAuthAsync(result.AccessToken, endpoint);
                    //string msg = $"I'm a simple bot that doesn't do much, but I know your name is {json.Value<string>("name")} and your {prov} id is {json.Value<string>("id")}";
                    //await authContext.PostAsync(msg);

                    //// Wait for another message
                    //authContext.Wait(MessageReceivedAsync);
                }, context.ConversationData.GetValue<Activity>("OriginalMessage"), CancellationToken.None);

            }
        }
    }
}