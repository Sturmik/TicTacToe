using System;
using System.Collections.Generic;
using UnityEngine;

// Class for field building and full control over it
public class FieldControlManager : MonoBehaviour
{
    // Dimension type
    public enum DimensionType
    {
        Dimension2D,
        Dimension3D
    }

    #region Events

    // Event, which signals that field was created
    public event Action<DimensionType> FieldIsCreated;
    // Event, which signals that field was marked
    public event Action CellIsMarked;
    // Event, which signals that game is over
    public event Action GameOverIsReached;

    // Event, which sets field inputs
    public static event Action<GameObject, MarkType, FieldControlManager> SetFieldInput;

    // Event, which returns mark type of specific gameobject mark cell
    public delegate void GetMarkType(GameObject obj, ref MarkType markType);
    public static GetMarkType GetCellMarkType;
    // Event, which sets mark type for specific game object
    public static event Action<GameObject, MarkType> SetCellMarkType;
    // Event, which signals specific mark cell gameobject to update itself to win condition state
    public static event Action<GameObject> SetCellAsWinning;

    #endregion

    #region Variables

    [Header("Field build blocks")]
    // Mark cell and build line 2D
    [SerializeField] private GameObject _markCell2D;
    [SerializeField] private GameObject _buildLine2D;
    // Mark cell and build line 3D
    [SerializeField] private GameObject _markCell3D;
    [SerializeField] private GameObject _buildLine3D;

    // Mark field prefab
    private GameObject _markCell;
    // Building lines, which will form the field
    private GameObject _buildLine;

    // Dimension type of the field
    private DimensionType _fieldDimensionType;
    /// <summary>
    /// Dimension type of the field
    /// </summary>
    public DimensionType FieldDimensionType { get { return _fieldDimensionType; } }
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
    // X center position
    private float _xCenterPos;
    // Y center position
    private float _yCenterPos;

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
    public void SetInputs(GameObject firstInputObject, GameObject secondInputObject,
        MarkType _firstObjectMark = MarkType.Cross, MarkType _secondObjectMark = MarkType.Circle)
    {
        // Check if objects are null referenced
        if (firstInputObject == null || secondInputObject == null)
        {
            Debug.LogError("Objects passed to the input of the field are null referenced!");
            return;
        }
        // Check gameobjects
        if (firstInputObject.GetComponent<InputBase>() == null
            || secondInputObject.GetComponent<InputBase>() == null)
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
        // Check if marks are correct
        if (_firstObjectMark != MarkType.Cross && _firstObjectMark != MarkType.Circle
            || _secondObjectMark != MarkType.Cross && _secondObjectMark != MarkType.Circle)
        {
            _firstObjectMark = MarkType.Cross;
            _secondObjectMark = MarkType.Circle;
        }
        // Set inputs
        SetFieldInput(_firstInputCheckGameObject, _firstObjectMark, this);
        SetFieldInput(_secondInputCheckGameObject, _secondObjectMark, this);
        // Invoke event
        CellIsMarked?.Invoke();
    }

