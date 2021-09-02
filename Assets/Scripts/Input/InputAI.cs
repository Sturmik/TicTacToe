using System.Collections.Generic;
using UnityEngine;

// Class for AI input
public class InputAI : InputBase
{
    // Turn check for AI 
    protected enum TurnCheck
    {
        Opponent,
        AI
    }

    // This struct helps AI to decide, which cells are more importnat to mark
    protected struct PriorityList
    {
        // If there is cell with more priority,
        // this variable will change its value to it.
        public int priorityLevel;
        // List of cells, with given priority level, if the last thing will be exceeded -
        // list will be cleaned and refilled with cells with according priority level
        public List<GameObject> markCellList;
    }

    #region Variables

    // Priority list
    private PriorityList _priorityList;

    #endregion

    #region Unity

    protected void Awake()
    {
        // Initialize variable
        _priorityList = new PriorityList();
        _priorityList.markCellList = new List<GameObject>();
    }

    protected override void OnDisable()
    {
        // Unsubscribe
        _fieldControl.CellIsMarked -= CheckTurn;
        _userMarkType = MarkType.Empty;
        // Call base on disable
        base.OnDisable();
    }

    #endregion

    #region Methods

    // Sets input to field
    protected override void SetInputToField(GameObject obj, MarkType inputMarkType, FieldControlManager fieldControl)
    {
        if (obj != gameObject) return;
        base.SetInputToField(obj, inputMarkType, fieldControl);
        // First unsubscribe(just in case) and then subscribe to the event
        _fieldControl.CellIsMarked -= CheckTurn;
        _fieldControl.CellIsMarked += CheckTurn;
    }

    // Checks, if it is turn of AI
    private void CheckTurn()
    {
        // If it is not our turn - do nothing
        if (_userMarkType != _fieldControl.TurnState ||
            _userMarkType == MarkType.Empty ||
            _fieldControl.IsGameOverConditionReached) 
        { 
            return;
        }
        // Else start to analyze
        AnalyzeAndMarkField();
    }

    // Makes analyze of the field and marks one the most prioritized cells
    private void AnalyzeAndMarkField()
    { 
        // Variable for comparing priority level
        int tempPriorityLevel = 0;
        // Reinitialize variables
        _priorityList.markCellList.Clear();
        _priorityList.priorityLevel = tempPriorityLevel;
        // We go through each cell and analyze it
        for (int i = 0; i < _fieldControl.MarksTypes2DList.Count; i++)
        {
            for (int j = 0; j < _fieldControl.MarksTypes2DList.Count; j++)
            {
                // If cell is not empty there is no reason to analyze it
                if (_fieldControl.MarksTypes2DList[i][j] != MarkType.Empty)
                {
                    continue;
                }
                // First, we analyze our opponent's marks and after that our ones
                // CHECK = 0 -> Opponent
                // CHECK = 1 -> AI
                for (TurnCheck turnCheck = TurnCheck.Opponent; turnCheck <= TurnCheck.AI; turnCheck++)
                {
                    // We mark sequentially empty fields with our/opponent mark and update priority
                    // list according to results of previous actions
                    if (turnCheck == TurnCheck.Opponent)
                    {
                        _fieldControl.MarksTypes2DList[i][j] = _userMarkType == MarkType.Cross ? MarkType.Circle : MarkType.Cross;
                    }
                    else
                    {
                        _fieldControl.MarksTypes2DList[i][j] = _userMarkType;
                    }
                    // Start analyze
                    _ = _fieldControl.CheckPointForGameOverCondition(i, j, _fieldControl.MarksTypes2DList[i][j], ref tempPriorityLevel);
                    // if priority level is the same as win row quant and we check cells with our mark then mark this cell immediately
                    if (tempPriorityLevel == _fieldControl.WinRowQuant && turnCheck == TurnCheck.AI)
                    {
                        // Mark this field as empty
                        _fieldControl.MarksTypes2DList[i][j] = MarkType.Empty;
                        // Actually mark it
                        _fieldControl.MarkCellInField(_fieldControl.GameObjectsMarksCells2DList[i][j]);
                        // End function
                        return;
                    }
                    // Else check priority level, if it has changed, update the list
                    else if (tempPriorityLevel > _priorityList.priorityLevel)
                    {
                        _priorityList.priorityLevel = tempPriorityLevel;
                        _priorityList.markCellList.Clear();
                        _priorityList.markCellList.Add(_fieldControl.GameObjectsMarksCells2DList[i][j]);
                    }
                    // Else if priority level is the same as recent in the list, just add this cell to it
                    else if (tempPriorityLevel == _priorityList.priorityLevel)
                    {
                        _priorityList.markCellList.Add(_fieldControl.GameObjectsMarksCells2DList[i][j]);
                    }
                    // Set field to being empty again
                    _fieldControl.MarksTypes2DList[i][j] = MarkType.Empty;
                }
            }
        }
        // After we have analyzed all cells, we randomly choose cell in our priority list and actually mark it
        if (_priorityList.markCellList.Count > 0)
        {
            int randomCell = Random.Range(0, _priorityList.markCellList.Count);
            _fieldControl.MarkCellInField(_priorityList.markCellList[randomCell]);
        }

    }

    #endregion
}
