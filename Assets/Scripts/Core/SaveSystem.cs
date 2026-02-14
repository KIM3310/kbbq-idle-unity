using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveSystem
{
    private const string SaveKey = "KBBQ_IDLE_SAVE";
    private const string SaveChecksumKey = "KBBQ_IDLE_SAVE_SHA256";
    private const int CurrentVersion = 2;

    public void Save(SaveData data)
    {
        if (data == null)
        {
            return;
        }

        data.version = CurrentVersion;
        data.Sanitize();
        var json = JsonUtility.ToJson(data);
        var checksum = ComputeSha256(json);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.SetString(SaveChecksumKey, checksum);
        PlayerPrefs.Save();
    }

    public SaveData Load()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            return new SaveData();
        }

        var json = PlayerPrefs.GetString(SaveKey);
        if (string.IsNullOrEmpty(json))
        {
            return new SaveData();
        }

        try
        {
            // Corruption guard: if checksum is present and doesn't match, discard the save.
            if (PlayerPrefs.HasKey(SaveChecksumKey))
            {
                var expected = PlayerPrefs.GetString(SaveChecksumKey);
                var actual = ComputeSha256(json);
                if (!string.Equals(expected, actual, System.StringComparison.OrdinalIgnoreCase))
                {
                    return new SaveData();
                }
            }

            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                data = new SaveData();
            }

            if (data.version < CurrentVersion)
            {
                // Basic migration hook. Keep it minimal: rely on default values + sanitize.
                data.version = CurrentVersion;
            }

            data.Sanitize();
            return data;
        }
        catch
        {
            return new SaveData();
        }
    }

    public void Clear()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.DeleteKey(SaveChecksumKey);
    }

    private static string ComputeSha256(string input)
    {
        if (input == null)
        {
            input = string.Empty;
        }

        using (var sha = SHA256.Create())
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            var sb = new StringBuilder(hash.Length * 2);
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
