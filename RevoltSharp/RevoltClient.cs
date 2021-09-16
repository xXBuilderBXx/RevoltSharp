﻿using Newtonsoft.Json;
using RevoltSharp.Rest;
using RevoltSharp.WebSocket;
using System.Threading.Tasks;

namespace RevoltSharp
{
    /// <summary>
    /// Revolt bot client used to connect to the Revolt chat API and WebSocket with a bot.
    /// </summary>
    public class RevoltClient : ClientEvents
    {
        /// <summary>
        /// Create a Revolt bot client.
        /// </summary>
        /// <param name="token">Bot token to connect with.</param>
        /// <param name="mode">Use http for http requests only with no websocket.</param>
        /// <param name="config">Optional config stuff for the bot and lib.</param>
        public RevoltClient(string token, ClientMode mode, ClientConfig config = null)
        {
            try
            {
                DisableConsoleQuickEdit.Go();
            }
            catch { }
            Token = token;
            Config = config ?? new ClientConfig();
            if (Config.Debug == null)
                Config.Debug = new ClientDebugConfig();
            Serializer = new JsonSerializer();
            Serializer.Converters.Add(new OptionConverter());
            Rest = new RevoltRestClient(this);
            if (mode == ClientMode.WebSocket)
                WebSocket = new RevoltSocketclient(this);

        }

        /// <summary>
        /// Revolt bot token used for http requests and websocket.
        /// </summary>
        public string Token { get; internal set; }

        internal JsonSerializer Serializer { get; set; }

        /// <summary>
        /// Client config options for user-agent and debug options including self-host support.
        /// </summary>
        public ClientConfig Config { get; internal set; }

        /// <summary>
        /// Internal rest/http client used to connect to the Revolt API.
        /// </summary>
        /// <remarks>
        /// You can also make custom requests with <see cref="RevoltRestClient.SendRequestAsync(RequestType, string, RevoltRequest)"/> and json class based on <see cref="RevoltRequest"/>
        /// </remarks>
        public RevoltRestClient Rest { get; internal set; }

        internal RevoltSocketclient WebSocket;

        /// <summary>
        /// Start the WebSocket connection to Revolt.
        /// </summary>
        /// <remarks>
        /// Will throw a <see cref="RevoltException"/> if <see cref="ClientMode.Http"/>
        /// </remarks>
        /// <exception cref="RevoltException"></exception>
        public async Task StartAsync()
        {
            if (WebSocket == null)
                throw new RevoltException("Client is in http-only mode.");

            if (WebSocket.WebSocket != null)
                return;
            WebSocket.SetupWebsocket();
            while (WebSocket.WebSocket == null || WebSocket.WebSocket.State != System.Net.WebSockets.WebSocketState.Open) { }
        }

        /// <summary>
        /// Stop the WebSocket connection to Revolt.
        /// </summary>
        /// <remarks>
        /// Will throw a <see cref="RevoltException"/> if <see cref="ClientMode.Http"/>
        /// </remarks>
        /// <exception cref="RevoltException"></exception>
        public async Task StopAsync()
        {
            if (WebSocket == null)
                throw new RevoltException("Client is in http-only mode.");

            if (WebSocket.WebSocket != null)
            {
                await WebSocket.WebSocket.CloseAsync(System.Net.WebSockets.WebSocketCloseStatus.NormalClosure, "", WebSocket.CancellationToken);
            }
        }

        /// <summary>
        /// Get the current bot <see cref="User"/> after ready event.
        /// </summary>
        /// <remarks>
        /// Using <see cref="ClientMode.Http"/> means this is always <see langword="null"/> or use <see cref="Rest"/> to get user.
        /// </remarks>
        public SelfUser CurrentUser
            => WebSocket != null ? WebSocket.CurrentUser : null;
    }

    /// <summary>
    /// Run the client with Http requests only or use websocket to get cached data such as servers, channels and users instead of just ids.
    /// </summary>
    /// <remarks>
    /// Using <see cref="ClientMode.Http"/> means that some data will be <see langword="null"/> such as <see cref="Message.Author"/> and will only contain ids <see cref="Message.AuthorId"/>
    /// </remarks>
    public enum ClientMode
    {
        /// <summary>
        /// Client will only use the http/rest client of Revolt and removes any usage/memory of websocket stuff. 
        /// </summary>
        Http, 
        /// <summary>
        /// Will use both WebSocket and http/rest client so you can get cached data for <see cref="User"/>, <see cref="Server"/> and <see cref="Channel"/>
        /// </summary>
        WebSocket
    }
}
