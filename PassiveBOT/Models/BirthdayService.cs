using System;
using System.Collections.Generic;
using System.Text;

namespace PassiveBOT.Models
{
    using System.Linq;
    using System.Threading.Tasks;

    using Discord.WebSocket;

    using Raven.Client.Documents;

    public class BirthdayService
    {
        public BirthdayService(DiscordShardedClient client, IDocumentStore store)
        {
            Client = client;
            Store = store;
        }

        private bool Initialized = false;

        private string BirthdayDocumentName = "BirthdayModel";

        public void Initialize()
        {
            if (!Initialized)
            {
                using (var session = Store.OpenSession())
                {
                    var model = session.Load<BirthdayModel>(BirthdayDocumentName);
                    if (model == null)
                    {
                        model = new BirthdayModel();
                        session.Store(model, BirthdayDocumentName);
                        session.SaveChanges();
                    }

                    Model = model;
                    Initialized = true;
                }
            }
        }

        public void Save()
        {
            if (!Initialized || Model == null)
            {
                Initialize();
            }

            if (Initialized && Model != null)
            {
                using (var session = Store.OpenSession())
                {
                    session.Store(Model, BirthdayDocumentName);
                    session.SaveChanges();
                }
            }
        }

        public IDocumentStore Store { get; set; }

        public DiscordShardedClient Client { get; set; }

        public BirthdayModel Model { get; set; } = null;

        public List<BirthdayModel.Person> CurrentBirthdays()
        {
            return Model?.Persons?.Where(x => x.IsToday()).ToList() ?? new List<BirthdayModel.Person>();
        }

        public async Task<List<(BirthdayModel.EnabledGuild, List<BirthdayModel.Person>)>> GetNotifiableList()
        {
            if (!Initialized || !Client.Shards.All(x => x.Guilds.Any()))
            {
                return new List<(BirthdayModel.EnabledGuild, List<BirthdayModel.Person>)>();
            }

            if (Model?.EnabledGuilds == null || Model.Persons == null)
            {
                return new List<(BirthdayModel.EnabledGuild, List<BirthdayModel.Person>)>();
            }

            var guilds = Client.Guilds.Where(
                x =>
                    {
                        var enabledGuild = Model.EnabledGuilds.FirstOrDefault(e => e.GuildId == x.Id);
                        if (enabledGuild != null)
                        {
                            if (x.GetTextChannel(enabledGuild.BirthdayChannelId) != null)
                            {
                                return true;
                            }
                        }

                        return false;
                    }).ToList();

            var currentBirthdays = CurrentBirthdays();
            var matchedList = new List<(BirthdayModel.EnabledGuild, List<BirthdayModel.Person>)>();
            foreach (var guild in guilds)
            {
                await guild.DownloadUsersAsync();
                var enabledGuild = Model.EnabledGuilds.FirstOrDefault(x => x.GuildId == guild.Id);
                if (enabledGuild != null)
                {
                    var users = currentBirthdays.Where(x => guild.Users.Any(u => u.Id == x.UserId)).ToList();
                    if (users.Any())
                    {
                        matchedList.Add((enabledGuild, users));
                    }
                }
            }

            return matchedList;
        }

        public class BirthdayModel
        {
            public BirthdayModel()
            {
                Persons = new HashSet<Person>();
                EnabledGuilds = new HashSet<EnabledGuild>();
            }

            public HashSet<Person> Persons { get; set; }
            public HashSet<EnabledGuild> EnabledGuilds { get; set; }

            public class EnabledGuild
            {
                public ulong GuildId { get; set; }
                public ulong BirthdayChannelId { get; set; }

                public ulong BirthdayRole { get; set; } = 0;
            }

            public void AddBirthday(ulong userId, DateTime dateOfBirth, bool yearProvided)
            {
                if (dateOfBirth == null)
                {
                    throw new ArgumentException("Date of birth wasn't entered");
                }

                if (dateOfBirth.Date > DateTime.Today)
                {
                    throw new ArgumentException("Date of birth can't be in the future");
                }

                Person person = new Person(dateOfBirth.Date, userId, yearProvided);
                Persons.Add(person);
            }
            
            public class Person
            {
                public Person(DateTime birthday, ulong userId, bool showYear)
                {
                    Birthday = birthday;
                    UserId = userId;
                    ShowYear = showYear;
                }

                public DateTime Birthday { get; set; }

                public ulong UserId { get; set; }

                public bool ShowYear { get; set; }

                public int RemainingDays()
                {
                    DateTime today = DateTime.Today;
                    DateTime nextBirthday;
                    if (today.DayOfYear > Birthday.DayOfYear)
                    {
                        nextBirthday = new DateTime(today.Year + 1, Birthday.Month, Birthday.Day);
                    }
                    else
                    {
                        nextBirthday = new DateTime(today.Year, Birthday.Month, Birthday.Day);
                    }

                    int remaining_days = nextBirthday.DayOfYear - today.DayOfYear;

                    return remaining_days;
                }

                public bool IsToday()
                {
                    DateTime today = DateTime.Today;
                    if (Birthday.DayOfYear == today.DayOfYear && Birthday.Month == today.Month)
                    {
                        return true;
                    }

                    return false;
                }

                public int Age()
                {
                    if (ShowYear)
                    {
                        DateTime today = DateTime.Today;
                        int age = today.Year - Birthday.Year;
                        if (today < Birthday.AddYears(age))
                        {
                            age--;
                        }

                        if (age <= 0)
                        {
                            return -1;
                        }

                        return age;
                    }

                    return -1;
                }
            }
        }
    }
}
