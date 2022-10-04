using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor.Audio;
using Random = UnityEngine.Random;
using TMPro;
using System;

public class Game_Manager : MonoBehaviour
{   
    public List<int> aiList = new List<int>();
    public List<int> userList = new List<int>();

    [SerializeField] private int maxGameRounds = 3;
    [SerializeField] private int currentRound = 0;
    [SerializeField] private int highScore;
    [SerializeField] private int currentScore;
    [SerializeField] private float percent;
    [SerializeField] private bool gameOver;

    [Header("Timer Elements")]
    [SerializeField] private float timer = 5f;
    [SerializeField] private float maxTimer = 5f;
    [SerializeField] private bool timerStart;
    [SerializeField] private bool timerGameOver;


    [Header("UI Elements")]
    public GameObject startButton;
    public GameObject playAgainButton;
    public GameObject gameOverImage;
    public GameObject finalStatsScreen;
    public Image medal;
    public Image sparkles;//should this be gameobject or image? lets experiment
    public Image newHighScore;

    public Button[] clickableButtons = new Button[4];
    public CanvasGroup buttons;
    private List<List<Color32>> colors = new List<List<Color32>>();

    [Header("UI_Texts")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI victoryText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Audio")]
    public List<AudioClip> buttonSoundsList = new List<AudioClip>();
    public AudioClip gameOverSound;
    public AudioClip nextRound;
    public AudioClip winGame;
    public AudioClip gameMusic;
    public AudioSource audioSource;

    [Header("Medals")]
    public Sprite[] medals = new Sprite[4];

    [Header("Animations")]
    [SerializeField] private AnimationClip finalStatsDropdown;
    [SerializeField] private AnimationClip medalFlip;


    public void Awake()
    {
        colors.Add(new List<Color32> { new Color32(164, 255, 124, 255), new Color32(72, 248, 0, 255) }); // green
        colors.Add(new List<Color32> { new Color32(255, 100, 100, 255), new Color32(255, 0, 0, 255) }); // red
        colors.Add(new List<Color32> { new Color32(255, 255, 136, 255), new Color32(255, 255, 0, 255) }); // yellow
        colors.Add(new List<Color32> { new Color32(57, 111, 255, 255), new Color32(0, 70, 255, 255) }); // blue

        for (int i = 0; i < clickableButtons.Length; i++)
        {
            clickableButtons[i].GetComponent<Image>().color = colors[i][0];
        }
    }

    public void Start()
    {
        playAgainButton.SetActive(false);
        buttons.interactable = false;
        finalStatsScreen.SetActive(false);
        medal.gameObject.SetActive(false);
        sparkles.gameObject.SetActive(false);
        gameOverImage.SetActive(false);
        victoryText.gameObject.SetActive(false);
        scoreText.enabled = false;
        newHighScore.enabled = false;
    }

    private void Update()
    {
        UpdateScoreTexts();

        Countdown();

        percent = ((float)currentScore / maxGameRounds) * 100;// why doesn't mathf.roundtoint work???

    }

    public void Countdown()
    {
        if (timerStart)
        {
            timer -= Time.deltaTime;
        }

        if (timer <= 0 && timerGameOver == false)
        {
            timerGameOver = true;
            StartCoroutine(GameOver());
        }
    }

    public void UpdateScoreTexts()
    {
        //show score/high score text at appropriate moments

        finalScoreText.text = currentScore.ToString();
        scoreText.text = currentScore.ToString();

        if (highScore < currentScore)
        {
            highScore = currentScore;
            newHighScore.enabled = true;
        }

        highScoreText.text = highScore.ToString();

        //add new high score image when appropriate
    }

    //clickable button to start game
    public void StartButton()
    {
        startButton.SetActive(false);
        scoreText.enabled = true;
        //finalScoreText.gameObject.SetActive(false);
        StartCoroutine(NextRound());
    }

    public void UserSelectButton(int buttonID)
    {
        userList.Add(buttonID);

        StartCoroutine (ChangeButtonColor(buttonID));
        //audioSource.Play(buttonSoundsList[buttonID]);//why does audioSource.play not work whie oneshot does?
        //audioSource.PlayOneShot(buttonSoundsList[buttonID]);
        ResetTimer();
        //timerStart = true;

            for (int i = 0; i < userList.Count; i++)
            {
                if (userList[i] == aiList[i])
                {
                    continue;
                }

                else
                {
                    gameOver = true;
                }

                if (gameOver && userList.Count == aiList.Count)
                {
                    StartCoroutine("GameOver");
                    return;
                }
            }

            if (userList.Count == aiList.Count)
            {
                buttons.interactable = false;
                //currentScore++;
                StartCoroutine(NextRound());
                //break;
            }
    }

    public IEnumerator NextRound()
    {
        Debug.Log("next round!");

        ResetTimer();
        timerStart = false;

        if (currentRound == 0)
        {
            yield return new WaitForSeconds(1f);
        }

        else
        {
            yield return new WaitForSeconds(Random.Range(2.5f, 4.5f));
            currentScore++;
        }

        if (currentRound > 0 && currentRound < maxGameRounds)
        {
            audioSource.PlayOneShot(nextRound);
        }

        currentRound++;

        userList.Clear();

        if (currentRound <= maxGameRounds)//might be less than or equal to maxgamerounds
        {
            buttons.interactable = false;

            //different way to write the same line as above?
            /*foreach (var button in clickableButtons)
            {
                button.interactable = false;
            }*/

            //if(buttonsinteractble = false) {Debug.Log("button interactable bro");}

            int random = Random.Range(0, 4);
            aiList.Add(random);

            yield return new WaitForSeconds(1f);

            foreach (int index in aiList)
            {
                yield return StartCoroutine(ChangeButtonColor(index));
            }

            //sound indication that its players turn???
            buttons.interactable = true;

            timerStart = true;
        }

        else if (currentRound > maxGameRounds)//might be greater than maxgamerounds
        {
            StartCoroutine(Victory());
        }
    }

    public IEnumerator ChangeButtonColor(int id)
    {
        audioSource.PlayOneShot(buttonSoundsList[id]);

        clickableButtons[id].GetComponent<Image>().color = colors[id][1];
        yield return new WaitForSeconds(.5f);
        clickableButtons[id].GetComponent<Image>().color = colors[id][0];
        yield return new WaitForSeconds(.5f);

        //clickableButtons[id].GetComponent<Image>().color = clickableButtons[2].colors.highlightedColor;
    }   

    public IEnumerator GameOver()
    {
        gameOver = true;
        timerStart = false;

        aiList.Clear();
        userList.Clear();
        buttons.interactable = false;

        if (timerGameOver)
        {
            Debug.Log("timer g.o");
            audioSource.PlayOneShot(gameOverSound);

            yield return new WaitForSeconds(gameOverSound.length);
            scoreText.enabled = false;
            finalStatsScreen.SetActive(true);
            gameOverImage.SetActive(true);

            yield return new WaitForSeconds(finalStatsDropdown.length);
            StartCoroutine(MedalCeremony());

            yield return new WaitForSeconds(medalFlip.length);
            playAgainButton.SetActive(true);

        }

        else
        {
            yield return new WaitForSeconds(Random.Range(2f, 4f));
            audioSource.PlayOneShot(gameOverSound);

            yield return new WaitForSeconds(gameOverSound.length);
            scoreText.enabled = false;
            finalStatsScreen.SetActive(true);
            gameOverImage.SetActive(true);

            yield return new WaitForSeconds(finalStatsDropdown.length);
            StartCoroutine(MedalCeremony());

            yield return new WaitForSeconds(medalFlip.length);
            playAgainButton.SetActive(true);
        }
    }

    public void PlayAgain()
    {
        gameOver = false;
        timerGameOver = false;
        currentRound = 0;
        currentScore = 0;

        newHighScore.enabled = false;
        playAgainButton.SetActive(false);
        finalStatsScreen.SetActive(false);
        gameOverImage.SetActive(false);
        victoryText.gameObject.SetActive(false);
        medal.gameObject.SetActive(false);
        sparkles.gameObject.SetActive(false);
        scoreText.enabled = true;
        aiList.Clear();
        userList.Clear();

        StartCoroutine("NextRound");
    }

    public IEnumerator Victory()
    {
        audioSource.PlayOneShot(winGame);
        timerStart = false;

        buttons.interactable = false;
        scoreText.enabled = false;
        aiList.Clear();
        userList.Clear();

        //yield return new WaitForSeconds(winGame.length);
        yield return new WaitForSeconds(1.5f);


        playAgainButton.SetActive(true);//set after sound clip plays???
        finalStatsScreen.SetActive(true);
        victoryText.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);
        StartCoroutine(MedalCeremony());

        yield return new WaitForSeconds(medalFlip.length);
        playAgainButton.SetActive(true);



    }

    public void ResetTimer()
    {
        timer = maxTimer;
    }

    public IEnumerator MedalCeremony()
    {
        //percent = Mathf.RoundToInt(currentScore/maxGameRounds) * (100);

        if (percent >= 0 && percent < 33.333)
        {
            //medal.sprite = medals[0];//sprite=null returned blank white square... here i just change opacity to zero
            medal.gameObject.SetActive(false);
            sparkles.gameObject.SetActive(false);
        }

        else if (percent >= 33.333 && percent < 66.666)
        {
            medal.gameObject.SetActive(true);
            medal.sprite = medals[1];
            yield return new WaitForSeconds(medalFlip.length);
            sparkles.gameObject.SetActive(true);
        }

        else if (percent >= 66.666 && percent < 100)
        {
            medal.gameObject.SetActive(true);
            medal.sprite = medals[2];
            yield return new WaitForSeconds(medalFlip.length);
            sparkles.gameObject.SetActive(true);
        }

        else if (percent == 100)
        {
            medal.gameObject.SetActive(true);
            medal.sprite = medals[3];
            yield return new WaitForSeconds(medalFlip.length);
            sparkles.gameObject.SetActive(true);
        }
    }
}




