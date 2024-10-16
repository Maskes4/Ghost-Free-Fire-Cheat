using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using DiscordRPC;

namespace External
{
    internal class RPC
    {
        public static DiscordRpcClient client;
        public static Timestamps rpctimestamp { get; set; }
        private static RichPresence presence;

        public static void InitializeRPC()
        {
            // Replace "YOUR_DISCORD_APP_ID_HERE" with your actual Discord application ID
            client = new DiscordRpcClient("1220519022748237885");
            client.Initialize();

            // Set up buttons with appropriate URLs
            DiscordRPC.Button[] buttons = {
                new DiscordRPC.Button() { Label = "Free Panel", Url = "" },
                new DiscordRPC.Button() { Label = "WhatsApp", Url = "" }
            };

            // Set up the initial presence
            presence = new RichPresence()
            {
                Buttons = buttons,
                Timestamps = rpctimestamp,

                Assets = new Assets()
                {
                    LargeImageKey = "https://i.postimg.cc/Kv91q0Bn/ghost-cheat.png",
                    LargeImageText = "Ghoost.xd",
                    SmallImageKey = "https://media4.giphy.com/media/xmOMPI63SsyZyKz2Tx/giphy.gif?cid=790b7611485d6e9b471bcd8f93609e96f8a02c35a7e05685&rid=giphy.gif&ct=s",
                    SmallImageText = ""
                }
            };

            client.SetPresence(presence);
            UpdateDiscordPresence();
        }

        public static void SetState(string state, bool watching = false)
        {
            if (watching)
                state = "Looking at " + state;

            presence.State = state;
            client.SetPresence(presence);
        }

        private static void UpdateDiscordPresence()
        {
            // Check if the user is logged in before accessing the username and expiry date
            if (Form1.KeyAuthApp.user_data != null)
            {
                string username = Form1.KeyAuthApp.user_data.username;
                DateTime expiryDateTime = UnixTimeToDateTime(long.Parse(Form1.KeyAuthApp.user_data.subscriptions[0].expiry));

                presence.Details = $"User : {username}";
                presence.State = $"Expiry Date: {expiryDateTime:yyyy-MM-dd HH:mm}";
            }
            else
            {
                presence.Details = "USER";
                presence.State = "";
            }

            client.SetPresence(presence);
        }

        private static DateTime UnixTimeToDateTime(long unixTime)
        {
            DateTime unixStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return unixStart.AddSeconds(unixTime).ToLocalTime();
        }
    }
}
