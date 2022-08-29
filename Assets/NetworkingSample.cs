using Network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkingSample : MonoBehaviour
{
    [SerializeField] UnityEngine.UI.Text _text;
    void Start()
    {
        WebRequest.GetRequest("https://stv8p80kb8.execute-api.ap-northeast-1.amazonaws.com/default/LambdaTest", (string text) => {
            Debug.Log(text);
            _text.text = text;
        });
    }
}
