using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for human input
public class InputUser : InputCheck
{
    #region Unity

    // Update is called once per frame
    void Update()
    {
        // Checking if player clicked on field
        if (Input.GetMouseButtonDown(0) == true)
        {
            // If, it is user turn, he can place a mark
            if (_fieldControl.TurnState != _userMarkType) { return; }
            // Getting mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            // Using raycast
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            // Checking for collision
            if (hit.collider != null && hit.transform.CompareTag(Globals.MARK_CELL_TAG))
            {
                Debug.Log("This hit at " + hit.collider.transform.position);
                _fieldControl.MarkCellInField(hit.collider.gameObject);
            }

        }
    }

    #endregion
}
