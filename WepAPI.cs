using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WepAPI : MonoBehaviour
{
    private byte[] Texture2dToBytes(Texture2D _tex2D)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                _tex2D.width,
                _tex2D.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);

        Graphics.Blit(_tex2D, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D tex = new Texture2D(_tex2D.width, _tex2D.height);
        tex.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);

        // Encode texture into PNG
        byte[] bytes = tex.EncodeToJPG();
        return bytes;
    }

    private IEnumerator UploadTexture2D(byte[] bytes, string fileName, string url)
    {
        WWWForm postForm = new WWWForm();
        System.DateTime myDate = System.DateTime.Now;
        string myDateString = myDate.ToString("yyyy_MM_dd_HH_mm_ss");
        fileName = fileName + "_" + UnityEngine.Random.Range(0, int.MaxValue).ToString() + myDateString + ".jpg";
        Debug.Log("UploadTexture2D fileName: " + fileName);
        postForm.AddBinaryData("files", bytes, fileName);
        UnityWebRequest uwr = UnityWebRequest.Post(url, postForm);
        uwr.chunkedTransfer = true;
        yield return uwr.SendWebRequest();
        if (uwr.isNetworkError || uwr.isHttpError)
        {
            Debug.Log(uwr.error);
        }
        else
        {
            string responseUrl = uwr.downloadHandler.text.Trim('"');
            Debug.Log("Upload complete!\n" + responseUrl);
        }
    }

    private IEnumerator DownloadTexture2D(string responseUrl, UNetPlayer uNetPlayer = null)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(responseUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                Texture2D tex2d = DownloadHandlerTexture.GetContent(uwr);
                Debug.Log("Download success!");
            }
        }
    }
}
