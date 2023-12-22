using AngleSharp.Dom;
using CoreTweet;
using DiffPlex.DiffBuilder;
using DiffPlex;
using Microsoft.Office.Interop.Excel;
using ShysterWatch.MembershipDatabases.CNHC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using ShysterWatch.MisleadingClaimFinder;
using AngleSharp.Html;
using System.Web.UI.WebControls.WebParts;
using System.Web;
using System.Diagnostics;
using OpenAI.ObjectModels.RequestModels;
using System.Text.RegularExpressions;
using System.Threading;
using OpenQA.Selenium;
using System.Runtime.InteropServices;

namespace ShysterWatch
{
    public partial class FalseClaimFinderForm : Form
    {
        public FalseClaimFinderForm()
        {
            InitializeComponent();
        }

        private void BtnProcessClaimsWithAi_Click(object sender, EventArgs e)
        {
            _ = ProcessClaimsWithAi();
        }

        private async Task<bool> ProcessClaimsWithAi()
        {

            await Task.Run(() => Sys.CurrentGroup.AiAnalysisHelper.ProcessClaimsWithAi(SendMessage));

            return true;



        }




        public void SendMessage(string message)
        {
            // Check if the call is from a different thread
            if (debugOutput.InvokeRequired)
            {
                // Use a delegate to call SendMessage in a thread-safe manner
                _ = debugOutput.Invoke(new Action<string>(SendMessage), message);
            }
            else
            {
                // This is the safe place to modify the control
                debugOutput.AppendText($"{message}\r\n");
                
            }
        }




        private void ComplaintGenerator_Load(object sender, EventArgs e)
        {


        }





        private DateTime StartTime = DateTime.Now;
        public void ResetTimeLog()
        {
            StartTime = DateTime.Now;
        }
        public void SendTimeLogMessage(string message)
        {
            var timeTaken = DateTime.Now.Subtract(StartTime);
            SendMessage($"{message} : {timeTaken.Hours}h {timeTaken.Minutes}m {timeTaken.Seconds}s / {DateTime.Now}");
        }


        private void BtnCreateReport_Click(object sender, EventArgs e)
        {
            Task.Run(() => Sys.CurrentGroup.AiAnalysisHelper.CreateReport(SendMessage));
        }

        private void BtnCreateSample_Click(object sender, EventArgs e)
        {
            Task.Run(() => Sys.CurrentGroup.AiAnalysisHelper.ExtractSampleForManualCheck(30, SendMessage));
        }

        private void BtnExportData_Click(object sender, EventArgs e)
        {
            Task.Run(() => Sys.CurrentGroup.AiAnalysisHelper.ExportData(SendMessage));
        }
    }
}
