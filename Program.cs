using System;
using ValAPINet;
using System.Diagnostics;
using DiscordRPC;
using DiscordRPC.Logging;
using System.Globalization;
using System.Drawing;
using Console = Colorful.Console;

namespace ValorantDRPC
{
    internal class Program
    {
        public static Task Main(string[] args) => new Program().MainAsync();

        public Auth auth;
        public DiscordRpcClient client;
        public UserPresence.Presence presence;

        // Vars
        public string mapName;
        public string gameMode;

        public async Task MainAsync()
        {
            Console.Title = "VALORANT Discord RPC - by CruSheR#2015";
            PrintLogo();
            Console.WriteLine("Checking If Valorant is Running...", Color.DarkRed);
            await Task.Delay(1500);
            if(isValorantRunning())
            {
                Console.WriteLine("VALORANT is Running!", Color.LimeGreen);
                PrepareAuthInst();
                Username usr = Username.GetUsername(auth);
                Console.WriteLine("Welcome: " + usr.GameName + "#" + usr.TagLine);
                await Task.Delay(1000);
            } else
            {
                Console.WriteLine("VALORANT is not running, please run it before launching the application!", Color.DarkRed);
                Console.WriteLine("Press any key to exit!", Color.DarkRed);
                Console.ReadKey();
                return;
            }
            Console.Clear();
            PrintLogo();
            PrepareDiscordRPC();
            await PreparePresence();
            Console.ReadKey();
        }

        private void PrepareAuthInst()
        {
            auth = Websocket.GetAuthLocal(false);
        }

        private void PrepareDiscordRPC()
        {
            client = new DiscordRpcClient("961899112440168458", autoEvents:false);
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.OnReady += (sender, e) =>
            {
                Console.WriteLine("Discord is Ready! user {0}", e.User.Username);
            };
            client.OnPresenceUpdate += (sender, msg) =>
            {
                Console.Clear();
                PrintLogo();
                Console.WriteLine("Discord RPC has been updated!", Color.Green);
            };
            client.Initialize();
        }

        private void PrintLogo()
        {
            Console.WriteLine($@" __      __     _        _____  _____   _____ 
 \ \    / /\   | |      |  __ \|  __ \ / ____|
  \ \  / /  \  | |      | |__) | |__) | |     
   \ \/ / /\ \ | |      |  _  /|  ___/| |     
    \  / ____ \| |____  | | \ \| |    | |____ 
     \/_/    \_\______| |_|  \_\_|     \_____|
       
  by CruSheR#2015 - https://github.com/Crv5heR
  {GetNameDyn()}
                                              ", Color.OrangeRed);
        }

        private string GetNameDyn()
        {
            if(auth != null)
            {
                Username usr = Username.GetUsername(auth);
                return $"Welcome {usr.GameName}#{usr.TagLine}, Have a great day!";
            }
            return "Loading...";
        }

