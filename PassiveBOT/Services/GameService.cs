using System;
using System.Collections.Generic;

namespace PassiveBOT.Services
{
    using System.Collections.Concurrent;
    using System.Threading;

    using Discord;

    using PassiveBOT.Handlers;
    using PassiveBOT.Models;

    using Raven.Client.Documents;

    public class GameService
    {
        private IDocumentStore Store { get; }

        private string documentName = "GameModel";

        private bool initialized = false;

        private Timer timer;

        public GameService(IDocumentStore store, DatabaseObject config)
        {
            Config = config;
            this.Store = store;
            
            timer = new Timer(
                _ =>
                    {
                        try
                        {
                            Save();
                        }
                        catch (Exception e)
                        {
                            LogHandler.LogMessage("Game Error:\n" + $"{e}", LogSeverity.Error);
                        }

                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                        GC.WaitForPendingFinalizers();

                        LastFireTime = DateTime.UtcNow;
                    },
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(FirePeriod));
        }

        public DatabaseObject Config { get; set; }

        public int FirePeriod { get; set; } = 10;

        public DateTime LastFireTime { get; set; } = DateTime.MinValue;

        public void Initialize()
        {
            if (!Config.RunGame)
            {
                return;
            }

            if (!initialized)
            {
                using (var session = Store.OpenSession())
                {
                    var model = session.Load<GameModel>(documentName);
                    if (model == null)
                    {
                        model = new GameModel();
                        session.Store(model, documentName);
                        session.SaveChanges();
                    }

                    Model = model;
                    initialized = true;
                }
            }
        }

        private void Save()
        {
            if (!initialized || Model == null)
            {
                Initialize();
            }

            if (initialized && Model != null)
            {
                using (var session = Store.OpenSession())
                {
                    session.Store(Model, documentName);
                    session.SaveChanges();
                }
            }
        }

        public GameModel Model { get; set; }

        public GameModel.GameServer GetServer(IGuild guild)
        {
            if (Model == null)
            {
                return new GameModel.GameServer(guild.Id);
            }

            if (Model.Guilds.TryGetValue(guild.Id, out var gameServer))
            {
                return gameServer;
            }

            if (Model.Guilds.TryAdd(guild.Id, new GameModel.GameServer(guild.Id)))
            {
                return GetServer(guild);
            }

            return null;
        }

        public class GameModel
        {
            public List<Connect4Game> Connect4List { get; set; } = new List<Connect4Game>();

            public class Connect4Game
            {
                public Connect4Game(ulong channelId)
                {
                    ChannelId = channelId;
                }

                public ulong ChannelId { get; set; }
                public bool GameRunning { get; set; }
            }

            public ConcurrentDictionary<ulong, GameServer> Guilds { get; set; } = new ConcurrentDictionary<ulong, GameServer>();

            public class GameServer
            {
                public GameServer(ulong guildId)
                {
                    GuildId = guildId;
                    Users = new ConcurrentDictionary<ulong, GameUser>();
                    Settings = new GameSettings("Coins");
                }

                public ulong GuildId { get; set; }

                public ConcurrentDictionary<ulong, GameUser> Users { get; set; }

                public GameSettings Settings { get; set; }

                public GameUser GetUser(ulong userId)
                {
                    if (Users.TryGetValue(userId, out var userModel))
                    {
                        return userModel;
                    }

                    if (Users.TryAdd(userId, new GameUser(userId)))
                    {
                        return GetUser(userId);
                    }

                    return null;
                }

                public GameUser GetUser(IUser user)
                {
                    return GetUser(user.Id);
                }

                public class GameSettings
                {
                    public GameSettings(string currencyName)
                    {
                        CurrencyName = currencyName;
                    }
                    public string CurrencyName { get; set; }
                }

                public class GameUser
                {
                    public GameUser(ulong userId)
                    {
                        UserId = userId;
                        Coins = 200;
                        Banned = false;
                        TotalBet = 0;
                        TotalPaidOut = 0;
                    }

                    public ulong UserId { get; set; }
                    public int Coins { get; set; }
                    public bool Banned { get; set; }
                    public int TotalBet { get; set; }
                    public int TotalPaidOut { get; set; }
                }
            }
        }


    }
}
