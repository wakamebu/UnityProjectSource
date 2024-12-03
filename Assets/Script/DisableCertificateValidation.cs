using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DisableCertificateValidation : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(MakeRequest());
    }

    IEnumerator MakeRequest()
    {
        UnityWebRequest request = UnityWebRequest.Get("fproject.starfree.jp");
        
        // 証明書の検証を無効化
        request.certificateHandler = new BypassCertificateHandler();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("Request failed: " + request.error);
        }
        else
        {
            Debug.Log("Request succeeded: " + request.downloadHandler.text);
        }
    }

    private class BypassCertificateHandler : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            // 常に証明書を信頼する
            return true;
        }
    }
}
