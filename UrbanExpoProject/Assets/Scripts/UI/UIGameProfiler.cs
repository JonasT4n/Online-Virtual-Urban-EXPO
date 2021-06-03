using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NaughtyAttributes;

namespace UrbanExpo
{
    [RequireComponent(typeof(RectTransform))]
    public class UIGameProfiler : MonoBehaviour
    {
        [Header("UI Profiler Placeholder")]
        public TextMeshProUGUI txtPlayerGridInfo = null;
        public TextMeshProUGUI txtPlayerAreaInfo = null;
        public TextMeshProUGUI txtMouseGridInfo = null;
        public TextMeshProUGUI txtInteractionHighlightInfo = null;

        public void ShowProfiler(bool show)
        {
            gameObject.SetActive(show);
        }
    }

}