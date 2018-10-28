using CollectionManager.DataTypes;
using CollectionManager.Modules.CollectionsManager;
using CollectionManager.Modules.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace 收藏夾匯入工具
{
    class Program
    {
        static OsuFileIo OsuFileIo = new OsuFileIo(new BeatmapExtension()); //CollectionManager的內部class
        private static readonly string loginDataStr = "username={0}&password={1}&login=login&sid=", loginAddress = @"https://osu.ppy.sh/forum/ucp.php?mode=login"; //登入osu帳號用
        static Language language; //語言

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (System.Threading.Thread.CurrentThread.CurrentCulture.Name.StartsWith("zh")) language = JsonConvert.DeserializeObject<Language>(Encoding.UTF8.GetString(Properties.Resources.zh_tw)); //假如是中文語系，就使用中文
                else language = new Language(); //否則，英文
            }
            catch (Exception) { language = new Language(); /*這邊是當跳出錯誤時，使用英文。正常來說是不會有錯誤才對，但有次比賽拿來做測試的時候出了錯誤，也抓不到點，就先認定是語系的問題了*/ }

            Console.Title = language.AppTitle;

            if (OsuPathResolver.Instance.OsuIsRunning) FormatWrite(language.DetectionOsuIsRunning, ConsoleColor.Red); 

            OpenFileDialog openFileDialog = new OpenFileDialog(); //選擇收藏夾檔案的對話方塊
            openFileDialog.AddExtension = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.CheckPathExists = true;
            openFileDialog.DefaultExt = "json";
            openFileDialog.Filter = language.OpenFileFilter;
            openFileDialog.Multiselect = false;

            if (openFileDialog.ShowDialog() == DialogResult.Cancel) Exit(language.PleaseSelectCollection, true);

            JsonData collectionData = null;
            try { collectionData = JsonConvert.DeserializeObject<JsonData>(File.ReadAllText(openFileDialog.FileName)); } //讀取收藏夾資料
            catch (Exception) { }
            if (collectionData == null || collectionData.collection_data == null) Exit(language.ReadCollectionFail, true);

            string osuPath = OsuPathResolver.Instance.GetOsuDir((path) => { //第一次會先偵測osu的路徑，然後提示使用者是否正確
                var dialogResult = MessageBox.Show(
                    string.Format(
                    language.ConfirmOsuPath, Environment.NewLine + path, Environment.NewLine),
                    "", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
                return dialogResult == DialogResult.Yes;
            }, (text) => {
                FolderBrowserDialog dialog = new FolderBrowserDialog(); //假如是錯誤的，那就開個資料夾選擇對話框，讓使用者自行選擇osu的路徑

                dialog.ShowNewFolderButton = false;
                dialog.Description = language.PleaseSelectOsuPath;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                if (dialog.ShowDialog() == DialogResult.OK && Directory.Exists(dialog.SelectedPath)) return dialog.SelectedPath;
                return "";
            });

            if (osuPath == string.Empty || !Directory.Exists(osuPath)) Exit(language.NeedValidOsuPath, true);
            if (!osuPath.EndsWith("\\")) osuPath += "\\";

            FormatWrite(string.Format(language.OsuPath, osuPath), ConsoleColor.Yellow);
            OsuFileIo.OsuSettings.Load(osuPath); 
            OsuFileIo.OsuDatabase.Load(osuPath + "osu!.db"); //載入osu的資料庫來抓取songs的資料夾路徑

            List<BeatmapData> needDownloadBeatmapList = new List<BeatmapData>();            
            var collectionManager = new CollectionsManager(OsuFileIo.OsuDatabase.LoadedMaps.Beatmaps); Collection collection; //初始化目前osu所有的Beatmap
            collectionManager.EditCollection(CollectionEditArgs.AddCollections(OsuFileIo.CollectionLoader.LoadCollection(osuPath + "collection.db"))); //載入原先的收藏夾

            foreach (CollectionData item in collectionData.collection_data)
            {
                FormatWrite(string.Format(language.CollectionName, item.collection_name), ConsoleColor.Yellow);

                if (collectionManager.CollectionNameExists(item.collection_name)) collection = collectionManager.GetCollectionByName(item.collection_name); //如果收藏夾已經存在，就讀取出來
                else collection = new Collection(OsuFileIo.LoadedMaps) { Name = item.collection_name }; //否則新建一個

                foreach (BeatmapData item2 in item.beatmap_data)
                {
                    if (!collection.AllBeatmaps().Any((x) => x.MapId == item2.beatmap_id)) //讀取收藏夾內的Beatmap，如果要匯入的收藏夾Beatmap ID不存在於收藏夾內，那就新增進去
                    {
                        FormatWrite(string.Format(language.AddBeatmapToCollection, item2.beatmap_name), ConsoleColor.Green);
                        collection.AddBeatmapByHash(item2.beatmap_md5); //新增Beatmap，使用Hash新增法
                        if (!OsuFileIo.OsuDatabase.LoadedMaps.Beatmaps.Any((x) => x.MapId == item2.beatmap_id)) //如果osu資料庫裡面沒有該Beatmap
                        {
                            needDownloadBeatmapList.Add(item2); //就新增到下載清單內
                            FormatWrite(language.NeedDownloadBeatmap, ConsoleColor.Cyan);                            
                        }
                    }
                }

                collectionManager.EditCollection(CollectionEditArgs.RemoveCollections(new Collections() { collection })); //先把收藏夾移除
                collectionManager.EditCollection(CollectionEditArgs.AddCollections(new Collections() { collection })); //再把收藏夾新增，以達成重整的效果
            }

            if (needDownloadBeatmapList.Count!= 0) //如果下載清單數量不為0，就進入下載程序
            {
                FormatWrite(language.DownloadBeatmapInfo, ConsoleColor.Green);
                CookieAwareWebClient cookieAwareWebClient = new CookieAwareWebClient(); //CollectionManager提供的class，可以讓WebClient使用cookie的功能
                string username, password;
                do
                {
                    Console.Write(language.OsuUsername); username = Console.ReadLine();
                    Console.Write(language.OsuPassword); password = "";
                    do
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                        {
                            password += key.KeyChar;
                            Console.Write("*");
                        }
                        else
                        {
                            if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                            {
                                password = password.Substring(0, (password.Length - 1));
                                Console.Write("\b \b");
                            }
                            else if (key.Key == ConsoleKey.Enter)
                            {
                                Console.WriteLine();
                                break;
                            }
                        }
                    } while (true);

                    try { if (!cookieAwareWebClient.Login(loginAddress, string.Format(loginDataStr, username, password)).Contains("I don't have an account")) break; } //登入失敗的話，回傳的網頁資料會有I don't have an account
                    catch (Exception ex) { Exit(string.Format(language.LoginErrorElseReason, ex.Message)); } 
                    
                    FormatWrite(language.LoginError, ConsoleColor.Red);
                } while (true);

                bool downloadVideo = MessageBox.Show(language.DownloadBeatmapWithVideo, "", MessageBoxButtons.YesNo) == DialogResult.Yes; //提示下載時是否包含背景影片

                needDownloadBeatmapList.ForEach((item) => {
                    string savePath = OsuFileIo.OsuSettings.CustomBeatmapDirectoryLocation + item.beatmap_setid + " " + StripInvalidCharacters(item.beatmap_name) + ".osz"; //存放路徑: osu的songs資料夾 + BeatmapSet ID + BeatmapSet Name
                    if (!File.Exists(savePath))
                    {
                        FormatWrite(string.Format(language.DownloadBeatmapName, Path.GetFileName(savePath)), ConsoleColor.Green);
                        try { File.WriteAllBytes(savePath, cookieAwareWebClient.DownloadData("https://osu.ppy.sh/d/" + item.beatmap_setid + (downloadVideo ? "n" : ""))); }
                        catch (Exception ex) { Console.WriteLine(string.Format(language.DownloadFali, ex.Message)); }
                    }
                    else FormatWrite(string.Format(language.DownloadDone, Path.GetFileName(savePath)), ConsoleColor.Yellow);
                });
            }

            string backupName = "collection.db-" + DateTime.Now.ToFileTime() + ".bak"; //備份舊的收藏夾檔案
            File.Move(osuPath + "collection.db", osuPath + backupName); //然後覆蓋新的收藏夾檔案
            Console.WriteLine(string.Format(language.BackupCollectionTo, osuPath + backupName));

            Console.WriteLine(language.WritingNewCollection);
            OsuFileIo.CollectionLoader.SaveOsuCollection(collectionManager.LoadedCollections, osuPath + "collection.db");

            if (OsuPathResolver.Instance.OsuIsRunning)
            {
                if (needDownloadBeatmapList.Count != 0) Exit(language.WriteDone1);
                else Exit(language.WriteDone2);
            }
            else
            {
                if (needDownloadBeatmapList.Count != 0) Exit(language.WriteDone3);
                else Exit(language.WriteDone4);
            }
        }

        static void Exit(string text, bool isError = false)
        {
            if (isError) FormatWrite(text, ConsoleColor.Red);
            else FormatWrite(text);
            Console.Write(language.PressAnyKeyToExit);
            Console.Read();
            Environment.Exit(0);
        }

        private static void FormatWrite(string text, ConsoleColor consoleColor = ConsoleColor.Gray)
        {
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(text);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public static string StripInvalidCharacters(string name)
        {
            foreach (var invalidChar in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(invalidChar.ToString(), string.Empty);
            }
            return name.Replace(".", string.Empty);
        }
    }

    public class BeatmapData
    {
        public int beatmap_setid { get; set; }
        public int beatmap_id { get; set; }
        public string beatmap_name { get; set; }
        public string beatmap_md5 { get; set; }
    }

    public class CollectionData
    {
        public string collection_name { get; set; }
        public List<BeatmapData> beatmap_data { get; set; }
    }

    public class JsonData
    {
        public List<CollectionData> collection_data { get; set; }
    }
}
