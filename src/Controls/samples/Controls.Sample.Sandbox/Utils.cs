#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Maui.Controls.Sample
{
    public static class Utils
    {
        public static void ShowTextInput(string LabelText, object TextValue, Object DataObject, Action<string> callback, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerName = null)
        {
            try
            {
                if (TextValue is null)
                {
                    TextValue = string.Empty;
                }

                // var inputPage = new PopupTextInputPage(LabelText, TextValue.ToString(), DataObject, callback);

                // // Make sure this window is not already open
                // if (Mopups.Services.MopupService.Instance.PopupStack.Count == 0 || !(Mopups.Services.MopupService.Instance.PopupStack[(Mopups.Services.MopupService.Instance.PopupStack.Count - 1)].GetType() == typeof(PopupTextInputPage)))
                // {
                //     //await Mopups.Services.MopupService.Instance.PushAsync(inputPage, false);
                //     App.Current.Dispatcher.DispatchAsync(async () => { await Mopups.Services.MopupService.Instance.PushAsync(inputPage, false); });
                // }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

#pragma warning disable CA1815 // Override equals and operator equals on value types
        public struct TextValidationResult
#pragma warning restore CA1815 // Override equals and operator equals on value types
        {
            public bool InError;
            public string ErrorMessage;
            public string NewTextValue;
            public Color RequiredIndColor;
        }

        public static TextValidationResult ValidateQuestionTextInput(string textValue, Question q)
        {
            TextValidationResult vResult = new TextValidationResult() { InError = false, ErrorMessage = "", NewTextValue = textValue, RequiredIndColor = Colors.Transparent };

            try
            {
                if (textValue == null)
                {
                    return vResult;
                }

                string newTextValue = string.Empty;

                // remove line breaks
                newTextValue = Utils.RemoveLineBreaks(textValue);

                vResult.NewTextValue = newTextValue;
                vResult.InError = false;
                vResult.ErrorMessage = string.Empty;

                return vResult;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return vResult;
            }

        }

        public static string RemoveLineBreaks(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\t|\n|\r", string.Empty);
        }

        //------ Error Log START --------

        private static readonly object objectLock = new object();
        public static void WriteErrLog(Exception ex = null, string msg = null, [CallerFilePath] string callerFilePath = null, [CallerMemberName] string callerName = null)
        {

            string strFmtCaller = "";
            string strFmtCallerPath = "";
            string strFmtError = "";
            string strError = "";

            if (callerName == null)
            {
                callerName = "";
            }
            if (msg == null)
            {
                msg = "";
            }

            if (callerName != "")
            {
                if (callerFilePath != null)
                {
                    var pathArray = callerFilePath.Split('\\');
                    var fileName = pathArray[(pathArray.Length - 1)];

                    var fileNameArray = fileName.Split('.');
                    strFmtCallerPath = fileNameArray[0];
                }

                strFmtCaller = "METHOD: " + (strFmtCallerPath != "" ? strFmtCallerPath + "." : "") + callerName + "(), ";
            }
            else
            {
                strFmtCaller = "METHOD: [unknown], ";
            }

            if (msg != "")
            {
                strFmtError = "Error: " + msg;
            }

            strError = strFmtCaller + strFmtError;
            LogError(strError, ex);
        }

        public static async Task PurgeErrorLog()
        {
            await Task.Run(() =>
            {
                var fullPath = GetLogFilePath();
                try
                {
                    if (File.Exists(fullPath))
                    {
                        File.Delete(fullPath);
                    }
                }
                catch (IOException ex)
                {
                    Debug.WriteLine("ErrorLogger DeleteFile() error = " + ex.Message);
                }
                SetupLogFiles();
            });
        }

        private static string GetLogFilePath()
        {
            var localFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Resources);
            return Path.Combine(localFolder, "CVIssueError");
        }

        public static void SetupLogFiles()
        {
            var fullPath = GetLogFilePath();
            if (!File.Exists(fullPath))
            {
                SaveToFile("");
            }
        }
        private static bool SaveToFile(string message)
        {
            try
            {
                var fullPath = GetLogFilePath();
                File.WriteAllText(fullPath, message);
                return true;
            }
            catch (System.IO.IOException ex)
            {
                // logging to file failed, we can't do anything better than return false 
                Debug.WriteLine("ErrorLogger SaveToFile() error = " + ex.Message);
                return false;
            }
        }

        public static bool AppendToFile(string filename, string message)
        {
            try
            {
                var fullPath = GetLogFilePath();
                File.AppendAllText(fullPath, message + "\r\n");
                return true;
            }
            catch (System.IO.IOException ex)
            {
                // logging to file failed, we can't do anything better than return false 
                Debug.WriteLine("ErrorLogger AppendToFile() error = " + ex.Message);
                return false;
            }
        }

        public static void LogError(string message, Exception ex)
        {
            lock (objectLock)
            {
                LogRecord(message, ex);
            }
        }

        private static void LogRecord(string message, Exception exc = null)
        {
            string rec = "";
            if (exc == null)
            {
                rec = string.Format("{0} - {1}\r\n", DateTime.Now, message);
            }
            else
            {
                if (message.Length > 2)
                {
                    if (message.Substring(message.Length - 2) != ", ")
                    {
                        message = message + ", ";
                    }
                }

                var exStackTraceMsg = "";
                if (exc.StackTrace != null)
                {
                    exStackTraceMsg = exc.StackTrace.ToString();
                }
                else
                {
                    if (exc.InnerException != null)
                    {
                        exStackTraceMsg = exc.InnerException.ToString();
                    }
                }

                rec = string.Format("{0} - {1}EXCEPTION: {2}\r\n   STACK TRACE: {3}\r\n", DateTime.Now, message, exc.Message, exStackTraceMsg);
            }

            try
            {
                var fullPath = GetLogFilePath();
                File.AppendAllText(fullPath, rec + "\r\n");
            }
            catch (System.IO.IOException ex)
            {
                // logging to file failed, we can't do anything better than return false 
                Debug.WriteLine("ErrorLogger AppendToFile() error = " + ex.Message);
            }
        }

        public static string LoadFromFile()
        {
            try
            {
                var fullPath = GetLogFilePath();
                if (File.Exists(fullPath))
                {
                    return File.ReadAllText(fullPath);
                }
                return "";
            }
            catch (Exception ex)
            {
                // file read failed, return empty string
                Debug.WriteLine("ErrorLogger LoadFromFile() error = " + ex.Message);
                return "";
            }
        }


        //------ Error Log END--------


        public static async Task<string> DummyAsyncTask1()
        {
            string result = "";

            await Task.Run(async () =>
            {
                for (var x = 0; x < 3; x++)
                {
                    await Task.Delay(50);
                }
            });

            await Task.Run(async () =>
            {
                for (var x = 0; x < 3; x++)
                {
                    if (x == 1)
                    {
                        result = await DummyAsyncTask1A();
                    }
                }
            });
            return result;
        }

        public static async Task<string> DummyAsyncTask1A()
        {
            await Task.Run(async () =>
            {
                for (var x = 0; x < 3; x++)
                {
                    await Task.Delay(30);
                }
            });
            return "ok";
        }

        public static async Task<string> DummyAsyncTask2()
        {
            string result = "";

            await Task.Run(async () =>
            {
                for (var x = 0; x < 5; x++)
                {
                    await Task.Delay(50);
                    if (x == 3)
                    {
                        await DummyAsyncTask1A();
                    }
                    if (x == 4)
                    {
                        result = "ok";
                    }
                }
            });
            return result;
        }


        public static async void DummyAsyncTask3()
        {

            await Task.Run(async () =>
            {
                for (var x = 0; x < 10; x++)
                {
                    await Task.Delay(500);
                }
            });
        }


    }
}
