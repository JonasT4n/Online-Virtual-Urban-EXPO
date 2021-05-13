using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class ObjectFollower : MonoBehaviour
{
    [Header("Follower Attributes")]
    [SerializeField] private Transform target = null;
    [SerializeField, Range(0f, 1f)] protected float followByX = 1;
    [SerializeField, Range(0f, 1f)] protected float followByY = 1;

    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 targetLastPos = Vector3.zero;
    [BoxGroup("DEBUG"), SerializeField, ReadOnly] private Vector3 origin = Vector3.zero;

    #region Properties
    protected Vector3 PositionOrigin => origin;
    public Transform TargetFollow
    {
        set
        {
            if (value != null)
            {
                target = value;
                targetLastPos = target.transform.position;
            }
        }
        get => target;
    }
    #endregion

    #region Unity BuiltIn Methods
    // Start is called before the first frame update
    protected virtual void Start()
    {
        origin = transform.position;

        if (target != null)
            targetLastPos = target.transform.position;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        Vector3 _currentTargetPos = target.transform.position;
        if (_currentTargetPos.x != targetLastPos.x || _currentTargetPos.y != targetLastPos.y)
        {
            float moveX = (_currentTargetPos.x - targetLastPos.x) * followByX;
            float moveY = (_currentTargetPos.y - targetLastPos.y) * followByY;

            origin += new Vector3(moveX, moveY);
            transform.position = origin;
            targetLastPos = _currentTargetPos;
        }
    }
    #endregion
}
