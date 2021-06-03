using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace UrbanExpo
{
    public class Building : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer = null;
        [SerializeField] private Animator animator = null;

        #region Unity BuiltIn Methods
        protected virtual void Start()
        {
            
        }
        #endregion

        public void SetAlpha(float alphaValue)
        {
            Color current = spriteRenderer.color;
            spriteRenderer.DOColor(new Color(current.r, current.g, current.b, alphaValue), 0.35f);
        }
    }

}