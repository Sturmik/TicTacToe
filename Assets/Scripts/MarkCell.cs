using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Defines mark type
public enum MarkType
{
    Empty,
    Cross,
    Circle
}

// Mark cell will represent cross or circle
public class MarkCell : MonoBehaviour
{
    #region Variables

    // Constant value for interacting with animator
    private const string ANIMATION_STATE_VARIABLE = "State";

    // Animator
    [SerializeField] private Animator _markCellAnimator;
    // Mark type
    private MarkType _markType;

    /// <summary>
    /// Mark type
    /// </summary>
    public MarkType MarkType 
    {
        get { return _markType; } 
        set
        { 
            _markType = value;
            // Update state, if needed
            StateCheck();
        }
    }


    #endregion

    #region Unity

    // Reset object
    private void OnEnable()
    {
        _markType = MarkType.Empty;
        StateCheck();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks recent state of an object
    /// </summary>
    private void StateCheck()
    {
        _markCellAnimator.SetInteger(ANIMATION_STATE_VARIABLE, (int)_markType);
    }

    #endregion
}
