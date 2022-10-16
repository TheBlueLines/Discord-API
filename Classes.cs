using System.Text;
using System.Text.Json;

namespace TTMC.Discord
{
    public class Guild
    {
        private HttpClient client = new();
        public string? id { get; set; }
        public List<Channel>? Channels()
        {
            string resp = client.GetStringAsync("https://discord.com/api/v9/guilds/" + id + "/channels").Result;
            return JsonSerializer.Deserialize<List<Channel>>(resp);
        }
    }
    public class Overwrite
    {
        public string? id { get; set; }
        public int type { get; set; }
        public string? allow { get; set; }
        public string? deny { get; set; }
    }
    public class ThreadMetadata
    {
        public bool archived { get; set; }
        public int auto_archive_duration { get; set; }
        public string? archive_timestamp { get; set; }
        public bool locked { get; set; }
        public bool? invitable { get; set; }
        public string? create_timestamp { get; set; }
    }
    public class ThreadMember
    {
        public string? id { get; set; }
        public string? user_id { get; set; }
        public string? join_timestamp { get; set; }
        public int flags { get; set; }
    }
    public class Channel
    {
        private Random random = new();
        internal HttpClient client = new();
        public string? id { get; set; }
        public int type { get; set; }
        public string? guild_id { get; set; }
        public int? position { get; set; }
        public List<Overwrite>? permission_overwrites { get; set; }
        public string? name { get; set; }
        public string? topic { get; set; }
        public bool? nsfw { get; set; }
        public string? last_message_id { get; set; }
        public int? bitrate { get; set; }
        public int? user_limit { get; set; }
        public int? rate_limit_per_user { get; set; }
        public List<User>? recipients { get; set; }
        public string? icon { get; set; }
        public string? owner_id { get; set; }
        public int? application_id { get; set; }
        public string? parent_id { get; set; }
        public string? last_pin_timestamp { get; set; }
        public string? rtc_region { get; set; }
        public int? video_quality_mode { get; set; }
        public int? message_count { get; set; }
        public int? member_count { get; set; }
        public ThreadMetadata? thread_metadata { get; set; }
        public ThreadMember? member { get; set; }
        public int? default_auto_archive_duration { get; set; }
        public string? permissions { get; set; }
        public int? flags { get; set; }
        public int? total_message_sent { get; set; }

        public Message? SendMessage(string text, bool tts = false, long? nonce = null)
        {
            long? tmp = nonce == null ? random.NextInt64(100000000000000000, 999999999999999999) : nonce;
            string data = "{\"content\":\"" + text + "\",\"nonce\":\"" + tmp + "\",\"tts\":" + tts.ToString().ToLower() + "}";
            var url = "https://canary.discord.com/api/v9/channels/" + id + "/messages";
            StringContent content = new(data, Encoding.UTF8, "application/json");
            HttpResponseMessage resp = client.PostAsync(url, content).Result;
            Message? message = JsonSerializer.Deserialize<Message>(resp.Content.ReadAsStringAsync().Result);
            if (message != null) { message.client = client; }
			return message;
        }
        public string React(string nonce, string emoji)
        {
            var url = "https://canary.discord.com/api/v9/channels/" + id + "/messages/" + nonce + "/reactions/" + emoji + "/@me";
            HttpResponseMessage resp = client.PutAsync(url, null).Result;
            return resp.Content.ReadAsStringAsync().Result;
        }
        public Channel? AddToGroup(string userID)
        {
            string url = "https://canary.discord.com/api/v9/channels/" + id + "/recipients/" + userID;
			HttpResponseMessage resp = client.PutAsync(url, null).Result;
            string? text = resp.Content.ReadAsStringAsync().Result;
			return string.IsNullOrEmpty(text) ? null : JsonSerializer.Deserialize<Channel>(text);
        }
        public Channel? LeaveGroup()
        {
            HttpResponseMessage resp = client.DeleteAsync("https://canary.discord.com/api/v9/channels/" + id).Result;
            return JsonSerializer.Deserialize<Channel>(resp.Content.ReadAsStringAsync().Result);
        }
    }
    public class User
    {
        public string? id { get; set; }
        public string? username { get; set; }
        public string? avatar { get; set; }
        public string? avatar_decoration { get; set; }
        public string? discriminator { get; set; }
        public int public_flags { get; set; }
        public string? banner { get; set; }
        public string? banner_color { get; set; }
        public int? accent_color { get; set; }
    }
    public class Message
    {
        internal HttpClient client = new();
        public string? id { get; set; }
        public ushort type { get; set; }
        public string? content { get; set; }
        public string? channel_id { get; set; }
        public User? author { get; set; }
        public List<string>? attachments { get; set; }
        public List<string>? embeds { get; set; }
        public List<string>? mentions { get; set; }
        public List<string>? mention_roles { get; set; }
        public bool pinned { get; set; }
        public bool mention_everyone { get; set; }
        public bool tts { get; set; }
        public string? timestamp { get; set; }
        public string? edited_timestamp { get; set; }
        public ushort? flags { get; set; }
        public List<string>? components { get; set; }
        public string? nonce { get; set; }
        public string? referenced_message { get; set; }
        public string Delete()
        {
			HttpResponseMessage resp = client.DeleteAsync("https://canary.discord.com/api/v9/channels/" + channel_id + "/messages/" + id).Result;
			return resp.Content.ReadAsStringAsync().Result;
		}
    }
	public class GatewayBot
	{
		public string? url { get; set; }
		public int shards { get; set; }
		public SessionStartLimit? session_start_limit { get; set; }
	}
	public class SessionStartLimit
	{
		public int total { get; set; }
		public int remaining { get; set; }
		public int reset_after { get; set; }
		public int max_concurrency { get; set; }
	}
	public class Identifying
	{
		public int op { get; set; }
		public IdentifyingX? d { get; set; }
	}
	public class IdentifyingX
	{
		public string? token { get; set; }
		public int intents { get; set; }
		public IdentifyingProperties? properties { get; set; }
	}
	public class IdentifyingProperties
	{
		public string? os { get; set; }
		public string? browser { get; set; }
		public string? device { get; set; }
	}
	public class opCheck
	{
		public int op { get; set; }
		public int? s { get; set; }
	}
	public class DiscordHello
	{
		public int op { get; set; }
		public DiscordHelloX? d { get; set; }
	}
	public class OpZero
	{
		public string? t { get; set; }
		public int? s { get; set; }
		public int op { get; set; }
		public object? d { get; set; }
	}
	public class DiscordHelloX
	{
		public int heartbeat_interval { get; set; }
	}
	public class Heartbeat
	{
		public int op { get; set; }
		public int? d { get; set; }
	}
}