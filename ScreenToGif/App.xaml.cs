﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ScreenToGif.Properties;
using ScreenToGif.Util;
using ScreenToGif.Util.Enum;
using ScreenToGif.Util.Writers;
using ScreenToGif.Windows;
using ScreenToGif.Windows.Other;

namespace ScreenToGif
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            #region Unhandled Exceptions

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            #endregion

            #region Arguments

            try
            {
                if (e.Args.Length > 0)
                {
                    //TODO: Watch for Args...                
                }
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();

                LogWriter.Log(ex, "Generic Exception - Arguments");
            }

            #endregion

            #region Language

            try
            {
                if (!Settings.Default.Language.Equals("auto"))
                {
                    Thread.CurrentThread.CurrentCulture = new CultureInfo(Settings.Default.Language);
                    Thread.CurrentThread.CurrentUICulture = new CultureInfo(Settings.Default.Language);
                }
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();

                LogWriter.Log(ex, "Language Settings Exception");
            }

            #endregion

            try
            {
                #region Startup

                if (Settings.Default.StartUp == 0)
                {
                    var startup = new Startup();
                    Current.MainWindow = startup;
                    startup.ShowDialog();
                }
                else if (Settings.Default.StartUp == 3)
                {
                    var edit = new Editor();
                    Current.MainWindow = edit;
                    edit.ShowDialog();
                }
                else
                {
                    var editor = new Editor();
                    List<FrameInfo> frames = null;
                    ExitAction exitArg = ExitAction.Exit;
                    bool? result = null;

                    #region Recorder or Webcam

                    if (Settings.Default.StartUp == 1)
                    {
                        var rec = new Recorder(true);
                        result = rec.ShowDialog();
                        exitArg = rec.ExitArg;
                        frames = rec.ListFrames;
                    }
                    else if (Settings.Default.StartUp == 2)
                    {
                        var web = new Windows.Webcam();
                        result = web.ShowDialog();
                        exitArg = web.ExitArg;
                        frames = web.ListFrames;
                    }

                    #endregion

                    if (result.HasValue && result.Value)
                    {
                        #region If Close

                        Environment.Exit(0);

                        #endregion
                    }
                    else if (result.HasValue)
                    {
                        #region If Backbutton or Stop Clicked

                        if (exitArg == ExitAction.Recorded)
                        {
                            editor.ListFrames = frames;
                            Current.MainWindow = editor;
                            editor.ShowDialog();
                        }

                        #endregion
                    }
                }

                #endregion
            }
            catch (Exception ex)
            {
                var errorViewer = new ExceptionViewer(ex);
                errorViewer.ShowDialog();
                LogWriter.Log(ex, "Generic Exception - Root");
            }
        }

        private void App_OnExit(object sender, ExitEventArgs e)
        {
            //TODO: Save all settings, stop all encoding.
        }

        #region Exception Handling

        private void App_OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogWriter.Log(e.Exception, "On Dispacher Unhandled Exception - Unknow");

            var errorViewer = new ExceptionViewer(e.Exception);
            errorViewer.ShowDialog();

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;

            if (exception == null) return;

            LogWriter.Log(exception, "Current Domain Unhandled Exception - Unknow");

            var errorViewer = new ExceptionViewer(exception);
            errorViewer.ShowDialog();
        }

        #endregion
    }
}
