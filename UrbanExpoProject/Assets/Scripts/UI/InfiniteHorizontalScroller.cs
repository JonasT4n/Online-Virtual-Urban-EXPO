using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;
using NaughtyAttributes;

namespace UrbanExpo
{
    [RequireComponent(typeof(ScrollRect))]
    public class InfiniteHorizontalScroller : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Custom Skin Attributes")]
        [SerializeField] private float snapSpeedX = 5f;
        [SerializeField, Range(0f, 10f)] private float blockTransitionSensitivity = 0.5f;

        private SpriteContent[] contents;
        private Image middleBlock;
        private ScrollRect contentScroller;
        private IEnumerator snapRoutine;

        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private List<Image> previewBlocks = new List<Image>();
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector2 lastContentPosition = Vector2.zero;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private int contentIndex = 0;
        [BoxGroup("DEBUG"), SerializeField, ReadOnly] private bool isBeingDrag = false;

        #region Properties
        public int ContentIndex => contentIndex;
        #endregion

        #region Unity BuiltIn Methods
        private void Awake()
        {
            // Initialize component and data requirements
            contentScroller = GetComponent<ScrollRect>();
            InitImageBlock();

            // Set and normalize initial data information
            RectTransform content = contentScroller.content;
            content.sizeDelta = new Vector2(contentScroller.viewport.rect.width * previewBlocks.Count, content.sizeDelta.y);
            content.anchoredPosition = Vector2.zero;
            lastContentPosition = content.anchoredPosition;
            blockTransitionSensitivity = blockTransitionSensitivity < 0 ? -blockTransitionSensitivity : blockTransitionSensitivity;
        }

        private void Update()
        {
            Vector2 contentAnchPos = contentScroller.content.anchoredPosition;

            // Check viewport index change
            if (!isBeingDrag)
            {
                float halfWidth = contentScroller.viewport.rect.width / 2f;
                if (contentAnchPos.x > halfWidth + blockTransitionSensitivity)
                {
                    contentIndex = contentIndex + 1 >= contents.Length ? 0 : contentIndex + 1;
                    MoveLastBlockToFirst(contentAnchPos.x);
                    return;
                }
                else if (contentAnchPos.x < -halfWidth - blockTransitionSensitivity)
                {
                    contentIndex = contentIndex - 1 < 0 ? contents.Length - 1 : contentIndex - 1;
                    MoveFirstBlockToLast(contentAnchPos.x);
                    return;
                }
            }

            // Check if scroller stopped, then snap to the center of viewport
            if (!isBeingDrag && snapRoutine == null)
            {
                if (CompareFloatingPoint(contentAnchPos.x, lastContentPosition.x, 0.001f) && !CompareFloatingPoint(contentAnchPos.x, 0f, 0.001f))
                {
                    contentScroller.StopMovement();
                    SnapScroller();
                }
            }

            lastContentPosition = contentAnchPos;
        }
        #endregion

        #region Event Methods
        public void OnPointerDown(PointerEventData eventData)
        {
            isBeingDrag = true;

            // Make routine stop when start dragging the content
            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 contentAnchPos = contentScroller.content.anchoredPosition;

            float halfWidth = contentScroller.viewport.rect.width / 2f;
            if (contentAnchPos.x > halfWidth + blockTransitionSensitivity)
            {
                contentIndex = contentIndex + 1 >= contents.Length ? 0 : contentIndex + 1;
                MoveLastBlockToFirst(contentAnchPos.x);
                eventData.dragging = false;
                return;
            }
            else if (contentAnchPos.x < -halfWidth - blockTransitionSensitivity)
            {
                contentIndex = contentIndex - 1 < 0 ? contents.Length - 1 : contentIndex - 1;
                MoveFirstBlockToLast(contentAnchPos.x);
                eventData.dragging = false;
                return;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isBeingDrag = false;
        }
        #endregion

        public void SetContents(SpriteContent[] data)
        {
            // Initialize content data
            contentIndex = 0;
            contents = data;

            // Create initial index data by block size and content size
            int midStart = Mathf.FloorToInt(previewBlocks.Count / 2);
            int startContentIndex = data.Length;
            if (startContentIndex < midStart)
            {
                startContentIndex *= Mathf.CeilToInt(midStart / data.Length);
            }
            startContentIndex -= midStart;

            // Assign image sprite into each block
            for (int i = 0; i < previewBlocks.Count; i++)
            {
                Image img = previewBlocks[i];
                img.sprite = contents[startContentIndex].sprite;

                startContentIndex = startContentIndex + 1 >= data.Length ? 0 : startContentIndex + 1;
            }
        }

        public void MoveLeft(int steps)
        {
            if (snapRoutine != null)
                return;

            snapRoutine = SnapMoveScrollXRoutine(steps);
            StartCoroutine(snapRoutine);
        }

        public void MoveRight(int steps)
        {
            if (snapRoutine != null)
                return;

            snapRoutine = SnapMoveScrollXRoutine(-steps);
            StartCoroutine(snapRoutine);
        }

        public void SnapScroller()
        {
            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }

            snapRoutine = SnapScrollXRoutine();
            StartCoroutine(snapRoutine);
        }

