using Azure;

namespace AuthenticationService.Interfaces;
public interface IResponseStructure{
    public bool Success {get; set;}
    public string Message {get; set;}
    public Object? Data {get; set;}
}