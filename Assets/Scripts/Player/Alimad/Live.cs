using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Collections;

public class OnlinePlayerCounter : MonoBehaviour
{
    public string url = "https://live.alimad.xyz/ping?app=bored&id=bored";
    public float interval = 20f;
    TextMeshProUGUI tmp;

    void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        StartCoroutine(PingLoop());
    }

    IEnumerator PingLoop()
    {
        while (true)
        {
            yield return StartCoroutine(UpdatePlayerCount());
            yield return new WaitForSeconds(interval);
        }
    }

    IEnumerator UpdatePlayerCount()
    {
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string res = req.downloadHandler.text.Trim();
            if (int.TryParse(res, out int count))
            {
                tmp.text = $"{count} player{(count == 1 ? "" : "s")} online";
            }
            else
            {
                tmp.text = "If you were online i'd tell you but lets suppose 1 player is online rn";
            }
        }
        else
        {
            tmp.text = "Network issue bruh";
        }
    }
}
