namespace FindV.MetroModel.Error
{
    public class ErrorInfo
    {
        public int Code { set; get; }
        public string Info { set; get; }

        public ErrorInfo(int code, string info)
        {
            this.Code = code;
            this.Info = info;
        }
    }
}
