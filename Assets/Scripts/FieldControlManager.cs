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
    [SerializeField] private GameObject _markCell;
    // Building lines, which will form the field
    [SerializeField] private GameObject _buildLine;

    [Header("Field characteristic")]
    // MarkField size
    [SerializeField] private float _markFieldSize;
    // Field size (number of cells _fieldSize x _fieldSize)
    [Range(MIN_FIELD_SIZE, MAX_FIELD_SIZE)]
    [SerializeField] private int _fieldSize;

    // List for work with mark cells
    private List<List<GameObject>> _markCells2DList;

    // Start is called before the first frame update
    private void Start()
    {
        // Initialize object
        _markCells2DList = new List<List<GameObject>>();
        CreateField();
    }

    /// <summary>
    /// Creates field for TicTacToe play 
    /// </summary>
    /// <param name="xStartPos">X center position</param>
    /// <param name="yStartPos">Y center position</param>
    private void CreateField(float xCenterPos = 0, float yCenterPos = 0)
    {
        // Variables for line draw (will be used later in the function)
        float xLineStartPos = xCenterPos;
        float yLineStartPos = yCenterPos;
        int lineOffset = 0;
        // Set mark field scale, depending on field size.
        // Larger the field size, the smaller mark field is.
        float adaptFieldSize = _markFieldSize / ((_markFieldSize * _fieldSize) / MIN_FIELD_SIZE);
        adaptFieldSize = _markFieldSize * adaptFieldSize;
        // Getting start position for the field
        float xStartPos = xCenterPos - ((_fieldSize / 2) * adaptFieldSize);
        // Set y start position as inverted x start position
        float yStartPos = yCenterPos + ((_fieldSize / 2) * adaptFieldSize);
        // If field size is even, it needs to be moved to be in center
        if (_fieldSize % 2 == 0)
        {
            // Moving start position of cells drawing for correct visualisation
            xStartPos += adaptFieldSize / 2;
            yStartPos -= adaptFieldSize / 2;
        }
        else
        {
            // Moving start position of lines drawing for correct visualisation
            xLineStartPos -= adaptFieldSize / 2;
            yLineStartPos += adaptFieldSize / 2;
            lineOffset = 1;
        }
        lineOffset = (_fieldSize - 1) / 2 - lineOffset;
        xLineStartPos -= adaptFieldSize* lineOffset;
        yLineStartPos += adaptFieldSize * lineOffset;
        // Draw vertical lines
        for (int lineIt = 0; lineIt < _fieldSize - 1; lineIt++)
        {
            // Spawn line
            GameObject xline = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.BuildLine, _buildLine);
            GameObject yline = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.BuildLine, _buildLine);
            // Scale object
            xline.transform.localScale = new Vector3(adaptFieldSize / 4 / 3, adaptFieldSize * _fieldSize);
            yline.transform.localScale = new Vector3(xline.transform.localScale.y, xline.transform.localScale.x);
            // Setting position of the line
            xline.transform.position = new Vector3(xLineStartPos + (lineIt * adaptFieldSize), yCenterPos);
            yline.transform.position = new Vector3(xCenterPos, yLineStartPos - (lineIt * adaptFieldSize));
        }
        // Create field, depending on the entered size
        for (int y = 0; y < _fieldSize; y++)
        {
            _markCells2DList.Add(new List<GameObject>());
            for (int x = 0; x < _fieldSize; x++)
            {
                // Spawn mark cell
                GameObject markCell = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.MarkCell, _markCell);
                // Rescale object
                markCell.transform.localScale = new Vector3(adaptFieldSize, adaptFieldSize);
                // Set it on new position
                markCell.transform.position = new Vector2(xStartPos + x * markCell.transform.localScale.x, yStartPos - y * markCell.transform.localScale.y);
                // Add cell to list
                _markCells2DList[y].Add(markCell);
            }
        }
    }
}
