namespace 收藏夾匯入工具
{
    public class Language
    {
        public string AppTitle { get; set; }
        public string DetectionOsuIsRunning { get; set; }
        public string OpenFileFilter { get; set; }
        public string PleaseSelectCollection { get; set; }
        public string ReadCollectionFail { get; set; }
        public string ConfirmOsuPath { get; set; }
        public string PleaseSelectOsuPath { get; set; }
        public string NeedValidOsuPath { get; set; }
        public string OsuPath { get; set; }
        public string CollectionName { get; set; }
        public string AddBeatmapToCollection { get; set; }
        public string NeedDownloadBeatmap { get; set; }
        public string DownloadBeatmapInfo { get; set; }
        public string OsuUsername { get; set; }
        public string OsuPassword { get; set; }
        public string LoginErrorElseReason { get; set; }
        public string LoginError { get; set; }
        public string DownloadBeatmapWithVideo { get; set; }
        public string DownloadBeatmapName { get; set; }
        public string DownloadFali { get; set; }
        public string DownloadDone { get; set; }
        public string BackupCollectionTo { get; set; }
        public string WritingNewCollection { get; set; }
        public string WriteDone1 { get; set; }
        public string WriteDone2 { get; set; }
        public string WriteDone3 { get; set; }
        public string WriteDone4 { get; set; }
        public string PressAnyKeyToExit { get; set; }

        public Language()
        {
            AppTitle = "Collection Inport Tools  By 孤之界(jun112561)";
            DetectionOsuIsRunning = "Detected that Osu has been running. Restart Osu to reveal the added collections.";
            OpenFileFilter = "Collections|*.json";
            PleaseSelectCollection = "Please select the collection you want to import!";
            ReadCollectionFail = "Failed to read the collection; please check if you choose the correct collection file!";
            ConfirmOsuPath = "Detected Osu at: {0}{1}Is this correct?";
            PleaseSelectOsuPath = "Please select the Osu folder path.";
            NeedValidOsuPath = "Need a valid Osu path to proceed!";
            OsuPath = "Osu path: {0}";
            CollectionName = "Collection name: {0}";
            AddBeatmapToCollection = "Add Beatmap to collection: {0}";
            NeedDownloadBeatmap = "This Beatmap is not in the library. Added to download list.";
            DownloadBeatmapInfo = "Downloading missing Beatmaps. Please enter your Osu account to start downloading. (Account informations will only be used to download Beatmaps.)";
            OsuUsername = "Username: ";
            OsuPassword = "Password: ";
            LoginErrorElseReason = "Login error. Please check Internet connection ({0})";
            LoginError = "Login error. Please enter the correct username and password!";
            DownloadBeatmapWithVideo = "Download Beatmaps with background videos?";
            DownloadBeatmapName = "Downloading: {0}";
            DownloadFali = "Download failed!  {0}";
            DownloadDone = "Downloaded: {0}";
            BackupCollectionTo = "Backuping collection.db to: {0}";
            WritingNewCollection = "Writing new collection.db...";
            WriteDone1 = "Import done! Detected Osu running; please press f5 to refresh Beatmaps and restart Osu.";
            WriteDone2 = "Import done! Please restart Osu to load the new collections.";
            WriteDone3 = "Import done! Please start Osu and press f5 to refresh Beatmaps and restart Osu to load the new collections.";
            WriteDone4 = "Import done! Please start Osu to load the new collections.";
            PressAnyKeyToExit = "Press any key to exit...";
        }
    }
}
