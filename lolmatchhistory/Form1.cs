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

            foreach (String key in ApiKeys)
            {
                _apis.Add(RiotApi.GetInstance(key));
                _staticApis.Add(StaticRiotApi.GetInstance(key));
            }
            
        }

    #region Properties and Constant

        private static List<string> ApiKeys = new List<String> { "005c3a72-9b7d-4f10-9b3b-9e957c1278ee" }; //, "bc786a7f-7dc8-4750-891f-b5704af3d667"

        private static List<RiotApi> _apis = new List<RiotApi>();
        private static int apiIndx;
        private static RiotApi api
        {
            get
            {
                apiIndx++;
                if (apiIndx == _apis.Count)
                {
                    apiIndx = 0;
                }
                return _apis[apiIndx];
            }
            set { }
        }

        private static List<StaticRiotApi> _staticApis = new List<StaticRiotApi>();
        private static int staticApiIndx;
        private static StaticRiotApi staticApi
        {
            get
            {
                staticApiIndx++;
                if (staticApiIndx == _staticApis.Count)
                {
                    staticApiIndx = 0;
                }
                return _staticApis[staticApiIndx];
            }
            set { }
        }

        enum opting {optIn, optOut};

    #endregion

    #region Configurations
        private const Region region = RiotSharp.Region.na;
        private List<String> players = new List<String> { "Erndra", "auribug", "faithinyou", "qoobime", "winkybbb", "macaronny", "dextiny", "arwater", "yolobellchan", "masa123", "o0garcia0o", "izayoisaya", "anymoreromfinder", "belibulu", "everloser", "kreivch", "oocosetteoo", "otiotu", "staxya" }; //"jinusaly",
        private List<String> includedPlayers = new List<String> { "macaronny", "Erndra", "faithinyou", "qoobime", "winkybbb", "dextiny", "arwater", "yolobellchan", "masa123" };
        private List<String> excludedPlayers = new List<String> { };
        private const opting loadPlyaerOption = opting.optOut;
        private const String UserName = "VICTOR";
    #endregion

    #region log
        Dictionary<TimeSpan, String> log = new Dictionary<TimeSpan, String>();
        DateTime startTime = DateTime.MinValue;
        private void addLog(String desc)
        {
            if (startTime == DateTime.MinValue)
                startTime = DateTime.Now;

            TimeSpan timeStamp = DateTime.Now - startTime;
            while (log.ContainsKey(timeStamp))
            {
                timeStamp += new TimeSpan(1);
            }
            log.Add(timeStamp, desc);
        }

        private void printLog(Dictionary<TimeSpan, String> log)
        {
            if (log == null)
            {
                log = this.log;
            }
            
            String msg = "";
            TimeSpan prev = new TimeSpan(0);
            foreach (KeyValuePair<TimeSpan, String> l in log)
            {
                TimeSpan timeElapsed = l.Key - prev;

                msg += (l.Key).ToString(@"mm\:ss\.fff") + " || elapsed: " + timeElapsed.ToString(@"mm\:ss\.fff") + " || " + l.Value + Environment.NewLine;
                prev = l.Key;
            }
            MessageBox.Show(msg);
        }
    #endregion  

        /**********/
        /** MAIN **/
        /**********/
        private async void form_load(Object sender, EventArgs e)
        {
            try
            {
                var start = DateTime.Now;
                addLog("start");
                //List<Summoner> summoners = reloadSummoners(players.Take(1).ToList());
                Dictionary<String, List<Game>> result = await loadRecentGamesAsync();
                //Dictionary<String, List<Game>> result = new Dictionary<string, List<Game>>();
                //Dictionary<String, Task<List<Game>>> gamesTasks = new Dictionary<string, Task<List<Game>>>();
                //int count = 0;
                //foreach (Summoner summoner in reloadSummoners(players))
                //{
                //    addLog("begin GetRecentGamesAsync #" + count);
                //    gamesTasks[summoner.Name] = summoner.GetRecentGamesAsync();
                //    addLog("finish GetRecentGamesAsync #" + count++);

                //}
                //reloadChampions();
                //reloadItems();
                //reloadSummonerSpells();
                MessageBox.Show("#player: " + /*result.Count +*/ Environment.NewLine + "time spent: " + (DateTime.Now - start).TotalSeconds);
                printLog(null);        
            }
            catch (Exception ex)
            {
                  var a = ex;
            }
            this.Close();
        }

        private async Task<Dictionary<String, List<Game>>> loadRecentGamesAsync()
        {
            try
            {
                List<String> playersToLoad = loadPlyaerOption == opting.optIn ? includedPlayers : players;
                Dictionary<String, long> summonerIds = getSummonerIds(playersToLoad.Skip(10).Take(10).ToList()); //.Skip(10)
                if (summonerIds == null || summonerIds.Count < playersToLoad.Count)
                {
                     //summoners = reloadSummoners(playersToLoad.ToList()); //Skip(18).Take(9)
                }
                addLog("finish reloadSummoners");

                Task<Dictionary<String, List<Game>>> summonersGamesTask = loadRecentGamesAsync(summonerIds);
                Dictionary<String, List<Game>> summonersGames = await summonersGamesTask;

                addLog("finish loadRecentGames");
                return summonersGames;
            }
            catch (Exception ex)
            {
                MessageBox.Show("error caught in " + this.GetType().Name + ".main:"  + Environment.NewLine + ex.ToString(), "Unexpected Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

       
        private List<ItemStatic> reloadItems()
        {
            ItemListStatic items;
            try 
            {
                items = staticApi.GetItems(region);
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to load latest items data from server.";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                items = null;
            }
            //if (items.Items.Values.ToList()[0].)
            if (!(items == null || items.Items == null || items.Items.Count <= 0))
            {
                using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                {
                    ctx.Database.ExecuteSqlCommand("TRUNCATE TABLE ITEM");

                    foreach (ItemStatic item in items.Items.Values)
                    {
                        ITEM model = new ITEM()
                        {
                            ITEM_ID = item.Id,
                            ITEMNAME = item.Name
                        };

                        ctx.ITEMs.Add(model);
                    }
                    ctx.SaveChanges();
                }
            }
            return items.Items.Values.ToList();
        }

        private List<SummonerSpellStatic> reloadSummonerSpells()
        {
            SummonerSpellListStatic spells;
            try
            {
                spells = staticApi.GetSummonerSpells(region);
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to load latest summoner spells data from server.";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                spells = null;
            }

            if (!(spells == null || spells.SummonerSpells == null || spells.SummonerSpells.Count <= 0))
            {
                using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                {
                    ctx.Database.ExecuteSqlCommand("TRUNCATE TABLE SPELL");

                    foreach (SummonerSpellStatic spell in spells.SummonerSpells.Values)
                    {
                        SPELL model = new SPELL()
                        {
                            SPELL_ID = spell.Id,
                            SPELLNAME = spell.Name
                        };

                        ctx.SPELLs.Add(model);
                    }
                    ctx.SaveChanges();
                }
            }
            return spells.SummonerSpells.Values.ToList();
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

        private List<Summoner> reloadSummoners(List<String> players)
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

        private Dictionary<String,long> getSummonerIds(List<String> summonerNames)
        {
            Dictionary<String, long> summoners = new Dictionary<String, long>();
            try
            {
                using (lolmatchhistoryEntities ctx = new lolmatchhistoryEntities())
                {
                    summonerNames = summonerNames.ConvertAll(o => o.ToLower());
                    List<SUMMONER> models = ctx.SUMMONERs.Where(o => summonerNames.Contains(o.SUMMONERNAME.ToLower().Replace(" ", ""))).ToList();
                    foreach (SUMMONER model in models)
                    {
                        summoners.Add(model.SUMMONERNAME, model.SUMMONER_ID);
                        //summoners.Add(new Summoner()
                        //{
                        //     Id = model.SUMMONER_ID,
                        //     Name = model.SUMMONERNAME,
                        //     Level = (long) model.LEVEL,
                        //     //Region = model.REGION,
                        //     ProfileIconId =  model.PROFILEICON_ID.HasValue ? model.PROFILEICON_ID.Value : 0,
                        //     RevisionDate = model.REVISIONDATE.HasValue ? model.REVISIONDATE.Value : DateTime.MinValue,
                        //});
                    }
                }
            }
            catch (Exception ex)
            {
                String custom_msg = "Unable to get summoners data from local database";
                String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                MessageBox.Show(message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                summoners = null;
            }
            return summoners;
        }

        private async Task<Dictionary<String, List<Game>>> loadRecentGamesAsync(Dictionary<String, long> summonerIds)
        {
            Dictionary<String, List<Game>> result = new Dictionary<string,List<Game>>();
            Dictionary<String, Task<List<Game>>> gamesTasks = new Dictionary<string, Task<List<Game>>>();
            int count = 0;
            foreach (System.Collections.Generic.KeyValuePair<String, long> pair in summonerIds)
            {
                addLog("begin GetRecentGamesAsync #" + count);
                try
                {
                    gamesTasks[pair.Key] = api.GetRecentGamesAsync(region, pair.Value);
                }
                catch (Exception ex)
                {
                    String custom_msg = "Unable to load match data for " + pair.Value + " from server";
                    String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                    MessageBox.Show(message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    gamesTasks[pair.Key] = null;
                }
                addLog("finish GetRecentGamesAsync #" + count++);
                
            }
            addLog("finish all GetRecentGamesAsync");

             count = 0;
             foreach (System.Collections.Generic.KeyValuePair<String, long> pair in summonerIds)
            {
                 try
                 {
                     List<Game> games = await gamesTasks[pair.Key];
                     if (games != null)
                     {
                         saveRecentGames(pair.Value, games);
                         result.Add(pair.Key, games);
                     }
                 }
                 catch (Exception ex)
                 {
                     String custom_msg = pair .Value + "in await gamesTasks";
                     String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
                     MessageBox.Show(message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                 }
                
                addLog("finish saveRecentGames #" + count++);
            }
            addLog("finish all saveRecentGames");
            
            return result;
        }


        //private  Task<List<Game>> loadRecentGames(Summoner summoner)
        //{
        //    Task<List<Game>> gamesTask;

        //    try
        //    {
        //        gamesTask = summoner.GetRecentGamesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        String custom_msg = "Unable to load recent games data for summoner [" + summoner.Name + "]from server.";
        //        String message = custom_msg + Environment.NewLine + Environment.NewLine + ex.ToString();
        //        MessageBox.Show(message, "Server Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //        return null;
        //    }

        //    return gamesTask;
        //}

        private void saveRecentGames(long summonerId, List<Game> games)
        {
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
                                    GAMEDATE = game.CreateDate.AddHours(8),
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

                            GAMESTAT _gamestat = ctx.GAMESTATs.Where(o => o.GAME_ID == game.GameId && o.SUMMONER_ID == summonerId).SingleOrDefault();
                            RawStat stat = game.Statistics;
                            if (_gamestat == null) //add 
                            {

                                _gamestat = new GAMESTAT()
                                {
                                    GAME_ID = game.GameId,
                                    SUMMONER_ID = summonerId,
                                    SUMMONERLEVEL = game.Level,
                                    CHAMPION_ID = game.ChampionId,
                                    CHAMPIONLEVEL = stat.Level,
                                    SPELL1_ID = game.SummonerSpell1,
                                    SPELL2_ID = game.SummonerSpell2,
                                    TEAM_ID = game.TeamId,
                                    WIN = fromBool(stat.Win),
                                    TIMEPLAYED = stat.TimePlayed,
                                    IPEARNED = game.IpEarned,
                                    CHAMPIONSKILLED = stat.ChampionsKilled,
                                    NUMDEATHS = stat.NumDeaths,
                                    ASSISTS = stat.Assists,
                                    KILLINGSPREES = stat.KillingSprees,
                                    LARGESTKILLINGSPREE = stat.KillingSprees,
                                    LARGESTMULTIKILL = stat.LargestMultiKill,
                                    FIRSTBLOOD = stat.FirstBlood,
                                    DOUBLEKILLS = stat.DoubleKills,
                                    TRIPLEKILLS = stat.TripleKills,
                                    QUADRAKILLS = stat.QuadraKills,
                                    PENTAKILLS = stat.PentaKills,
                                    UNREALKILLS = stat.UnrealKills,
                                    TURRETSKILLED = stat.TurretsKilled,
                                    BARRACKSKILLED = stat.BarracksKilled,
                                    NEXUSKILLED = fromBool(stat.NexusKilled),
                                    GOLD = stat.Gold,
                                    GOLDEARNED = stat.GoldEarned,
                                    GOLDSPENT = stat.GoldSpent,
                                    ITEMSPURCHASED = stat.ItemsPurchased,
                                    NUMITEMSBOUGHT = stat.NumItemsBought,
                                    CONSUMABLESPURCHASED = stat.ConsumablesPurchased,
                                    ITEM0 = stat.Item0,
                                    ITEM1 = stat.Item1,
                                    ITEM2 = stat.Item2,
                                    ITEM3 = stat.Item3,
                                    ITEM4 = stat.Item4,
                                    ITEM5 = stat.Item5,
                                    ITEM6 = stat.Item6,
                                    LEGENDARYITEMSCREATED = stat.LegendaryItemsCreated,
                                    TOTALDAMAGEDEALT = stat.TotalDamageDealt,
                                    TOTALDAMAGEDEALTTOCHAMPIONS = stat.TotalDamageDealtToChampions,
                                    DAMAGEDEALTPLAYER = stat.DamageDealtPlayer,
                                    PHYSICALDAMAGEDEALTPLAYER = stat.PhysicalDamageDealtPlayer,
                                    PHYSICALDAMAGEDEALTTOCHAMPIONS = stat.PhysicalDamageDealtToChampions,
                                    LARGESTCRITICALSTRIKE = stat.LargestCriticalStrike,
                                    MAGICDAMAGEDEALTPLAYER = stat.MagicDamageDealtPlayer,
                                    MAGICDAMAGEDEALTTOCHAMPIONS = stat.MagicDamageDealtToChampions,
                                    TRUEDAMAGEDEALTPLAYER = stat.TrueDamageDealtPlayer,
                                    TRUEDAMAGEDEALTTOCHAMPIONS = stat.TrueDamageDealtToChampions,
                                    TOTALDAMAGETAKEN = stat.TotalDamageTaken,
                                    PHYSICALDAMAGETAKEN = stat.PhysicalDamageTaken,
                                    MAGICDAMAGETAKEN = stat.MagicDamageTaken,
                                    TRUEDAMAGETAKEN = stat.TrueDamageTaken,
                                    TOTALHEAL = stat.TotalHeal,
                                    TOTALUNITSHEALED = stat.TotalUnitsHealed,
                                    TOTALTIMECROWDCONTROLDEALT = stat.TotalTimeCrowdControlDealt,
                                    MINIONSDENIED = stat.MinionsDenied,
                                    MINIONSKILLED = stat.MinionsKilled,
                                    NEUTRALMINIONSKILLED = stat.NeutralMinionsKilled,
                                    NEUTRALMINIONSKILLEDYOURJUNGLE = stat.NeutralMinionsKilledYourJungle,
                                    NEUTRALMINIONSKILLEDENEMYJUNGLE = stat.NeutralMinionsKilledEnemyJungle,
                                    SUPERMONSTERKILLED = stat.SuperMonsterKilled,
                                    SIGHTWARDSBOUGHT = stat.SightWardsBought,
                                    VISIONWARDSBOUGHT = stat.VisionWardsBought,
                                    WARDPLACED = stat.WardPlaced,
                                    WARDKILLED = stat.WardKilled
                                };
                                ctx.GAMESTATs.Add(_gamestat);
                            }
                            else //update
                            {
                                _gamestat.GAME_ID = game.GameId;
                                _gamestat.SUMMONER_ID = summonerId;
                                _gamestat.SUMMONERLEVEL = game.Level;
                                _gamestat.CHAMPION_ID = game.ChampionId;
                                _gamestat.CHAMPIONLEVEL = stat.Level;
                                _gamestat.SPELL1_ID = game.SummonerSpell1;
                                _gamestat.SPELL2_ID = game.SummonerSpell2;
                                _gamestat.TEAM_ID = game.TeamId;
                                _gamestat.WIN = fromBool(stat.Win);
                                _gamestat.TIMEPLAYED = stat.TimePlayed;
                                _gamestat.IPEARNED = game.IpEarned;
                                _gamestat.CHAMPIONSKILLED = stat.ChampionsKilled;
                                _gamestat.NUMDEATHS = stat.NumDeaths;
                                _gamestat.ASSISTS = stat.Assists;
                                _gamestat.KILLINGSPREES = stat.KillingSprees;
                                _gamestat.LARGESTKILLINGSPREE = stat.KillingSprees;
                                _gamestat.LARGESTMULTIKILL = stat.LargestMultiKill;
                                _gamestat.FIRSTBLOOD = stat.FirstBlood;
                                _gamestat.DOUBLEKILLS = stat.DoubleKills;
                                _gamestat.TRIPLEKILLS = stat.TripleKills;
                                _gamestat.QUADRAKILLS = stat.QuadraKills;
                                _gamestat.PENTAKILLS = stat.PentaKills;
                                _gamestat.UNREALKILLS = stat.UnrealKills;
                                _gamestat.TURRETSKILLED = stat.TurretsKilled;
                                _gamestat.BARRACKSKILLED = stat.BarracksKilled;
                                _gamestat.NEXUSKILLED = fromBool(stat.NexusKilled);
                                _gamestat.GOLD = stat.Gold;
                                _gamestat.GOLDEARNED = stat.GoldEarned;
                                _gamestat.GOLDSPENT = stat.GoldSpent;
                                _gamestat.ITEMSPURCHASED = stat.ItemsPurchased;
                                _gamestat.NUMITEMSBOUGHT = stat.NumItemsBought;
                                _gamestat.CONSUMABLESPURCHASED = stat.ConsumablesPurchased;
                                _gamestat.ITEM0 = stat.Item0;
                                _gamestat.ITEM1 = stat.Item1;
                                _gamestat.ITEM2 = stat.Item2;
                                _gamestat.ITEM3 = stat.Item3;
                                _gamestat.ITEM4 = stat.Item4;
                                _gamestat.ITEM5 = stat.Item5;
                                _gamestat.ITEM6 = stat.Item6;
                                _gamestat.LEGENDARYITEMSCREATED = stat.LegendaryItemsCreated;
                                _gamestat.TOTALDAMAGEDEALT = stat.TotalDamageDealt;
                                _gamestat.TOTALDAMAGEDEALTTOCHAMPIONS = stat.TotalDamageDealtToChampions;
                                _gamestat.DAMAGEDEALTPLAYER = stat.DamageDealtPlayer;
                                _gamestat.PHYSICALDAMAGEDEALTPLAYER = stat.PhysicalDamageDealtPlayer;
                                _gamestat.PHYSICALDAMAGEDEALTTOCHAMPIONS = stat.PhysicalDamageDealtToChampions;
                                _gamestat.LARGESTCRITICALSTRIKE = stat.LargestCriticalStrike;
                                _gamestat.MAGICDAMAGEDEALTPLAYER = stat.MagicDamageDealtPlayer;
                                _gamestat.MAGICDAMAGEDEALTTOCHAMPIONS = stat.MagicDamageDealtToChampions;
                                _gamestat.TRUEDAMAGEDEALTPLAYER = stat.TrueDamageDealtPlayer;
                                _gamestat.TRUEDAMAGEDEALTTOCHAMPIONS = stat.TrueDamageDealtToChampions;
                                _gamestat.TOTALDAMAGETAKEN = stat.TotalDamageTaken;
                                _gamestat.PHYSICALDAMAGETAKEN = stat.PhysicalDamageTaken;
                                _gamestat.MAGICDAMAGETAKEN = stat.MagicDamageTaken;
                                _gamestat.TRUEDAMAGETAKEN = stat.TrueDamageTaken;
                                _gamestat.TOTALHEAL = stat.TotalHeal;
                                _gamestat.TOTALUNITSHEALED = stat.TotalUnitsHealed;
                                _gamestat.TOTALTIMECROWDCONTROLDEALT = stat.TotalTimeCrowdControlDealt;
                                _gamestat.MINIONSDENIED = stat.MinionsDenied;
                                _gamestat.MINIONSKILLED = stat.MinionsKilled;
                                _gamestat.NEUTRALMINIONSKILLED = stat.NeutralMinionsKilled;
                                _gamestat.NEUTRALMINIONSKILLEDYOURJUNGLE = stat.NeutralMinionsKilledYourJungle;
                                _gamestat.NEUTRALMINIONSKILLEDENEMYJUNGLE = stat.NeutralMinionsKilledEnemyJungle;
                                _gamestat.SUPERMONSTERKILLED = stat.SuperMonsterKilled;
                                _gamestat.SIGHTWARDSBOUGHT = stat.SightWardsBought;
                                _gamestat.VISIONWARDSBOUGHT = stat.VisionWardsBought;
                                _gamestat.WARDPLACED = stat.WardPlaced;
                                _gamestat.WARDKILLED = stat.WardKilled;
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

/*
 * TO-DO LIST:
 *  1. api key roation (maybe with db)
 *  2. load mh in depth (other 9 players)
 *  3. 
 * last update date ()
 * smaller piece of try catch (if a piece fail, dont fail other)
 * in-depth load match (templist<loadedplayer>, check gamestat record exist/only do for new game)
 * store to local if no connection (json of entity model?)
 * show count of new game/gamestat added
 *
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
*/