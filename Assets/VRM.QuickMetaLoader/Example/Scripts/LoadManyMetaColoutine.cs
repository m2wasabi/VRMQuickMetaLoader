using System.Collections;
using System.IO;
using UnityEngine;
using VRM;
using VRM.QuickMetaLoader;

public class LoadManyMetaColoutine : MonoBehaviour
{
    [SerializeField] private RectTransform _cardPrefab;

    void Start()
    {
        var vrms = Directory.GetFiles(".", "*.vrm", System.IO.SearchOption.TopDirectoryOnly);
        StartCoroutine(LoadVrmColoutine(vrms));

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
            StartCoroutine(LoadVrmColoutine(vrms));
        }
    }

    private void LoadMeta(string file)
    {
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        var bytes = File.ReadAllBytes(file);
        VRMMetaObject meta;
        using (var metaLoader = new MetaLoader(bytes))
        {
            meta = metaLoader.Read(true);
        }
        stopwatch.Stop();
        Debug.Log("LoadTime: " + (float)stopwatch.Elapsed.TotalSeconds + " sec");

        var go = Instantiate(_cardPrefab, transform);
        var card = go.GetComponent<CharacterCard>();
        card.SetMetaText(meta);
        card.SetThumbnail(meta.Thumbnail);
    }

    private IEnumerator LoadVrmColoutine(string[] files)
    {
        foreach (var file in files)
        {
            LoadMeta(file);
            yield return null;
        }
    }
}