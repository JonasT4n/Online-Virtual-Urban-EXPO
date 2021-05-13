using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace UrbanExpo
{
    public struct MsgLine
    {
        public string msg;
        public Color color;
    }

    public class ChatBoxMessager : MonoBehaviour
    {
        [SerializeField] private int msgHolderLimit = 100;

        [Header("Chat Box UI")]
        [SerializeField] private Image chatBoxBackground = null;
        [SerializeField] private RectTransform msgContentMaster = null;
        [SerializeField] private TMP_Text msgBlockPref = null;
        [SerializeField] private InputField chatInputField = null;

        [Space, Header("Chat Box Ghost Content")]
        [SerializeField] private RectTransform msgContentGhost = null;
        [SerializeField] private float msgStartingToFade = 0.3f;
        [SerializeField] private float msgFadeTime = 3f;

        private List<IEnumerator> msgGhostRoutine = new List<IEnumerator>();
        private Queue<RectTransform> msgFading = new Queue<RectTransform>();
        private static Queue<MsgLine> msgHistory = new Queue<MsgLine>();

        #region Properties
        public bool IsChatBoxOpen { get; private set; } = true;
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            List<MsgLine> lines = new List<MsgLine>(msgHistory);
            foreach (MsgLine line in lines)
            {
                OpenChatBox(false);
                SendMessageToChatBox(line);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (IsChatBoxOpen)
                    SendMessageToChatBox(true);
                else
                    OpenChatBox(true);
            }
        }
        #endregion

        public void SendMessageToChatBox(bool addToHistory = false)
        {
            if (chatInputField.text.Trim() == string.Empty)
            {
                OpenChatBox(false);
                return;
            }

            SendMessageToChatBox(new MsgLine { msg = chatInputField.text, color = Color.cyan });
            chatInputField.text = string.Empty;
            OpenChatBox(false);
        }

        public void SendMessageToChatBox(MsgLine msg, bool addToHistory = false)
        {
            // Create new message block
            TMP_Text newTxt = Instantiate(msgBlockPref, msgContentMaster);
            newTxt.text = $"{msg.msg}";
            newTxt.color = msg.color;

            //// Add message to history log
            //if (addToHistory)
            //{
            //    msgHistory.Enqueue(msg);

            //    // Check message history limit count
            //    if (msgHistory.Count > msgHolderLimit) msgHistory.Dequeue();
            //}

            Debug.Log($"Prefered Height: {newTxt.preferredHeight}; Prefered Width: {newTxt.preferredWidth}");
            //Debug.Log($"Flexible Height: {newTxt.flexibleHeight}; Flexible Width: {newTxt.flexibleWidth}");
            //Debug.Log($"Rendered Height: {newTxt.renderedHeight}; Rendered Width: {newTxt.renderedWidth}");

            // Check message history limit
            if (msgContentMaster.childCount > msgHolderLimit)
                Destroy(msgContentMaster.GetChild(0).gameObject);

            // Check line count
            int lineCount = newTxt.textInfo.lineCount;
            if (lineCount > 1)
            {
                Vector2 blockSize = newTxt.rectTransform.sizeDelta;
                newTxt.rectTransform.sizeDelta = new Vector2(blockSize.x, blockSize.y * lineCount);
            }

            // Adjust content height and it's block position
            float contentHeight = 0;
            for (int i = msgContentMaster.childCount; i > 0; i--)
            {
                RectTransform child = msgContentMaster.GetChild(i - 1) as RectTransform;
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, contentHeight);

                // Add height to container
                contentHeight += child.sizeDelta.y;
            }
            msgContentMaster.sizeDelta = new Vector2(msgContentMaster.sizeDelta.x, contentHeight);

            // Make a ghost message
            IEnumerator routine = SendGhostMessage(msg);
            msgGhostRoutine.Add(routine);
            StartCoroutine(routine);
        }

        private IEnumerator SendGhostMessage(MsgLine msg)
        {
            float fadeTimeHolder = msgFadeTime;
            float fadeTimeStarted = msgFadeTime * msgStartingToFade;

            // Create new message block but with fading effect
            TMP_Text newTxt = Instantiate(msgBlockPref, msgContentGhost);
            newTxt.text = $"{msg.msg}";
            newTxt.color = msg.color;

            //// Check line count
            //int lineCount = newTxt.textInfo.lineCount;
            //if (lineCount > 1)
            //{
            //    Vector2 blockSize = newTxt.rectTransform.sizeDelta;
            //    newTxt.rectTransform.sizeDelta = new Vector2(blockSize.x, blockSize.y * lineCount);
            //}

            // Adjust content height and it's block position
            float contentHeight = 0;
            for (int i = msgContentGhost.childCount; i > 0; i--)
            {
                RectTransform child = msgContentGhost.GetChild(i - 1) as RectTransform;
                child.anchoredPosition = new Vector2(child.anchoredPosition.x, contentHeight);

                // Add height to container
                contentHeight += child.sizeDelta.y;
            }
            msgContentGhost.sizeDelta = new Vector2(msgContentGhost.sizeDelta.x, contentHeight);

            // Add to fading message queue
            msgFading.Enqueue(newTxt.rectTransform);

            // Fading Effect
            while (fadeTimeHolder > 0)
            {
                fadeTimeHolder -= Time.deltaTime;

                if (fadeTimeHolder < fadeTimeStarted)
                {
                    float fadingPercent = fadeTimeHolder / fadeTimeStarted;
                    newTxt.color = new Color(newTxt.color.r, newTxt.color.g, newTxt.color.b, fadingPercent);
                }

                yield return null;
            }

            // Add to fading message queue
            RectTransform t = msgFading.Dequeue();
            Destroy(t.gameObject);
            msgGhostRoutine.RemoveAt(0);
        }

        public void OpenChatBox(bool open)
        {
            if (open)
            {
                chatInputField.ActivateInputField();
                chatInputField.Select();
            }

            msgContentGhost.gameObject.SetActive(!open);
            chatBoxBackground.gameObject.SetActive(open);
            IsChatBoxOpen = open;
        }
    }

}
