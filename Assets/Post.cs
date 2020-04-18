using System;
using System.Collections;

using System.Security.Cryptography;
using System.Text;
using UnityEngine;
//using AWSSDK;
//using Newtonsoft.Json;


public class Post : MonoBehaviour
{
    private string method = "POST";
    private static string host = "runtime.lex.us-west-2.amazonaws.com";
    private String region = "us-west-2";
    private string service = "lex";
    private string endpoint = "https://runtime.lex.us-west-2.amazonaws.com";

    public string botName = "BookTrip";
    public String botAlias = "JessyFirstTravelBot";
    public String userId = "myFirstBot";
    private string postAction = "text";
    private string accessKey = "AKIAWJX4S4CIG7EAN5MW";
    private string secretKey = "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";


    private static string contentType = "application/json";
    private string canonicalUri = "";
    string canonicalQueryString = "";
    //private string signedHeaders = "content-type;host;x-amz-date";
    private string signedHeaders = "content-type;host;x-amz-content-sha256;x-amz-date";
    private string algorithm = "AWS4-HMAC-SHA256";






    public string inputText = "";
    public string outputText = "";
    //private String inputTextJsonFront = "{\"inputText\": \"";
    //private String inputTextJsonBack = "\",\"requestAttributes\": {\"string\" : \"string\"}}";


    public bool sentRequest = true;

    #region AWS Steps
    #region STEP 1
    private static string getCanRequest(string dt, string payloadHash)
    {
        string canonicalHeaders = "content-type:" + contentType + "\n" + "host:" + host + "\n" + "x-amz-content-sha256:" + payloadHash + "\n" + "x-amz-date:" + dt + "\n";
        string canonicalUri = "/bot/BookTrip/alias/JessyFirstTravelBot/user/myFirstBot/text/";
        string signedHeaders = "content-type;host;x-amz-content-sha256;x-amz-date";
        string canonicalRequest = "POST\n" + canonicalUri + "\n\n" + canonicalHeaders + "\n" + signedHeaders + "\n" + payloadHash;
        byte[] bytes = Encoding.Default.GetBytes(canonicalRequest);
        canonicalRequest = Encoding.UTF8.GetString(bytes);
        string finalStep1 = hexEncode(SHA256(canonicalRequest), false);
        Debug.Log("finalStep1: " + finalStep1);
        return finalStep1;
    }
    #endregion

    private static string stringToSign(string dt, string date, string payloadHash)
    {
        string Algorithm = "AWS4-HMAC-SHA256";
        string CredentialScope = date + "/us-west-2/lex/aws4_request";
        string HashedCanonicalRequest = getCanRequest(dt, payloadHash);
        string finalStep2 = Algorithm + "\n" + dt + "\n" + CredentialScope + "\n" + HashedCanonicalRequest;
        Debug.Log("finalStep2: " + finalStep2);
        return finalStep2;
    }


    private byte[] sKey(string date)
    {
        byte[] finalsKey = getSignatureKey(secretKey, date, region, service);
        Debug.Log("finalsKey: " + hexEncode(finalsKey, false));
        return finalsKey;
    }

    private string signature(string dt, string payloadHash)
    {
        string date = dt.Substring(0, 8);
        Debug.Log("date: " + date);
        string finalSignature = hexEncode(Sign(sKey(date), stringToSign(dt, date, payloadHash)), false);
        Debug.Log("finalSignature: " + finalSignature);
        return finalSignature;

    }
    #endregion



    #region HelperFunctions
    #region signFunctions provided by aws
    static byte[] Sign(byte[] key, String data)
    {
        String algorithm = "HmacSHA256";
        KeyedHashAlgorithm kha = KeyedHashAlgorithm.Create(algorithm);
        kha.Key = key;

        return kha.ComputeHash(Encoding.UTF8.GetBytes(data));
    }

    static byte[] getSignatureKey(String key, String dateStamp, String regionName, String serviceName)
    {
        byte[] kSecret = Encoding.UTF8.GetBytes(("AWS4" + key).ToCharArray());
        byte[] kDate = Sign(kSecret, dateStamp);
        byte[] kRegion = Sign(kDate, regionName);
        byte[] kService = Sign(kRegion, serviceName);
        byte[] kSigning = Sign(kService, "aws4_request");
        return kSigning;
    }
    #endregion

    private static string hexEncode(byte[] bytes, bool upperCase)
    {
        StringBuilder result = new StringBuilder(bytes.Length * 2);
        for (int i = 0; i < bytes.Length; i++)
            result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
        return result.ToString();
    }

