using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MastermindController : MonoBehaviour
{

    [SerializeField] List<int> aIGuesses; // array of the 4 digits of the AI's current guess
    [SerializeField] List<int> correctNums; // array to hold 4 digits deduced as being in the correct answer
    [SerializeField] List<bool> correctNumsBool; //array telling which of the 4 digits have been correctly deduced
    [SerializeField] List<string> aIGuessLog; // array of all guesses 
    [SerializeField] List<string> potentialAnswers; // array to hold permutations of correctly deduced digits
    [SerializeField] int potAnsPtr; //int to traverse potentialAnswers list

    [SerializeField] bool correctNumsFound; //is true when all 4 correct digits have been deduced


    [SerializeField] TMP_InputField playerInputBox;  // for player to input the number
    [SerializeField] TMP_InputField bullsInput, cowsInput; // for player to input cows and bulls of each AI guess while not in auto mode

    //various ui text objects
    [SerializeField] TextMeshProUGUI clickToStartText, enterNumberText, aIGuessText, previousGuessesText, eachDigitDiffText, enterNumberOfBullsAndCowsText, gameOverText, turnsText;

    // lists to hold digits in and not in the correct code
    [SerializeField] List<int> notInAnsNarrow; // to be used in NarrowingDownCorrectNumbers state
    [SerializeField] List<int> isInAnsNarrow;

    [SerializeField] List<int> notInAns; // used in StartingGuesses state
    [SerializeField] List<int> isInAns; // used in StartingGuesses state

    [SerializeField] bool[,] notInSlot; // 2D array to tell what numbers have been deduced as not in each specific place of the code

    [SerializeField] int narrowingCounter; //increments every turn in the NarrowingDownCorrectNumbers state to test each as correct or not
    [SerializeField] bool cancelling; // if true,  numbers are being cancelled out in NarrowingDownCorrectNumbers
    [SerializeField] List<string> previousGuesses;


    [SerializeField] int bulls, cows, turns;

    //used in StartingGuesses
    [SerializeField] int prevBullPlusCow; // bulls+cows of the previous turn
    [SerializeField] int firstTurnBullsPlusCows; // bulls+cows of the first turn (3210)
    [SerializeField] int startingGuessCounter; // used to go through numbers 4 - 9 to check 
    [SerializeField] int lastFourCounter; //used to check if numbers 0 - 3 are correct digits 


    [SerializeField] int resetTurns; // to keep track of the first 2 turns of StartingGuesses when SoftReset() is called
    public GameState gameState;
    public int recurringGuesses;

    [SerializeField] Toggle auto; // toggle for automatic mode, on by default

    // Start is called before the first frame update
    void Start()
    {
        notInSlot = new bool[4, 10];
        notInAns = new List<int>();
        isInAns = new List<int>();

    }

    // Update is called once per frame
    void Update()
    {
        GameStateMachine();

        if (auto.isOn)
        {
            bullsInput.text = bulls.ToString();
            cowsInput.text = cows.ToString();
        }
    }

    void GameStateMachine()
    {
        switch (gameState)
        {
            case GameState.BeforeStart:

                if (Input.GetKeyDown(KeyCode.Mouse0) && clickToStartText.enabled)
                {
                    gameState = GameState.PlayerInput;

                    clickToStartText.enabled = false;

                    playerInputBox.Select();
                    enterNumberText.gameObject.SetActive(true);
                }

                break;

            case GameState.PlayerInput:

                if (playerInputBox.text.Length == 4 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    int count = 0;
                    bool repeatedDigit = false;
                    for (int i = 0; i < 4; i++)
                    {
                        for (int j = 0; j < 4; j++)
                        {

                            if (playerInputBox.text.Substring(i, 1) == playerInputBox.text.Substring(j, 1) && i != j)
                            {
                                repeatedDigit = true;
                                i = 4;
                                break;
                            }
                        }
                    }
                    if (repeatedDigit)
                    {
                        eachDigitDiffText.color = Color.red;
                        count = 0;
                    }
                    else //if digits in player's code don't repeat move on to next state
                    {
                        gameState = GameState.StartingGuesses;
                        eachDigitDiffText.color = Color.black;
                        enterNumberText.gameObject.SetActive(false);
                    }
                }
                break;

            case GameState.StartingGuesses:
                if (turns > 0)
                {
                    previousGuessesText.text += "bulls:" + bulls + " cows:" + cows + "\n";
                }
                aIGuessText.text = "";
                bulls = 0;
                cows = 0;
                StartingGuesses();
                break;

            case GameState.NarrowingDownCorrectNumbers:
                previousGuessesText.text += "bulls:" + bulls + " cows:" + cows + "\n";

                aIGuessText.text = "";
                bulls = 0;
                cows = 0;
                turns++;

                CancelOut();
                break;
            case GameState.GoingThroughPermutations:
                previousGuessesText.text += "bulls:" + bulls + " cows:" + cows + "\n";

                CheckPermutationList();

                aIGuessText.text = "";
                bulls = 0;
                cows = 0;


                gameState = GameState.CheckForBulls;
                break;

            case GameState.BullsAndCows:
                StartCoroutine(CheckForBullsAndCows());

                break;
            case GameState.CheckForBulls:
                StartCoroutine(CheckForBulls());

                //gameState = GameState.WrongRightGuess;

                break;

            case GameState.WrongRightGuess:

                turns++;
                turnsText.text = "Turns: " + (turns);

                if (bulls == 4)
                {
                    gameState = GameState.GameOver;

                    if (correctNumsFound && potAnsPtr > 0)
                    {
                        aIGuessText.text = "";
                        foreach (var listMem in potentialAnswers[potAnsPtr - 1])
                        {
                            aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
                        }
                    }
                    previousGuessesText.text += "bulls:" + bulls + " cows:" + cows + "\n";

                }
                else
                {
                    gameState = GameState.StartingGuesses;

                    if (bulls == 0)
                    {
                        gameState = GameState.StartingGuesses;
                    }
                    if (cows + bulls == 0)
                    {
                        gameState = GameState.NarrowingDownCorrectNumbers;
                    }
                    if ((bulls + cows == 4) || correctNumsFound)
                    {
                        gameState = GameState.GoingThroughPermutations;
                    }

                }
                break;

            case GameState.GameOver:

                gameOverText.gameObject.SetActive(true);

                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    Reset();

                }
                break;

        }
    }

    public void Reset() //Resets the game back to PlayerInput state
    {
        turnsText.text = "Turns: 0"; 
        playerInputBox.text = "";
        potAnsPtr = 0;
        correctNumsFound = false;
        enterNumberText.gameObject.SetActive(true);
        aIGuessText.text = "";
        previousGuessesText.text = "";
        bulls = 0;
        cows = 0;
        turns = 0;
        prevBullPlusCow = 0;
        firstTurnBullsPlusCows = 0;
        startingGuessCounter = 0;
        lastFourCounter = 0;
        for (int i = 0; i < 4; i++)
        {
            correctNumsBool[i] = false;
            correctNums[i] = 0;
            aIGuesses[i] = 0;

        }
        potentialAnswers.Clear();
        aIGuessLog.Clear();
        previousGuesses.Clear();
        notInSlot = new bool[4, 10];
        recurringGuesses = 0;
        cancelling = false;
        narrowingCounter = 0;
        isInAnsNarrow.Clear();
        notInAns.Clear();
        notInAnsNarrow.Clear();
        isInAns.Clear();
        enterNumberOfBullsAndCowsText.gameObject.SetActive(false);
        gameState = GameState.PlayerInput;

        gameOverText.gameObject.SetActive(false);

    }

    void SoftReset() //reset AI guesses back to StartingGuesses state, for when players enters wrong value for bulls or cows and game is unable to find correct code
    {
        potAnsPtr = 0;
        correctNumsFound = false;
        bulls = 0;
        cows = 0;
        prevBullPlusCow = 0;
        firstTurnBullsPlusCows = 0;
        startingGuessCounter = 3;
        lastFourCounter = 0;
        for (int i = 0; i < 4; i++)
        {
            correctNumsBool[i] = false;
            correctNums[i] = 0;
            aIGuesses[i] = 0;

        }
        potentialAnswers.Clear();
        previousGuesses.Clear();
        notInSlot = new bool[4, 10];
        recurringGuesses = 0;
        cancelling = false;
        narrowingCounter = 0;
        isInAnsNarrow.Clear();
        notInAns.Clear();
        notInAnsNarrow.Clear();
        isInAns.Clear();
        gameState = GameState.StartingGuesses;
        resetTurns = 0;
        previousGuessesText.text += "IMPOSSIBLE NUMBER, STARTING OVER" + "\n";


    }
    void StartingGuesses()
    {
        if (notInAns.Count == 0 && startingGuessCounter > 9)
        {
            SoftReset();
        }
        else
        {
            if (startingGuessCounter <= 9)
            {
                if (turns == 0 || resetTurns == 0)
                {
                    aIGuesses[0] = 3;
                    aIGuesses[1] = 2;
                    aIGuesses[2] = 1;
                    aIGuesses[3] = 0;
                    startingGuessCounter = 4;
                    resetTurns++;
                }
                else if (turns == 1 || resetTurns == 1)
                {

                    aIGuesses[3] = startingGuessCounter;
                    resetTurns++;
                }
                else if (turns > 1)
                {
                    if (prevBullPlusCow >= firstTurnBullsPlusCows)
                    {
                        isInAns.Add(startingGuessCounter);
                    }
                    if (prevBullPlusCow <= firstTurnBullsPlusCows)
                    {
                        notInAns.Add(startingGuessCounter);
                    }
                    startingGuessCounter++;
                    aIGuesses[3] = startingGuessCounter;

                }
            }
            else if (isInAns.Count < 4)
            {
                if (lastFourCounter > 0)
                {
                    if (prevBullPlusCow > 0)
                    {
                        isInAns.Add(lastFourCounter - 1);
                    }
                }
                for (int i = 0; i < isInAns.Count; i++)
                {
                    if (notInAns.Contains(isInAns[i]))
                    {
                        notInAns.Remove(isInAns[i]);
                    }
                }
                aIGuesses[0] = notInAns[0];
                aIGuesses[1] = notInAns[1];
                aIGuesses[2] = notInAns[2];
                aIGuesses[3] = lastFourCounter;

                lastFourCounter++;
            }
            else if (isInAns.Count > 4)
            {
                aIGuesses[0] = notInAns[0];
                aIGuesses[1] = notInAns[1];
                aIGuesses[2] = notInAns[2];
                if (notInAns.Count < 4)
                {
                    notInAns.Add(0);
                }
                aIGuesses[3] = notInAns[3];

            }
            else if (isInAns.Count == 4)
            {
                aIGuesses[0] = isInAns[0];
                aIGuesses[1] = isInAns[1];
                aIGuesses[2] = isInAns[2];
                aIGuesses[3] = isInAns[3];
            }

            if (startingGuessCounter == 10)
            {
                gameState = GameState.StartingGuesses;
                startingGuessCounter++;
            }
            else
            {
                foreach (var listMem in aIGuesses)
                {
                    aIGuessText.text += listMem.ToString();
                }
                aIGuessLog.Add(aIGuessText.text);
                previousGuessesText.text += turns + "       " + aIGuessText.text + "\n";


                gameState = GameState.BullsAndCows;
            }
        }
    }
    IEnumerator CheckForBullsAndCows() // counts bulls and cows, if auto is off take player input for bulls and cows
    {

        print("break2");
        if (auto.isOn)
        {
            for (int i = 0; i < 4; i++)
            {
                if (aIGuesses[i].ToString() == playerInputBox.text.Substring(i, 1))
                {
                    bulls++;
                }
                else
                {
                    print("break21");

                    for (int j = 0; j < 4; j++)
                    {
                        if (aIGuesses[i].ToString() == playerInputBox.text.Substring(j, 1))
                        {
                            cows++;
                        }
                    }
                }
            }

            if (cancelling)
            {
                if (cows > 0 || bulls > 0)
                {
                    isInAnsNarrow.Add(aIGuesses[3]);
                }
            }
            yield return new WaitForEndOfFrame();

            if (cancelling)
            {
                gameState = GameState.NarrowingDownCorrectNumbers;

            }
            else
            {
                if (bulls == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (aIGuesses[i] < 10)
                        {
                            notInSlot[i, aIGuesses[i]] = true;
                        }
                    }
                }
                if (cows + bulls == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        notInAnsNarrow.Add(aIGuesses[i]);
                    }
                    cancelling = true;
                }
                if ((bulls + cows == 4))
                {
                    print("break22");

                    for (int k = 0; k < 4; k++)
                    {
                        correctNums[k] = aIGuesses[k];
                    }
                    correctNumsFound = true;

                }
                gameState = GameState.WrongRightGuess;
            }
            prevBullPlusCow = bulls + cows;
            if (turns == 0)
            {
                firstTurnBullsPlusCows = bulls + cows;
            }

        }
        else if (!auto.isOn)
        {
            enterNumberOfBullsAndCowsText.gameObject.SetActive(true);
            bool go = false;

            if (bullsInput.text.Length == 1 && cowsInput.text.Length == 1 && int.Parse(bullsInput.text) + int.Parse(cowsInput.text) <= 4 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
            {
                bulls = int.Parse(bullsInput.text);
                cows = int.Parse(cowsInput.text);
                gameState = GameState.WrongRightGuess;

                if (cancelling)
                {
                    if (cows > 0 || bulls > 0)
                    {
                        isInAnsNarrow.Add(aIGuesses[3]);
                    }
                }
                go = true;
            }
            yield return new WaitUntil(() => go == true);
            enterNumberOfBullsAndCowsText.gameObject.SetActive(false);

            if (cancelling)
            {
                gameState = GameState.NarrowingDownCorrectNumbers;

            }
            else
            {
                if (bulls == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (aIGuesses[i] < 10)
                        {
                            notInSlot[i, aIGuesses[i]] = true;
                        }
                    }
                }
                if (cows + bulls == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        notInAnsNarrow.Add(aIGuesses[i]);
                    }
                    cancelling = true;
                }
                if ((bulls + cows == 4))
                {
                    print("break22");

                    for (int k = 0; k < 4; k++)
                    {
                        correctNums[k] = aIGuesses[k];
                    }
                    correctNumsFound = true;

                }
            }
            prevBullPlusCow = bulls + cows;
            if (turns == 0)
            {
                firstTurnBullsPlusCows = bulls + cows;
            }

        }

    }

    void CancelOut() //narrows down which numbers are in and not in the correct code if StartingGuesses was unable to do so fully
    {
        if (narrowingCounter > 9 && isInAnsNarrow.Count != 4) //if all possible nuimbers have been checked and no number is deduced, start back at StartingGuesses
        {
            SoftReset();
        }
        else
        {
            if (isInAns.Count == 4) //if list has exactly 4 in it assign those to the current and correct guesses 
            {
                aIGuesses[0] = isInAns[0];
                aIGuesses[1] = isInAns[1];
                aIGuesses[2] = isInAns[2];
                aIGuesses[3] = isInAns[3];

                correctNums[0] = isInAns[0];
                correctNums[1] = isInAns[1];
                correctNums[2] = isInAns[2];
                correctNums[3] = isInAns[3];

                for (int i = 0; i < 4; i++)
                {
                    correctNumsFound = true;
                }

                cancelling = false;
            }
            else if (isInAnsNarrow.Count == 4) //if list has exactly 4 in it assign those to the correct guesses 
            {
                for (int i = 0; i < 4; i++)
                {
                    correctNums[i] = isInAnsNarrow[i];
                    correctNumsFound = true;
                }
                cancelling = false;
            }
            else // narrow down the numbers as being in the answer or not guess by guess
            {
                do
                {
                    aIGuesses[3] = narrowingCounter;
                    narrowingCounter++;
                } while (notInAnsNarrow.Contains(aIGuesses[3]));
            }

            foreach (var listMem in aIGuesses)
            {
                aIGuessText.text += listMem.ToString();
            }
            aIGuessLog.Add(aIGuessText.text);
            previousGuessesText.text += turns + "       " + aIGuessText.text + "N" + "\n";

            if (cancelling)
            {
                gameState = GameState.BullsAndCows;
            }
            else
            {
                gameState = GameState.CheckForBulls;
            }
        }
    }

    IEnumerator CheckForBulls() // counts bulls, if auto is off take player input for bulls
    {
        if (recurringGuesses > 1 && !correctNumsFound)
        {
            gameState = GameState.StartingGuesses;
            recurringGuesses = 0;
        }
        else
        {
            if (potentialAnswers.Count == 0 && correctNumsFound)
            {
                Permute(correctNums, 0, correctNums.Count - 1); // fill potentialAnswers with permutations of correctNums

                previousGuessesText.text += turns + "       " + potentialAnswers[potAnsPtr] + " C" + "\n"; // adds current guess to the list of previous guesses
                aIGuessText.text = "";
                foreach (var listMem in potentialAnswers[potAnsPtr])
                {
                    aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
                }

            }

            if (auto.isOn)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (potentialAnswers[potAnsPtr].Substring(i, 1) == playerInputBox.text.Substring(i, 1))
                    {
                        bulls++;
                    }
                }
                potAnsPtr++;

                gameState = GameState.WrongRightGuess;

                yield return new WaitForEndOfFrame();
            }
            else if (!auto.isOn)
            {
                enterNumberOfBullsAndCowsText.gameObject.SetActive(true);

                bool go = false;
                if (bullsInput.text.Length == 1 && cowsInput.text.Length == 1 && int.Parse(bullsInput.text) + int.Parse(cowsInput.text) <= 4 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    bulls = int.Parse(bullsInput.text);

                    potAnsPtr++;

                    gameState = GameState.WrongRightGuess;

                    go = true;
                }
                yield return new WaitUntil(() => go == true);
                enterNumberOfBullsAndCowsText.gameObject.SetActive(false);

            }



            if (potAnsPtr == 24 && bulls != 4) // reset if the potentialAnswers array has no answer deduced as correct
            {
                SoftReset();
                potAnsPtr = 0;
                correctNumsFound = false;
                potentialAnswers.Clear();
                gameState = GameState.StartingGuesses;
                for (int i = 0; i < 4; i++)
                {
                    correctNums[i] = 0;
                    correctNumsBool[i] = false;
                }
                recurringGuesses++;

            }
        }
        potAnsPtr = Mathf.Clamp(potAnsPtr, 0, 23);
    }

    void CheckPermutationList() // goes through and presents each element of potentialAnswers as a guess until correct answer is found
    {
        if (potentialAnswers.Count == 0)
        {
            Permute(correctNums, 0, correctNums.Count - 1); // fill potentialAnswers with permutations of correctNums
        }
        bool skip = false;
        do //used to skip any guess that has a number deduced as not being in a certain spot
        {
            skip = false;
            for (int i = 0; i < 4; i++)
            {
                if (notInSlot[i, int.Parse(potentialAnswers[potAnsPtr].Substring(i, 1))])
                {
                    potAnsPtr++;
                    potAnsPtr = Mathf.Clamp(potAnsPtr, 0, 23);

                    skip = true;
                }
            }
        } while (skip && potAnsPtr == 23);

        if (bulls == 0 && potAnsPtr > 0)// used to traverse potentialAnswers quicker
        {

            if (potAnsPtr <= 6)
            {
                potAnsPtr = 6;
            }
            else if (potAnsPtr <= 12)
            {
                potAnsPtr = 12;
            }
            else if (potAnsPtr <= 18)
            {
                potAnsPtr = 18;
            }

        }
        aIGuessText.text = "";

        foreach (var listMem in potentialAnswers[potAnsPtr])
        {
            aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
        }
        previousGuessesText.text += turns + "       " + aIGuessText.text + " C" + "\n"; // adds current guess to the list of previous guesses

    }

    private void Permute(List<int> nums, int l, int r) // used to fill out potentialAnswers list
    {
        if (l == r)
        {
            string s = "";
            foreach (var listMem in nums)
            {
                s += listMem.ToString();
            }

            potentialAnswers.Add(s);

        }
        else
        {
            for (int i = l; i <= r; i++)
            {
                Swap(nums, l, i);
                Permute(nums, l + 1, r);
                Swap(nums, l, i);
            }
        }
    }
    public void Swap(List<int> a, int i, int j) //used in Permute()
    {
        int temp;

        temp = a[i];
        a[i] = a[j];
        a[j] = temp;

    }
}

public enum GameState //enum used to make game states
{
    BeforeStart,
    PlayerInput,
    AIGuess,
    BullsAndCows,
    CheckForBulls,
    WrongRightGuess,
    GameOver,
    StartingGuesses,
    NarrowingDownCorrectNumbers,
    GoingThroughPermutations
}

