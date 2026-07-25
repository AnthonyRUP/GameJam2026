using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace Countdown.Data
{
    // Uses UnityWebRequest rather than File.ReadAllText because StreamingAssets is served
    // over HTTP in a WebGL build, where plain file I/O silently fails.
    public static class CodexLoader
    {
        private const string FileName = "countdown-codex.json";

        public static IEnumerator Load(Action<CountdownCodex> onLoaded)
        {
            string path = Path.Combine(Application.streamingAssetsPath, FileName);
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_WEBGL)
            // In the Editor (regardless of active build target) and in Standalone/iOS builds,
            // streamingAssetsPath is a raw filesystem path - UnityWebRequest.Get() fails to
            // resolve it ("Cannot connect to destination host") without an explicit file://
            // scheme. Android already returns a jar:/content: URI, and an actual WebGL build
            // serves StreamingAssets over HTTP via a relative URL - both need the raw path as-is.
            path = "file://" + path;
#endif
            using var req = UnityWebRequest.Get(path);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Failed to load {FileName}: {req.error}");
                yield break;
            }

            var codex = JsonUtility.FromJson<CountdownCodex>(req.downloadHandler.text);
            onLoaded(codex);
        }
    }
}
