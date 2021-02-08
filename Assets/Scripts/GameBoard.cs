using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour
{
    private static int boardWidth = 100;
    private static int boardHeight = 100;

    private bool didStartDeath = false;
    private bool didStartConsumed = false;
    private bool shouldBlink = false;

    public int pelletsConsumed = 0;
    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;
    public static int playerLevel = 1;

    public Text playerText;
    public Text readyText;

    public static int Score = 0;
    public int totalPellets = 0;
    public int pacManLives = 3;

    public AudioClip backgroundAudioNorm;
    public AudioClip backgroundAudioFrightened;
    public AudioClip backgroundPacManDeathSound;
    public AudioClip consumedGhostAudioClip;

    public Text highScoreText;
    public Text OneUpScoreText;
    public Image playerLives2;
    public Image playerLives3;
    public Text consumedGhostScoreText;

    bool didSpawnBonusItem1;
    bool didSpawnBonusItem2;


    public GameObject[,] board = new GameObject[boardWidth, boardHeight];

    public Image[] levelImages;

    private bool didIncrementLevel = false;
    // Start is called before the first frame update
    void Start()
    {
        consumedGhostScoreText.GetComponent<Text>().enabled = false;
        Object[] objects = GameObject.FindObjectsOfType(typeof(GameObject));

        foreach (GameObject o in objects)
        {
            Vector2 pos = o.transform.position;

            if (o.name != "PacMan" && o.name != "Nodes" && o.name != "NonNodes" && o.tag != "maze"&& o.tag != "maze_fg" && o.name != "Pellets" && o.tag != "Ghost" && o.tag != "ghostHome" && o.name !="Canvas" && o.tag != "UIElements")
            {
                board[(int)pos.x, (int)pos.y] = o;
                if (o.GetComponent<Tiles>() != null)
                {
                    if (o.GetComponent<Tiles>().isPellet || o.GetComponent<Tiles>().isSuperPellet)
                    {
                        totalPellets++;
                    }
                }

            }
        }
        Debug.Log(playerLevel);
        
        StartGame();
    }


    void Update()
    {
        UpdateUI();
        CheckPelletsConsumed();
        CheckShouldBlink();
        //BonusItems();
    }
    void BonusItems()
    {
        SpawnBonusItem();
    }

    void SpawnBonusItem()
    {
        if (pelletsConsumed >= 70 && pelletsConsumed < 170)
        {

            if (!didSpawnBonusItem1)
            {
               
                didSpawnBonusItem1 = true;
                SpawnBonusItemForLevel(playerLevel);
            }
            
        }
        else if (pelletsConsumed >= 170)
        {
            if (!didSpawnBonusItem2)
            {
                didSpawnBonusItem2 = true;
                SpawnBonusItemForLevel(playerLevel);
            }
        }
    }

    void SpawnBonusItemForLevel(int level)
    {
        GameObject bonusitem = null;

        if(level == 1)
        {

            bonusitem = Resources.Load("Prefabs/jellyfish", typeof(GameObject)) as GameObject;
        }
        else if (level == 2)
        {
            bonusitem = Resources.Load("Prefabs/slug", typeof(GameObject)) as GameObject;
        }

        Instantiate(bonusitem);
    }

    void UpdateUI()
    {
        OneUpScoreText.text = Score.ToString();

        int currentLevel;
        currentLevel = playerLevel;
        if (pacManLives == 3)
        {
            playerLives3.enabled = true;
            playerLives2.enabled = true;
        }
        else if(pacManLives == 2)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = true;
        }
        else if (pacManLives == 1)
        {
            playerLives3.enabled = false;
            playerLives2.enabled = false;
        }

        for(int i = 1;i < levelImages.Length; i++)
        {
            Image li = levelImages[i];
            li.enabled = false;
        }
        for (int i = 1;i < levelImages.Length + 1; i++)
        {
            if (currentLevel >= i)
            {
                Image li = levelImages[i - 1];
                li.enabled = true;
            }
        }
    }

    void CheckPelletsConsumed()
    {
        GameObject pacMan = GameObject.Find("PacMan");
        

        if(totalPellets == pelletsConsumed)
        {
            PlayerWin();
        }
    }

    void PlayerWin()
    {
        if (!didIncrementLevel)
        {
            didIncrementLevel = true;
            playerLevel++;
            didSpawnBonusItem1 = false;
            didSpawnBonusItem2 = false;
            StartCoroutine(ProcessWin(2));
        }
        if(playerLevel == 3)
        {
            StopAllCoroutines();
            SceneManager.LoadScene("GameOverMenu 1");
        }
    }

    IEnumerator ProcessWin (float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;

        transform.GetComponent<AudioSource>().Stop();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");
        
        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().canMove = false;
            ghost.transform.GetComponent<Animator>().enabled = false;
        }

        yield return new WaitForSeconds(delay);
        
        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard (float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
      
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {

            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }
        // - Blink Board
        shouldBlink = true;
        yield return new WaitForSeconds (delay);
        //- Restart the game
        shouldBlink = false;
        StartNextLevel();

    }

    private void StartNextLevel()
    {
        Debug.Log("ran startnextlevel");
        SceneManager.LoadScene("Level 1");
    }

    private void CheckShouldBlink()
    {
        if (shouldBlink)
        {
            if(blinkIntervalTimer < blinkIntervalTime)
            {
                blinkIntervalTimer += Time.deltaTime;
            }
            else
            {
                blinkIntervalTimer = 0;
                if(GameObject.Find("maze_fg").transform.GetComponent<SpriteRenderer>().enabled == true)
                {
                    GameObject.Find("maze_fg").transform.GetComponent<SpriteRenderer>().enabled = false;
                }
                else
                {
                    GameObject.Find("map_fg").transform.GetComponent<SpriteRenderer>().enabled = true;
                }
            }
        }
    }

    public void StartGame()
    {
        //-Hide All Ghosts
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        pacMan.transform.GetComponent<PacMan>().canMove = false;

        StartCoroutine(ShowObjectsAfter(2.25f));
    }
    public void StartConsumed(Ghost consumedGhost)
    {
        if (!didStartConsumed)
        {
            didStartConsumed = true;

            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            Vector2 pos = consumedGhost.transform.position;

            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

            consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostScoreText.GetComponent<Text>().enabled = true;

            transform.GetComponent<AudioSource>().PlayOneShot(consumedGhostAudioClip);

            StartCoroutine(ProcessConsumedAfter(0.75f, consumedGhost));
        }
    }

    public void StartConsumedBonusItem(GameObject bonusItem, int scoreValue)
    {
        Debug.Log("attempted");
        Vector2 pos = bonusItem.transform.position;

        Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(pos);

        consumedGhostScoreText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
        consumedGhostScoreText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

        consumedGhostScoreText.text = scoreValue.ToString();

        consumedGhostScoreText.GetComponent<Text>().enabled = true;

        Destroy(bonusItem.gameObject);

        StartCoroutine(ProcessConsumedBonusItem(0.75f));
    }

    IEnumerator ProcessConsumedBonusItem (float delay)
    {
        yield return new WaitForSeconds(delay);
        consumedGhostScoreText.GetComponent<Text>().enabled = false;
    }

    IEnumerator StartBlinking (Text blinkText)
    {
        yield return new WaitForSeconds(0.25f);

        blinkText.GetComponent<Text>().enabled = !blinkText.GetComponent<Text>().enabled;
        StartCoroutine(StartBlinking(blinkText));
    }
    IEnumerator ProcessConsumedAfter (float delay, Ghost consumedGhost)
    {
        yield return new WaitForSeconds(delay);

        consumedGhostScoreText.GetComponent<Text>().enabled = false;

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {

            ghost.transform.GetComponent<Ghost>().canMove = true;
        }
        pacMan.transform.GetComponent<PacMan>().canMove = true;

        transform.GetComponent<AudioSource>().Play();

        didStartConsumed = false;
    }

    IEnumerator ShowObjectsAfter (float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;

        }
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;


        playerText.transform.GetComponent<Text>().enabled = false;

        StartCoroutine(StartGameAfter(2));
    }

    IEnumerator StartGameAfter (float delay)
    {
        yield return new WaitForSeconds(delay);
        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {

            ghost.transform.GetComponent<Ghost>().canMove = true;
        }
        GameObject pacMan = GameObject.Find("PacMan");

        pacMan.transform.GetComponent<PacMan>().canMove = true;

        readyText.transform.GetComponent<Text>().enabled = false;
    }

    public void StartDeath()
    {
        if (!didStartDeath)
        {
            StopAllCoroutines();
            GameObject bonusItem = GameObject.Find("bonusItem");
            if (bonusItem)
                Destroy(bonusItem.gameObject);
            didStartDeath = true;

            GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

            foreach (GameObject ghost in o)
            {
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            pacMan.transform.GetComponent<Animator>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            StartCoroutine(ProcessDeathAfter(2));

        }
    }

    IEnumerator ProcessDeathAfter (float delay)
    {
        yield return new WaitForSeconds(delay);

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }
        StartCoroutine(ProcessDeathAnimation(1.3f));
    }

    IEnumerator ProcessDeathAnimation (float delay)
    {
        GameObject pacMan = GameObject.Find("PacMan");

        pacMan.transform.localScale = new Vector3(1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;

        transform.GetComponent<AudioSource>().clip = backgroundPacManDeathSound;
        transform.GetComponent<AudioSource>().Play();

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(2));
    }

    IEnumerator ProcessRestart (float delay)
    {
        pacManLives -= 1;
        if (pacManLives == 0)
        {
            SceneManager.LoadScene("GameOverMenu");
        }
        else
        {
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            transform.GetComponent<AudioSource>().Stop();

            yield return new WaitForSeconds(delay);

            Restart();
        }
    }

    public void Restart()
    {
        
        didStartDeath = false;
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().Restart();

        GameObject[] o = GameObject.FindGameObjectsWithTag("Ghost");

        foreach (GameObject ghost in o)
        {
            ghost.transform.GetComponent<Ghost>().Restart();
        }
        transform.GetComponent<AudioSource>().clip = backgroundAudioNorm;
        transform.GetComponent<AudioSource>().Play();
    }
}
