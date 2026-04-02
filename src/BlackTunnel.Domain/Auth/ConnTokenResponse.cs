namespace BlackTunnel.Domain.Auth; 
public class ConnTokenResponse : AuthBaseResponse {
	public ConnTokenResponse (bool isSuccess, string msg) : base(isSuccess, msg) {}
    public string? ConnectionToken { get; set; }
}
