using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;

namespace YouTubeLive読み上げ君
{
    public partial class Form2 : Form
    {
        private PrivateFontCollection privateFonts = new PrivateFontCollection();

        public Form2()
        {
            InitializeComponent();
            InitializeForm();
            LoadCustomFont();

            listBoxParticipants.BackColor = System.Drawing.Color.Black; // ListBoxの背景色を黒に設定
            listBoxParticipants.ForeColor = System.Drawing.Color.White; // テキストの色を白に設定
        }
        private void InitializeForm()
        {
            this.BackColor = Color.Black; // 背景色を黒に設定
            listBoxParticipants.Items.Add("参加希望者リスト"); // 見出しを追加
            listBoxParticipants.ForeColor = Color.White; // テキストの色を白に設定

            // フォントを設定
            listBoxParticipants.Font = new Font("Microsoft Sans Serif", 24, FontStyle.Bold); // フォントサイズを14、太字に設定
        }


        private void LoadCustomFont()
        {
            string fontPath = @"C:\Users\ryuya\AppData\Local\Microsoft\Windows\Fonts\ikamodoki1.ttf"; // フォントのパス
            privateFonts.AddFontFile(fontPath);
            listBoxParticipants.Font = new Font(privateFonts.Families[0], 12, FontStyle.Regular);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            // クリックイベントを無視する
            e = null;
            base.OnMouseDown(e);
        }

        // 「参加希望」のユーザー名を追加するメソッド
        public void AddParticipant(string userName)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AddParticipant), userName);
            }
            else
            {
                if (!listBoxParticipants.Items.Contains(userName)) // 重複チェック
                {
                    listBoxParticipants.Items.Add(userName);
                }
            }
        }

        private void Form2_Load(object sender, EventArgs e)
        {

        }
    }
}
