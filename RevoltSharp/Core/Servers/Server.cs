using Optional.Unsafe;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace RevoltSharp
{
    public class Server : Entity
    {
        public string Id { get; internal set; }

        public string OwnerId { get; internal set; }

        public string Name { get; internal set; }

        public string Description { get; internal set; }

        public HashSet<string> ChannelIds { get; internal set; }

        //public ServerCategory[] Categories;
        //public ServerSystemMessages SystemMessages;
        public ConcurrentDictionary<string, Role> Roles { get; internal set; }

        public ConcurrentDictionary<string, ServerMember> Members { get; internal set; } = new ConcurrentDictionary<string, ServerMember>();

        public ServerPermissions DefaultPermissions { get; internal set; }

        public Attachment Icon { get; internal set; }

        public Attachment Banner { get; internal set; }

        public Role GetRole(string roleId)
        {
            if (Roles.TryGetValue(roleId, out Role role))
                return role;
            return null;
        }

        public TextChannel GetTextChannel(string channelId)
        {
            if (!Client.WebSocket.ChannelCache.TryGetValue(channelId, out Channel chan))
                return null;

            if (chan is not TextChannel textChannel)
                return null;

            if (textChannel.ServerId != Id)
                return null;

            return textChannel;
        }

        public VoiceChannel GetVoiceChannel(string channelId)
        {
            if (!Client.WebSocket.ChannelCache.TryGetValue(channelId, out Channel chan))
                return null;

            if (chan is not VoiceChannel voiceChannel)
                return null;

            if (voiceChannel.ServerId != Id)
                return null;

            return voiceChannel;
        }

        internal void Update(PartialServerJson json)
        {
            if (json.Name.HasValue)
                Name = json.Name.ValueOrDefault();

            if (json.Icon.HasValue)
                Icon = new Attachment(Client, json.Icon.ValueOrDefault());

            if (json.Banner.HasValue)
                Banner = new Attachment(Client, json.Icon.ValueOrDefault());

            if (json.DefaultPermissions.HasValue)
                DefaultPermissions = new ServerPermissions(json.DefaultPermissions.ValueOrDefault());

            if (json.Description.HasValue)
                Description = json.Description.ValueOrDefault();
        }

        internal Server Clone()
        {
            return (Server) this.MemberwiseClone();
        }

        internal void AddMember(ServerMember member)
        {
            Members.TryAdd(member.Id, member);
            member.User.MutualServers.TryAdd(Id, this);
        }

        internal void RemoveMember(User user, bool delete)
        {
            if (!delete)
                Members.TryRemove(user.Id, out _);

            user.MutualServers.TryRemove(Id, out _);
            if (user.Id != user.Client.CurrentUser.Id && !user.HasMutuals())
            {
                user.Client.WebSocket.UserCache.TryRemove(user.Id, out _);
            }
        }

        public Server(RevoltClient client, ServerJson model)
            : base(client)
        {
            if (model == null)
                return;
            Id = model.Id;
            Name = model.Name;
            DefaultPermissions = new ServerPermissions(model.DefaultPermissions);
            Description = model.Description;
            Banner = model.Banner != null ? new Attachment(client, model.Banner) : null;
            Icon = model.Icon != null ? new Attachment(client, model.Icon) : null;
            ChannelIds = model.Channels != null ? model.Channels.ToHashSet() : new HashSet<string>();
            OwnerId = model.Owner;
            Roles = model.Roles != null
                ? new ConcurrentDictionary<string, Role>(model.Roles.ToDictionary(x => x.Key, x => new Role(client, x.Value, Id, x.Key)))
                : new ConcurrentDictionary<string, Role>();
        }
    }
}
