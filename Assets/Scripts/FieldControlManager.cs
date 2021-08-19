using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Globals
{
    #region Tags

    public const string MARK_CELL_TAG = "MarkCell";

    #endregion
}

// Class for field building and full control over it
public class FieldControlManager : MonoBehaviour
{
    #region Variables

    // Singleton
    private static FieldControlManager _instance;
    public static FieldControlManager GetInstance()
    {
        if (_instance == null)
        {
            Debug.LogWarning("Instance of " + nameof(FieldControlManager) + " is null referenced!");
            throw new System.Exception("Instance of " + nameof(FieldControlManager) + " is null referenced!");
        }
        return _instance;
    }

    // Constants
    private const int MIN_FIELD_SIZE = 3;
    private const int MAX_FIELD_SIZE = 30;

    [Header("Field build blocks")]
    // Mark field prefab
    [SerializeField] private GameObject _markCell;
    // Building lines, which will form the field
    [SerializeField] private GameObject _buildLine;

    [Header("Field characteristic")]
    // Win row quantity
    [SerializeField] private int _winRowQuant;
    // MarkField size
    [SerializeField] private float _markFieldSize;
    // Field size (number of cells _fieldSize x _fieldSize)
    [Range(MIN_FIELD_SIZE, MAX_FIELD_SIZE)]
    [SerializeField] private int _fieldSize;

    // List for lines
    private List<GameObject> _linesList;
    // List for work with mark cells
    private List<List<GameObject>> _gameObjectsMarkCells2DList;

    // List, which holds state of cells
    private List<List<MarkType>> _marksTypes2DList;

    // Defines turn state
    private MarkType _turnState;
    /// <summary>
    /// Returns mark, which must be placed now
    /// </summary>
    public  MarkType TurnState
        { get { return _turnState; }}

    #endregion

    #region Unity

    // Start is called before the first frame update
    private void Start()
    {
         // First turn is after cross
        _turnState = MarkType.Cross;
        // Singleton initialization
        _instance = this;
        // Initialize 
        _linesList = new List<GameObject>();
        _gameObjectsMarkCells2DList = new List<List<GameObject>>();
        _marksTypes2DList = new List<List<MarkType>>();
        CreateField();
    }

    #endregion

