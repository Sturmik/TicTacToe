using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for field building
public class FieldControlManager : MonoBehaviour
{
    // Constants
    private const int MIN_FIELD_SIZE = 3;
    private const int MAX_FIELD_SIZE = 30;

    [Header("Field build blocks")]
    // Mark field prefab
    [SerializeField] private GameObject _markField;
    // Building lines, which will form the field
    [SerializeField] private GameObject _buildLine;
    [Header("Field characteristic")]
    // Field size 
    [Range(MIN_FIELD_SIZE, MAX_FIELD_SIZE)]
    [SerializeField] private int _fieldSize;

    // Start is called before the first frame update
    private void Start()
    {
        // Checking given gameobjects and check if they correspond with what we need for correct work of the script
        if (_markField.transform.localScale.x != _markField.transform.localScale.y)
        {
            Debug.LogError("Game object " + _markField.name + " must have same size in X and Y axis!\nOtherwise, it can cause unpredictable behaviour!");
        }

        CreateField();
    }

    private void CreateField(bool defPos = true, float xStartPos = 0, float yStartPos = 0)
    {
        // Set mark field scale, depending on field size.
        // Larger the field size, the smaller mark field is.
        float adaptFieldSize = _markField.transform.localScale.x / ((_markField.transform.localScale.x * _fieldSize) / MIN_FIELD_SIZE);
        adaptFieldSize = _markField.transform.localScale.x * adaptFieldSize;
        // If we need to set field in the default position, we go through this segment
        if (defPos == true)
        {
            // Getting start position for the field
            xStartPos = -((_fieldSize / 2) * adaptFieldSize);
            // If field size is even, it needs to be moved to be in center
            if (_fieldSize % 2 == 0)
            {
                xStartPos += adaptFieldSize / 2;
            }
            // Set y start position as inverted x start position
            yStartPos = -xStartPos;
        }
        // Create field, depending on the entered size
        for (int y = 0; y < _fieldSize; y++)
        {
            for (int x = 0; x < _fieldSize; x++)
            {
                // Spawn mark field
                GameObject toSpawn = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.MarkField, _markField);
                // Rescale object
                toSpawn.transform.localScale = new Vector3(adaptFieldSize, adaptFieldSize);
                // Set it on new position
                toSpawn.transform.position = new Vector2(xStartPos + x * toSpawn.transform.localScale.x, yStartPos - y * toSpawn.transform.localScale.y);
            }
        }
    }
}
