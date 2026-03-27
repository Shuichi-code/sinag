namespace Sinag.App.Services;

public class ConnectivityService
{
    public bool IsOnline => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
}
