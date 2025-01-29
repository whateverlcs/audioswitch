using System.IO;

namespace AudioSwitch.Controllers
{
    public class ControlLogs
    {
        public void LogException(string exception, string localException)
        {
            string err = $"{DateTime.Now} | Local: {localException} | Exception: {exception}\n\n";
            File.AppendAllText(@$"{Directory.GetCurrentDirectory()}\Errors.txt", err);
        }
    }
}