        private void MoveLastBlockToFirst(float lastXPos)
        {
            // Move content to normal position
            RectTransform content = contentScroller.content;
            content.anchoredPosition = new Vector2(lastXPos - contentScroller.viewport.rect.width, content.anchoredPosition.y);

            // Move block
            content.GetChild(content.childCount - 1).SetSiblingIndex(0);
            InitImageBlock();
            int midPoint = Mathf.FloorToInt(previewBlocks.Count / 2);
            middleBlock = previewBlocks[midPoint];

            // Set content sprite on first block
            int firstBlockContentIndex = contentIndex - midPoint;
            if (firstBlockContentIndex < 0)
            {
                firstBlockContentIndex = -(firstBlockContentIndex % contents.Length);
                //Debug.Log($"Move Last to First, Content Index on First: {firstBlockContentIndex}");
            }
            previewBlocks[0].sprite = contents[firstBlockContentIndex].sprite;
        }

        private void MoveFirstBlockToLast(float lastXPos)
        {
            // Move content to normal position
            RectTransform content = contentScroller.content;
            content.anchoredPosition = new Vector2(lastXPos + contentScroller.viewport.rect.width, content.anchoredPosition.y);

            // Move block
            content.GetChild(0).SetSiblingIndex(content.childCount - 1);
            InitImageBlock();
            int midPoint = Mathf.FloorToInt(previewBlocks.Count / 2);
            middleBlock = previewBlocks[midPoint];

            // Set content sprite on last block
            int lastBlockContentIndex = contentIndex + midPoint;
            if (lastBlockContentIndex >= contents.Length)
            {
                lastBlockContentIndex %= contents.Length;
                //Debug.Log($"Move First to Last, Content Index on Last: {lastBlockContentIndex}");
            }
            previewBlocks[previewBlocks.Count - 1].sprite = contents[lastBlockContentIndex].sprite;
        }

        private void InitImageBlock()
        {
            previewBlocks.Clear();
            RectTransform content = contentScroller.content;
            for (int i = 0; i < content.childCount; i++)
            {
                Image img = content.GetChild(i).GetComponent<Image>();

                // Add image component if not exists
                if (img == null)
                {
                    img = content.GetChild(i).gameObject.AddComponent<Image>();
                }

                // Check middle block
                if (i == Mathf.FloorToInt(content.childCount / 2f))
                {
                    middleBlock = img;
                }

                previewBlocks.Add(img);
            }
        }

        private IEnumerator SnapMoveScrollXRoutine(int blockCountMovement)
        {
            RectTransform content = contentScroller.content;
            float xMoveAmount = contentScroller.viewport.rect.width * blockCountMovement;
            while (Mathf.Abs(xMoveAmount) > 0)
            {
                Vector2 anchPos = content.anchoredPosition;
                float xSpeedDir = (xMoveAmount < 0 ? -1f : 1f) * snapSpeedX * Time.deltaTime;
                xMoveAmount -= xSpeedDir;

                if (xMoveAmount > 0f && xSpeedDir < 0f)
                {
                    xSpeedDir = xMoveAmount;
                    xMoveAmount = 0f;
                }
                else if (xMoveAmount < 0f && xSpeedDir > 0f)
                {
                    xSpeedDir = xMoveAmount;
                    xMoveAmount = 0f;
                }

                anchPos.x += xSpeedDir;
                content.anchoredPosition = anchPos;
                yield return null;
            }

            snapRoutine = SnapScrollXRoutine();
            StartCoroutine(snapRoutine);
        }

        private IEnumerator SnapScrollXRoutine()
        {
            RectTransform content = contentScroller.content;
            while (!CompareFloatingPoint(content.anchoredPosition.x, 0f, 0.001f))
            {
                Vector2 currentAnchPos = content.anchoredPosition;
                float xDir = content.anchoredPosition.x < 0f ? 1f : -1f;
                float xDistance = Mathf.Abs(currentAnchPos.x);
                if (xDistance > 0)
                {
                    float xAfterMove = currentAnchPos.x + xDir * snapSpeedX * Time.deltaTime;
                    float xDistanceAfter = Mathf.Abs(xAfterMove);
                    if (xDistanceAfter > xDistance)
                    {
                        xAfterMove = 0f;
                    }

                    content.anchoredPosition = new Vector2(xAfterMove, currentAnchPos.y);
                }
                yield return null;
            }

            snapRoutine = null;
        }

        #region Static Methods
        public static bool CompareFloatingPoint(float a, float b, float epsilon)
        {
            float aAbs = Mathf.Abs(a);
            float bAbs = Mathf.Abs(b);
            float diff = Mathf.Abs(a - b);

            if (a == b)
                return true;
            else if (a == 0 || b == 0 || aAbs + bAbs < float.Epsilon)
                return diff < epsilon * float.Epsilon;
            else
                return diff / (aAbs + bAbs) < epsilon;
        }
        #endregion
    }

}