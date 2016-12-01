using FindV.MetroModel.Bean.Json;
using FindV.MetroModel.Error;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace FindV.MetroModel.Utils
{
    public class FileUtils
    {
        public static V ReadV(string fileUrl)
        {
            return ReadV(fileUrl, null);
        }

        public static V ReadV(string fileUrl, OnErrorDelegate onError)
        {
            if (!File.Exists(fileUrl)) // File isn't exist.
            {
                onError?.Invoke(ErrorInfoManager.FILE_NOT_EXIST.Code, ErrorInfoManager.FILE_NOT_EXIST.Info);
                return null;
            }
            string file = ReadFile(fileUrl, onError);
            Debug.WriteLine(file);
            try
            {
                V metro = JsonConvert.DeserializeObject<V>(file);
                return metro;
            }
            catch (Exception e) // File isn't a valid json.
            {
                Debug.WriteLine(e);
                onError?.Invoke(ErrorInfoManager.JSON_NOT_VAILD.Code, ErrorInfoManager.JSON_NOT_VAILD.Info);
            }
            return null;
        }

        private static string ReadFile(string fileUrl, OnErrorDelegate onError)
        {
            StreamReader sr = new StreamReader(fileUrl, Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            return result;
        }
    }
}
