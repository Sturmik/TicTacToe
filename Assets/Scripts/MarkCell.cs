using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Mark cell will represent cross or circle
public class MarkCell : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // ...
    }

    // Update is called once per frame
    void Update()
    {
        // Checking if player clicked on field
        if (Input.GetMouseButtonDown(0))
        {
            // Getting mouse position
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);
            // Using raycast
            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            // Checking for collision
            if (hit.collider != null)
            {
                Debug.Log("This hit at " + hit.collider.transform.position);
                hit.collider.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }
    }
}
