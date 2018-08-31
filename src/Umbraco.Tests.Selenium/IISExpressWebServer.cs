﻿using System;
using System.Diagnostics;
using Holf.AllForOne;
using TestStack.Seleno.Configuration.WebServers;

namespace Umbraco.Tests.Selenium
{
    /// <summary>
    /// see http://www.michael-whelan.net/testing-mvc-application-with-iis-express-webdriver/
    /// to run selenium tests you have to fire up your own iisexpress instance of the site first 
    /// </summary>
    public class IisExpressWebServer
    {
        private static WebApplication _application;
        private static Process _webHostProcess;

        public IisExpressWebServer(WebApplication application)
        {
            if (application == null)
                throw new ArgumentNullException("The web application must be set.");
            _application = application;
        }

        public void Start()
        {
            var webHostStartInfo = InitializeIisExpress(_application);
            _webHostProcess = Process.Start(webHostStartInfo);
            _webHostProcess.TieLifecycleToParentProcess();
        }

        public void Stop()
        {
            if (_webHostProcess == null)
                return;
            if (!_webHostProcess.HasExited)
                _webHostProcess.Kill();
            _webHostProcess.Dispose();
        }

        public string BaseUrl
        {
            get { return string.Format("http://localhost:{0}", _application.PortNumber); }
        }

        private static ProcessStartInfo InitializeIisExpress(WebApplication application)
        {
            // todo: grab stdout and/or stderr for logging purposes?
            var key = Environment.Is64BitOperatingSystem ? "programfiles(x86)" : "programfiles";
            var programfiles = Environment.GetEnvironmentVariable(key);

            var startInfo = new ProcessStartInfo
            {
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false,
                Arguments = String.Format("/path:\"{0}\" /port:{1}", application.Location.FullPath, application.PortNumber),
                FileName = string.Format("{0}\\IIS Express\\iisexpress.exe", programfiles)
            };

            foreach (var variable in application.EnvironmentVariables)
                startInfo.EnvironmentVariables.Add(variable.Key, variable.Value);

            return startInfo;
        }
    }
}
