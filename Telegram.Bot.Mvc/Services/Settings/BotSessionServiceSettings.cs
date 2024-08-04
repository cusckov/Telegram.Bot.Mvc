using System.IO;
using System;

namespace Telegram.Bot.Mvc.Services.Settings
{
    public class BotSessionServiceSettings
    {
        public string CertificateFilePath { get; set; }
        public string PublicBaseUrl  { get; set; }
        public bool RegisterCertificate { get; set; }
    }
}