namespace BlackTunnel.Core.Exceptions; 
public class NetworkLostException : Exception {
	public NetworkLostException (string reason) : base(reason) {}
}
