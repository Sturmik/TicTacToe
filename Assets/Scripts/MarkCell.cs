using UnityEngine;

// Defines mark type
public enum MarkType
{
    Empty,
    Cross,
    CrossWin,
    Circle,
    CircleWin
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
        // Initialization
        _markType = MarkType.Empty;
        // Update state
        StateCheck();
        // Unsubscribe and subscribe to events
        FieldControlManager.GetCellMarkType -= GetMarkType;
        FieldControlManager.GetCellMarkType += GetMarkType;
        FieldControlManager.SetCellMarkType -= SetMarkType;
        FieldControlManager.SetCellMarkType += SetMarkType;
        FieldControlManager.SetCellAsWinning -= SetWinningState;
        FieldControlManager.SetCellAsWinning += SetWinningState;
    }

    private void OnDisable()
    {
        // Unsubscribe to events
        FieldControlManager.GetCellMarkType -= GetMarkType;
        FieldControlManager.SetCellMarkType -= SetMarkType;
        FieldControlManager.SetCellAsWinning -= SetWinningState;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Checks recent state of an object
    /// </summary>
    private void StateCheck()
    {
        if (_markCellAnimator != null)
        {
            _markCellAnimator.SetInteger(ANIMATION_STATE_VARIABLE, (int)_markType);
        }
    }

    /// <summary>
    /// Returns mark type of an object to the parameter
    /// </summary>
    private void GetMarkType(GameObject obj, ref MarkType checkMark)
    {
        // Checking, if objects are equal
        if (gameObject == obj)
        {
            checkMark = _markType;
        }
    }

    private void SetMarkType(GameObject obj, MarkType markType)
    {
        // Checking, if objects are equal
        if (gameObject == obj)
        {
            _markType = markType;
            StateCheck();
        }
    }

    /// <summary>
    /// Sets mark cell to win state
    /// </summary>
    /// <param name="obj">Checking, if object is equal to our one</param>
    private void SetWinningState(GameObject obj)
    {
        // Checking, if objects are equal
        if (gameObject == obj)
        {
            if (_markType == MarkType.Cross) { _markType = MarkType.CrossWin; }
            if (_markType == MarkType.Circle) { _markType = MarkType.CircleWin; }
            StateCheck();
        }
    }

    #endregion
}
