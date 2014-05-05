using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;


namespace SMT.Foundation.Log
{
    public class LogConfig
    {
        private static LogConfig _instance = null;
        // private string _errorLogConfigFilePath = ConfigurationManager.AppSettings["ErrorLogConfigFilePath"];
        private string _errorLogPath;
        private string _loggers;
        private string _logRetrieveProvider;
        private string _traceLevel;
        private string _tracePath;
        private string _connectionString;
        private string _errorEmailReceiver;
        private string _errorEmailSender;
        private string _sendErrorEmail;
        private string[] _loggerArray;
        private string[] _traceLevelArray;
        private string _smtpServer;
        private string _smtpLogin;
        private string _smtpPassword;
        private string _smtpPort;
        public string _smtVersion;
        public string _assemblyVersion;
        private LogConfig()
        {
            try
            {
                //ExeConfigurationFileMap map = new ExeConfigurationFileMap();
                //map.ExeConfigFilename = ConfigurationManager.AppSettings["LogConfigPath"];
                //map.ExeConfigFilename = AppDomain.CurrentDomain.BaseDirectory + "\\ErrorLog.config"; ;
                //Configuration config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);


                _errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"].ToString();
                _loggers = ConfigurationManager.AppSettings["LoggerProvider"].ToString();
                _loggerArray = _loggers.Split(',');
                _traceLevel = ConfigurationManager.AppSettings["TraceLevel"].ToString();
                _traceLevelArray = _traceLevel.Split(',');
                _logRetrieveProvider = ConfigurationManager.AppSettings["LogRetrieveProvider"].ToString();
                _tracePath = ConfigurationManager.AppSettings["TracePath"].ToString();
                _connectionString = ConfigurationManager.AppSettings["ConnectionString"].ToString();
                ///Email
                _errorEmailReceiver = ConfigurationManager.AppSettings["ErrorEmailReceiver"].ToString();
                _errorEmailSender = ConfigurationManager.AppSettings["ErrorEmailSender"].ToString();
                _sendErrorEmail = ConfigurationManager.AppSettings["SendErrorEmail"].ToString();
                _errorLogPath = ConfigurationManager.AppSettings["ErrorLogPath"].ToString();
                _smtpServer = ConfigurationManager.AppSettings["SMTPServer"].ToString();
                _smtpLogin = ConfigurationManager.AppSettings["SMTPLogin"].ToString();
                _smtpPassword = ConfigurationManager.AppSettings["SMTPPassword"].ToString();
                _smtpPort = ConfigurationManager.AppSettings["SMTPPort"].ToString();
                //_awbVersion = ConfigurationManager.AppSettings["AWBVersion"].ToString();
                //_assemblyVersion = ConfigurationManager.AppSettings["AssemblyVersion"].ToString();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error config file format or content is not right!The excetpition is {0}", ex.ToString()));
            }
        }
        public string SMTVersion
        {
            get
            {
                return _smtVersion;
            }
            set
            {
                _smtVersion = value;
            }
        }
        public string AssemblyVersion
        {
            get
            {
                return _assemblyVersion;
            }
            set
            {
                _assemblyVersion = value;
            }
        }
        //SMTP Attribute
        public string SMTPPort
        {
            get
            {
                return _smtpPort;
            }
            set
            {
                _smtpPort = value;
            }
        }
        public string SMTPLogin
        {
            get
            {
                return _smtpLogin;
            }
            set
            {
                _smtpLogin = value;
            }
        }
        public string SMTPPassword
        {
            get
            {
                return _smtpPassword;
            }
            set
            {
                _smtpPassword = value;
            }
        }
        public string SMTPServer
        {
            get
            {
                return _smtpServer;
            }
            set
            {
                _smtpServer = value;
            }
        }

        /// <summary>
        /// Email Property
        /// </summary>

        public string ErrorLogPath
        {
            get
            {
                return _errorLogPath;
            }
            set
            {
                _errorLogPath = value;
            }
        }
        public string ErrorEmailReceiver
        {
            get
            {
                return _errorEmailReceiver;
            }
            set
            {
                _errorEmailReceiver = value;
            }
        }
        public string ErrorEmailSender
        {
            get
            {
                return _errorEmailSender;
            }
            set
            {
                _errorEmailSender = value;
            }
        }

        public string SendErrorEmail
        {
            get
            {
                return _sendErrorEmail;
            }
            set
            {
                _sendErrorEmail = value;
            }
        }
        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                _connectionString = value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public string TracePath
        {
            get
            {
                return _tracePath;
            }
            set
            {
                _tracePath = value;
            }
        }

        public string[] Loggers
        {
            get
            {

                return _loggerArray;
            }
            set
            {
                _loggerArray = value;
            }
        }
        /// <summary>
        /// Return Trace Level 
        /// </summary>
        public string[] TraceLevel
        {
            get
            {

                return _traceLevelArray;
            }
            set
            {
                _traceLevelArray = value;
            }
        }
        public string LogRetrieveProvider
        {
            get
            {
                return _logRetrieveProvider;
            }
            set
            {
                _logRetrieveProvider = value;
            }
        }

        public static LogConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LogConfig();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

    }

}
