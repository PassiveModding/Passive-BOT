using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using PassiveBOT.Models;
using Raven.Client.Documents;
using Raven.Client.Documents.Operations.Backups;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

namespace PassiveBOT.Handlers
{
    public class DatabaseHandler
    {
        public DatabaseHandler(IDocumentStore store)
        {
            Store = store;
        }

        ///This is our configuration for the database handler, DBName is the database you created in RavenDB when setting it up
        ///ServerURL is the URL to the local server. NOTE: This bot has not been configured to use public addresses
        public static string DBName { get; set; } = ConfigModel.Load().DBName;

        public static string ServerURL { get; set; } = ConfigModel.Load().DBUrl;

        /// <summary>
        ///     This is the document store, an interface that represents our database
        /// </summary>
        public static IDocumentStore Store { get; set; }

        /// <summary>
        ///     Check whether RavenDB is running
        ///     Check whether or not a database already exists with the DBName
        ///     Set up auto-backup of the database
        ///     Ensure that all guilds shared with the bot have been added to the database
        /// </summary>
        /// <param name="client"></param>
        public static async void DatabaseInitialise(DiscordSocketClient client)
        {
            if (Process.GetProcesses().FirstOrDefault(x => x.ProcessName == "Raven.Server") == null)
            {
                LogHandler.LogMessage("RavenDB: Server isn't running. Please make sure RavenDB is running.\nExiting ...", LogSeverity.Critical);
                await Task.Delay(5000);
                Environment.Exit(Environment.ExitCode);
            }

            var dbcreated = false;
            if (Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != DBName))
            {
                await Store.Maintenance.Server.SendAsync(new CreateDatabaseOperation(new DatabaseRecord(DBName)));
                LogHandler.LogMessage($"Created Database {DBName}.");
                dbcreated = true;
            }


            LogHandler.LogMessage("Setting up backup operation...");
            var newbackup = new PeriodicBackupConfiguration
            {
                Name = "Backup",
                BackupType = BackupType.Backup,
                FullBackupFrequency = "*/10 * * * *",
                IncrementalBackupFrequency = "0 2 * * *",
                LocalSettings = new LocalSettings {FolderPath = Path.Combine(AppContext.BaseDirectory, "setup/backups/")}
            };
            var Record = Store.Maintenance.ForDatabase(DBName).Server.Send(new GetDatabaseRecordOperation(DBName));
            var backupop = Record.PeriodicBackups.FirstOrDefault(x => x.Name == "Backup");
            if (backupop == null)
            {
                await Store.Maintenance.ForDatabase(DBName).SendAsync(new UpdatePeriodicBackupOperation(newbackup)).ConfigureAwait(false);
            }
            else
            {
                //In the case that we already have a backup operation setup, ensure that we update the backup location accordingly
                backupop.LocalSettings = new LocalSettings {FolderPath = Path.Combine(AppContext.BaseDirectory, "setup/backups/")};
                await Store.Maintenance.ForDatabase(DBName).SendAsync(new UpdatePeriodicBackupOperation(backupop));
            }

            if (!dbcreated) return;

            using (var session = Store.OpenSession(DBName))
            {
                try
                {
                    //Check to see wether or not we can actually load the Guilds List saved in our RavenDB
                    var _ = session.Query<GuildModel>().ToList();
                }
                catch
                {
                    //In the case that the check fails, ensure we initalise all servers that contain the bot.
                    var glist = client.Guilds.Select(x => new GuildModel
                    {
                        ID = x.Id
                    }).ToList();
                    foreach (var gobj in glist)
                    {
                        session.Store(gobj, gobj.ID.ToString());
                    }

                    session.SaveChanges();
                }
            }
        }


        /// <summary>
        ///     This adds a new guild to the RavenDB
        /// </summary>
        /// <param name="Id">The Server's ID</param>
        /// <param name="Name">Optionally say the server name was added to the DB</param>
        public static void AddGuild(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession(DBName))
            {
                if (Session.Advanced.Exists($"{Id}")) return;
                Session.Store(new GuildModel
                {
                    ID = Id
                }, Id.ToString());
                Session.SaveChanges();
            }

            LogHandler.LogMessage(string.IsNullOrWhiteSpace(Name) ? $"Added Server With Id: {Id}" : $"Created Config For {Name}", LogSeverity.Debug);
        }

        /// <summary>
        ///     Remove a guild's config completely from the database
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        public static void RemoveGuild(ulong Id, string Name = null)
        {
            using (var Session = Store.OpenSession(DBName))
            {
                Session.Delete($"{Id}");
            }

            LogHandler.LogMessage(string.IsNullOrWhiteSpace(Name) ? $"Removed Server With Id: {Id}" : $"Deleted Config For {Name}", LogSeverity.Debug);
        }

        /// <summary>
        ///     Load a Guild Object from the database
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static GuildModel GetGuild(ulong Id)
        {
            using (var Session = Store.OpenSession(DBName))
            {
                return Session.Load<GuildModel>(Id.ToString());
            }
        }

        /// <summary>
        ///     Load all documents matching GuildModel from the database
        /// </summary>
        /// <returns></returns>
        public static List<GuildModel> GetFullConfig()
        {
            using (var session = Store.OpenSession(DBName))
            {
                List<GuildModel> dbGuilds;
                try
                {
                    dbGuilds = session.Query<GuildModel>().ToList();
                }
                catch
                {
                    dbGuilds = new List<GuildModel>();
                }

                return dbGuilds;
            }
        }
    }
}