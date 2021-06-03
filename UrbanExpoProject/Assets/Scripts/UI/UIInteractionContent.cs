using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NaughtyAttributes;

namespace UrbanExpo
{
    [RequireComponent(typeof(RectTransform))]
    public class UIInteractionContent : MonoBehaviour, IPointerClickHandler
    {
        [Header("UI")]
        [SerializeField] private Image background = null;

        #region Event Methods
        public void OnPointerClick(PointerEventData eventData)
        {
            if (GameManager.singleton.uiGameManager.IsConfiguring)
            {
                GameManager.singleton.uiGameManager.IsConfiguring = false;
            }

            if (Application.platform != RuntimePlatform.WebGLPlayer)
            {
                GameManager.singleton.webHook.StopInteraction();
            }

            ActivateIContent(false);
        }
        #endregion

        public void ActivateIContent(bool active)
        {
            gameObject.SetActive(active);
        }
    }

}