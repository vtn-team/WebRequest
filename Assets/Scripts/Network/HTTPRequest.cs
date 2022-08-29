using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

using static Network.WebRequest;

public class HTTPRequest : MonoBehaviour
{
    int RetryCount = 0;
    public bool IsActive { get; private set; }

    public class Header
    {
        public string Name;
        public string Value;
    }

    public class Options
    {
        public List<Header> Header = new List<Header>();
    }

    class Packet
    {
        public string Uri;
        public string Body = "";
        public RequestMethod Method;
        public GetString Delegate;
        public Options Opt = null;
    }

    HTTPRequest()
    {
        IsActive = false;
    }
    
    public void Request(RequestMethod method, string uri, GetString dlg, string body = null, Options opt = null)
    {
        IsActive = true;
        Packet p = new Packet();
        p.Uri = uri;
        p.Delegate = dlg;
        p.Method = method;
        p.Body = body;
        p.Opt = opt;
        StartCoroutine(Send(p));
    }

    IEnumerator Send(Packet p)
    {
        UnityWebRequest req = null;
        if (p.Method == RequestMethod.GET)
        {
            req = UnityWebRequest.Get(p.Uri);
        }
        if (p.Method == RequestMethod.POST)
        {
            byte[] postData = System.Text.Encoding.UTF8.GetBytes(p.Body);
            req = new UnityWebRequest(p.Uri, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(postData),
                downloadHandler = new DownloadHandlerBuffer()
            };
            req.SetRequestHeader("Content-Type", "application/json");
        }
        if (p.Opt != null)
        {
            p.Opt.Header.ForEach(h => req.SetRequestHeader(h.Name, h.Value));
        }
        yield return req.SendWebRequest();

        if (req.error != null)
        {
            RetryCount++;
            if(RetryCount > 5)
            {
                IsActive = false;
                yield break;
            }
            Debug.LogError(req.uri + ":" + req.error);
            yield return new WaitForSeconds(1);
            Request(p.Method, p.Uri, p.Delegate, p.Body, p.Opt);
        }
        else
        {
            DataParse(p, req);
            IsActive = false;
        }
    }

    void DataParse(Packet p, UnityWebRequest req)
    {
        string str = req.downloadHandler.text;
        p.Delegate(req.downloadHandler.text);
        IsActive = false;
    }    
}