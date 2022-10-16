using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace TTMC.Discord
{
    public class Discord
    {
		public virtual void MessageCreate(Message message) { }
	}
    public class Profile
    {
        public Discord discord = new();
		private static int? sequence = null;
		private static int heartbeat_interval = 0;
		private int intents = 1 << 9;
		private string? token = null;
        private ClientWebSocket webSocket = new();
		private HttpClient client = new();
        private Task? getMessages = null;
        private Task? heartbeat = null;
		public Profile(string token)
        {
            this.token = token;
            client.DefaultRequestHeaders.Add("Authorization", token);
            string json = client.GetStringAsync("https://discord.com/api/v10/gateway/bot").Result;
			GatewayBot? gatewayBot = JsonSerializer.Deserialize<GatewayBot>(json);
            if (gatewayBot != null && !string.IsNullOrEmpty(gatewayBot.url))
            {
				webSocket.ConnectAsync(new Uri(gatewayBot.url), new()).Wait();
                getMessages = new(() => GetMessage());
				heartbeat = new(() => SendHeartBeat());
				getMessages.Start();
			}
            SafeExit.Trigger();
        }
		public void SendHeartBeat()
		{
			while (true)
			{
				if (heartbeat_interval > 0)
				{
					byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Heartbeat() { op = 1, d = sequence }));
					webSocket.SendAsync(data, WebSocketMessageType.Text, true, new()).Wait();
					Thread.Sleep(heartbeat_interval);
				}
			}
		}
        
		public void SendIdentity()
		{
			IdentifyingProperties identifyingProperties = new() { os = "windows", browser = "ttmc", device = "ttmc" };
			IdentifyingX identifyingX = new() { token = token, intents = intents, properties = identifyingProperties };
			Identifying identifying = new() { op = 2, d = identifyingX };
			byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(identifying));
			webSocket.SendAsync(data, WebSocketMessageType.Text, true, new()).Wait();
		}
		public void GetMessage()
        {
			byte[] bytes = new byte[ushort.MaxValue];
			while (true)
            {
				WebSocketReceiveResult resp = webSocket.ReceiveAsync(bytes, new()).Result;
				if (resp.Count > 0)
                {
					string json = Encoding.UTF8.GetString(bytes, 0, resp.Count);
					opCheck? nzx = JsonSerializer.Deserialize<opCheck>(json);
                    if (nzx != null)
                    {
                        if (nzx.op == 0)
                        {
                            OpZero? zero = JsonSerializer.Deserialize<OpZero>(json);
                            if (zero != null && !string.IsNullOrEmpty(zero.t) && zero.d != null)
                            {
                                if (zero.t == "MESSAGE_CREATE")
                                {
                                    JsonElement element = (JsonElement)zero.d;
                                    Message? message = JsonSerializer.Deserialize<Message>(element.GetRawText());
                                    if (message != null)
                                    {
                                        message.client = client;
                                        discord.MessageCreate(message);
                                    }
                                }
                            }
                        }
						else if (nzx.op == 10)
						{
							DiscordHello? hello = JsonSerializer.Deserialize<DiscordHello>(json);
							if (hello != null && hello.d != null && heartbeat != null)
							{
								heartbeat_interval = hello.d.heartbeat_interval - 1000;
								heartbeat.Start();
								SendIdentity();
							}
						}
					}
				}
			}
        }
        public string SetBIO(string text, string emoji = "")
        {
            StringContent content = new("{\"custom_status\":{\"text\":\"" + text + "\",\"emoji_name\":\"" + emoji + "\"}}", Encoding.UTF8, "application/json");
            var url = "https://com/api/v9/users/@me/settings";
            HttpResponseMessage resp = client.PatchAsync(url, content).Result;
            return resp.Content.ReadAsStringAsync().Result;
        }
        public User? UserInfo(string userID = "@me")
        {
            string resp = client.GetStringAsync("https://discord.com/api/v9/users/" + userID).Result;
            return JsonSerializer.Deserialize<User>(resp);
        }
        public string? AvatarURL(string userID = "@me")
        {
            User? usr = UserInfo(userID);
            if (usr != null && usr.avatar != null)
            {
                return "https://cdn.discordapp.com/avatars/" + userID + "/" + usr.avatar + ".png?size=2048";
            }
            return null;
        }
        public Channel? CreateDM(string userID)
        {
            StringContent content = new("{\"recipient_id\":\"" + userID + "\"}", Encoding.UTF8, "application/json");
            HttpResponseMessage resp = client.PostAsync("https://discord.com/api/v9/users/@me/channels", content).Result;
            Channel? channel = JsonSerializer.Deserialize<Channel>(resp.Content.ReadAsStringAsync().Result);
            if (channel != null) { channel.client = client; }
            return channel;
        }
        public Guild? GetGuild(string guildID)
        {
            string resp = client.GetStringAsync("https://discord.com/api/v9/guilds/" + guildID).Result;
            return JsonSerializer.Deserialize<Guild>(resp);
        }
    }
}