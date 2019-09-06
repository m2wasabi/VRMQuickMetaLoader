using System.IO;
using UnityEngine;
using VRM.QuickMetaLoader;

public sealed class LoadManyMeta : MonoBehaviour
{
    [SerializeField] private RectTransform _cardPrefab;

    void Start()
    {
        var vrms = Directory.GetFiles(".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);

        foreach (var vrm in vrms)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            LoadMeta(vrm);
            stopwatch.Stop();
            Debug.Log("LoadTime: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");
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
            var vrms = Directory.GetFiles(".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);

            foreach (var vrm in vrms)
            {
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                LoadMeta(vrm);
                stopwatch.Stop();
                Debug.Log("LoadTime: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");
            }
        }
    }

    private void LoadMeta(string file)
    {
        var bytes = File.ReadAllBytes(file);
        using (var metaLoader = new MetaLoader(bytes))
        {
            var meta = metaLoader.Read(true);

            var go = Instantiate(_cardPrefab, transform);
            var card = go.GetComponent<CharacterCard>();
            card.SetMetaText(meta);
            card.SetThumbnail(meta.Thumbnail);
        }
    }
}
