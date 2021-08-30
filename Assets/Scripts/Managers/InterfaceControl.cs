using UnityEngine;
using UnityEngine.UI;

public class InterfaceControl : MonoBehaviour
{
    /// <summary>
    /// Game types
    /// </summary>
    protected enum GameMode
    {
        HumanVsComputer,
        HumanVsHuman
    }

    #region Variables

    // Constants

    // Variables for demo field representation
    private float DEMO_FIELD_MARK_SIZE_ON_SCREEN = 5;
    private float DEMO_FIELD_X_CENTER_OFFSET = 7;
    private float DEMO_FIELD_Y_CENTER_OFFSET = -1;

    // Variables for field default parameters
    private GameManager.PlayerMark FIRST_TURN_MARK;

    [Header("Player sprites prefabs")]
    // Player sprite image
    [SerializeField] private Sprite _playerSpriteImage;
    // AI sprite image
    [SerializeField] private Sprite _AISpriteImage;

    [Header("Interface prefabs")]
    // Main menu interface
    [SerializeField] private GameObject _mainMenuInterface;
    // Game edit interface
    [SerializeField] private GameObject _gameEditInterface;
    // Game edit field size
    [SerializeField] private Slider _gameEditInterfaceFieldSizeSlider;
    [SerializeField] private Text _gameEditInterfaceFieldSizeText;
    // Game edit field win row quant
    [SerializeField] private Slider _gameEditInterfaceWinRowQuantSlider;
    [SerializeField] private Text _gameEditInterfaceWinRowQuantText;
    // Game area interface
    [SerializeField] private GameObject _gameAreaInterface;
    // Game are recent turn
    // There must be two interface elements. First is cross turn, second is circle turn
    [SerializeField] private GameObject[] _gameAreaRecentTurn;
    // Game area cross player
    [SerializeField] private Image _gameAreaCrossPlayerSprite;
    [SerializeField] private Text _gameAreaCrossPlayerWinCounterText;
    // Game area circle player
    [SerializeField] private Image _gameAreaCirclePlayerSprite;
    [SerializeField] private Text _gameAreaCirclePlayerWinCounterText;
    // Game are draw counter
    [SerializeField] private Text _gameAreaDrawCounterText;
    // Button for next round start
    [SerializeField] private GameObject _nextRoundButton;

    [Header("Game manager")]
    // Game manager script
    [SerializeField] private GameManager _gameManagerScript;
    // Players mark
    private GameManager.PlayerMark _firstPlayerMark;
    private GameManager.PlayerMark _secondPlayerMark;
    // Game mode
    private GameMode _gameMode;
    // Input types
    private GameManager.InputType _firstPlayerInputType;
    private GameManager.InputType _secondPlayerInputType;

    #endregion

    #region Unity

    // Start is called before the first frame update
    void Start()
    {
        // Set default variables
        SetPlayerMarkType();
        SetGameMode();
        // First we activate main menu and disable all others
        BackToMainMenu();
        // Spawn demo field
        MainMenuDemoField();
    }

    #endregion

    #region Methods

    #region InterfaceInteraction

    #region MainMenu

    // Gets you back to the main menu
    public void BackToMainMenu()
    {
        // Disable field
        _gameManagerScript.StopGame();
        // Disable all other interfaces
        _gameEditInterface.SetActive(false);
        _gameAreaInterface.SetActive(false);
        // Activa main menu and demo field
        _mainMenuInterface.SetActive(true);
        // Set input back, disable field and detach input
        MainMenuDemoField();
    }

    // Main menu demo field, where AI will fight with each other 
    // It is made only for showcase, can be called again by pushing title button
    public void MainMenuDemoField()
    {
        // Randomizing the field size
        int fieldSize = Random.Range(Globals.MIN_FIELD_SIZE + 1, Globals.MAX_FIELD_SIZE);
        // Set AI input
        _gameManagerScript.SetInput(GameManager.InputType.AI, GameManager.PlayerMark.CrossMark,
            GameManager.InputType.AI, GameManager.PlayerMark.CircleMark);
        // Start game
        _gameManagerScript.StartGame(GameManager.PlayerMark.CrossMark, fieldSize, fieldSize,
            DEMO_FIELD_MARK_SIZE_ON_SCREEN, DEMO_FIELD_X_CENTER_OFFSET, DEMO_FIELD_Y_CENTER_OFFSET);
    }

    // Play button method
    public void PlayButton()
    {
        // Deactivate main menu
        _mainMenuInterface.SetActive(false);
        // Disable demo field
        _gameManagerScript.StopGame();
        // Activate game edit
        _gameEditInterface.SetActive(true);
    }

    // Exit button method
    public void ExitButton()
    {
        // Quit the game
        Application.Quit();
    }

    #endregion

    #region GameMode

