using Microsoft.AspNetCore.Diagnostics;

namespace Harmonify.ErrorHandlers
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
      await httpContext.Response.WriteAsJsonAsync("Something gone wrong!");
      return true;
    }
  }
}
