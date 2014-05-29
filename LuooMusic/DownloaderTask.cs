/*
  * Project: Code project article
  * Author: Veaceslav Macari
  * email: vmacari@gmail.com
  * Title: Windows Communication, Web Client Asynchronous file downloader
   */
using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace LuooMusic
{
    class DownloaderTask
    {
        #region Private fields
        private WebClient webClient;
        private string fileName=null;
        private Uri uriData;
        private long fileSize;
        private FileInfo fi;

        private System.Windows.Forms.Panel pDetails;
        private System.Windows.Forms.Panel pDetail;
        private System.Windows.Forms.Label associatedLabel;
        private List<System.Windows.Forms.Label> descriptionItems;

        private const int beginY = 20;
        private const int ItemHeight = 60;
        private const int DETAIL_URI = 0;
        private const int DETAIL_BYTES = 1;
        private const int DETAIL_CANCEL = 2;
        #endregion

        #region Constructors
        /// <summary>
        ///     Default constructor  
        /// </summary>
        /// <param name="uriData">the URI to the downloadable resource</param>
        public DownloaderTask (Uri uriData)
            : this(uriData, null, null)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriString"></param>
        public DownloaderTask(string uriString)
            : this(new Uri(uriString), null, null)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uriString"></param>
        /// <param name="lvDetails"></param>
        public DownloaderTask(string uriString, System.Windows.Forms.Panel pDetails)
            : this(new Uri(uriString), null, pDetails)
        {
        }
        public DownloaderTask(string uriString, string fileName, System.Windows.Forms.Panel pDetails)
            : this(new Uri(uriString), fileName, pDetails)
        { 
        }

        /// <summary>
        ///     The intialization constructor
        /// </summary>
        /// <param name="uriData">The URI to downloadable resource</param>
        /// <param name="lvDetails">The list view wich holds status info</param>
        public DownloaderTask(Uri uriData, string fileNameT, System.Windows.Forms.Panel pDetails)
        {
            #region Prepare for download
            this.uriData = uriData;
            this.fileName = fileNameT;
            if (string.IsNullOrEmpty(fileName) ) 
                fileName = this.uriData.Segments[this.uriData.Segments.Length - 1];
            webClient = new WebClient();
            webClient.Proxy = null;
            this.pDetails = pDetails;
            #endregion

            #region Assign call backs
            if (pDetails != null)
            {
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
                webClient.DownloadFileCompleted += new System.ComponentModel.AsyncCompletedEventHandler(webClient_DownloadFileCompleted);
                fi = new FileInfo(fileName);
                pDetail = new System.Windows.Forms.Panel();
                int index=pDetails.Controls.Count;
                int y = index * ItemHeight + beginY;
                pDetail.Location = new System.Drawing.Point(0,y);
                pDetail.Size=new System.Drawing.Size(pDetails.Width-25,ItemHeight);
                pDetails.Controls.Add(pDetail);
                associatedLabel  = new System.Windows.Forms.Label();
                associatedLabel.Location = new System.Drawing.Point(0,0);
                associatedLabel.Size=new System.Drawing.Size(pDetails.Width,ItemHeight/4);
                associatedLabel.AutoSize=true;
                associatedLabel.ForeColor=System.Drawing.Color.DodgerBlue;
                pDetail.Controls.Add(associatedLabel);
                associatedLabel.Text = String.Format("{0} - starting ... ", fi.Name);
                descriptionItems = new List<System.Windows.Forms.Label>();

                descriptionItems.Add(new System.Windows.Forms.Label()); // DETAIL_URI
                descriptionItems[DETAIL_URI].Location = new System.Drawing.Point(10, ItemHeight / 4 * (DETAIL_URI + 1));
                descriptionItems[DETAIL_URI].Size = new System.Drawing.Size(pDetails.Width, ItemHeight / 4);
                descriptionItems[DETAIL_URI].AutoSize = true;
                pDetail.Controls.Add(descriptionItems[DETAIL_URI]);
                descriptionItems[DETAIL_URI].Text = uriData.OriginalString;
                descriptionItems.Add(new System.Windows.Forms.Label()); // DETAIL_BYTES
                descriptionItems[DETAIL_BYTES].Location = new System.Drawing.Point(10, ItemHeight / 4 * (DETAIL_BYTES + 1));
                descriptionItems[DETAIL_BYTES].Size = new System.Drawing.Size(pDetails.Width, ItemHeight / 4);
                descriptionItems[DETAIL_BYTES].AutoSize = true;
                pDetail.Controls.Add(descriptionItems[DETAIL_BYTES]);
                descriptionItems[DETAIL_BYTES].Text = "Downloaded 0/? bytes"  ;
                descriptionItems.Add(new System.Windows.Forms.Label()); // DETAIL_CANCEL
                descriptionItems[DETAIL_CANCEL].Location = new System.Drawing.Point(pDetails.Width/2, ItemHeight / 4 * (DETAIL_CANCEL + 1));
                descriptionItems[DETAIL_CANCEL].Size = new System.Drawing.Size(pDetails.Width, ItemHeight / 4);
                descriptionItems[DETAIL_CANCEL].AutoSize = true;
                pDetail.Controls.Add(descriptionItems[DETAIL_CANCEL]);
                descriptionItems[DETAIL_CANCEL].Text = "Cancel"  ;
                descriptionItems[DETAIL_CANCEL].BackColor = System.Drawing.Color.Silver;
                descriptionItems[DETAIL_CANCEL].ForeColor = System.Drawing.Color.Blue;
                descriptionItems[DETAIL_BYTES].ForeColor = System.Drawing.Color.Navy; 
                 
            }
            #endregion

            #region Start file download
            webClient.DownloadFileAsync(uriData, fileName, associatedLabel);
            #endregion
        }
        #endregion

        #region Downloader callback
        /// <summary>
        ///     Call back for download complete.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Download complete status</param>
        private void webClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            #region Check if additional info. is required
            if (associatedLabel == null)
            {
                return;
            }
            #endregion

            #region Process error state
            if (e.Error == null)
            {
                associatedLabel.Text = String.Format("{0} - completed", fi.Name);
                descriptionItems[DETAIL_BYTES].ForeColor = System.Drawing.Color.Green;
                descriptionItems[DETAIL_BYTES].Text = String.Format("File size - {0}", GetHumanReadableFileSize(fileSize));
            }
            else if (e.Cancelled == false && e.Error != null)
            {
                associatedLabel.Text = String.Format("{0} - Failed", fi.Name);
                descriptionItems[DETAIL_BYTES].Text = e.Error.Message;
                descriptionItems[DETAIL_BYTES].ForeColor = System.Drawing.Color.Red;
            }
            #endregion
            
            #region Process canceled download
            if (e.Cancelled == true)
            {
                associatedLabel.Text = String.Format("{0} - Canceled", fi.Name);
                descriptionItems[DETAIL_BYTES].ForeColor = System.Drawing.Color.DarkGray;
            }
            #endregion

            descriptionItems[DETAIL_CANCEL].Text = "Remove";
        }

        /// <summary>
        ///     A download progress is reported
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">the download progress information</param>
        private void webClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            #region Check if additional info. is required
            if (associatedLabel == null)
            {
                return;
            }
            #endregion

            #region Show progress info 
            fileSize = e.TotalBytesToReceive;
            string ReceivedTemp=GetHumanReadableFileSize(e.BytesReceived);
            if (!ReceivedTemp.Equals(ReceivedStr))
            { 
            ReceivedStr = ReceivedTemp;
            descriptionItems[DETAIL_BYTES].Text = String.Format("Downloaded {0}/{1}", ReceivedStr, GetHumanReadableFileSize(fileSize));
            }
            Random r = new Random(new Guid().GetHashCode());
            if (e.ProgressPercentage - ProgressPercentage > 3 + r.Next(7) || e.ProgressPercentage==100)
            {
                associatedLabel.Text = String.Format("{0} - {1}%", fi.Name, e.ProgressPercentage);
                 ProgressPercentage = e.ProgressPercentage;
            }
            #endregion
        }
        #endregion

        private string ReceivedStr;
        private int ProgressPercentage=0;

        #region Helper classes
        /// <summary>
        ///     Convert bytes to a human readable format
        /// </summary>
        /// <param name="fileSize">the number bytes</param>
        /// <returns>apropriate amount of bytes (Gb, Mb, Kb, bytes)</returns>
        private string GetHumanReadableFileSize(long fileSize)
        {
            #region Gb
            if ((fileSize / (1024 * 1024 * 1024)) > 0)
            {

                return String.Format("{0} Gb", (double)Math.Round((double)(fileSize / (1024 * 1024 * 1024)), 2));
            }
            #endregion

            #region Mb
            if ((fileSize / (1024 * 1024)) > 0)
            {
                return String.Format("{0} Mb", (double)Math.Round((double)(fileSize / (1024 * 1024)), 2));
            }
            #endregion

            #region Kb
            if ((fileSize / 1024) > 0)
            {
                return String.Format("{0} Kb", (double)Math.Round((double)(fileSize /1024), 2));
            }
            #endregion

            #region Bytes
            return String.Format("{0} b", fileSize);
            #endregion
        }
        #endregion

        #region Component event handler
        /// <summary>
        ///     Process the item click event. Cancel or remove a download. 
        /// </summary>
        /// <param name="item">The item which was clicked</param>
        /// <returns>True if the clicked item is managed by this component</returns>
        public bool ItemClicked (System.Windows.Forms.Label item)
        {

            if (associatedLabel != null && descriptionItems != null && descriptionItems.Count > 0 && item == descriptionItems[DETAIL_CANCEL])
            {
                if (webClient.IsBusy == true)
                {
                    webClient.CancelAsync();
                } else 
                {
                    webClient.CancelAsync();
                    pDetail.Visible = false;
                }

                return true;
            }

            return false;
        }
        #endregion
    }
}
