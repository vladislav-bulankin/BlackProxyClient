namespace BlackTunnel.Domain.Auth; 
public class LoginResponse : AuthBaseResponse {
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
}
