using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;

namespace LuooMusic
{
    class LuooUntil
    {

        public static LuooObject getHtmlDocument(string url, int timeout, Encoding defaulte)
        {
            string htmltext = null;
            LuooObject luooObj = new LuooObject();
            try
            {//
                HttpWebRequest hwr = (HttpWebRequest)HttpWebRequest.Create(url);
                //hwr.Proxy = null;
                hwr.Accept = "text/html, application/xhtml+xml, */*";
                hwr.UserAgent = "Mozilla/5.0 (js) my baiduRobot 2014 (get luoo HTML by Robot)";
                hwr.Timeout = timeout;
                using (HttpWebResponse hwrs = (HttpWebResponse)hwr.GetResponse())
                {
                    string encoding = null;
                    //根据Headers判断编码
                    string ctype = hwrs.Headers["content-type"];
                    if (ctype != null)
                    {
                        ctype = ctype.ToLower();
                        int i = ctype.IndexOf("charset");
                        if (i > 0)
                            encoding = ctype.Substring(ctype.IndexOf("=") + 1).Trim();
                    }
                    //  Encoding defaulte;
                    if (!string.IsNullOrWhiteSpace(encoding))
                    {
                        defaulte = Encoding.GetEncoding(encoding);
                    }
                    if (defaulte == null)
                    { defaulte = Encoding.GetEncoding("gbk"); encoding = defaulte.BodyName; }
                    using (Stream stream = hwrs.GetResponseStream())
                    {
                        BinaryReader br = new BinaryReader(stream);
                        MemoryStream ms = new MemoryStream();
                        BinaryWriter bw = new BinaryWriter(ms);
                        const int size = 1024;
                        int index = 0;
                        int lenght = 0;
                        byte[] datas = new byte[size];
                        while ((lenght = br.Read(datas, 0, size)) != 0)
                        {
                            bw.Write(datas, 0, lenght);
                            index = index + lenght;
                        }
                        bw.Flush(); ms.Flush();
                        byte[] htmldata = ms.GetBuffer();
                        ms.Close(); bw.Close(); br.Close();
                        //如果Headers无法判断，则从网页meta标签中读取 
                        MemoryStream msdata = new MemoryStream(htmldata);

                        using (StreamReader sr = new StreamReader(msdata, defaulte))
                        {

                            string line;
                            Regex regex = new Regex("<meta([^<]*)charset=[\"']?(?<code>[\\w-]+)", RegexOptions.IgnoreCase);
                            Match m;
                            while ((line = sr.ReadLine()) != null)
                            {
                                htmltext += (line + Environment.NewLine);
                                if (line.Contains("<meta") && regex.IsMatch(htmltext))
                                {
                                    m = regex.Match(htmltext);
                                    encoding = m.Groups["code"].Value.ToLower().Replace("\"", "").Replace("'", "").Trim();
                                }
                                if (line.Trim().StartsWith("var volPlaylist"))
                                {
                                    luooObj.volPlaylist = line.Replace("var volPlaylist", "").Replace("=", "").Replace(";", "").Replace(@"\/", "/").Trim();
                                    var js = new JavaScriptSerializer();
                                     
                                    luooObj.playlist.playlist = js.Deserialize<List<Play>>(
                                     new Regex(@"(?i)\\[uU]([0-9a-f]{4})").Replace(luooObj.volPlaylist, delegate(Match ma) { return ((char)Convert.ToInt32(ma.Groups[1].Value, 16)).ToString(); })
                                              );
                                }
                            }
                            if (encoding != defaulte.BodyName)
                            {
                                try
                                {

                                    Encoding e = Encoding.GetEncoding(encoding);
                                    htmltext = e.GetString(htmldata);//

                                }
                                catch (System.Exception ex1)
                                {
                                    MessageBox.Show(ex1.Message);
                                }
                            }
                        }
                    }
                }
            }/**/
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null;
            }
            HtmlAgilityPack.HtmlDocument htmlDocument = luooObj.htmlDocument = new HtmlAgilityPack.HtmlDocument();
            htmlDocument.LoadHtml(htmltext);
            return luooObj;
        }
    }
    class LuooObject
    {
        public HtmlAgilityPack.HtmlDocument htmlDocument;
        public string volPlaylist = "";
        public Playlist playlist = new Playlist();
    }
    class Playlist
    {
        public List<Play> playlist { get; set; }
    }

    class Play
    {
        public string id { get; set; }
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string mp3 { get; set; }
        public string poster { get; set; }
        public string poster_small { get; set; }
    }

}