    private static byte[] SHA256(string StringIn)
    {
        byte[] hashByte;
        using (var sha256 = SHA256Managed.Create())
        {
            hashByte = sha256.ComputeHash(Encoding.Default.GetBytes(StringIn));

        }

        return hashByte;
    }
    #endregion




    // Start is called before the first frame update
    void Start()
    {
        canonicalUri = "/bot/" + botName + "/alias/" + botAlias + "/user/" + userId + "/" + postAction + "/";
        //Debug.Log("final URL: "+ finalURL);
        inputText = "Car Reservation";
        //Debug.Log(inputTextJsonFront + inputText + inputTextJsonBack);
    }






    IEnumerator postRequest()
    {

        sentRequest = false;
        string requestParameters = "{\"inputText\": \"" + inputText + "\"}";
        Debug.Log(requestParameters);
        string payloadHash = hexEncode(SHA256(requestParameters), false);
        Debug.Log("payloadHash: " + payloadHash);

        string amzDate = DateTime.UtcNow.ToString("yyyyMMddTHHmmssZ");
        string date = amzDate.Substring(0, 8);
        //string amzDate = "20200417T215249Z";

        //string URL = "https://httpbin.org/post";
        string URL = "https://runtime.lex.us-west-2.amazonaws.com/bot/BookTrip/alias/JessyFirstTravelBot/user/myFirstBot/text/";
        string auth = "AWS4-HMAC-SHA256 Credential=AKIAWJX4S4CIG7EAN5MW/"+ date +"/us-west-2/lex/aws4_request, SignedHeaders=content-type;host;x-amz-content-sha256;x-amz-date, Signature=" + signature(amzDate, payloadHash);

        WWWForm form = new WWWForm();
        form.AddField("inputText", inputText);
        Debug.Log("Content-Type: " + contentType);
        Debug.Log("X-Amz-Date: " + amzDate);
        Debug.Log("Authorization: " + auth);
        Debug.Log("X-Amz-Content-Sha256: " + payloadHash);



        var headers = new Hashtable();
        headers.Add("Authorization", auth);
        headers.Add("Content-Type", contentType);
        headers.Add("X-Amz-Content-Sha256", payloadHash);
        headers.Add("X-Amz-Date", amzDate);
        headers.Add("Accept-Encoding", "deflate, gzip");


        Debug.Log(headers);

        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(requestParameters);
        WWW www = new WWW(URL, jsonToSend, headers);
        yield return www;
        Debug.Log(www.text);
        getOutput(www.text);
        /*
        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.ToString());
            Debug.Log("POST Unsuccessful!");
            //StringBuilder sb = new StringBuilder();
            //foreach (System.Collections.Generic.KeyValuePair<string, string> dict in www.GetResponseHeaders())
            //{
            //    sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
            //}

            // Print Headers
            //Debug.Log(sb.ToString());

            // Print Body
            Debug.Log(www.downloadHandler.text);
        }
        else
        {
            Debug.Log("Form upload complete!");
            Debug.Log("POST successful!");
            Debug.Log("Return Code: " + www.responseCode);
            StringBuilder sb = new StringBuilder();
            foreach (System.Collections.Generic.KeyValuePair<string, string> dict in www.GetResponseHeaders())
            {
                sb.Append(dict.Key).Append(": \t[").Append(dict.Value).Append("]\n");
            }

            // Print Headers
            Debug.Log("headers: " + sb.ToString());
            Debug.Log(www.downloadHandler.text);
            //DownloadHandlerBuffer downloadHandlerBuffer = new DownloadHandlerBuffer();
            //www.downloadHandler = downloadHandlerBuffer;
            // Print Body
            //string response = www.downloadHandler.text;
            //print(response);
            //var botResponse = JsonUtility.FromJson<BotResponse>
        }
        */
    }

    //Temporary function due to time limit, should improvement with deserializer
    void getOutput(string phrase) {
        Debug.Log(phrase);
        string[] All = phrase.Split('"');
        Debug.Log(All);
        outputText = All[11];
        print(outputText);
        //string[] message2 = All[2].Split(':');
        //string message = message2[1];
        //Debug.Log(message);
        //outputText = message.Substring(1, message.Length - 1);
        //print(outputText);
    }

    // Update is called once per frame
    void Update()
    {
        if (inputText != "")
        {
            StartCoroutine(postRequest());
            inputText = "";
        }
    }
}

