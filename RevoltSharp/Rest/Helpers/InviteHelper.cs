using RevoltSharp.Rest;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RevoltSharp
{
    public static class InviteHelper
    {
        public static Task<HttpResponseMessage> DeleteInviteAsync(this Server server, string inviteId)
            => DeleteInviteAsync(server.Client.Rest, inviteId);

        public static async Task<HttpResponseMessage> DeleteInviteAsync(this RevoltRestClient rest, string inviteId)
        {
            return await rest.SendRequestAsync(RequestType.Delete, $"/invites/{inviteId}");
        }

        public static Task<Invite[]> GetInvitesAsync(this Server server)
            => GetInvitesAsync(server.Client.Rest, server.Id);

        public static async Task<Invite[]> GetInvitesAsync(this RevoltRestClient rest, string serverId)
        {
            InviteJson[] Json = await rest.SendRequestAsync<InviteJson[]>(RequestType.Get, $"/servers/{serverId}/invites");
            return Json.Select(x => new Invite { Code = x.Code }).ToArray();
        }

        public static Task<Invite> CreateInviteAsync(this TextChannel channel)
            => CreateInviteAsync(channel.Client.Rest, channel.Id);

        public static async Task<Invite> CreateInviteAsync(this RevoltRestClient rest, string channelId)
        {
            InviteJson Json = await rest.SendRequestAsync<InviteJson>(RequestType.Post, $"/channels/{channelId}/invites");
            return new Invite { Code = Json.Code };
        }
    }
}