    #region Methods

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
            GameObject spawnedXLine = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.BuildLine, _buildLine);
            GameObject spawnedYline = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.BuildLine, _buildLine);
            // Scale object
            spawnedXLine.transform.localScale = new Vector3(adaptFieldSize / 4 / 3, adaptFieldSize * _fieldSize);
            spawnedYline.transform.localScale = new Vector3(spawnedXLine.transform.localScale.y, spawnedXLine.transform.localScale.x);
            // Setting position of the line
            spawnedXLine.transform.position = new Vector3(xLineStartPos + (lineIt * adaptFieldSize), yCenterPos);
            spawnedYline.transform.position = new Vector3(xCenterPos, yLineStartPos - (lineIt * adaptFieldSize));
            // Add lines to the list
            _linesList.Add(spawnedXLine);
            _linesList.Add(spawnedYline);
        }
        // Create field, depending on the entered size
        for (int y = 0; y < _fieldSize; y++)
        {
            // Add new sub list 
            _gameObjectsMarkCells2DList.Add(new List<GameObject>());
            _marksTypes2DList.Add(new List<MarkType>());
            for (int x = 0; x < _fieldSize; x++)
            {
                // Spawn mark cell
                GameObject spawnedMarkCell = SpawnManager.GetInstance().SpawnObject(SpawnManager.PoolType.MarkCell, _markCell);
                // Rescale object
                spawnedMarkCell.transform.localScale = new Vector3(adaptFieldSize, adaptFieldSize);
                // Set it on new position
                spawnedMarkCell.transform.position = new Vector2(xStartPos + x * spawnedMarkCell.transform.localScale.x, yStartPos - y * spawnedMarkCell.transform.localScale.y);
                // Add cell to list
                _gameObjectsMarkCells2DList[y].Add(spawnedMarkCell);
                // Add mark state
                _marksTypes2DList[y].Add(MarkType.Empty);
            }
        }
    }

    /// <summary>
    /// Disables whole field
    /// </summary>
    private void DisableField()
    {
        for (int i = 0; i < _linesList.Count; i++)
        {
            _linesList[i].SetActive(false);
        }
        for (int i = 0; i < _gameObjectsMarkCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarkCells2DList.Count; j++)
            {
                _gameObjectsMarkCells2DList[i][j].SetActive(false);
            }
        }
    }

    /// <summary>
    /// Checks whole field for win condition
    /// </summary>
    /// <returns></returns>
    private bool CheckFieldForWinCondition()
    {
        for (int i = 0; i < _gameObjectsMarkCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarkCells2DList.Count; j++)
            {
                if (CheckPointForWinCondition(i, j, _marksTypes2DList[i][j]) == true)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Checks field for condition of winning
    /// </summary>
    /// <param name="x">Start X point</param>
    /// <param name="y">Start Y point</param>
    /// <param name="markType">Mark Type</param>
    /// <param name="winCount">Count of matched marks in the row</param>
    /// <param name="nextX">Next x to check for the match</param>
    /// <param name="nextY">Next y to check for the match</param>
    /// <param name="isReverse">For reverse check in recursion</param>
    /// <returns></returns>
    private bool CheckPointForWinCondition(int y, int x, MarkType markType,
        int winCount = 1, int nextX = 0, int nextY = 0, bool isReverse = false)
    {
        // Check, if X and Y are correct
        if (y < 0 || y >= _marksTypes2DList.Count
            || x < 0 || x >= _marksTypes2DList.Count)
        {
            return false;
        }
        // If mark type is empty or is not equal to ours,
        // there is nore reason to analyze this point further
        if (markType == MarkType.Empty || _marksTypes2DList[y][x] != markType)
        {
            return false;
        }
        // Next, we analyze all surroundings of the point
        // (If nextX and nextY equals zero, this point counts as starting one)
        if (nextX == 0 && nextY == 0)
        {
            for (int yIt = y - 1; yIt <= y + 1; yIt++)
            {
                for (int xIt = x - 1; xIt <= x + 1; xIt++)
                {
                    // Ignore same point as ours
                    if (xIt == x && yIt == y) { continue; }
                    // Check, if we are in the boundaries
                    if (yIt >= 0 && yIt < _marksTypes2DList.Count
                        && xIt >= 0 && xIt < _marksTypes2DList.Count)
                    {
                        // Check next point for win condition
                        if (CheckPointForWinCondition(yIt, xIt, markType, winCount + 1, xIt - x, yIt - y) == true)
                        {
                            Debug.Log("Win condition reached! From point [" + y + "][" + x + "] towards point"
                                + "[" + yIt + "]" + "[" + xIt + "]");
                            return true;
                        }
                    }
                }
            }
        }
        // Else, we go to the destination (nextX and nextY)
        else
        {
            // If, we reached our win row quantity - return true
            if (winCount == _winRowQuant)
            {
                return true;
            }
            // Else, go further 
            else
            {
                // Check forward and backwards for marks
                bool forwardBackwardCheck;
                // Check forward
                forwardBackwardCheck = CheckPointForWinCondition(y + nextY, x + nextX, markType, winCount + 1, nextX, nextY, isReverse);
                if (forwardBackwardCheck == false && isReverse == false)
                {
                    // Decrease win count, because we go backwards
                    winCount = 1;
                    // Check backwards
                    forwardBackwardCheck = CheckPointForWinCondition(y - nextY, x - nextX, markType, winCount + 1, -nextX, -nextY, true);
                }
                // Return result
                return forwardBackwardCheck;
            }
        }
        return false;
    }

    /// <summary>
    /// Marks cell refered to the gameobject, with corresponding mark type
    /// </summary>
    /// <param name="markCell"></param>
    /// <param name="markType"></param>
    public void MarkCellInField(GameObject markCell)
    {
        MarkCell markCellScript = null;
        // Checking, if there is already some mark on this cell
        try
        {
            markCellScript = markCell.GetComponent<MarkCell>();
            if (markCellScript.MarkType != MarkType.Empty)
            {
                return;
            }
        }
        catch (Exception error)
        {
            Debug.LogError(error.Message);
        }
        // Checking object
        for (int i = 0; i < _gameObjectsMarkCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarkCells2DList.Count; j++)
            {
                if (markCell.Equals(_gameObjectsMarkCells2DList[i][j]))
                {
                    try
                    {
                        markCellScript.MarkType = TurnState;
                        _marksTypes2DList[i][j] = _turnState;
                        if (CheckPointForWinCondition(i,j,TurnState) == true)
                        {
                            Debug.Log(TurnState.ToString() + " has won!");
                        }
                        break;
                    }
                    catch (Exception error)
                    {
                        Debug.LogError(error.Message);
                    }
                }
            }
            if (markCellScript.MarkType == TurnState)
            {
                break;
            }
        }
        // Changing next turn mark
        if (_turnState == MarkType.Cross) { _turnState = MarkType.Circle; }
        else { _turnState = MarkType.Cross; }
    }

    #endregion
}
