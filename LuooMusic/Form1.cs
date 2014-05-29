using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using HtmlAgilityPack;
using System.IO;

namespace LuooMusic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        WMPLib.WindowsMediaPlayerClass MediaPlayer;
        private void Form1_Load(object sender, EventArgs e)
        {

            MediaPlayer = new WMPLib.WindowsMediaPlayerClass();
            MediaPlayer.volume = 100;
            timer1.Enabled = true;
            label6.Text = Environment.CurrentDirectory;
            //luoo.
        }

        private void button1_Click(object sender, EventArgs e)
        {
            initData("" + listBox1.SelectedItem);
            playIndex = -1;
        }
        private LuooObject luoo;
        private void initData(string Vol)
        {
            luoo = LuooUntil.getHtmlDocument("http://www.luoo.net/music/" + Vol, 3000, null);
            if (luoo == null)
            {
                button2.Enabled = false;
                return;
            }
            if (luoo.playlist.playlist.Count > 0)
            {
                button2.Enabled = true;
            }
            ImageList imgList = new ImageList();

            imgList.ImageSize = new Size(60, 60);
            for (int i = 0; i < luoo.playlist.playlist.Count; i++)
            {
                imgList.Images.Add(Image.FromStream(
                    (HttpWebRequest.Create(luoo.playlist.playlist[i].poster_small)).GetResponse().GetResponseStream()
                    ));

            }

            listView1.SmallImageList = imgList;
            this.listView1.Items.Clear();
            this.listView1.BeginUpdate();
            for (int i = 0; i < luoo.playlist.playlist.Count; i++)
            {
                ListViewItem lvi = new ListViewItem();

                lvi.ImageIndex = i;

                lvi.Text = luoo.playlist.playlist[i].title;

                lvi.SubItems.Add(luoo.playlist.playlist[i].artist);

                lvi.SubItems.Add(luoo.playlist.playlist[i].album);

                this.listView1.Items.Add(lvi);
            }

            this.listView1.EndUpdate();
            string html = luoo.htmlDocument.DocumentNode.OuterHtml;
            if (string.IsNullOrEmpty(Vol))
            {
                listBox1.Items.Clear();
                string vols = HtmlNode.CreateNode(html).SelectSingleNode("//div[@class='current-vol']//span").InnerText;
                vols = vols.Replace("vol. ", "").Trim();
                int vol = int.Parse(vols);
                Vol = "" + vol;
                while (vol > 0)
                {
                    listBox1.Items.Add(vol);
                    vol--;
                }
            }
            label1.Text = HtmlNode.CreateNode(html).SelectSingleNode("//h1[@class='fm-title']").InnerText;
            label2.Text = HtmlNode.CreateNode(html).SelectSingleNode("//p[@class='fm-intro']").InnerText;
            label3.Text = "Vol." + Vol;
            //
            string cover = HtmlNode.CreateNode(html).SelectSingleNode("//img[@class='fm-cover']").Attributes["src"].Value;
            listView1.BackgroundImage = Image.FromStream((HttpWebRequest.Create(cover)).GetResponse().GetResponseStream());
        }
        private int playIndex = 0;
        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 1)
            {
                playIndex = listView1.SelectedIndices[0];
                MediaPlayer.URL = "" + luoo.playlist.playlist[playIndex].mp3;
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (MediaPlayer.playState == WMPLib.WMPPlayState.wmppsPlaying)
            {
                double per = (MediaPlayer.controls.currentPosition * 1.0 / MediaPlayer.currentMedia.duration);
                label5.Left = (int)(label4.Width * per);
                label4.Text = MediaPlayer.currentPositionString + " / " + MediaPlayer.currentMedia.durationString;
                if (per > 0.98)
                {
                    MediaPlayer.stop();
                    playIndex++;
                    playIndex = playIndex % luoo.playlist.playlist.Count;
                    MediaPlayer.URL = "" + luoo.playlist.playlist[playIndex].mp3;
                    for (int i = 0; i < listView1.Items.Count; i++)
                    {
                        listView1.Items[i].Selected = false;
                    }
                    listView1.Items[playIndex].Selected = true;
                }
            }

        }

        private List<DownloaderTask> dwnTasks = new List<DownloaderTask>();
        private void button2_Click(object sender, EventArgs e)
        {
            button2.Enabled = false;
            Application.DoEvents();
            string dir = label6.Text + "\\" + label3.Text + label1.Text;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            panel1.Controls.Clear();
            foreach (Play play in luoo.playlist.playlist)
            {
                Uri url = new Uri(play.mp3);
                string index = url.Segments[url.Segments.Length - 1].Replace("mp3", "");
                string errChar = "\\/:*?\"<>|";
                string fn = play.title + "-" + play.artist;
                for (int i = 0; i < errChar.Length; i++)
                    fn = fn.Replace(errChar[i].ToString(), "");
                dwnTasks.Add(new DownloaderTask(play.mp3, dir + "\\" + index + fn + ".mp3", panel1));
            }
            panel1.Visible = true;
        }

        private void panel1_DoubleClick(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }
    }
}