    /// <summary>
    /// Creates field for TicTacToe play 
    /// </summary>
    /// <param name="firstTurn">Who will have first turn</param>
    /// <param name="winRowQuant"></param>
    /// <param name="fieldSize"></param>
    /// <param name="markFieldSizeOnScreen"></param>
    /// <param name="xCenterPos"></param>
    /// <param name="yCenterPos"></param>
    public void CreateField(SpawnManager spawnManager, MarkType firstTurn = MarkType.Cross, DimensionType dimensionType = DimensionType.Dimension2D,
        int winRowQuant = 3, int fieldSize = 3, float markFieldSizeOnScreen = 2, float xCenterPos = 0, float yCenterPos = 0, float zCenterPos = 0)
    {
        // Disable field before creating new one
        DisableField();
        // Setting field parameters
        if (fieldSize < Globals.MIN_FIELD_SIZE) { fieldSize = Globals.MIN_FIELD_SIZE; }
        if (fieldSize > Globals.MAX_FIELD_SIZE) { fieldSize = Globals.MAX_FIELD_SIZE; }
        if (winRowQuant > fieldSize){winRowQuant = fieldSize; }
        _turnState = firstTurn;
        _winRowQuant = winRowQuant;
        _fieldSize = fieldSize;
        _markFieldSizeOnScreen = markFieldSizeOnScreen;
        _xCenterPos = xCenterPos;
        _yCenterPos = yCenterPos;
        _isGameOverConditionReached = false;
        _fieldDimensionType = dimensionType;
        // Spawn manager pool types for further usage
        SpawnManager.PoolType linePoolType;
        SpawnManager.PoolType markCellPoolType;
        // Setting dimension type
        switch(_fieldDimensionType)
        {
            default:
                // 2D
                case DimensionType.Dimension2D:
                _buildLine = _buildLine2D;
                linePoolType = SpawnManager.PoolType.BuildLine2D;
                _markCell = _markCell2D;
                markCellPoolType = SpawnManager.PoolType.MarkCell2D;
                break;
            // 3D
            case DimensionType.Dimension3D:
                _buildLine = _buildLine3D;
                linePoolType = SpawnManager.PoolType.BuildLine3D;
                _markCell = _markCell3D;
                markCellPoolType = SpawnManager.PoolType.MarkCell3D;
                break;
        }
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
        // Draw vertical and horizontal lines
        for (int lineIt = 0; lineIt < _fieldSize - 1; lineIt++)
        {
            // Spawn line
            GameObject spawnedXLine = spawnManager.SpawnObject(linePoolType, _buildLine);
            GameObject spawnedYLine = spawnManager.SpawnObject(linePoolType, _buildLine);
            // Scale object
            spawnedXLine.transform.localScale
                = new Vector3(adaptFieldSize / 4 / 8, adaptFieldSize * _fieldSize);
            spawnedYLine.transform.localScale 
                = new Vector3(spawnedXLine.transform.localScale.y, spawnedXLine.transform.localScale.x);
            // Setting position of the line
            spawnedXLine.transform.position = new Vector3(xLineStartPos + (lineIt * adaptFieldSize), yCenterPos, zCenterPos);
            spawnedYLine.transform.position = new Vector3(xCenterPos, yLineStartPos - (lineIt * adaptFieldSize), zCenterPos);
            // Add lines to the list
            _linesList.Add(spawnedXLine);
            _linesList.Add(spawnedYLine);
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
                GameObject spawnedMarkCell = spawnManager.SpawnObject(markCellPoolType, _markCell);
                // Rescale object
                spawnedMarkCell.transform.localScale 
                    = new Vector3(adaptFieldSize, adaptFieldSize, spawnedMarkCell.transform.localScale.z);
                // Set it on new position
                spawnedMarkCell.transform.position 
                    = new Vector3(xStartPos + x * spawnedMarkCell.transform.localScale.x, yStartPos - y * spawnedMarkCell.transform.localScale.y, zCenterPos);
                // Add cell to list
                _gameObjectsMarksCells2DList[y].Add(spawnedMarkCell);
                // Add mark state
                _marksTypes2DList[y].Add(MarkType.Empty);
            }
        }
        // Invoke events
        CellIsMarked?.Invoke();
        FieldIsCreated?.Invoke(_fieldDimensionType);
    }

    /// <summary>
    /// Recreates field with the same data
    /// </summary>
    public void RecreateField(SpawnManager spawnManager, MarkType firstTurn)
    {
        CreateField(spawnManager, firstTurn, _fieldDimensionType, _winRowQuant, _fieldSize, _markFieldSizeOnScreen, _xCenterPos, _yCenterPos);
    }

    /// <summary>
    /// Disables whole field and detaches input, if needed
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
    /// <returns>Does point create condition of winning</returns>
    public bool CheckPointForGameOverCondition(int y, int x, MarkType markType, ref int maxMarkCount, bool toMarkWinRow = false)
    {
        // We analyze all surroundings of the point
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
                    if (CheckRowForGameOverCondition(yIt, xIt, markType, ref maxMarkCount, toMarkWinRow, xIt - x, yIt - y) == true)
                    {
                        // Mark cell as winning one via script 
                        if (toMarkWinRow == true)
                        {
                            SetCellAsWinning(_gameObjectsMarksCells2DList[y][x]);
                        }
                        Debug.Log("(Possible) Win condition from point [" + y + "][" + x + "] towards point"
                            + "[" + yIt + "]" + "[" + xIt + "]" + " with " + maxMarkCount + " marks");
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // Recursive function for row analyze
    private bool CheckRowForGameOverCondition(int y, int x, MarkType markType, ref int maxMarkCount, bool toMarkWinRow = false,
        int nextX = 0, int nextY = 0, int markCount = 1, bool isReverse = false)
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
        // Update max mark count
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
                SetCellAsWinning(_gameObjectsMarksCells2DList[y][x]);
            }
            return true;
        }
        // Check forward and backwards for marks
        bool forwardBackwardCheck;
        // Check forward
        forwardBackwardCheck = CheckRowForGameOverCondition(y + nextY, x + nextX, markType, ref maxMarkCount, toMarkWinRow, nextX, nextY, markCount + 1, isReverse);
        if (forwardBackwardCheck == false && isReverse == false)
        {
            // Decrease win count, because we go backwards
            markCount = 1;
            // Check backwards
            forwardBackwardCheck = CheckRowForGameOverCondition(y - nextY, x - nextX, markType, ref maxMarkCount, toMarkWinRow, -nextX, -nextY, markCount + 1, true);
        }
        // Check if we were asked to mark win row
        if (toMarkWinRow == true)
        {
            if (forwardBackwardCheck == true)
            {
                SetCellAsWinning(_gameObjectsMarksCells2DList[y][x]);
            }
        }
        // Return result
        return forwardBackwardCheck;
    }

    /// <summary>
    /// Marks cell refered to the gameobject, with corresponding mark type
    /// </summary>
    /// <param name="markCell">Which cell to mark</param>
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
        MarkType markCellTypeCheck = MarkType.Empty;
        GetCellMarkType(markCell,ref markCellTypeCheck);
        // Checking, if there is already some mark on this cell
        if (markCellTypeCheck != MarkType.Empty)
        {
            return;
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
                        SetCellMarkType(_gameObjectsMarksCells2DList[i][j],TurnState);
                        _marksTypes2DList[i][j] = _turnState;
                        // Checking win condition
                        _isGameOverConditionReached = CheckPointForGameOverCondition(i, j, TurnState, ref maxQuantityOfMarks);
                        if (IsGameOverConditionReached == true)
                        {
                            // Win
                            Debug.Log(TurnState.ToString() + " has won!");
                            // Ask to mark win row
                            CheckPointForGameOverCondition(i, j, TurnState, ref maxQuantityOfMarks, true);
                            // Invoke game over event
                            GameOverIsReached?.Invoke();
                        }
                        // Check for draw
                        else if (CheckFieldForDrawCondition() == true)
                        {
                            Debug.Log("Draw!");
                            // Invoke game over method
                            GameOverIsReached?.Invoke();
                        }
                        // Else change turn
                        else
                        {
                            // Changing next turn mark
                            TurnChange();
                        }
                        Debug.Log(TurnState.ToString() + " in a row: " + maxQuantityOfMarks);
                        // Calling event
                        CellIsMarked?.Invoke();
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

    /// <summary>
    /// Changes turn
    /// </summary>
    private void TurnChange()
    {
        if (_turnState == MarkType.Cross) { _turnState = MarkType.Circle; }
        else { _turnState = MarkType.Cross; }
    }

    #endregion
}