using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Abstrac class for input check by different entities
public abstract class InputCheck : MonoBehaviour
{
    // Mark type of the playe
    protected MarkType _userMarkType;
    // Field, where user must operate
    protected FieldControlManager _fieldControl;
    /// <summary>
    /// Field control, to which this input check is attached
    /// </summary>
    public FieldControlManager FieldControl { get { return _fieldControl; } }

    public void OnEnable()
    {
        _userMarkType = MarkType.Empty;
        _fieldControl = null;
    }

    public virtual void SetInputToField(MarkType inputMarkType, FieldControlManager fieldControl)
    {
        // Initialize variables
        _userMarkType = inputMarkType;
        _fieldControl = fieldControl;
    }
}
