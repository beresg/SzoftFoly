using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Threading;
using System.Runtime.Remoting.Contexts;
using System.Reflection;

namespace HFS
{
    public enum LoggingDestination { LogToFile, LogToControl, LogToAll };
    public enum LoggingComplexity { Base, Extended };
    public enum LogMessageType { Information, Error };

    public static partial class StringExtensions
    {
        private static Object Mutex = new Object();

        public static string Explain(this Exception ex)
        {
            string result = "";
            lock (Mutex)
            {
                try
                {
#if(DEBUG)
                    StackTrace stackTrace = new StackTrace(ex, true);
                    string fileNames = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileName();
                    fileNames = fileNames.Substring(fileNames.LastIndexOf(Application.ProductName));
                    Int32 lineNumber = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetFileLineNumber();
                    MethodBase methodBase = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetMethod();
                    string methodName = methodBase.Name;
                    result = "Error occured in " + fileNames + ", Method name is " + methodName + ", at line number " + lineNumber.ToString() + " , Error Message " + ex.Message;
#else
                    StackTrace stackTrace = new StackTrace(ex, true);
                    MethodBase methodBase = stackTrace.GetFrame((stackTrace.FrameCount - 1)).GetMethod();
                    string methodName = methodBase.Name;
                    result = "Error occured in " + methodName + " method. Error Message: " + ex.Message + ".";
#endif
                }
                catch (Exception)
                {
                    return "Maybe missing the PDB file(s) of the program - Error in StringExtensions - Explain";
                }
            }
            return result;
        }
        public static string Log(this string s)
        {
            Logger.Info(s, LogMessageType.Information);
            return s;
        }
        public static string LogError(this string s)
        {
            Logger.Info(s, LogMessageType.Error);
            return s;
        }
        public static Exception LogError(this Exception ex)
        {
            Logger.InfoEx(ex);
            return ex;
        }
        public static string LogError(this string s, int logLevel)
        {
            if (logLevel <= Logger.LogLevel)
            {
                Logger.Info(s, LogMessageType.Error);
            }
            return s;
        }
        public static string LogInfo(this string s, int logLevel)
        {
            if (logLevel <= Logger.LogLevel)
            {
                Logger.Info(s, LogMessageType.Information);
            }
            return s;
        }
        public static Exception Log(this Exception ex, int logLevel)
        {
            if (logLevel <= Logger.LogLevel)
            {
                Logger.InfoEx(ex);
            }
            return ex;
        }
    }
    public class Logger
    {
        private static int logLevel = 0;
        private static LoggingComplexity logComplexity = LoggingComplexity.Base;
        private static string logDirectoryName = "Log";
        private static string logFileName = "log";
        private static string dateTimeFormat = "yyyy/MM/dd HH:mm:ss";
        private static LoggingDestination destination = LoggingDestination.LogToFile;
        private static ListView logControl = null;

        public static int LogLevel { get { return logLevel; } set { logLevel = value; } }
        public static LoggingComplexity LogComplexity { get { return logComplexity; } set { logComplexity = value; } }
        public static string LogDirectoryName { get { return logDirectoryName; } set { logDirectoryName = value; } }
        public static string LogFileName { get { return string.Format("{0}{1}.log", logFileName, DateTime.Now.ToString("yyyyMMdd")); } set { logFileName = value; } }
        public static string LogTimeStampFormat { get { return dateTimeFormat; } set { dateTimeFormat = value; } }
        public static LoggingDestination LogDestination { get { return destination; } set { destination = value; } }
        public static ListView LogControl
        {
            get { return logControl; }
            set
            {
                logControl = value;
                logControl.Items.Clear();
                logControl.Columns.Clear();

                logControl.Columns.Add("Date", -1, HorizontalAlignment.Left);
                logControl.Columns.Add("Type", -1, HorizontalAlignment.Left);
                logControl.Columns.Add("Message", -2, HorizontalAlignment.Left);

                ImageList imageListSmall = new ImageList();
                imageListSmall.Images.Add(Properties.Resources.exception);
                imageListSmall.Images.Add(Properties.Resources.error);
                logControl.SmallImageList = imageListSmall;

                LogControl.UseCompatibleStateImageBehavior = false;
                LogControl.View = System.Windows.Forms.View.Details;
                LogControl.FullRowSelect = true;

                LogControl.SizeChanged += new EventHandler(LogControl_SizeChanged);
                LogControl.TopLevelControl.Disposed += new EventHandler(ParentForm_Disposed);
                LogControl.TopLevelControl.HandleCreated += new EventHandler(LogControl_HandleCreated);
            }
        }

        static void LogControl_HandleCreated(object sender, EventArgs e)
        {
            Info((object)"Logger started", LogMessageType.Information);
        }

        static void ParentForm_Disposed(object sender, EventArgs e)
        {
            Info("Logger stopped", LogMessageType.Information);
        }

        private static void LogControl_SizeChanged(object sender, EventArgs e)
        {
            LogControlSizeChanged();
        }
        private static void LogControlSizeChanged()
        {
            if (LogControl.InvokeRequired)
            {
                SizeChanged delegete = new SizeChanged(LogControlSizeChanged);
                LogControl.Invoke(delegete);
            }
            else
            {
                if (LogControl.Columns.Count >= 2) LogControl.Columns[2].Width = -2;
            }
        }

        private static System.IO.DirectoryInfo directoryInfo;
        //private static System.Diagnostics.StackTrace stackTrace;
        //private static System.Reflection.MethodBase methodBase;

        private static object Mutex = new Object();

        private delegate void AddListItemInfo(Object info, LogMessageType msgType);
        private delegate void AddListItemException(Exception info);
        private delegate void SizeChanged();

