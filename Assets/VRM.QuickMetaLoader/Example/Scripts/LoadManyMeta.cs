using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VRM.QuickMetaLoader;

public class LoadManyMeta : MonoBehaviour
{
    [SerializeField] private RectTransform _cardPrefab;

    void Start()
    {
        var vrms = Directory.GetFiles (".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);

        foreach (var vrm in vrms)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            LoadMeta(vrm);
            stopwatch.Stop();
            Debug.Log("LoadTime: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");
        }

    }

    private void LoadMeta(string file)
    {
        var bytes = File.ReadAllBytes(file);
        var metaLoader = new MetaLoader(bytes);
        var meta = metaLoader.Read();

        var go = Instantiate(_cardPrefab, transform);
        var card = go.GetComponent<CharacterCard>();
        card.SetMetaText(meta);
        LoadThumbnail(card, metaLoader);

    }

    private void LoadThumbnail(CharacterCard card ,MetaLoader metaLoader)
    {
        var t = metaLoader.LoadAsyncThumbnail();
        card.SetThumbnail(t);
    }
}
