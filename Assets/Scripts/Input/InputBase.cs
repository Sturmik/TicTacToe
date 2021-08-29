using UnityEngine;

// Abstract class for input check by different entities
public abstract class InputBase : MonoBehaviour
{
    #region Variables

    // Mark type of the playe
    protected MarkType _userMarkType;
    // Field, where user must operate
    protected FieldControlManager _fieldControl;
    /// <summary>
    /// Field control, to which this input check is attached
    /// </summary>
    public FieldControlManager FieldControl { get { return _fieldControl; } }

    #endregion

    #region Unity

    protected void OnEnable()
    {
        // Initialization
        _userMarkType = MarkType.Empty;
        _fieldControl = null;
        // Unsubscribe from event
        FieldControlManager.SetFieldInput -= SetInputToField;
        // Subscibe to event
        FieldControlManager.SetFieldInput += SetInputToField;
    }

    protected virtual void OnDisable()
    {
        // Unsubscribe
        FieldControlManager.SetFieldInput -= SetInputToField;
    }

    #region Methods

    protected virtual void SetInputToField(GameObject obj, MarkType inputMarkType, FieldControlManager fieldControl)
    {
        if (obj != gameObject) return;
        // Initialize variables
        _userMarkType = inputMarkType;
        _fieldControl = fieldControl;
    }

    #endregion

    #endregion
}
