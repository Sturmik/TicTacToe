using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Global variables
/// </summary>
public static class Globals
{
    #region Constants

    // Field size
    public const int MIN_FIELD_SIZE = 3;
    public const int MAX_FIELD_SIZE = 10;

    #endregion

    #region Tags

    public const string MARK_CELL_TAG = "MarkCell";

    #endregion
}

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Input type
    /// </summary>
    public enum InputType
    {
        User,
        AI
    }

    /// <summary>
    /// Player mark type
    /// </summary>
    public enum PlayerMark
    {
        CrossMark,
        CircleMark
    }

    #region Event

    // Counter update event
    public event Action CountersUpdate;
    // Turn update event
    public event Action<PlayerMark> TurnUpdate;

    #endregion

    #region Variables

    [Header("Managers")]
    // Spawn manager control
    [SerializeField] private SpawnManager _spawnManagerScript;
    // Field control manager
    [SerializeField] private FieldControlManager _fieldControlScript;

    [Header("Field control prefab")]
    // Win counters
    private int _crossWinCounter;
    /// <summary>
    /// Cross win counter
    /// </summary>
    public int CrossWinCounter { get { return _crossWinCounter; } }
    private int _circleWinCounter;
    /// <summary>
    /// Circle counter
    /// </summary>
    public int CircleWinCounter { get { return _circleWinCounter; } }
    private int _drawCounter;
    /// <summary>
    /// Draw counter
    /// </summary>
    public int DrawCounter { get { return _drawCounter; } }
    // Field mark is changing the right of the first turn to the circle and then to the cross 
    // (from round to round)
    private MarkType _turnCycle;

    [Header("Inputs prefabs")]
    // Human input
    [SerializeField] private GameObject _inputUser;
    // AI input
    [SerializeField] private GameObject _inputAI;

    #endregion

    #region Methods

    /// <summary>
    /// Returns gameobject according to specific type of input
    /// </summary>
    private GameObject GetGameObjectViaInputType(InputType inputType)
    {
        GameObject inputTypeObject = null;
        switch (inputType)
        {
            case InputType.User:
                inputTypeObject = _inputUser;
                break;
            case InputType.AI:
                inputTypeObject = _inputAI;
                break;
        }
        return inputTypeObject;
    }

    /// <summary>
    /// Converts player mark enum to mark type enum
    /// </summary>
    /// <param name="playerMark"></param>
    /// <returns></returns>
    private MarkType ConvertPlayerMarkToMarkType(PlayerMark playerMark)
    {
        return playerMark == PlayerMark.CrossMark ? MarkType.Cross : MarkType.Circle;
    }

    /// <summary>
    /// Converts mark type enum to player mark enum
    /// </summary>
    /// <param name="markType"></param>
    /// <returns></returns>
    private PlayerMark ConvertMarkTypeToPlayerMark(MarkType markType)
    {
        return markType == MarkType.Cross ? PlayerMark.CrossMark : PlayerMark.CircleMark;
    }

    /// <summary>
    /// Sets input to the field
    /// </summary>
    public void SetInput(InputType firstPlayerInputType, PlayerMark firstPlayerMarkType,
        InputType secondPlayerInputType, PlayerMark secondPlayerMarkType)
    {
        // Preparing input game objects
        GameObject firstPlayerObj = GetGameObjectViaInputType(firstPlayerInputType);
        GameObject secondPlayerObj = GetGameObjectViaInputType(secondPlayerInputType);
        // Setting input
        _fieldControlScript.SetInputs(_spawnManagerScript.SpawnObject(firstPlayerInputType == InputType.User ? SpawnManager.PoolType.InputUser : SpawnManager.PoolType.InputAI, firstPlayerObj),
            _spawnManagerScript.SpawnObject(secondPlayerInputType == InputType.User ? SpawnManager.PoolType.InputUser : SpawnManager.PoolType.InputAI, secondPlayerObj),
            ConvertPlayerMarkToMarkType(firstPlayerMarkType), ConvertPlayerMarkToMarkType(secondPlayerMarkType));
    }

    // Starts game
    public void StartGame(PlayerMark firstTurn = PlayerMark.CrossMark, int winRowQuant = 3, int fieldSize = 3,
        float markFieldSizeOnScreen = 2, float xCenterPos = 0, float yCenterPos = 0)
    {
        // Detach previous inputs
        _fieldControlScript.DisableField();
        // Getting recent turn
        _turnCycle = _fieldControlScript.TurnState;
        // Subscribe to field updates
        _fieldControlScript.CellIsMarked -= NextTurn;
        _fieldControlScript.CellIsMarked += NextTurn;
        _fieldControlScript.GameOverIsReached -= UpdateCounters;
        _fieldControlScript.GameOverIsReached += UpdateCounters;
        // Update counters
        ResetCounters();
        // Create field
        _fieldControlScript.CreateField(_spawnManagerScript, ConvertPlayerMarkToMarkType(firstTurn), winRowQuant,
            fieldSize, markFieldSizeOnScreen, xCenterPos, yCenterPos);
    }

    /// <summary>
    /// Entirely stops game and reset counters
    /// </summary>
    public void StopGame()
    {
        // Disable field
        _fieldControlScript.DisableField();
        // Reset counters
        ResetCounters();
    }

    // Resets all counters
    private void ResetCounters()
    {
        _crossWinCounter = _circleWinCounter = _drawCounter = 0;
        // Invoke event for counters update
        CountersUpdate?.Invoke();
    }

    // Updates game data
    private void UpdateCounters()
    {
        // If we finished game
        if (_fieldControlScript.IsGameOverConditionReached == true)
        {
            // Check who has won
            switch (_fieldControlScript.TurnState)
            {
                // Cross win
                case MarkType.Cross:
                    _crossWinCounter++;
                    break;
                // Circle win
                case MarkType.Circle:
                    _circleWinCounter++;
                    break;
                // Draw
                case MarkType.Empty:
                    _drawCounter++;
                    break;
            }
            // Invoke counters update event
            CountersUpdate?.Invoke();
        }
    }

    /// <summary>
    /// Turn update
    /// </summary>
    private void NextTurn()
    {        
        // Invoke turn update event
        TurnUpdate?.Invoke(ConvertMarkTypeToPlayerMark(_fieldControlScript.TurnState));
    }
   
    // Starts next round
    public void NextRound()
    {
        // Change turn cycle
        _turnCycle = _turnCycle == MarkType.Cross ? MarkType.Circle : MarkType.Cross;
        // Create the field
        _fieldControlScript.RecreateField(_spawnManagerScript, _turnCycle);
    }


    #endregion
}
