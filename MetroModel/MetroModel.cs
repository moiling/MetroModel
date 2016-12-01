using FindV.MetroModel.Adapter;
using FindV.MetroModel.Bean;
using FindV.MetroModel.Bean.Json;
using FindV.MetroModel.Error;
using FindV.MetroModel.Utils;
using System.Collections.Generic;

namespace FindV.MetroModel
{
    public class MetroModel
    {
        public List<MetroLine> MetroLines;

        public MetroModel(List<MetroLine> lines)
        {
            this.MetroLines = lines;
        }

        public MetroModel(V v) : this(new UIDataAdapter(v).GetMetroLines()) { }

        public class Builder
        {
            private MetroModel _model;
            private string _fileUrl;
            private IErrorCallback _callback;

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
            /// 用于抓取创建时的报错，不想看可以不使用该方法。
            /// </summary>
            /// <param name="callback">错误返回的回调，具体<seealso cref = "IErrorCallback" /></param>
            /// <returns>构造器本身</returns>
            public Builder Catch(IErrorCallback callback) 
            {
                this._callback = callback;
                return this;
            }

            /// <summary>
            /// 创建方法，将构造器获取的数据构造成MetroModel
            /// </summary>
            /// <returns>MetroModel，失败时返回null</returns>
            public MetroModel Create()
            {
                if (_fileUrl == null)
                {
                    OnError(ErrorInfoManager.BUILDER_NO_FROM);
                    return null;
                }
                // Read v from file.
                V v = FileUtils.ReadV(_fileUrl, _callback);
                // v to metroLines.
                _model = new MetroModel(new UIDataAdapter(v).GetMetroLines());
                return _model;
            }

            private void OnError(ErrorInfo error)
            {
                if (_callback != null)
                    _callback.OnError(error.Code, error.Info);
            }
        }
    }
}
