using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MastermindController : MonoBehaviour
{
    [SerializeField] List<int> aIGuesses, correctNums;
    [SerializeField] List<bool> correctNumsBool;
    [SerializeField] int corr;
    [SerializeField] List<string> aIGuessLog;
    [SerializeField] List<string> potentialAnswers;
    [SerializeField] int potAnsPtr;

    [SerializeField] bool correctNumsFound;

    [SerializeField] TMP_InputField playerInputBox;

    [SerializeField] TextMeshProUGUI clickToStartText, enterNumberText, aIGuessText, previousGuessesText;

    [SerializeField] List<int> notInAns;

    [SerializeField] bool[,] notInSlot;
    [SerializeField] List<string> previousGuesses;
    [SerializeField] int bulls, cows, turns;

    public GameState gameState;
    public int idiot;

    // Start is called before the first frame update
    void Start()
    {
        notInSlot = new bool[4, 10];
    }

    // Update is called once per frame
    void Update()
    {
        GameStateMachine();

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
                    enterNumberText.enabled = true;
                }

                break;

            case GameState.PlayerInput:

                if(playerInputBox.text.Length == 4 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    gameState = GameState.AIGuess;
                }
                break;

            case GameState.AIGuess:
                aIGuessText.text = "";
                bulls = 0;
                cows = 0;
                //StartCoroutine(DoGuess());
                Guess();
                break;

            case GameState.BullsAndCows:
                CheckForBullsAndCows();

                gameState = GameState.WrongRightGuess;

                break;
            case GameState.CheckForBulls:
                CheckForBulls();

                gameState = GameState.WrongRightGuess;

                break;

            case GameState.WrongRightGuess:
                turns++;
                if(bulls == 4)
                {
                    gameState = GameState.GameOver;
                }
                else
                {
                    gameState = GameState.AIGuess;
                }
                break;

            case GameState.GameOver:

                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {

                    playerInputBox.text = "";
                    potAnsPtr = 0;
                    correctNumsFound = false;
                    enterNumberText.enabled = true;
                    aIGuessText.text = "";
                    previousGuessesText.text = "";
                    bulls = 0;
                    cows = 0;
                    corr = 0;
                    turns = 0;
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
                    idiot = 0;
                    gameState = GameState.PlayerInput;
                }
                break;
        }
    }
    int RandomNumber()
    {
        int rand = Random.Range(0, 10);

        return rand;
    }
    void CheckForBullsAndCows()
    {
        Infer();

        print("break2");

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
        if (bulls == 0)
        {
            for(int i = 0; i < 4; i++)
            {
                notInSlot[i, aIGuesses[i]] = true;
            }
        }
        if(cows + bulls == 0)
        {
            for(int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    notInSlot[j, aIGuesses[i]] = true;
                }
            }
        }
        if ((bulls + cows == 4) || corr + cows == 4)
        {
            print("break22");

            for (int k = 0; k < 4; k++)
            {
                //if (!correctNumsBool[k])
                //{
                    correctNums[k] = aIGuesses[k];
                //}
            }
            correctNumsFound = true;
        }

    }
    void CheckForBulls()
    {
        for (int i = 0; i < 4; i++)
        {
            if (potentialAnswers[potAnsPtr].Substring(i, 1) == playerInputBox.text.Substring(i, 1))
            {
                bulls++;
            }
        }
        potAnsPtr++;
        potAnsPtr = Mathf.Clamp(potAnsPtr, 0, 23);
    }
    void Infer()// checks notInSlot for any group of bool with only one false and assigns that to correctNums
    {
        for(int i = 0; i < 4; i++)
        {
            int check = 0;
            int num = 0;
            for(int j = 0; j < 10; j++)
            {
                if(!notInSlot[i, j])
                {
                    check++;
                    num = j;
                }
            }
            if(check == 1)
            {
                if(correctNumsBool[i] == false)
                {
                    corr++;
                }
                correctNums[i] = num;
                correctNumsBool[i] = true;
            }
        }
    }

    void Guess()
    {
        if (correctNumsFound)
        {
            if (potentialAnswers.Count == 0)
            {
                Permute(correctNums, 0, correctNums.Count - 1); // fill potentialAnswers with permutations of correctNums
            }

            foreach (var listMem in potentialAnswers[potAnsPtr])
            {
                aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
            }
            previousGuessesText.text += aIGuessText.text + " C" + "\n"; // adds current guess to the list of previous guesses

            gameState = GameState.CheckForBulls;
        }
        else
        {

            string s = "";
            int counter = -1;

            //do
            //{
            print(2);
            List<int> usedNums = new List<int>(); // to be filled with number that were already used in the current guess

            AddCorrectNums(usedNums);

            s = "";
            print(3);
            for (int i = 0; i < aIGuesses.Count; i++) //loop to guess the four numbers of current guess
            {

                if (correctNumsBool[i])
                {
                    aIGuesses[i] = correctNums[i];
                }
                else
                {
                    int looped = 0;

                    do
                    {
                        do
                        {
                            print("1");
                            counter++;
                            if (counter > 9)// numbers can only be 0 - 9
                            {
                                counter = 0;
                            }
                            counter = RandomNumber();
                            looped++;
                        }
                        while (notInSlot[i, counter]/* || usedNums.Contains(counter)*//* && looped < 10*/);

                    } while (usedNums.Contains(counter) && looped < 10);
                    if (looped < 10 && !usedNums.Contains(counter))
                    {
                        usedNums.Add(counter);
                        aIGuesses[i] = counter;
                    }
                    else
                    {
                        i = -1;
                        usedNums = new List<int>(); // to be filled with number that were already used in the current guess

                        AddCorrectNums(usedNums);

                    }
                }
                if (i >= 0)
                {
                    s += aIGuesses[i];
                }
            }
            //} while (previousGuesses.Contains(s));
            if (previousGuesses.Contains(s))
            {
                idiot++;
            }
            previousGuesses.Add(s);

            foreach (var listMem in aIGuesses)
            {
                aIGuessText.text += listMem.ToString();
            }
            aIGuessLog.Add(aIGuessText.text);
            previousGuessesText.text += aIGuessText.text + "\n";

            gameState = GameState.BullsAndCows;

        }

    }

    void AddCorrectNums(List<int> a)
    {
        for (int i = 0; i < correctNumsBool.Count; i++)// checks if the correct number for this slot has already been found
        {
            if (correctNumsBool[i])
            {
                a.Add(correctNums[i]); //if so, add it to usedNums so it wont be used again in current guess
            }
        }

    }
    private void Permute(List<int> nums, int l, int r)
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
    public void Swap(List<int> a, int i, int j)
    {
        int temp;

        temp = a[i];
        a[i] = a[j];
        a[j] = temp;

    }
}



public enum GameState
{
    BeforeStart,
    PlayerInput,
    AIGuess,
    BullsAndCows,
    CheckForBulls,
    WrongRightGuess,
    GameOver
}
