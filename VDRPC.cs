using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValAPINet;
using DiscordRPC;
using System.Diagnostics;
using System.Windows;
using DiscordRPC.Logging;
using Serilog;
using static ValAPINet.UserPresence;
using static ValAPINet.Content;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using RestSharp;
using System.Net.Sockets;

namespace ValorantDRPC
{
    public class VDRPC
    {
        public static Auth? authentication;
        public static DiscordRpcClient? Client;
        public static Presence? presence;
        public static Username? username;
        public static string? MapName { get; set; }
        public static string? GameMode { get; set; }
        public static string? DUsername { get; set; }
        public static bool IsValorantNotRunning => Process.GetProcessesByName("VALORANT-Win64-Shipping").Length is 0;
        public static bool ranFromStartup = false;
        public static void LogFile(string text)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            Log.Information(text);
        }

        public static async Task InitApp()
        {
            if (!IsValorantNotRunning) {
                authentication = Websocket.GetAuthLocal(false);
                username = Username.GetUsername(authentication);
                await PreparePresence();
            }
        }

        public static void PrepareDiscordRPC()
        {
            Client = new("ADD DISCORD CLIENTID HERE", pipe: 0);
            //Client.RegisterUriScheme();
            Client.OnReady += (sender, e) => { LogFile($"Discord is Ready! user {e.User.Username}"); DUsername = e.User.Username; };
            Client.OnPresenceUpdate += (sender, msg) =>
            {
                LogFile("Discord Presence Updated!");
            };
            Client.OnJoin += Client_OnJoin;
            Client.OnJoinRequested += Client_OnJoinRequested;
            Client.Initialize();
        }

