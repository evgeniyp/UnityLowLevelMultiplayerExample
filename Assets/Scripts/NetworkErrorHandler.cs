using UnityEngine;
using UnityEngine.Networking;

internal class NetworkErrorHandler
{
    public static bool IsOk(byte error)
    {
        NetworkError e = (NetworkError)error;
        if (e == NetworkError.Ok)
        {
            return true;
        }
        else
        {
            Debug.Log("Network error: " + e.ToString());
            return false;
        }
    }
}

