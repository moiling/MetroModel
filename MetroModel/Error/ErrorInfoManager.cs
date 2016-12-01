namespace FindV.MetroModel.Error
{
    /// <summary>
    /// -------------------------------------------
    /// |  code  |              info              |
    /// -------------------------------------------
    /// |   0    |         BUILDER_NO_FROM        |
    /// |   1    |         FILE_NOT_EXIST         |
    /// |   2    |         JSON_NOT_VAILD         |
    /// |  10086 |          UNKNOEW_ERROR         |
    /// -------------------------------------------
    /// </summary>
    public class ErrorInfoManager
    {
        public static ErrorInfo BUILDER_NO_FROM
        {
            get
            {
                return new ErrorInfo(0, "Error: You must use Builder.From(string fileUrl) to add fileUrl.");
            }
        }

        public static ErrorInfo FILE_NOT_EXIST
        {
            get
            {
                return new ErrorInfo(1, "Error: File not exist.");
            }
        }
        public static ErrorInfo JSON_NOT_VAILD
        {
            get
            {
                return new ErrorInfo(2, "Error: Json is not vaild.");
            }
        }
    }
}