        private static void Client_OnJoinRequested(object sender, DiscordRPC.Message.JoinRequestMessage args)
        {
            DiscordRpcClient client = (DiscordRpcClient)sender;
            MessageBoxResult result = MessageBox.Show($"{args.User.Username} has requested to join your party.", "Valorant DRPC", MessageBoxButton.YesNo);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    client.Respond(args, true);
                    break;
                case MessageBoxResult.No:
                    client.Respond(args, false);
                    break;
            }
        }

        private static void Client_OnJoin(object sender, DiscordRPC.Message.JoinMessage args)
        {
        }

        private async static Task PreparePresence()
        {
            TextInfo ti = new CultureInfo("en-US", false).TextInfo;
            await Task.Run(async () =>
            {
                while (true)
                {
                    Client?.Invoke();

                    Presence presence = GetPresence(authentication.subject);

                    if (presence != null)
                    {
                        if (presence.privinfo.matchMap != "")
                        {
                            MapName = presence.privinfo?.matchMap;
                            GameMode = presence.privinfo?.queueId;
                        }
                    }

                    if (presence == null || presence.privinfo?.sessionLoopState == "MENUS")
                    {
                        if (IsValorantNotRunning) break;
                        if (presence != null && presence.privinfo?.partyAccessibility == "OPEN")
                        {
                            if (presence.privinfo.partyState == "MATCHMAKING")
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Searching",
                                    State = $"{ti.ToTitleCase(presence.privinfo.queueId)}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = "logo"
                                    },
                                    Party = new Party
                                    {
                                        ID = presence.privinfo.partyId,
                                        Max = presence.privinfo.maxPartySize,
                                        Privacy = Party.PrivacySetting.Public,
                                        Size = presence.privinfo.partySize
                                    }/*,
                                    Secrets = new Secrets()
                                    {
                                        JoinSecret = Secure.encrypt(presence.privinfo.partyId),
                                        SpectateSecret = "123123"
                                    }*/
                                });
                                SetPresence(pre);
                            }
                            else
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Lobby",
                                    State = $"{ti.ToTitleCase(presence.privinfo.queueId)}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = "logo"
                                    },
                                    Party = new Party
                                    {
                                        ID = presence.privinfo.partyId,
                                        Max = presence.privinfo.maxPartySize,
                                        Privacy = Party.PrivacySetting.Private,
                                        Size = presence.privinfo.partySize
                                    }/*,
                                    Secrets = new Secrets()
                                    {
                                        JoinSecret = presence.privinfo.partyId,
                                        SpectateSecret = "123123"
                                    }*/
                                });
                                SetPresence(pre);
                            }
                        }

                        else if (presence != null)
                        {
                            if (presence.privinfo.partyState == "MATCHMAKING")
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Searching",
                                    State = $"{ti.ToTitleCase(presence.privinfo.queueId)}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = "logo"
                                    },
                                    Party = new Party
                                    {
                                        ID = presence.privinfo.partyId,
                                        Max = presence.privinfo.maxPartySize,
                                        Privacy = Party.PrivacySetting.Private,
                                        Size = presence.privinfo.partySize
                                    }
                                });
                                SetPresence(pre);
                            }
                            else
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Lobby",
                                    State = $"{ti.ToTitleCase(presence.privinfo.queueId)}",
                                    Assets = new Assets()
                                    {
                                        LargeImageKey = "logo"
                                    },
                                    Party = new Party
                                    {
                                        ID = presence.privinfo.partyId,
                                        Max = presence.privinfo.maxPartySize,
                                        Privacy = Party.PrivacySetting.Private,
                                        Size = presence.privinfo.partySize
                                    }
                                });
                                SetPresence(pre);
                            }
                        }

                        await Task.Delay(3000);

                    }
                    else
                    {
                        if (presence.privinfo.provisioningFlow == "ShootingRange")
                        {
                            GameMode = "Shooting Range";
                            MapName = "/Game/Maps/Poveglia/Range";
                            var pre = (new RichPresence()
                            {
                                Details = "Training on " + ti.ToTitleCase(GameMode),
                                Assets = new Assets()
                                {
                                    LargeImageKey = GetMapName(MapName).ToLower().Replace(" ", "_"),
                                    LargeImageText = GetMapName(MapName)
                                },
                                Timestamps = new Timestamps()
                                {
                                    Start = null
                                }
                            });
                            SetPresence(pre);
                            await Task.Delay(3000);
                        }
                        else
                        {
                            var pre = (new RichPresence()
                            {
                                Details = "Playing " + ti.ToTitleCase(GameMode) + " on " + GetMapName(MapName),
                                State = presence.privinfo.partyOwnerMatchScoreAllyTeam + "-" + presence.privinfo.partyOwnerMatchScoreEnemyTeam,
                                Assets = new Assets()
                                {
                                    LargeImageKey = GetMapName(MapName).ToLower().Replace(" ", "_"),
                                    LargeImageText = GetMapName(MapName)
                                },
                                Party = new Party
                                {
                                    ID = presence.privinfo.partyId,
                                    Max = presence.privinfo.maxPartySize,
                                    Privacy = Party.PrivacySetting.Private,
                                    Size = presence.privinfo.partySize
                                },
                                Timestamps = new Timestamps()
                                {
                                    Start = null
                                }
                            });
                            SetPresence(pre);
                            await Task.Delay(3000);
                        }
                    }
                }
            });
        }

        private static void SetPresence(RichPresence pre)
        {
            Client?.SetPresence(pre);
            Client?.Invoke();
        }
        
        public static string GetMapName(string mapid) => mapid switch
        {
            "/Game/Maps/Ascent/Ascent" => "Ascent",
            "/Game/Maps/Bonsai/Bonsai" => "Split",
            "/Game/Maps/Duality/Duality" => "Bind",
            "/Game/Maps/Port/Port" => "Icebox",
            "/Game/Maps/Triad/Triad" => "Haven",
            "/Game/Maps/Foxtrot/Foxtrot" => "Breeze",
            "/Game/Maps/Canyon/Canyon" => "Fracture",
            "/Game/Maps/Poveglia/Range" => "The Range",
            "/Game/Maps/Pitt/Pitt" => "Pearl",
            _ => mapid
        };

        public static string GetModeName(string mode) => mode switch
        {
            "/Game/GameModes/Bomb/BombGameMode.BombGameMode_C" => "Standard",
            "/Game/GameModes/Deathmatch/DeathmatchGameMode.DeathmatchGameMode_C" => "Deathmatch",
            "Ggteam" => "Escalation",
            "Onefa" => "Replication",
            "Spikerush" => "Spike Rush",
            "/Game/GameModes/ShootingRange/ShootingRangeGameMode.ShootingRangeGameMode_C" => "Shooting Range",
            _ => "Unknown Mode"
        };
    }
}

