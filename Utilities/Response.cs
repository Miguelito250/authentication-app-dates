using AuthenticationService.Interfaces;

namespace AuthenticationService.Utilities;
public class Response : IResponseStructure
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public object? Data { get; set; }

    public Response(bool success, string message, object? data = null)
    {
        Success = success;
        Message = message;
        Data = data;
    }
}
