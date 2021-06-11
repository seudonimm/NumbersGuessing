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

    [SerializeField] TextMeshProUGUI clickToStartText, enterNumberText, aIGuessText, previousGuessesText, eachDigitDiffText;

    [SerializeField] List<int> notInAnsNarrow, notInAns, isInAns;

    [SerializeField] bool[,] notInSlot;
    [SerializeField] List<bool> notInSlot1;
    [SerializeField] List<bool> notInSlot2;
    [SerializeField] List<bool> notInSlot3;
    [SerializeField] List<bool> notInSlot4;

    [SerializeField] List<int> wrongCheck;
    [SerializeField] int wrongCheckCounter;
    [SerializeField] bool cancelling;
    [SerializeField] List<string> previousGuesses;
    [SerializeField] int bulls, cows, turns, prevBullPlusCow, firstTurnBullsPlusCows, startingGuessCounter, lastFourCounter, lastFourBullsCows;

    public GameState gameState;
    public int idiot;

    // Start is called before the first frame update
    void Start()
    {
        notInSlot = new bool[4, 10];
        notInAns = new List<int>();
        isInAns = new List<int>();

        //notInSlot1 = new List<bool>();
        //notInSlot2 = new List<bool>();
        //notInSlot3 = new List<bool>();
        //notInSlot4 = new List<bool>();
    }

    // Update is called once per frame
    void Update()
    {
        GameStateMachine();
        for (int i = 0; i < 10; i++)
        {
            notInSlot1[i] = notInSlot[0, i];
        }
        for (int i = 0; i < 10; i++)
        {
            notInSlot2[i] = notInSlot[1, i];
        }
        for (int i = 0; i < 10; i++)
        {
            notInSlot3[i] = notInSlot[2, i];
        }
        for (int i = 0; i < 10; i++)
        {
            notInSlot4[i] = notInSlot[3, i];
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

                if(playerInputBox.text.Length == 4 && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {
                    int count = 0;
                    bool repeatedDigit = false;
                    for(int i = 0; i < 4; i++)
                    {
                        for(int j = 0; j < 4; j++)
                        {

                            if(playerInputBox.text.Substring(i, 1) == playerInputBox.text.Substring(j, 1) && i != j)
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
                    else
                    {
                        gameState = GameState.RandomNumberGuessing;
                        eachDigitDiffText.color = Color.black;
                        enterNumberText.gameObject.SetActive(false);
                    }
                }
                break;

            //case GameState.AIGuess:
            //    //StartCoroutine(DoGuess());
            //    Guess();

            //    aIGuessText.text = "";
            //    bulls = 0;
            //    cows = 0;

            //    break;
            case GameState.RandomNumberGuessing:

                aIGuessText.text = "";
                bulls = 0;
                cows = 0;
                //RandomGuess();
                StartingGuesses();
                break;

            case GameState.NarrowingDownCorrectNumbers:
                aIGuessText.text = "";
                bulls = 0;
                cows = 0;
                turns++;

                CancelOut();
                break;
            case GameState.GoingThroughPermutations:
                aIGuessText.text = "";
                bulls = 0;
                cows = 0;

                CheckPermutationList();

                gameState = GameState.CheckForBulls;
                break;

            case GameState.BullsAndCows:
                CheckForBullsAndCows();
                if (cancelling)
                {
                    gameState = GameState.NarrowingDownCorrectNumbers;

                }
                else
                {
                    if (bulls == 0)
                    {
                        //bullZeroCount++;
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
                            notInAns.Add(aIGuesses[i]);
                        }
                        cancelling = true;
                    }
                    if ((bulls + cows == 4) || corr + cows == 4)
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
                if(turns == 0)
                {
                    firstTurnBullsPlusCows = bulls + cows;
                }

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

                    if (correctNumsFound && potAnsPtr > 0)
                    {
                        aIGuessText.text = "";
                        foreach (var listMem in potentialAnswers[potAnsPtr - 1])
                        {
                            aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
                        }
                    }
                }
                else
                {
                    gameState = GameState.RandomNumberGuessing;

                    if (bulls == 0)
                    {
                        gameState = GameState.RandomNumberGuessing;
                    }
                    if (cows + bulls == 0)
                    {
                        gameState = GameState.NarrowingDownCorrectNumbers;
                    }
                    if ((bulls + cows == 4 || corr + cows == 4) || correctNumsFound)
                    {
                        gameState = GameState.GoingThroughPermutations;
                    }

                    //Infer();
                }
                //if(gameState != GameState.WrongRightGuess)
                //{
                //    bulls = 0;
                //    cows = 0;

                //}

                break;

            case GameState.GameOver:

                if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
                {

                    playerInputBox.text = "";
                    potAnsPtr = 0;
                    correctNumsFound = false;
                    enterNumberText.gameObject.SetActive(true);
                    aIGuessText.text = "";
                    previousGuessesText.text = "";
                    bulls = 0;
                    cows = 0;
                    corr = 0;
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
                    idiot = 0;
                    cancelling = false;
                    wrongCheckCounter = 0;
                    wrongCheck.Clear();
                    notInAns.Clear();
                    notInAnsNarrow.Clear();
                    isInAns.Clear();
                    gameState = GameState.PlayerInput;
                }
                break;

            case GameState.WrongChecking:
                aIGuessText.text = "";
                bulls = 0;
                cows = 0;

                turns++;
                CancelOut();

                break;
        }
    }

    void StartingGuesses()
    {
        if (startingGuessCounter <= 9)
        {
            if (turns == 0)
            {
                aIGuesses[0] = 3;
                aIGuesses[1] = 2;
                aIGuesses[2] = 1;
                aIGuesses[3] = 0;
                startingGuessCounter = 4;
            }
            else if (turns == 1)
            {

                if (prevBullPlusCow == 0)
                {
                    notInAns.Add(aIGuesses[0]);
                    notInAns.Add(aIGuesses[1]);
                    notInAns.Add(aIGuesses[2]);
                    notInAns.Add(aIGuesses[3]);
                }
                else
                {

                    aIGuesses[3] = startingGuessCounter;
                }
            }
            else if (turns > 1)
            {
                if (prevBullPlusCow >= firstTurnBullsPlusCows)
                {
                    isInAns.Add(startingGuessCounter);
                }
                if (prevBullPlusCow <= firstTurnBullsPlusCows)
                {
                    //if (!isInAns.Contains(startingGuessCounter))
                    //{
                        notInAns.Add(startingGuessCounter);
                    //}
                }
                startingGuessCounter++;
                aIGuesses[3] = startingGuessCounter;
            }
        }
        else if(isInAns.Count <= 4)
        {
            if(lastFourCounter > 0)
            {
                if(prevBullPlusCow > 0)
                {
                    isInAns.Add(lastFourCounter - 1);
                }
            }
            for(int i = 0; i < isInAns.Count; i++)
            {
                if (notInAns.Contains(isInAns[i]))
                {
                    notInAns.Remove(isInAns[i]);
                }
            }
            //for(int i = 0; i < 3; i++)
            //{
            //    if(notInAns[i] == undefined)
            //    aIGuesses[i] = notInAns[i];

            //}
            aIGuesses[0] = notInAns[0];
            aIGuesses[1] = notInAns[1];
            aIGuesses[2] = notInAns[2];
            aIGuesses[3] = lastFourCounter;

            lastFourCounter++;
        }
        else if(isInAns.Count > 4)
        {
            aIGuesses[0] = notInAns[0];
            aIGuesses[1] = notInAns[1];
            aIGuesses[2] = notInAns[2];
            if(notInAns.Count < 4)
            {
                notInAns.Add(0);
            }
            aIGuesses[3] = notInAns[3];

        }
        else
        {
            aIGuesses[0] = isInAns[0];
            aIGuesses[1] = isInAns[1];
            aIGuesses[2] = isInAns[2];
            aIGuesses[3] = isInAns[3];
        }
        foreach (var listMem in aIGuesses)
        {
            aIGuessText.text += listMem.ToString();
        }
        aIGuessLog.Add(aIGuessText.text);
        previousGuessesText.text += turns + "       " + aIGuessText.text + "\n";

        gameState = GameState.BullsAndCows;

    }
    int RandomNumber()
    {
        int rand = Random.Range(0, 10);

        return rand;
    }
    void CheckForBullsAndCows()
    {

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

        if (cancelling)
        {
            if(cows > 0 || bulls > 0)
            {
                wrongCheck.Add(aIGuesses[3]);
            }
        }
        else
        {

            //if (bulls == 0)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        notInSlot[i, aIGuesses[i]] = true;
            //    }
            //}
            //if (cows + bulls == 0)
            //{
            //    for (int i = 0; i < 4; i++)
            //    {
            //        notInAns.Add(aIGuesses[i]);
            //    }
            //    cancelling = true;

            //}
            //if ((bulls + cows == 4) || corr + cows == 4)
            //{
            //    print("break22");

            //    for (int k = 0; k < 4; k++)
            //    {
            //        correctNums[k] = aIGuesses[k];
            //    }
            //    correctNumsFound = true;
            //}
        }


    }

    void CancelOut()
    {
        if(isInAns.Count == 4)
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
        else if (/*wrongCheckCounter > 9 ||*/ wrongCheck.Count == 4)
        {
            for (int i = 0; i < 4; i++)
            {
                correctNums[i] = wrongCheck[i];
                correctNumsFound = true;
            }
            cancelling = false;
        }
        else
        {
            do
            {
                aIGuesses[3] = wrongCheckCounter;
                wrongCheckCounter++;
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

    void CheckForBulls()
    {
        if (idiot > 1 && !correctNumsFound)
        {
            gameState = GameState.RandomNumberGuessing;
            idiot = 0;
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

            for (int i = 0; i < 4; i++)
            {
                if (potentialAnswers[potAnsPtr].Substring(i, 1) == playerInputBox.text.Substring(i, 1))
                {
                    bulls++;
                }
            }
            potAnsPtr++;
            if (potAnsPtr == 24 && bulls != 4)
            {
                potAnsPtr = 0;
                correctNumsFound = false;
                //notInSlot = new bool[4, 10];
                potentialAnswers.Clear();
                gameState = GameState.RandomNumberGuessing;
                corr = 0;
                for (int i = 0; i < 4; i++)
                {
                    correctNums[i] = 0;
                    correctNumsBool[i] = false;
                }
                idiot++;

            }
        }
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
                    correctNums[i] = num;
                    correctNumsBool[i] = true;

                }
            }
        }
    }

    void Guess()
    {
        if (correctNumsFound)
        {
            if (wrongCheck.Count == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    correctNums[i] = wrongCheck[i];
                    correctNumsFound = true;
                }
                gameState = GameState.CheckForBulls;

                aIGuessText.text = "";

                foreach (var listMem in potentialAnswers[potAnsPtr])
                {
                    aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
                }
                previousGuessesText.text += turns + "       " + aIGuessText.text + " C" + "\n"; // adds current guess to the list of previous guesses


            }
            else
            {

                if (potentialAnswers.Count == 0)
                {
                    Permute(correctNums, 0, correctNums.Count - 1); // fill potentialAnswers with permutations of correctNums
                }
                bool skip = false;
                //do
                //{
                //    skip = false;
                //    for (int i = 0; i < 4; i++)
                //    {
                //        if (notInSlot[i, int.Parse(potentialAnswers[potAnsPtr].Substring(i, 1))])
                //        {
                //            potAnsPtr++;
                //            potAnsPtr = Mathf.Clamp(potAnsPtr, 0, 23);

                //            skip = true;
                //        }
                //    }
                //} while (skip && potAnsPtr >= 23);

                //if (bulls == 0)
                //{
                //    if (potAnsPtr <= 6)
                //    {
                //        potAnsPtr = 6;
                //    }
                //    else if (potAnsPtr > 6 && potAnsPtr <= 12)
                //    {
                //        potAnsPtr = 12;
                //    }
                //    else if (potAnsPtr > 12 && potAnsPtr <= 18)
                //    {
                //        potAnsPtr = 18;
                //    }

                //}
                aIGuessText.text = "";

                foreach (var listMem in potentialAnswers[potAnsPtr])
                {
                    aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
                }
                previousGuessesText.text += turns + "       " + aIGuessText.text + " C" + "\n"; // adds current guess to the list of previous guesses
            }
            gameState = GameState.CheckForBulls;
        }
        else
        {

            string s = "";
            int counter = -1;

            do
            {
                print(2);
                List<int> usedNums = new List<int>(); // to be filled with number that were already used in the current guess

                AddCorrectNums(usedNums);

                s = "";
                print(3);
                for (int i = 0; i < aIGuesses.Count; i++) //loop to guess the four numbers of current guess
                {

                    //if (correctNumsBool[i])
                    //{
                    //    aIGuesses[i] = correctNums[i];
                    //}
                    //else
                    //{
                        int looped = 0;

                        do
                        {
                            //do
                            //{
                                print("1");
                                counter++;
                                if (counter > 9)// numbers can only be 0 - 9
                                {
                                    counter = 0;
                                }
                                counter = RandomNumber();
                                looped++;
                            //}
                            //while (notInSlot[i, counter]/* || usedNums.Contains(counter)*//* && looped < 10*/);

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
                    //}
                    if (i >= 0)
                    {
                        s += aIGuesses[i];
                    }
                }
            }
            while (previousGuesses.Contains(s));

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
            previousGuessesText.text += turns + "       " + aIGuessText.text + "\n";

            gameState = GameState.BullsAndCows;

        }

    }
    void CheckPermutationList()
    {
        if (potentialAnswers.Count == 0)
        {
            Permute(correctNums, 0, correctNums.Count - 1); // fill potentialAnswers with permutations of correctNums
        }
        bool skip = false;
        do
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

        //if (bulls == 0)
        //{
        //    if (potAnsPtr <= 5)
        //    {
        //        potAnsPtr = 6;
        //    }
        //    else if (potAnsPtr > 5 && potAnsPtr <= 11)
        //    {
        //        potAnsPtr = 12;
        //    }
        //    else if (potAnsPtr > 11 && potAnsPtr <= 17)
        //    {
        //        potAnsPtr = 18;
        //    }

        //}
        aIGuessText.text = "";

        foreach (var listMem in potentialAnswers[potAnsPtr])
        {
            aIGuessText.text += listMem.ToString(); // sets aIGuessText to the current AI guess
        }
        previousGuessesText.text += turns + "       " + aIGuessText.text + " C" + "\n"; // adds current guess to the list of previous guesses

    }
    void RandomGuess()
    {
        string s = "";
        int counter = -1;

        do
        {
            print(2);
            List<int> usedNums = new List<int>(); // to be filled with number that were already used in the current guess

            AddCorrectNums(usedNums);

            s = "";
            print(3);
            for (int i = 0; i < aIGuesses.Count; i++) //loop to guess the four numbers of current guess
            {

                //if (correctNumsBool[i])
                //{
                //    aIGuesses[i] = correctNums[i];
                //}
                //else
                //{
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
                //}
                if (i >= 0)
                {
                    s += aIGuesses[i];
                }
            }
        }
        while (previousGuesses.Contains(s));

        previousGuesses.Add(s);

        foreach (var listMem in aIGuesses)
        {
            aIGuessText.text += listMem.ToString();
        }
        aIGuessLog.Add(aIGuessText.text);
        previousGuessesText.text += turns + "       " + aIGuessText.text + "\n";

        gameState = GameState.BullsAndCows;

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
    GameOver,
    WrongChecking,
    RandomNumberGuessing,
    NarrowingDownCorrectNumbers,
    GoingThroughPermutations
}

public enum GuessState
{
    Random,
    Cancelling,
    Permutations,
}
