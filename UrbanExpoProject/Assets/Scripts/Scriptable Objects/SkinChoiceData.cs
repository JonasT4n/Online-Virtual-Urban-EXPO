using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NaughtyAttributes;

namespace UrbanExpo
{
    [CreateAssetMenu(fileName = "SkinChoiceDataObject", menuName = "Urban Expo/Skin Choice Data")]
    public class SkinChoiceData : ScriptableObject, ITemporaryData
    {
        [HorizontalLine(color: EColor.Gray)]
        [SerializeField] private SpriteContent[] customSkinChoice = null;

        #region Properties
        public SpriteContent[] SkinContents => customSkinChoice;
        #endregion

        public void ResetInputData()
        {
            
        }
    }

    [System.Serializable]
    public struct SpriteContent
    {
        public string name;
        public Sprite sprite;
    }
}