        public static void Info(Object info, LogMessageType msgType)
        {
            switch (LogDestination)
            {
                case LoggingDestination.LogToFile:
                    LogInfoToFile(info, msgType);
                    break;
                case LoggingDestination.LogToControl:
                    LogInfoToControl(info, msgType);
                    break;
                case LoggingDestination.LogToAll:
                    LogInfoToFile(info, msgType);
                    LogInfoToControl(info, msgType);
                    break;
                default:
                    break;
            }
        }
        public static void InfoEx(Exception ex)
        {
            switch (LogDestination)
            {
                case LoggingDestination.LogToFile:
                    LogExceptionToFile(ex);
                    break;
                case LoggingDestination.LogToControl:
                    LogExceptionToControl(ex);
                    break;
                case LoggingDestination.LogToAll:
                    LogExceptionToFile(ex);
                    LogExceptionToControl(ex);
                    break;
                default:
                    break;
            }
        }

        private static void LogInfoToFile(Object info, LogMessageType msgType)
        {

            if (msgType == LogMessageType.Error) info = string.Format("{0,-14}", "[ERROR]") + info;
            else if (msgType == LogMessageType.Information) info = string.Format("{0,-14}", "[INFORMATION]") + info;

            string folderName = LogDirectoryName;

            string fileName = LogDirectoryName + "/" + LogFileName;

            directoryInfo = new DirectoryInfo(folderName);

            lock (Mutex)
            {
                if (File.Exists(fileName))
                {
                    try
                    {

                        using (FileStream fileStream = new FileStream(fileName, FileMode.Append, FileAccess.Write))
                        {
                            using (TextWriter streamWriter = new StreamWriter(fileStream))
                            {
                                string val = DateTime.Now.ToString(dateTimeFormat) + " " + info.ToString();
                                streamWriter.WriteLine(val);
                                streamWriter.Flush();
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else
                {
                    try
                    {
                        directoryInfo = Directory.CreateDirectory(directoryInfo.FullName);
                        using (FileStream fileStream = File.Create(fileName))
                        {
                            using (TextWriter streamWriter = new StreamWriter(fileStream))
                            {
                                string val1 = DateTime.Now.ToString(dateTimeFormat) + " " + info.ToString();
                                streamWriter.WriteLine(val1);
                                streamWriter.Flush();
                                streamWriter.Close();
                                fileStream.Close();
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
        private static void LogExceptionToFile(Exception ex)
        {
            try
            {
                string msg = "";
                switch (logComplexity)
                {
                    case LoggingComplexity.Base:
                        msg = ex.Explain();
                        break;
                    case LoggingComplexity.Extended:
                        msg = ex.Message;
                        break;
                    default:
                        break;
                }
                LogInfoToFile(msg, LogMessageType.Error);
            }
            catch (Exception)
            {
            }
        }

        private static void LogInfoToControl(Object info, LogMessageType msgType)
        {
            ListViewItem item;
            string typeIndicator;
            Color typeColor;
            Font font;
            if (msgType == LogMessageType.Error)
            {
                typeIndicator = string.Format("{0,-14}", "[ERROR]");
                typeColor = Color.Red;
                font = new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold);
                item = new ListViewItem(DateTime.Now.ToString(LogTimeStampFormat), 0);
            }
            else if (msgType == LogMessageType.Information)
            {
                typeIndicator = string.Format("{0,-14}", "[INFORMATION]");
                typeColor = Color.Green;
                font = new Font(FontFamily.GenericSansSerif, 8);
                item = new ListViewItem(DateTime.Now.ToString(LogTimeStampFormat));
            }
            else
            {
                typeIndicator = string.Format("{0,-14}", "[UNKNOWN]");
                typeColor = Color.Black;
                font = new Font(FontFamily.GenericSansSerif, 8);
                item = new ListViewItem(DateTime.Now.ToString(LogTimeStampFormat));
            }
            item.UseItemStyleForSubItems = false;
            item.SubItems.Add(typeIndicator, typeColor, Color.White, new Font(FontFamily.GenericSansSerif, 8));
            item.SubItems.Add((string)info, Color.Black, Color.White, font);

            if (LogControl.InvokeRequired)
            {
                AddListItemInfo delegete = new AddListItemInfo(LogInfoToControl);
                LogControl.Invoke(delegete, new object[] { info, msgType });
            }
            else
            {
                LogControl.Items.Add(item);
                LogControl.EnsureVisible(LogControl.Items.Count - 1);
            }
        }
        private static void LogExceptionToControl(Exception ex)
        {
            ListViewItem item = new ListViewItem(DateTime.Now.ToString(LogTimeStampFormat), 1);
            item.UseItemStyleForSubItems = false;
            item.SubItems.Add("[EXCEPTION]", Color.Red, Color.White, new Font(FontFamily.GenericSansSerif, 8));

            string msg = "";
            switch (logComplexity)
            {
                case LoggingComplexity.Base:
                    msg = ex.Message;
                    break;
                case LoggingComplexity.Extended:
                    msg = ex.Explain();
                    break;
                default:
                    break;
            }
            item.SubItems.Add(msg, Color.Black, Color.White, new Font(FontFamily.GenericSansSerif, 8, FontStyle.Bold));

            if (LogControl.InvokeRequired)
            {
                AddListItemException delegete = new AddListItemException(LogExceptionToControl);
                LogControl.Invoke(delegete, new object[] { ex });
            }
            else
            {
                LogControl.Items.Add(item);
                LogControl.EnsureVisible(LogControl.Items.Count - 1);
            }
        }
    }
}