using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Class for field building and full control over it
public class FieldControlManager : MonoBehaviour
{
    #region Events

    // Event, which signals that field was marked
    public event Action FieldIsMarked;

    #endregion

    #region Variables

    [Header("Field default build blocks")]
    // Mark field prefab
    [SerializeField] private GameObject _markCell;
    // Building lines, which will form the field
    [SerializeField] private GameObject _buildLine;

    // Win row quantity
    private int _winRowQuant;
    /// <summary>
    /// Return number of marks in a row required for win
    /// </summary>
    public int WinRowQuant { get { return _winRowQuant; } }
    // Field size (number of cells _fieldSize x _fieldSize)
    private int _fieldSize;
    // MarkField size
    private float _markFieldSizeOnScreen;

    // List for lines
    private List<GameObject> _linesList;
    // List for work with mark cells
    private List<List<GameObject>> _gameObjectsMarksCells2DList;
    /// <summary>
    /// List of mark fields in the field
    /// </summary>
    public List<List<GameObject>> GameObjectsMarksCells2DList { get { return _gameObjectsMarksCells2DList; } }

    // List, which holds state of cells
    private List<List<MarkType>> _marksTypes2DList;
    /// <summary>
    /// List of mark types in the field
    /// </summary>
    public List<List<MarkType>> MarksTypes2DList { get { return _marksTypes2DList; } }

    // Defines turn state
    private MarkType _turnState;
    /// <summary>
    /// Returns mark, which must be placed now
    /// </summary>
    public  MarkType TurnState
        { get { return _turnState; }}

    // Win condition defines, if the game was finished
    private bool _isGameOverConditionReached;
    /// <summary>
    /// Is win condition reached (if it is true, you can see who has won by getting TurnState property)
    /// </summary>
    public bool IsGameOverConditionReached { get { return _isGameOverConditionReached; } }

    // Input check for the field and the game objects, which they are attached to 
    private GameObject _firstInputCheckGameObject;
    private GameObject _secondInputCheckGameObject;

    #endregion

    #region Unity

    private void Awake()
    {
        // Initialize lists
        _linesList = new List<GameObject>();
        _gameObjectsMarksCells2DList = new List<List<GameObject>>();
        _marksTypes2DList = new List<List<MarkType>>();
    }

