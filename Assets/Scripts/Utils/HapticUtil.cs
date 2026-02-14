using UnityEngine;

public static class HapticUtil
{
    public static void Light()
    {
#if UNITY_IOS || UNITY_ANDROID
        Handheld.Vibrate();
#endif
    }
}
