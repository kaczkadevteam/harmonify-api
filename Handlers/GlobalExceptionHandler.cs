using Harmonify.Messages;
using Microsoft.AspNetCore.Diagnostics;

namespace Harmonify.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
  public async ValueTask<bool> TryHandleAsync(
    HttpContext httpContext,
    Exception exception,
    CancellationToken cancellationToken
  )
  {
    var response = new MessageError
    {
      Type = MessageType.UnknownError,
      ErrorMessage = "Something went wrong!"
    };

    httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
    return true;
  }
}