        private async Task PreparePresence()
        {
            TextInfo myTI = new CultureInfo("en-US", false).TextInfo;
            await Task.Run(async () =>
            {
                while (true)
                {
                    client.Invoke();
                    presence = UserPresence.GetPresence().presences[0];
                    if (presence != null)
                    {
                        if (presence.privinfo.matchMap != "")
                        {
                            mapName = presence.privinfo.matchMap;
                            gameMode = presence.privinfo.queueId;
                        }
                    }

                    if (presence == null || presence.privinfo.sessionLoopState == "MENUS")
                    {
                        if (!isValorantRunning()) break;
                        if (presence != null && presence.privinfo.partyAccessibility == "OPEN")
                        {
                            if (presence.privinfo.partyState == "MATCHMAKING")
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Menus",
                                    State = $"Searching ({myTI.ToTitleCase(presence.privinfo.queueId)})",
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
                                    }
                                });
                                SetPresence(pre);
                            }
                            else
                            {
                                //If not waiting in match
                                var pre = (new RichPresence()
                                {
                                    Details = "Menus",
                                    State = $"Waiting ({myTI.ToTitleCase(presence.privinfo.queueId)})",
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

                        else if (presence != null)
                        {
                            //If matchmaking and party is closed
                            if (presence.privinfo.partyState == "MATCHMAKING")
                            {
                                var pre = (new RichPresence()
                                {
                                    Details = "Menus",
                                    State = $"Searching ({myTI.ToTitleCase(presence.privinfo.queueId)})",
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
                                //If waiting and party is private
                                var pre = (new RichPresence()
                                {
                                    Details = "Menus",
                                    State = $"Waiting ({myTI.ToTitleCase(presence.privinfo.queueId)})",
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

                        await Task.Delay(7500);

                    }
                    else
                    {
                        if (presence.privinfo.provisioningFlow == "ShootingRange")
                        {
                            gameMode = "Shooting Range";
                            mapName = "/Game/Maps/Poveglia/Range";
                        }
                        //One size fits all in game presence
                        var pre = (new RichPresence()
                        {
                            Details = "Playing " + myTI.ToTitleCase(gameMode) + " on " + GetMapName(mapName),
                            State = presence.privinfo.partyOwnerMatchScoreAllyTeam + "-" + presence.privinfo.partyOwnerMatchScoreEnemyTeam,
                            Assets = new Assets()
                            {
                                LargeImageKey = GetMapName(mapName).ToLower().Replace(" ", "_"),
                                LargeImageText = GetMapName(mapName)
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
                        await Task.Delay(10000);
                    }
                }
            });
        }

        private RichPresence currentPre;
        private void SetPresence(RichPresence pre) 
        {
            currentPre = pre;
            client.SetPresence(pre);
            client.Invoke();

            if (presence.privinfo.queueId != null)
            {
                Console.WriteLine($"Current Queue: {presence.privinfo.queueId}", Color.LightGoldenrodYellow);
            }
            Console.WriteLine($"Current Party Size: {presence.privinfo.partySize}", Color.LightGoldenrodYellow);
            if(presence.privinfo.matchMap != String.Empty)
            {
                Console.WriteLine($"Current Map: {GetMapName(presence.privinfo.matchMap)}", Color.LightGoldenrodYellow);
            }

        }

        public string GetMapName(string mapid)
        {
            string displayName;
            switch (mapid)
            {
                case "/Game/Maps/Ascent/Ascent":
                    displayName = "Ascent";
                    break;
                case "/Game/Maps/Bonsai/Bonsai":
                    displayName = "Split";
                    break;
                case "/Game/Maps/Duality/Duality":
                    displayName = "Bind";
                    break;
                case "/Game/Maps/Port/Port":
                    displayName = "Icebox";
                    break;
                case "/Game/Maps/Triad/Triad":
                    displayName = "Haven";
                    break;
                case "/Game/Maps/Foxtrot/Foxtrot":
                    displayName = "Breeze";
                    break;
                case "/Game/Maps/Canyon/Canyon":
                    displayName = "Fracture";
                    break;
                case "/Game/Maps/Poveglia/Range":
                    displayName = "The Range";
                    break;
                default:
                    displayName = mapid;
                    break;
            }
            return displayName;
        }
        public static string GetModeName(string mode)
        {
            string displayName;
            switch (mode)
            {
                case "/Game/GameModes/Bomb/BombGameMode.BombGameMode_C":
                    displayName = "Standard";
                    break;
                case "/Game/GameModes/Deathmatch/DeathmatchGameMode.DeathmatchGameMode_C":
                    displayName = "Deathmatch";
                    break;
                case "/Game/GameModes/GunGame/GunGameTeamsGameMode.GunGameTeamsGameMode_C":
                    displayName = "Escalation";
                    break;
                case "/Game/GameModes/OneForAll/OneForAll_GameMode.OneForAll_GameMode_C":
                    displayName = "Replication";
                    break;
                case "/Game/GameModes/QuickBomb/QuickBombGameMode.QuickBombGameMode_C":
                    displayName = "Spike Rush";
                    break;
                case "/Game/GameModes/ShootingRange/ShootingRangeGameMode.ShootingRangeGameMode_C":
                    displayName = "Shooting Range";
                    break;
                default:
                    displayName = "Unknown Mode";
                    break;
            }
            return displayName;

        }

        private bool isValorantRunning()
        {
            Process[] processes = Process.GetProcessesByName("VALORANT-Win64-Shipping");
            if (processes.Length == 0)
                return false;
            return true;
        }
    }
}