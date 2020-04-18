using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int maxMessages = 25;

    [SerializeField]
    public GameObject chatPanel, textObject;
    public GameObject Post;
    public Post post;
    public InputField chatbox;

    [SerializeField]
    List<Message> messageList = new List<Message>();

 


    // Start is called before the first frame update
    void Start()
    {
        SendMessageToChat(" Bot: Hi there!", Message.MessageType.Bot);
    }

    // Update is called once per frame
    void Update()
    {
        if (post.outputText != "") {
            string botMessage = post.outputText;
            print("\\\\\\\\\\\\\\\\: " + botMessage);
            SendMessageToChat(" Bot: " + botMessage, Message.MessageType.Bot);
            post.outputText = "";
        }

        if (chatbox.text != "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                post.inputText = chatbox.text;
                SendMessageToChat(" You: " + chatbox.text, Message.MessageType.playerMessage);
                chatbox.text = "";

            }
        }
        else {
            if (!chatbox.isFocused && Input.GetKeyDown(KeyCode.Return)) {
                chatbox.ActivateInputField();
            }
        }
    }

    public void SendMessageToChat(string text, Message.MessageType messageType) {
        if (messageList.Count >= maxMessages) {
            Destroy(messageList[0].textObject.gameObject);
            messageList.Remove(messageList[0]);
        }

        Message newMessage = new Message();
        newMessage.text = text;

        GameObject newText = Instantiate(textObject, chatPanel.transform);
        newMessage.textObject = newText.GetComponent<Text>();
        newMessage.textObject.text = newMessage.text;

        messageList.Add(newMessage);
    }
}

[System.Serializable]
public class Message
{
    public string text;
    public Text textObject;
    public MessageType messageType;

    public enum MessageType
    {
        playerMessage,
        Bot,
    }
}


