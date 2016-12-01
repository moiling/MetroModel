namespace FindV.MetroModel.Error
{
    public interface IErrorCallback
    {
        void OnError(int code, string info);
    }
}
