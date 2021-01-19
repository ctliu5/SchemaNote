using System;
using System.Data.SqlClient;
using System.Text;

namespace SchemaNote.Models
{
    public abstract class Error_Flag
    {
        public Error_Flag()
        {
            ErrorMessages = new StringBuilder();
        }
        public ExceResultType ResultType { get; set; }
        public StringBuilder ErrorMessages { get; set; }

        public void SetError(string message)
        {
            ResultType |= ExceResultType.Failed;
            ErrorMessages.Append(message);
        }

        public void SetError(SqlException ex)
        {
            ResultType |= ExceResultType.Exception;
            string NewLine = Environment.NewLine;
            for (int i = 0; i < ex.Errors.Count; i++)
            {
                ErrorMessages.Append(
                    "Index #" + i + NewLine +
                    "Message: " + ex.Errors[i].Message + NewLine +
                    "LineNumber: " + ex.Errors[i].LineNumber + NewLine +
                    "Source: " + ex.Errors[i].Source + NewLine +
                    "Procedure: " + ex.Errors[i].Procedure + NewLine +
                    "Severity level:" + ex.Errors[i].Number);
            }
        }

        public void SetError(Exception ex)
        {
            ResultType |= ExceResultType.Exception;
            ErrorMessages.Append(ex.Message);
        }

        public string ErrorMessagesHtmlString()
        {
            return ErrorMessages.Replace(Environment.NewLine, "<br />").ToString();
        }
    }

    public class DTO_Flag<T> : Error_Flag where T : new()
    {
        readonly Type BaseType = typeof(Error_Flag);

        public DTO_Flag(string methodName)
        {
            MethodName = methodName;
            OBJ = new T();
        }
        public string MethodName { get; private set; }
        public T OBJ { get; set; }

        public void Transfer<U>(ref U flag) where U : Error_Flag
        {
            if (flag is Error_Flag _flag)
            {
                _flag.ErrorMessages.AppendLine();
                _flag.ErrorMessages.Append(ErrorMessages);
                _flag.ResultType |= ResultType;
            }
        }
    }
}
