namespace BlackTunnel.Domain.Auth; 
public class AuthBaseResponse {
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public AuthBaseResponse () { }
    public AuthBaseResponse (bool isSuccess, string msg) {
        this.IsSuccess = isSuccess;
        this.Message = msg;
    }
}
