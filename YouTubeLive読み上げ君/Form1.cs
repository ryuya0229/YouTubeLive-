using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq; // OrderByのために必要
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace YouTubeLive読み上げ君
{
    public partial class Form1 : Form
    {
        private Form2 form2;
        const string configFilePath = "appsetting.json"; // 実行ファイルと同じ場所にある場合
        private AppSettings settings;
        private string voiceBoxPath; // VoiceBoxのパス
        private string liveChatId = ""; // ライブチャットID
        private string apiKey; // YouTube Data APIキー
        private HttpClient httpClient;
        private HashSet<string> processedMessages = new HashSet<string>(); // 処理済みメッセージを記録するハッシュセット

        public Form1()
        {
            InitializeComponent();
            InitializeForm();
            LoadAppSettings(); // 設定を読み込む
            LoadSpeakers(); // スピーカーを読み込む
            httpClient = new HttpClient();
            CheckAndStartVoiceBox();
            form2 = new Form2();
            form2.Show(); // Form2を表示
        }

        private void LoadAppSettings()
        {
            settings = JsonConfigLoader.LoadConfig(configFilePath);
            voiceBoxPath = settings.VoiceBoxPath;
            apiKey = settings.ApiKey;
        }

        private void LoadSpeakers()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFilePath);
            var config = JsonConfigLoader.LoadConfig(filePath);

            // speakersを名前順にソート（Keyで）
            var sortedSpeakers = config.speakers.OrderBy(s => s.Key).ToList();

            // ComboBoxに追加
            comboBox1.Items.Clear();
            foreach (var speaker in sortedSpeakers)
            {
                comboBox1.Items.Add(speaker.Key); // speaker.Key からキャラ名を取得
            }

            // ComboBoxの最初のアイテムを選択
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        



        private void InitializeForm()
        {
            this.Width = 800;  // 幅を800ピクセルに設定
            this.Height = 600; // 高さを600ピクセルに設定
        }

        private void CheckAndStartVoiceBox()
        {
            if (!IsVoiceBoxRunning())
            {
                StartVoiceBox();
            }
        }

        private bool IsVoiceBoxRunning()
        {
            Process[] processes = Process.GetProcessesByName("VoiceBox");
            return processes.Length > 0;
        }

        private void StartVoiceBox()
        {
            if (File.Exists(voiceBoxPath))
            {
                // ProcessStartInfoを使用して通常で起動
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = voiceBoxPath,
                    WindowStyle = ProcessWindowStyle.Normal // 通常で起動
                };

                Process.Start(startInfo);
            }
            else
            {
                MessageBox.Show("VoiceBoxの実行ファイルが見つかりません。パスを確認してください。");
            }
        }

        private async void btnGetChannelId_Click(object sender, EventArgs e)
        {
            string channelName = txtChannelName.Text;
            if (!string.IsNullOrWhiteSpace(channelName))
            {
                await RetrieveChannelIds(channelName);
            }
            else
            {
                MessageBox.Show("チャンネル名を入力してください。");
            }
        }

        private async Task RetrieveChannelIds(string channelName)
        {
            try
            {
                string url = $"https://www.googleapis.com/youtube/v3/search?key={apiKey}&q={channelName}&type=channel&part=id,snippet";
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                var items = json["items"];

                listChat.Items.Clear(); // 既存のアイテムをクリア
                listBoxChannels.Items.Clear(); // 新しいListBoxをクリア

                if (items != null && items.HasValues)
                {
                    foreach (var item in items)
                    {
                        var channelId = item["id"]?["channelId"]?.ToString();
                        var snippet = item["snippet"];

                        if (snippet != null)
                        {
                            string channelTitle = snippet["title"]?.ToString(); // チャンネル名を取得
                            var subscriberCount = await GetSubscriberCount(channelId ?? ""); // channelIdがnullの場合に空文字を渡す

                            // 新しいListBoxに追加
                            if (!string.IsNullOrEmpty(channelId))
                            {
                                // チャンネルIDをパスワード形式で表示
                                listBoxChannels.Items.Add($"{channelTitle} (登録者数: {subscriberCount})|{channelId}");
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("チャンネルIDが見つかりませんでした。");
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"HTTPエラー: {ex.Message}");
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"JSONエラー: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"エラーが発生しました: {ex.Message}");
            }
        }


        private async Task<string> GetSubscriberCount(string channelId)
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return "不明"; // channelIdが空の場合
            }

            try
            {
                string url = $"https://www.googleapis.com/youtube/v3.channels?key={apiKey}&id={channelId}&part=statistics";
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                return json["items"]?[0]?["statistics"]?["subscriberCount"]?.ToString() ?? "不明"; // null合体演算子で不明を返す
            }
            catch
            {
                return "不明"; // エラーが発生した場合は「不明」を返す
            }
        }

        private async void listBoxChannels_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxChannels.SelectedItem != null)
            {
                string selectedItem = listBoxChannels.SelectedItem.ToString();
                string channelId = GetChannelIdFromItem(selectedItem); // 選択したアイテムからチャンネルIDを取得

                if (!string.IsNullOrEmpty(channelId))
                {
                    liveChatId = await GetLiveChatId(channelId); // ライブチャットIDを取得
                    if (!string.IsNullOrEmpty(liveChatId))
                    {
                        listChat.Items.Clear(); // listChatをクリア

                        // 参加時刻を現在の時間で設定（ここでjoinTimeを設定）
                        joinTime = DateTime.Now; // 現在の時刻を設定

                        await StartLiveChatRetrieval(); // ライブチャットの取得を開始
                    }
                    else
                    {
                        MessageBox.Show("ライブチャットが存在しません。");
                    }
                }
            }
        }

        private string GetChannelIdFromItem(string item)
        {
            if (string.IsNullOrEmpty(item))
            {
                return string.Empty; // itemが空の場合は空文字を返す
            }
            return item.Split('|')[1]; // アイテムからチャンネルIDを取得
        }

        private async Task<string?> GetLiveChatId(string channelId) // string? に変更
        {
            if (string.IsNullOrEmpty(channelId))
            {
                return null; // channelIdが空の場合はnullを返す
            }

            try
            {
                string url = $"https://www.googleapis.com/youtube/v3/search?key={apiKey}&channelId={channelId}&eventType=live&part=snippet&type=video";
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);
                var videoId = json["items"]?[0]?["id"]?["videoId"]?.ToString();

                if (!string.IsNullOrEmpty(videoId))
                {
                    url = $"https://www.googleapis.com/youtube/v3/videos?id={videoId}&key={apiKey}&part=liveStreamingDetails";
                    response = await httpClient.GetStringAsync(url);
                    json = JObject.Parse(response);
                    return json["items"]?[0]?["liveStreamingDetails"]?["activeLiveChatId"]?.ToString();
                }
            }
            catch
            {
                return null; // エラーが発生した場合はnullを返す
            }
            return null;
        }

        private async Task StartLiveChatRetrieval()
        {
            if (!string.IsNullOrEmpty(liveChatId))
            {
                MessageBox.Show("ライブチャットの取得を開始します。");
                while (true)
                {
                    await RetrieveChatMessages(); // チャットメッセージを取得
                    await Task.Delay(5000); // 5秒待機
                }
            }
        }

        private DateTime joinTime = DateTime.MinValue; // チャット参加時のタイムスタンプ

        private async Task RetrieveChatMessages()
        {
            if (string.IsNullOrEmpty(liveChatId)) return;

            string url = $"https://www.googleapis.com/youtube/v3/liveChat/messages?liveChatId={liveChatId}&key={apiKey}&part=snippet,authorDetails";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                MessageBox.Show($"エラー: {response.StatusCode}");
                return;
            }

            string content = await response.Content.ReadAsStringAsync();
            var json = JObject.Parse(content);
            var items = json["items"];

            if (items != null && items.HasValues)
            {
                foreach (var item in items)
                {
                    var message = item["snippet"]["displayMessage"]?.ToString();
                    var authorName = item["authorDetails"]["displayName"]?.ToString();
                    var timestamp = item["snippet"]["publishedAt"]?.ToString();

                    if (!string.IsNullOrEmpty(message) && !string.IsNullOrEmpty(timestamp))
                    {
                        // タイムスタンプを UTC としてパースし JST に変換
                        var utcTime = DateTime.ParseExact(timestamp, "yyyy-MM-ddTHH:mm:ss.fffZ", null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                        var jstTime = TimeZoneInfo.ConvertTimeFromUtc(utcTime, TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
                        var formattedTime = jstTime.ToString("yyyy-MM-dd HH:mm:ss");

                        MessageBox.Show($"Formatted time: {formattedTime}"); // フォーマットされた時間を表示

                        // 参加時刻が未設定の場合、現在のメッセージの時刻を設定
                        if (joinTime == DateTime.MinValue)
                        {
                            joinTime = jstTime; // チャット参加時刻を設定
                        }

                        // 参加時刻以降のメッセージを表示
                        if (jstTime >= joinTime)
                        {
                            var formattedMessage = $"[{formattedTime}] {authorName}: {message}";

                            if (!listChat.Items.Contains(formattedMessage))
                            {
                                listChat.Items.Add(formattedMessage);
                                SpeakMessage(message); // メッセージを読み上げ
                                CheckForParticipation(message, authorName);
                                listChat.TopIndex = listChat.Items.Count - 1; // 最後のアイテムが表示されるようにする
                            }
                        }
                    }
                }
            }
        }






        private void CheckForParticipation(string comment, string userName)
        {
            if (comment.Contains("参加希望"))
            {
                // Form2のインスタンスを取得し、参加希望者を追加
                form2.AddParticipant(userName);
                
            }
        }
        private async Task ReadCommentWithVoiceVox(string text)
        {
            try
            {
                // ComboBoxで選択したスピーカー名からIDを取得
                if (comboBox1.SelectedItem == null) return; // 未選択の場合は処理を中断
                string selectedSpeaker = comboBox1.SelectedItem.ToString();

                // speakers辞書からIDを取得
                if (!settings.speakers.TryGetValue(selectedSpeaker, out int speakerId)) // out int型で取得
                {
                    MessageBox.Show("スピーカーIDが見つかりません。");
                    return;
                }

                // 音声合成クエリを生成
                var queryResponse = await httpClient.PostAsync(
                    $"http://127.0.0.1:50021/audio_query?speaker={speakerId}&text={Uri.EscapeDataString(text)}",
                    null);

                if (!queryResponse.IsSuccessStatusCode) return;

                // 音声クエリの取得
                var queryJson = await queryResponse.Content.ReadAsStringAsync();

                // 音声データを生成
                var synthesisResponse = await httpClient.PostAsync(
                    $"http://127.0.0.1:50021/synthesis?speaker={speakerId}",
                    new StringContent(queryJson, System.Text.Encoding.UTF8, "application/json"));

                if (!synthesisResponse.IsSuccessStatusCode) return;

                // 音声データの保存
                var audioBytes = await synthesisResponse.Content.ReadAsByteArrayAsync();
                string audioFilePath = Path.Combine(Path.GetTempPath(), "comment.wav");
                File.WriteAllBytes(audioFilePath, audioBytes);

                // 音声を再生
                using (var soundPlayer = new System.Media.SoundPlayer(audioFilePath))
                {
                    soundPlayer.PlaySync(); // 音声を再生して同期する
                }

                // 再生が終わったら一時ファイルを削除
                File.Delete(audioFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"音声合成エラー: {ex.Message}");
            }
        }



        private async void SpeakMessage(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                await ReadCommentWithVoiceVox(message); // メッセージを読み上げ
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // speakers辞書からキー（キャラ名）を取得してComboBoxに設定
            comboBox1.DataSource = new BindingSource(settings.speakers.Keys.ToList(), null);
        }
    }

    public static class JsonConfigLoader
    {
        public static AppSettings LoadConfig(string filePath)
        {
            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppSettings>(json);
        }
    }

    public class AppSettings
    {
        public string VoiceBoxPath { get; set; }
        public string ApiKey { get; set; }
        public Dictionary<string, int> speakers { get; set; } // スピーカーを格納する
    }
}
