using FindV.MetroModel.Bean.Json;
using FindV.MetroModel.Error;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;


namespace FindV.MetroModel.Utils
{
    public class FileUtils
    {
        public static V ReadV(string fileUrl)
        {
            return ReadV(fileUrl, null);
        }

        public static V ReadV(string fileUrl, IErrorCallback callback)
        {
            if (!File.Exists(fileUrl)) // File isn't exist.
            {
                if (callback != null)
                    callback.OnError(ErrorInfoManager.FILE_NOT_EXIST.Code, ErrorInfoManager.FILE_NOT_EXIST.Info);
                return null;
            }
            string file = ReadFile(fileUrl, callback);
            try
            {
                V metro = JsonConvert.DeserializeObject<V>(file);
                return metro;
            }
            catch (Exception e) // File isn't a valid json.
            {
                Debug.WriteLine(e);
                if (callback != null)
                    callback.OnError(ErrorInfoManager.JSON_NOT_VAILD.Code, ErrorInfoManager.JSON_NOT_VAILD.Info);
            }
            return null;
        }

        private static string ReadFile(string fileUrl, IErrorCallback callback)
        {
            StreamReader sr = new StreamReader(fileUrl, System.Text.Encoding.Default);
            string result = sr.ReadToEnd();
            sr.Close();
            return result;
        }
    }
}
