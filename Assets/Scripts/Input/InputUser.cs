using UnityEngine;

// Class for human input
public class InputUser : InputBase
{
    #region Variables

    // Z ray scalar for it's length
    private const int RAY_Z_SCALAR_INCREASE = 100;

    #endregion

    #region Unity


    // Update is called once per frame
    void Update()
    {
        // Checking if player clicked on field
        if (Input.GetMouseButtonDown(0) == true)
        {
            // If, it is user turn, he can place a mark
            if (_fieldControl.TurnState != _userMarkType) { return; }
            // Raycasting to check, if we hit something
            RaycastHit rayCastInfo = RayCastFromCamera();
            // Checking for collision
            if (rayCastInfo.collider != null && rayCastInfo.transform.CompareTag(Globals.MARK_CELL_TAG))
            {
                Debug.Log("This hit at " + rayCastInfo.collider.transform.position);
                _fieldControl.MarkCellInField(rayCastInfo.collider.gameObject);
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Ray cast depends on dimension type of the field
    /// </summary>
    /// <returns></returns>
    private RaycastHit RayCastFromCamera()
    {
        Vector3 posToRayFrom, posToRayTo;
        switch (_fieldControl.FieldDimensionType)
        {
            default:
            case FieldControlManager.DimensionType.Dimension2D:
                // Ray cast for two dimensions
                posToRayFrom = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                posToRayTo = new Vector3(posToRayFrom.x, posToRayFrom.y + Camera.main.transform.up.y, posToRayFrom.z + Camera.main.transform.forward.z * RAY_Z_SCALAR_INCREASE);
                break;
            case FieldControlManager.DimensionType.Dimension3D:
                posToRayFrom = Camera.main.transform.position;
                posToRayTo = Camera.main.transform.position + Camera.main.transform.forward * RAY_Z_SCALAR_INCREASE;
                break;
        }
        Physics.Raycast(posToRayFrom, posToRayTo, out RaycastHit rayCastInfo);
        return rayCastInfo;
    }

    #endregion
}
