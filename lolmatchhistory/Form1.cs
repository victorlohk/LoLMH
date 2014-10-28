using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RiotSharp;
using RiotSharp.StaticDataEndpoint;
using RiotSharp.ChampionEndpoint;
using RiotSharp.SummonerEndpoint;
using RiotSharp.GameEndpoint;

namespace lolmatchhistory
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Load += form_load;
        }

    #region Properties and Constant

        private const string MY_API_KEY = "005c3a72-9b7d-4f10-9b3b-9e957c1278ee";

        private static RiotApi _api;
        private static RiotApi api
        {
            get
            {
                if (_api == null)
                    _api = RiotApi.GetInstance(MY_API_KEY);
                return _api;
            }
            set { }
        }

        private static StaticRiotApi _staticApi;
        private static StaticRiotApi staticApi
        {
            get
            {
                if (_staticApi == null)
                    _staticApi = StaticRiotApi.GetInstance(MY_API_KEY);
                return _staticApi;
            }
            set { }
        }

        enum opting {optIn, optOut};

    #endregion

    #region Configurations
        private const Region region = RiotSharp.Region.na;
        private List<String> players = new List<String> {"Erndra", "auribug"};
        private List<String> includedPlayers = new List<String> {"Erndra", "auribug"};
        private List<String> excludedPlayers = new List<String> { };
        private const opting loadPlyaerOption = opting.optOut;
        private const String UserName = "VICTOR";
    #endregion

        private void form_load(Object sender, EventArgs e)
        {
            try
            {
                reloadChampions();
                List<Summoner> summoners = reloadSummoners();
                if (summoners == null)
                {
                    summoners = getSummoners(players);
                }

                Dictionary<String, List<Game>> summonersGames = loadRecentGames(summoners);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("error caught in " + this.GetType().Name + ":"  + Environment.NewLine + ex.ToString(), "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private List<ChampionStatic> reloadChampions()
        {
            ChampionListStatic static_champions;

            try { 
                static_champions = staticApi.GetChampions(region);
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to load latest champions data from server.";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                static_champions = null;
            }

            if (!(static_champions == null || static_champions.Champions == null || static_champions.Champions.Count == 0))
            {
                try
                {
                    using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                    {
                        ctx.Database.ExecuteSqlCommand("TRUNCATE TABLE CHAMPION");

                        foreach (ChampionStatic champion in static_champions.Champions.Values)
                        {
                            //RiotSharp.StaticDataEndpoint.ChampionStatic statChamp = static_champions.Champions.Values.Where(o => (long)o.Id == champion.Id).Single();

                            CHAMPION model = new CHAMPION()
                            {
                                CHAMPION_ID = champion.Id,
                                CHAMPIONNAME = champion.Name,
                                CHAMPIONTITLE = champion.Title
                            };

                            ctx.CHAMPIONs.Add(model);
                        }
                        ctx.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    String custom_msg = "Unable to update champions data to local database";
                    String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                    MessageBox.Show(message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return static_champions.Champions.Values.ToList();
        }

        private List<Summoner> reloadSummoners()
        {
            List<Summoner> summoners;

            try {
                summoners = api.GetSummoners(region, players);
                
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to load latest summoners data from server";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                summoners = null;
            }

            if (summoners != null)
            {
                try
                {
                    using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                    {

                        foreach (Summoner summoner in summoners)
                        {
                            //RiotSharp.StaticDataEndpoint.ChampionStatic statChamp = static_champions.Champions.Values.Where(o => (long)o.Id == champion.Id).Single();

                            SUMMONER model = ctx.SUMMONERs.Where(o => o.SUMMONER_ID == summoner.Id).SingleOrDefault();
                            if (model == null)
                            {
                                model = new SUMMONER()
                                {
                                    SUMMONER_ID = summoner.Id,
                                    LEVEL = (short)summoner.Level,
                                    SUMMONERNAME = summoner.Name,
                                    PROFILEICON_ID = summoner.ProfileIconId,
                                    REVISIONDATE = summoner.RevisionDate
                                };
                                ctx.SUMMONERs.Add(model);
                            }
                            else
                            {
                                model.SUMMONER_ID = summoner.Id;
                                model.LEVEL = (short)summoner.Level;
                                model.SUMMONERNAME = summoner.Name;
                                model.PROFILEICON_ID = summoner.ProfileIconId;
                                model.REVISIONDATE = summoner.RevisionDate;
                            }
                        }
                        ctx.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    String custom_msg = "Unable to update summoners data to local database";
                    String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                    MessageBox.Show(message , "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return summoners;
        }

        private List<Summoner> getSummoners(List<String> summonerNames)
        {
            return null; //todo
        }

        private Dictionary<String, List<Game>> loadRecentGames(List<Summoner> summoners)
        {
            Dictionary<String, List<Game>> result = new Dictionary<string,List<Game>>();
            foreach (Summoner summoner in summoners)
            {
                List<Game> games = loadRecentGames(summoner);
                if (games != null)
                {
                    result.Add(summoner.Name, games);
                }
            }
            return result;
        }

        private List<Game> loadRecentGames(Summoner summoner)
        {
            List<Game> games;

            try
            {
                games = summoner.GetRecentGames();
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to load recent games data for summoner [" + summoner.Name + "]from server.";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                games = null;
            }

            if (games != null && games.Count > 0)
            {
                try
                {
                    using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                    {
                        foreach (Game game in games)
                        {
                            //RiotSharp.StaticDataEndpoint.ChampionStatic statChamp = static_champions.Champions.Values.Where(o => (long)o.Id == champion.Id).Single();

                            GAME _game = ctx.GAMEs.Where(o => o.GAME_ID == game.GameId).SingleOrDefault();
                            if (_game == null)
                            {
                                _game = new GAME()
                                {
                                    GAME_ID = game.GameId,
                                    GAMEDATE = game.CreateDate,
                                    MAPTYPE = toEnumName(game.MapType),
                                     GAMEMODE = toEnumName(game.GameMode),
                                     GAMESUBTYPE = toEnumName(game.SubType),
                                     IPEARNED = game.IpEarned,
                                     INVALID = fromBool(game.Invalid),
                                     CREATIONDATE = DateTime.Now,
                                     CREATIONUSER = UserName
                                };
                                ctx.GAMEs.Add(_game);
                            }

                            GAMESTAT _gamestat = ctx.GAMESTATs.Where(o => o.GAME_ID == game.GameId && o.SUMMONER_ID == summoner.Id).SingleOrDefault();
                            if (_gamestat == null)
                            {
                                RawStat stat = game.Statistics;
                                _gamestat = new GAMESTAT()
                                {
                                    GAME_ID = game.GameId,
                                    SUMMONER_ID = summoner.Id,
                                    SUMMONERLEVEL = game.Level,
                                    CHAMPION_ID = game.ChampionId,
                                    CHAMPIONLEVEL = stat.Level,
                                    SPELL1_ID = game.SummonerSpell1,
                                    SPELL2_ID = game.SummonerSpell2,
                                    TEAM_ID = game.TeamId,
                                    WIN = fromBool(stat.Win),
                                    TIMEPLAYED = stat.TimePlayed,
                                    CHAMPIONSKILLED = stat.ChampionsKilled,
                                    NUMDEATHS = stat.NumDeaths,
                                    ASSISTS = stat.Assists
                                };
                                ctx.GAMESTATs.Add(_gamestat);
                            }
                        }
                        ctx.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    String custom_msg = "Unable to load game stats data to local database";
                    String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                    MessageBox.Show(message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            return games;
        }

        private sbyte? fromBool(bool? b)
        {
            if (b.HasValue)
                return b.Value ? (sbyte)1 : (sbyte)0;
            else
                return null;
        }

        private bool? toBool(sbyte? s)
        {
            if (s.HasValue)
                return (s.Value != 0) ? true : false;
            else
                return null;
        }

        private String toEnumName<T>(T value)
        {
            return Enum.GetName(typeof(T), value);
        }
    }
}
