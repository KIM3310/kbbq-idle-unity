using UnityEngine;

[CreateAssetMenu(menuName = "KBBQ/Api Config")]
public class ApiConfig : ScriptableObject
{
    public string baseUrl = "https://api.example.com";
    public string region = "KR";
    public string hmacSecret = "CHANGE_ME"; // Placeholder; replace in production.
    public int timeoutSeconds = 10;
    public bool enableNetwork = false;
    public bool allowInEditor = false;
}
