using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using VRM.QuickMetaLoader;

public class LoadManyMetaAsync : MonoBehaviour
{
    [SerializeField] private RectTransform _cardPrefab;

    void Start()
    {
        var vrms = Directory.GetFiles (".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);
        foreach (var vrm in vrms)
        {
            LoadMetaAsync(vrm);
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var vrms = Directory.GetFiles (".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);
            foreach (var vrm in vrms)
            {
                LoadMetaAsync(vrm);
            }
        }
    }

    private async void LoadMetaAsync(string file)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var bytes = await Task.Run(() => File.ReadAllBytes(file));
        var metaLoader = new MetaLoader(bytes);
        var meta = await metaLoader.ReadAsync(true);
        stopwatch.Stop();
        Debug.Log("LoadTime: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");

        var go = Instantiate(_cardPrefab, transform);
        var card = go.GetComponent<CharacterCard>();
        card.SetMetaText(meta);
        card.SetThumbnail(meta.Thumbnail);
    }
}