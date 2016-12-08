using FindV.MetroModel.Adapter;
using FindV.MetroModel.Bean;
using FindV.MetroModel.Bean.Json;
using FindV.MetroModel.Error;
using FindV.MetroModel.Utils;
using System.Collections.Generic;
using System.Linq;

namespace FindV.MetroModel
{
    public class MetroModel
    {
        public List<MetroLine> MetroLines;
        public List<Station> Stations;
        public string Name { get; set; }
        private Thinker _thinker;

        MetroModel(List<MetroLine> lines, List<Station> stations, string name)
        {
            this.MetroLines = lines;
            this.Stations = stations;
            this.Name = name;
            this._thinker = new Thinker(this); // Only one thinker.
        }

        MetroModel(V v, string name) : this(new UIDataAdapter(v).GetMetroLines(),
            new UIDataAdapter(v).GetAllStations(), name) { }

        /// <summary>
        /// 获取所有换乘点
        /// </summary>
        /// <returns>所有换乘点</returns>
        public List<Station> GetAllTransferStations()
        {
            List<Station> result = new List<Station>();
            foreach (MetroLine l in MetroLines)
                result = result.Union(l.GetTransferStations()).ToList<Station>();
            return result;
        }

        /// <summary>
        /// 获取两点之间的最短路径 (站点数组)
        /// </summary>
        /// <param name="start">起点的id</param>
        /// <param name="end">终点的id</param>
        /// <returns>最短路径所经过的所有站点</returns>
        public List<Station> ShortestPath(int start, int end)
        {
            return ShortestPath(start, end, 0);
        }

        /// <summary>
        /// 获取两点之间的最短路径 (站点数组)，算上换乘浪费的时间
        /// </summary>
        /// <param name="start">起点的id</param>
        /// <param name="end">终点的id</param>
        /// <param name="transferTime">换乘浪费的时间（相当于几站）</param>
        /// <returns>最短路径所经过的所有站点</returns>
        public List<Station> ShortestPath(int start, int end, int transferTime)
        {
            return _thinker.ThinkShortestPath(start, end, transferTime);
        }

        /// <summary>
        /// 将站点数组转换为标识字符串
        /// </summary>
        /// <param name="stations">要转换的站点数组</param>
        /// <returns>转换完成的站点字符串</returns>
        public string Stations2Str(List<Station> stations)
        {
            string result = "";
            int currentLine = -1;
            if (stations == null)
                return result;

            for (int i = 0; i < stations.Count; i++)
            {
                result += stations[i].Name;
                if (i == stations.Count - 1)
                    break;

                if (stations[i].IsTransfer)
                {
                    int startLine = -1, endLine = -1;
                    for (int j = i; j > 0; j--)
                        if (!stations[j].IsTransfer)
                        {
                            startLine = stations[j].PassLineIds[0];
                            break;
                        }
                    for (int j = i; j < stations.Count; j++)
                        if (!stations[j].IsTransfer)
                        {
                            endLine = stations[j].PassLineIds[0];
                            break;
                        }
                    if (startLine != -1 && endLine != -1 && startLine != endLine && currentLine != endLine)
                        result += " [换乘" + MetroLines.Find((MetroLine l) => l.Id == endLine).Name + "]";
                    currentLine = endLine;
                }
                result += "→";
            }
            return result;
        }

        /// <summary>
        /// 最佳遍历
        /// </summary>
        /// <param name="start">起点站id</param>
        /// <returns>整个遍历过程经过的站点数组</returns>
        public List<Station> GoThrough(int start)
        {
            return GoThrough(start, 0);
        }

        /// <summary>
        /// 最佳遍历，算上换乘浪费的时间
        /// </summary>
        /// <param name="start">起点站id</param>
        /// <param name="transferTime">换乘浪费的时间（相当于几站）</param>
        /// <returns>整个遍历过程经过的站点数组</returns>
        public List<Station> GoThrough(int start, int transferTime)
        {
            return _thinker.ThinkGoThrough(start, transferTime);
        }

        public class Builder
        {
            private MetroModel _model;
            private string _fileUrl;
            private string _name;
            private OnErrorDelegate _onError;

            /// <summary>
            /// 必须使用的方法，指定该MetroModel的数据来源 (地铁数据文件地址)。
            /// </summary>
            /// <param name="fileUrl">保存地铁数据文件路径</param>
            /// <returns>构造器本身</returns>
            public Builder From(string fileUrl)
            {
                this._fileUrl = fileUrl;
                return this;
            }

            /// <summary>
            /// 设置城市名字
            /// </summary>
            /// <param name="name">城市名字</param>
            /// <returns>构造器本身</returns>
            public Builder Name(string name)
            {
                this._name = name;
                return this;
            }

            /// <summary>
            /// 用于抓取创建时的报错，不想看可以不使用该方法。
            /// </summary>
            /// <param name="onError">错误返回的委托，具体<seealso cref = "OnErrorDelegate" /></param>
            /// <returns>构造器本身</returns>
            public Builder Catch(OnErrorDelegate onError) 
            {
                this._onError = onError;
                return this;
            }

            /// <summary>
            /// 创建方法，将构造器获取的数据构造成MetroModel
            /// </summary>
            /// <returns>MetroModel，失败时返回null</returns>
            public MetroModel Build()
            {
                if (_fileUrl == null)
                {
                    OnError(ErrorInfoManager.BUILDER_NO_FROM);
                    return null;
                }
                // Read v from file.
                V v;
                if ((v = FileUtils.ReadV(_fileUrl, _onError)) == null)
                    return null;
                // v to metroLines.
                _model = new MetroModel(new UIDataAdapter(v).GetMetroLines(),
                     new UIDataAdapter(v).GetAllStations(), _name);
                return _model;
            }

            private void OnError(ErrorInfo error)
            {
                _onError?.Invoke(error.Code, error.Info);
            }
        }
    }
}