    // Sets player mark type
    public void SetPlayerMarkType(int markType = 0)
    {
        // Convert int to enum
        _firstPlayerMark = (GameManager.PlayerMark)markType;
        // Set another player mark to opposite
        _secondPlayerMark = _firstPlayerMark == GameManager.PlayerMark.CrossMark ? GameManager.PlayerMark.CircleMark : GameManager.PlayerMark.CrossMark;
        // Update game mode
        SetGameMode((int)_gameMode);
    }

    // Sets player game mode
    public void SetGameMode(int gameMode = 0)
    {
        // Convert int to enum
        _gameMode = (GameMode)gameMode;
        // Set input types according to game mode
        switch(_gameMode)
        {
            case GameMode.HumanVsComputer:
                _firstPlayerInputType = GameManager.InputType.User;
                _secondPlayerInputType = GameManager.InputType.AI;
                // Correct placement of human sprite
                if (_firstPlayerMark == GameManager.PlayerMark.CrossMark)
                {
                    _gameAreaCrossPlayerSprite.sprite = _playerSpriteImage;
                    _gameAreaCirclePlayerSprite.sprite = _AISpriteImage;
                }
                else
                {
                    _gameAreaCrossPlayerSprite.sprite = _AISpriteImage;
                    _gameAreaCirclePlayerSprite.sprite = _playerSpriteImage;
                }
                break;
            case GameMode.HumanVsHuman:
                _firstPlayerInputType = _secondPlayerInputType = GameManager.InputType.User;
                _gameAreaCrossPlayerSprite.sprite = _gameAreaCirclePlayerSprite.sprite = _playerSpriteImage;
                break;
        }
    }

    // Sets field size
    public void SetFieldSize(float fieldSize)
    {
        // Set field size 
        _gameEditInterfaceFieldSizeText.text = fieldSize.ToString();
        _gameEditInterfaceFieldSizeSlider.value = fieldSize;
        // Updates win row quant, if it exceeds field size
        if (_gameEditInterfaceWinRowQuantSlider.value > fieldSize)
        {
            SetWinRowQuant(fieldSize);
        }
    }

    // Sets win row quant
    public void SetWinRowQuant(float winRowQuant)
    {
        // Set win row
        _gameEditInterfaceWinRowQuantText.text = winRowQuant.ToString();
        _gameEditInterfaceWinRowQuantSlider.value = winRowQuant;
        // If win row quant doesn't exceed field size, change it
        if (_gameEditInterfaceWinRowQuantSlider.value >= _gameEditInterfaceFieldSizeSlider.value)
        {
            // Update slider and text
            _gameEditInterfaceWinRowQuantSlider.value = _gameEditInterfaceFieldSizeSlider.value;
            _gameEditInterfaceWinRowQuantText.text = _gameEditInterfaceFieldSizeSlider.value.ToString();
        }
    }

    // Start game button
    public void StartGameButton()
    {
        // Disable edit interface
        _gameEditInterface.SetActive(false);
        // Show game area screen
        _gameAreaInterface.SetActive(true);
        // Subscribe to the event
        _gameManagerScript.CountersUpdate -= UpdateCounters;
        _gameManagerScript.CountersUpdate += UpdateCounters;
        _gameManagerScript.TurnUpdate -= UpdateTurnState;
        _gameManagerScript.TurnUpdate += UpdateTurnState;
        // Set input
        _gameManagerScript.SetInput(_firstPlayerInputType, _firstPlayerMark,
                    _secondPlayerInputType, _secondPlayerMark);
        // Create field
        _gameManagerScript.StartGame(GameManager.PlayerMark.CrossMark, (int)_gameEditInterfaceWinRowQuantSlider.value,
            (int)_gameEditInterfaceFieldSizeSlider.value);        
        // Hide button
        _nextRoundButton.SetActive(false);
    }

    #endregion

    #region GameArea

    // Updates counters
    public void UpdateCounters()
    {
        // Update counters values
        _gameAreaCrossPlayerWinCounterText.text = _gameManagerScript.CrossWinCounter.ToString();
        _gameAreaCirclePlayerWinCounterText.text = _gameManagerScript.CircleWinCounter.ToString();
        _gameAreaDrawCounterText.text = _gameManagerScript.DrawCounter.ToString();
        // Set next round button active
        _nextRoundButton.SetActive(true);
    }

    // Updates game area data
    public void UpdateTurnState(GameManager.PlayerMark turnState)
    {
        switch (turnState)
        {
            // Cross win
            case GameManager.PlayerMark.CrossMark:
                _gameAreaRecentTurn[0].SetActive(true);
                _gameAreaRecentTurn[1].SetActive(false);
                break;
            // Circle win
            case GameManager.PlayerMark.CircleMark:
                _gameAreaRecentTurn[0].SetActive(false);
                _gameAreaRecentTurn[1].SetActive(true);
                break;
        }
    }

    // Starts next round
    public void NextRoundButton()
    {
        // Hide button
        _nextRoundButton.SetActive(false);
        // Start next round
        _gameManagerScript.NextRound();
    }

    #endregion

    #endregion

    #endregion
}
