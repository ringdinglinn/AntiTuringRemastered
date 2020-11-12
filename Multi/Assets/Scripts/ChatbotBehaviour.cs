using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class ChatbotBehaviour : MonoBehaviour {

    protected string text, response;
    protected string sessionId; // Remains null until after first message is parsed
    protected bool waiting;

    public string botid = "rabidrosie";// Name of chatbot to use. Has to be made using website
    public string appid = "una2165008"; // From Pandorabots application
    public string userkey = "2a901bdef12f158b9b6e9bd277d04766";//From Pandorabots application

    public List<Response> responses = new List<Response>();

    public NetworkManagerAT networkManager;

    public string getResponse() {
        return response;
    }

    // Use this for initialization
    void Start() {
        text = "";
        response = "Waiting for text";
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update() {

    }

    string sanitizePandoraResponse(string wwwText) {
        string responseString = "";

        int startIndex = wwwText.IndexOf(" [") + 2;
        int endIndex = wwwText.IndexOf("],");
        responseString = wwwText.Substring(startIndex, endIndex - startIndex);

        Debug.Log("Sanitized response: " + responseString);
        return responseString;
    }

    void getSessionIdOfPandoraResponse(string wwwText) {
        int startIndex = wwwText.IndexOf("sessionid") + 12;
        int endIndex = wwwText.IndexOf("}") - 1;

        sessionId = wwwText.Substring(startIndex, endIndex - startIndex);
    }

    private IEnumerator PandoraBotRequestCoRoutine(string text, int chatroomID) {
        //string url = "https://api.pandorabots.com/atalk/" + appid;
        //url = url + "/" + botid;
        //url = url + "&user_key=" + userkey;
        //url = url + "?input=" + UnityWebRequest.EscapeURL(text);
        //if (sessionId != null) {
        //    url = url + "&sessionid=" + sessionId;
        //}

        string url = "https://api.pandorabots.com/talk?botkey=RssstjtodsmGn5b1IstcJtNZI9khFR8B6xS0_Qvmtrrq5dalb0KYSIeonmRa15PUOL2I-8EtsPdp9rI_1dsWOQ~~&input=";
        url += UnityWebRequest.EscapeURL(text);

        Debug.Log(url);

        UnityWebRequest wr = UnityWebRequest.Post(url, ""); //You cannot do POST with empty post data, new byte is just dummy data to solve this problem

        yield return wr.SendWebRequest();

        if (wr.error == null) {
            Debug.Log(wr.downloadHandler.text);
            getSessionIdOfPandoraResponse(wr.downloadHandler.text);
            Debug.Log("SessionId:" + sessionId + ".");

            string r = sanitizePandoraResponse(wr.downloadHandler.text);//Where we get our chatbots response message
            Response response = new Response(chatroomID, r);
            responses.Add(response);
            SendResponseToServer(response);
        }
        else {
            Debug.LogWarning(wr.error);
        }
    }

    public struct Response {
        public Response(int id, string t) {
            chatroomID = id;
            text = t;
        }
        public int chatroomID;
        public string text;
    }

    //void OnGUI() {
    //    int label_width = 90;
    //    int edit_width = 250;

    //    text = GUI.TextArea(new Rect(10, Screen.height - 60, edit_width + label_width, 50), text, 512);

    //    if (text.Contains("\n") || text.Contains("...")) // ... is so I can send messages on my android device after building a .apk
    //    {

    //        StartCoroutine(PandoraBotRequestCoRoutine(text));
    //        text = "";
    //    }
    //    GUI.Label(new Rect(10, Screen.height - 80, 300, 20), "Type something below and hit enter!");

    //}

    public void SendTextToChatbot(string text, int chatroomID) {
        StartCoroutine(PandoraBotRequestCoRoutine(text, chatroomID));
    }

    private void SendResponseToServer(Response response) {
        Debug.Log(networkManager);
        Debug.Log(networkManager.GamePlayers[0]);
        Debug.Log(networkManager.GamePlayers[0].chatBehaviour);
        Debug.Log("SendResponseToServer");
        networkManager.GamePlayers[0].ReceiveMessageFromChatbot(response.text, response.chatroomID);
    }
}
