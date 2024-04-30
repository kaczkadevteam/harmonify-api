using Microsoft.AspNetCore.Diagnostics;

namespace Harmonify.Handlers
{
  public class GlobalExceptionHandler : IExceptionHandler
  {
    public async ValueTask<bool> TryHandleAsync(
      HttpContext httpContext,
      Exception exception,
      CancellationToken cancellationToken
    )
    {
      // Return false to continue with the default behavior
      // - or - return true to signal that this exception is handled
      httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
      //TODO: Use DTO response
      await httpContext.Response.WriteAsJsonAsync("Something gone wrong!", cancellationToken);
      return true;
    }
  }
}