    private void OnDisable()
    {
        DisableField(true);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sets up inputs for this field
    /// </summary>
    /// <param name="firstInputObject">First input(will have the right to go first)</param>
    /// <param name="secondInputObject">Second input</param>
    public void SetInputs(GameObject firstInputObject, GameObject secondInputObject)
    {
        // Check if objects are null referenced
        if (firstInputObject == null || secondInputObject == null)
        {
            Debug.LogError("Objects passed to the input of the field are null referenced!");
            return;
        }
        // Check gameobjects
        if (firstInputObject.GetComponent<InputCheck>() == null
            || secondInputObject.GetComponent<InputCheck>() == null)
        {
            Debug.LogError("Objects, which were passed to the input of the field do not have InputCheck inherited script!");
            return;
        }
        // Deactivate previous inputs, if there are ones
        if(_firstInputCheckGameObject != null || _secondInputCheckGameObject != null)
        {
            _firstInputCheckGameObject.SetActive(false);
            _secondInputCheckGameObject.SetActive(false);
        }
        // Attach new inputs and set their marks
        _firstInputCheckGameObject = firstInputObject;
        _secondInputCheckGameObject = secondInputObject;
        UpdateInputsTurnStates();
        // Invoke event
        FieldIsMarked?.Invoke();
    }

    // Updates turn states of inputs
    public void UpdateInputsTurnStates()
    {
        if (_firstInputCheckGameObject != null || _secondInputCheckGameObject != null)
        {
            _firstInputCheckGameObject.GetComponent<InputCheck>().SetInputToField(TurnState, this);
            _secondInputCheckGameObject.GetComponent<InputCheck>().SetInputToField(TurnState == MarkType.Cross ? MarkType.Circle : MarkType.Cross, this);
        }
    }

    /// <summary>
    /// Creates field for TicTacToe play 
    /// </summary>
    /// <param name="xStartPos">X center position</param>
    /// <param name="yStartPos">Y center position</param>
    public void CreateField(MarkType firstTurn = MarkType.Cross, int winRowQuant = 3, int fieldSize = 3,
        float markFieldSizeOnScreen = 2, float xCenterPos = 0, float yCenterPos = 0)
    {
        // Disable field before creating new one
        DisableField();
        // Setting field parameters
        if (fieldSize < Globals.MIN_FIELD_SIZE) { fieldSize = Globals.MIN_FIELD_SIZE; }
        if (fieldSize > Globals.MAX_FIELD_SIZE) { fieldSize = Globals.MAX_FIELD_SIZE; }
        if (winRowQuant > fieldSize){winRowQuant = fieldSize; }
        _turnState = firstTurn;
        _winRowQuant = winRowQuant;
        _markFieldSizeOnScreen = markFieldSizeOnScreen;
        _fieldSize = fieldSize;
        _isGameOverConditionReached = false;
        // Variables for line draw (will be used later in the function)
        float xLineStartPos = xCenterPos;
        float yLineStartPos = yCenterPos;
        int lineOffset = 0;
        // Set mark field scale, depending on field size.
        // Larger the field size, the smaller mark field is.
        float adaptFieldSize = _markFieldSizeOnScreen / ((_markFieldSizeOnScreen * _fieldSize) / Globals.MIN_FIELD_SIZE);
        adaptFieldSize = _markFieldSizeOnScreen * adaptFieldSize;
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
            spawnedXLine.transform.localScale = new Vector3(adaptFieldSize / 4 / 8, adaptFieldSize * _fieldSize);
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
            _gameObjectsMarksCells2DList.Add(new List<GameObject>());
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
                _gameObjectsMarksCells2DList[y].Add(spawnedMarkCell);
                // Add mark state
                _marksTypes2DList[y].Add(MarkType.Empty);
            }
        }
        // Update inputs
        UpdateInputsTurnStates();
        // Invoke event
        FieldIsMarked?.Invoke();
    }

    /// <summary>
    /// Disables whole field
    /// </summary>
    public void DisableField(bool detachInputs = false)
    {
        // Clear all lists
        if (_linesList == null ||
            _gameObjectsMarksCells2DList == null ||
            _marksTypes2DList == null)
        {
            return;
        }
        // Disable all squares and lines
        for (int i = 0; i < _linesList.Count; i++)
        {
            _linesList[i].SetActive(false);
        }
        for (int i = 0; i < _gameObjectsMarksCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarksCells2DList.Count; j++)
            {
                _gameObjectsMarksCells2DList[i][j].SetActive(false);
            }
        }
        // Clear lists
        _linesList.Clear();
        _gameObjectsMarksCells2DList.Clear();
        _marksTypes2DList.Clear();
        // Detach inputs
        if (detachInputs == true)
        {
            if (_firstInputCheckGameObject != null
                && _secondInputCheckGameObject != null)
            {
                _firstInputCheckGameObject.SetActive(false);
                _secondInputCheckGameObject.SetActive(false);
                _firstInputCheckGameObject = _secondInputCheckGameObject = null;
            }
        }
    }

    /// <summary>
    /// Checks whole field for win condition
    /// </summary>
    /// <returns></returns>
    private bool CheckFieldForDrawCondition()
    {
        // Variable for storing max amount of marks, which go through this point
        for (int i = 0; i < _gameObjectsMarksCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarksCells2DList.Count; j++)
            {
                if (_marksTypes2DList[i][j] == MarkType.Empty)
                {
                    return false;
                }
            }
        }
        // If we have draw, we set it as game over condition
        _isGameOverConditionReached = true;
        // And then set turn state to empty, which means, that it is draw
        _turnState = MarkType.Empty;
        return true;
    }

    /// <summary>
    /// Checks field for condition of winning
    /// </summary>
    /// <param name="x">Start X point</param>
    /// <param name="y">Start Y point</param>
    /// <param name="markType">Mark Type</param>
    /// <param name="maxMarkCount">Optional variable for storing max quantity of specific type of marks in a row</param>
    /// <param name="toMarkWinRow">Optional variable to ask, if we want to mark win row</param>
    /// <param name="markCount">Count of matched marks in the row</param>
    /// <param name="nextX">Next x to check for the match</param>
    /// <param name="nextY">Next y to check for the match</param>
    /// <param name="isReverse">For reverse check in recursion</param>
    /// <returns>Does point create condition of winning</returns>
    public bool CheckPointForGameOverCondition(int y, int x, MarkType markType, ref int maxMarkCount, bool toMarkWinRow = false,
        int markCount = 1, int nextX = 0, int nextY = 0, bool isReverse = false)
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
            // Variable for storing max amount of marks, which go through this point
            maxMarkCount = 1;
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
                        if (CheckPointForGameOverCondition(yIt, xIt, markType,ref maxMarkCount, toMarkWinRow, markCount + 1, xIt - x, yIt - y) == true)
                        {
                            // Mark cell as winning one via script 
                            if (toMarkWinRow == true)
                            {
                                MarkCell markCellScript = _gameObjectsMarksCells2DList[y][x].GetComponent<MarkCell>();
                                if (markCellScript.MarkType == MarkType.Cross) { markCellScript.MarkType = MarkType.CrossWin; }
                                if (markCellScript.MarkType == MarkType.Circle) { markCellScript.MarkType = MarkType.CircleWin; }
                            }
                            Debug.Log("(Possible) Win condition from point [" + y + "][" + x + "] towards point"
                                + "[" + yIt + "]" + "[" + xIt + "]" + " with " + maxMarkCount + " marks");
                            return true;
                        }
                    }
                }
            }
        }
        // Else, we go to the destination (nextX and nextY)
        else
        {
            if (markCount >= maxMarkCount)
            {
                maxMarkCount = markCount;
            }
            // If, we reached our win row quantity - return true
            if (markCount == _winRowQuant)
            {
                // Check if we were asked to mark win row
                if (toMarkWinRow == true)
                {
                    // Mark cell as winning one via script 
                    MarkCell markCellScript = _gameObjectsMarksCells2DList[y][x].GetComponent<MarkCell>();
                    if (markCellScript.MarkType == MarkType.Cross) { markCellScript.MarkType = MarkType.CrossWin; }
                    if (markCellScript.MarkType == MarkType.Circle) { markCellScript.MarkType = MarkType.CircleWin; }
                }
                return true;
            }
            // Else, go further 
            else
            {
                // Check forward and backwards for marks
                bool forwardBackwardCheck;
                // Check forward
                forwardBackwardCheck = CheckPointForGameOverCondition(y + nextY, x + nextX, markType, ref maxMarkCount, toMarkWinRow, markCount + 1, nextX, nextY, isReverse);
                if (forwardBackwardCheck == false && isReverse == false)
                {
                    // Decrease win count, because we go backwards
                    markCount = 1;
                    // Check backwards
                    forwardBackwardCheck = CheckPointForGameOverCondition(y - nextY, x - nextX, markType, ref maxMarkCount, toMarkWinRow, markCount + 1, -nextX, -nextY, true);
                }
                // Check if we were asked to mark win row
                if (toMarkWinRow == true)
                {
                    if (forwardBackwardCheck == true)
                    {
                        // Mark cell as winning one via script 
                        MarkCell markCellScript = _gameObjectsMarksCells2DList[y][x].GetComponent<MarkCell>();
                        if (markCellScript.MarkType == MarkType.Cross) { markCellScript.MarkType = MarkType.CrossWin; }
                        if (markCellScript.MarkType == MarkType.Circle) { markCellScript.MarkType = MarkType.CircleWin; }
                    }
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
    /// <param name="markCell">Which cell to mark</param>
    /// <param name="inputObject">Who asked to mark the cell</param>
    public void MarkCellInField(GameObject markCell)
    {
        // Check, if win condition is reached
        if (IsGameOverConditionReached == true)
        {
            Debug.Log("You can't mark this field anymore. Win condition has been reached");
            // We don't allow anyone to mark field anymore,
            // because someone has already win
            return;
        }
        // We will need mark cell script to check, if it is possible to mark a field
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
        // Variable for storing max amount of marks, which go through this point
        int maxQuantityOfMarks = 1;
        // Looking for passed gameobject in list
        for (int i = 0; i < _gameObjectsMarksCells2DList.Count; i++)
        {
            for (int j = 0; j < _gameObjectsMarksCells2DList.Count; j++)
            {
                if (markCell.Equals(_gameObjectsMarksCells2DList[i][j]))
                {
                    try
                    {
                        // Changing mark type of the cell according to turn state
                        markCellScript.MarkType = TurnState;
                        _marksTypes2DList[i][j] = _turnState;
                        // Checking win condition
                        _isGameOverConditionReached = CheckPointForGameOverCondition(i, j, TurnState, ref maxQuantityOfMarks);
                        if (IsGameOverConditionReached == true)
                        {
                            // Win
                            Debug.Log(TurnState.ToString() + " has won!");
                            // Ask to mark win row
                            CheckPointForGameOverCondition(i, j, TurnState, ref maxQuantityOfMarks, true);
                            return;
                        }
                        // Check for draw
                        else if (CheckFieldForDrawCondition() == true)
                        {
                            Debug.Log("Draw!");
                            return;
                        }
                        Debug.Log(TurnState.ToString() + " in a row: " + maxQuantityOfMarks);
                        // Changing next turn mark
                        if (_turnState == MarkType.Cross) { _turnState = MarkType.Circle; }
                        else { _turnState = MarkType.Cross; }
                        // Calling event
                        FieldIsMarked?.Invoke();
                        return;
                    }
                    catch (Exception error)
                    {
                        Debug.LogError(error.Message);
                    }
                }
            }
        }
    }

    #endregion
}
