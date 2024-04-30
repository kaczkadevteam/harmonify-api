namespace Harmonify.Responses
{
  public class ResponseError<T> : Response<T>
  {
    public required string ErrorMessage { get; set; }
  }
}
