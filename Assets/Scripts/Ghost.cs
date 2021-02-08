using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    public float moveSpeed = 3.99f;
    public float normalMoveSpeed = 3.99f;
    public float frightenedModeMoveSpeed = 2.99f;
    public float consumedMoveSpeed = 15.0f;

    public bool canMove = true;

    public Node startingPosition;
    public Node homeNode;
    public Node ghostHouse;

    public int pinkyReleaseTimer = 5;
    public int inkyReleaseTimer = 14;
    public int clydeReleaseTimer = 21;
    public float ghostReleaseTimer = 0;

    public int frightenedModeDuration = 10;
    public int startBlinkingAt = 7;

    public bool isInGhostHouse = false;

    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;

    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;

    public RuntimeAnimatorController consumedHook;
    public RuntimeAnimatorController normalHook;
    public RuntimeAnimatorController wormHook;
    public RuntimeAnimatorController whiteHook;

    private float previousMoveSpeed;

    private float frightenedModeTimer = 0;
    private float blinkTimer = 0;

    private bool frightenedEndModeIsWhite = false;

    private AudioSource backgroundAudio;
    public enum Mode
    {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }

    Mode currentMode = Mode.Scatter;
    Mode previousMode;

    public enum GhostType
    {
        Red,
        Pink,
        Blue,
        Orange
    }

    public GhostType ghostType = GhostType.Red;
    private GameObject PacMan;

    private Node currentNode, targetNode, previousNode;
    private Vector2 direction, nextDirection;
    // Start is called before the first frame update
    void Start()
    {
        SetDifficultyForLevel(GameBoard.playerLevel);
        backgroundAudio = GameObject.Find("GameBoard").GetComponent<AudioSource>();
        PacMan = GameObject.FindGameObjectWithTag("PacMan");
        Node node = GetNodeAtPosition(transform.localPosition);

        
        if(node != null)
        {
            currentNode = node;
            
        }
        if (isInGhostHouse)
        {
            direction = Vector2.up;

            targetNode = currentNode.neighbors[0];
        } else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }
        previousNode = currentNode;

    }

    void SetDifficultyForLevel(int level)
    {
        if (level == 2)
        {
            scatterModeTimer1 = 7;
            scatterModeTimer2 = 7;
            scatterModeTimer3 = 5;
            scatterModeTimer4 = 1;

            chaseModeTimer1 = 20;
            chaseModeTimer2 = 20;
            chaseModeTimer3 = 1033;

            frightenedModeDuration = 9;
            startBlinkingAt = 6;

            pinkyReleaseTimer = 4;
            inkyReleaseTimer = 12;
            clydeReleaseTimer = 18;

            moveSpeed = 4.95f;
            normalMoveSpeed = 4.95f;
            frightenedModeMoveSpeed = 3.9f;
            consumedMoveSpeed = 18f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove)
        {
            ModeUpdate();

            Move();

            ReleaseGhosts();

            CheckCollision();

            CheckIsInGhostHouse();
        }
    }

    void CheckIsInGhostHouse()
    {
 
        if (currentMode == Mode.Consumed)
        {
            
            GameObject tile = GetTileAtPosition(transform.position);
            if (tile != null)
            {
                
                if (tile.transform.GetComponent<Tiles> () != null)
                {

                    if (tile.transform.GetComponent<Tiles> ().isGhostHouse)
                    {

                        moveSpeed = normalMoveSpeed;
                        
                        Node node = GetNodeAtPosition(transform.position);
                        
                        if(node != null)
                        {
                            currentNode = node;
                            direction = Vector2.up;                                        
                            targetNode = currentNode.neighbors[0];

                            previousNode = currentNode;
                            currentMode = Mode.Chase;

                            UpdateAnimatorController();
                        }
                    }
                }
            }
        }
    }

    public void Restart()
    {
        canMove = true;

        transform.GetComponent<SpriteRenderer>().enabled = true;

        currentMode = Mode.Scatter;

        moveSpeed = normalMoveSpeed;

        previousMoveSpeed = 0;

        transform.position = startingPosition.transform.position;

        ghostReleaseTimer = 0;
        modeChangeIteration = 1;
        modeChangeTimer = 0;

        if (transform.name != "hook (red)")
            isInGhostHouse = true;
        currentNode = startingPosition;
        if (isInGhostHouse)
        {
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        }
        else
        {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }
        previousNode = currentNode;
        UpdateAnimatorController();
    }

    void CheckCollision()
    {
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);
        Rect pacManRect = new Rect(PacMan.transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size / 4);

        if (ghostRect.Overlaps(pacManRect))
        {
            if (currentMode == Mode.Frightened)
            {
                Consumed();
            }
            else
            {
                if (currentMode != Mode.Consumed)
                {
                    Debug.Log("Im Running");
                    //-Pac Man Dies
                    GameObject.Find("GameBoard").transform.GetComponent<GameBoard>().StartDeath();
                }
            }
        }
    }

    void Consumed()
    {
        GameBoard.Score += 200;
        currentMode = Mode.Consumed;
        previousMoveSpeed = moveSpeed;
        moveSpeed = consumedMoveSpeed;
        UpdateAnimatorController();

        GameObject.Find("GameBoard").transform.GetComponent<GameBoard>().StartConsumed(this.GetComponent<Ghost>());
    }
    public void StartFrightenedMode()
    {
        if (currentMode != Mode.Consumed)
        {
            frightenedModeTimer = 0;
            backgroundAudio.clip = GameObject.Find("GameBoard").transform.GetComponent<GameBoard>().backgroundAudioFrightened;
            backgroundAudio.Play();
            ChangeMode(Mode.Frightened);
            UpdateAnimatorController();
        }
    }

    void UpdateAnimatorController()
    {
        if (currentMode != Mode.Frightened && currentMode != Mode.Consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = normalHook;
        }
        else if (currentMode == Mode.Frightened)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = wormHook;
        }
        else if(currentMode == Mode.Consumed)
        {
            transform.GetComponent<Animator>().runtimeAnimatorController = consumedHook;
        }
    }

    void Move()
    {
        if (targetNode != currentNode && targetNode != null && !isInGhostHouse)
        {
            if (OverShotTarget())
            {
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;
                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;
            }
            else
            {
                transform.localPosition += (Vector3)direction * moveSpeed * Time.deltaTime;
            }
        }
    }

    void ModeUpdate()
    {
        if (currentMode != Mode.Frightened)
        {

            modeChangeTimer += Time.deltaTime;
            if(modeChangeIteration == 1)
            {
                if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1)
                {
                    modeChangeIteration = 2;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            } else if (modeChangeIteration == 2)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2)
                {
                    modeChangeIteration = 3;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            } else if(modeChangeIteration == 3)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if (currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3)
                {
                    modeChangeIteration = 4;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            } else if (modeChangeIteration == 4)
            {
                if (currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4)
                {
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        } else if (currentMode == Mode.Frightened)
        {
            frightenedModeTimer += Time.deltaTime;

            if (frightenedModeTimer >= frightenedModeDuration)
            {
                backgroundAudio.clip = GameObject.Find("GameBoard").transform.GetComponent<GameBoard>().backgroundAudioNorm;
                backgroundAudio.Play();
                frightenedModeTimer = 0;
                ChangeMode(previousMode);
            }
            if (frightenedModeTimer >= startBlinkingAt)
            {
                blinkTimer += Time.deltaTime;
                if(blinkTimer >= 0.1f)
                {
                    blinkTimer = 0f;
                    if (frightenedEndModeIsWhite)
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = wormHook;
                        frightenedEndModeIsWhite = false;
                    }
                    else
                    {
                        transform.GetComponent<Animator>().runtimeAnimatorController = whiteHook;
                        frightenedEndModeIsWhite = true;
                    }
                }
            }
        }
    }
    void ChangeMode (Mode m)
    {

        if(currentMode == Mode.Frightened)
        {
            moveSpeed = previousMoveSpeed;
        }
        if (m == Mode.Frightened)
        {
            previousMoveSpeed = moveSpeed;
            moveSpeed = frightenedModeMoveSpeed;
        }
        if (currentMode != m)
        {
            previousMode = currentMode;
            currentMode = m;
        }

        UpdateAnimatorController();

    }

    Vector2 GetRedGhostTargetTile()
    {
        Vector2 pacManPosition = PacMan.transform.localPosition;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));

        return targetTile;
    }
    Vector2 GetPinkGhostTargetTile()
    {
        //- Four Tiles ahead of Pac Man
        //- Taking account Position and Orientation
        Vector2 pacManPosition = PacMan.transform.localPosition;
        Vector2 pacManOrientation = PacMan.GetComponent<PacMan>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;
    }

    Vector2 GetBlueGhostTargetTile()
    {
        Vector2 pacManPosition = PacMan.transform.localPosition;
        Vector2 pacManOrientation = PacMan.GetComponent<PacMan>().orientation;

        int pacManPositionX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPositionY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPositionX, pacManPositionY);

        Vector2 targetTile = pacManTile + (2 * pacManOrientation);
        Vector2 tempBlinkyPosition = GameObject.Find("hook (red)").transform.localPosition;

        int blinkyPositionX = Mathf.RoundToInt(tempBlinkyPosition.x);
        int blinkyPositionY = Mathf.RoundToInt(tempBlinkyPosition.y);

        tempBlinkyPosition = new Vector2(blinkyPositionX, blinkyPositionY);

        float distance = GetDistance(tempBlinkyPosition, targetTile);
        distance *= 2;

        targetTile = new Vector2(tempBlinkyPosition.x + distance, tempBlinkyPosition.y + distance);

        return targetTile;
    }

    Vector2 GetOrangeGhostTargetTile()
    {
        Vector2 pacManPosition = PacMan.transform.localPosition;

        float distance = GetDistance(transform.localPosition, pacManPosition);
        Vector2 targetTile = Vector2.zero;

        if(distance > 8)
        {
            targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));
        }else if (distance < 8)
        {
            targetTile = homeNode.transform.position;
        }
        return targetTile;
    }
    Vector2 GetTargetTile()
    {
        Vector2 targetTile = Vector2.zero;
        if (ghostType == GhostType.Red) {
            targetTile = GetRedGhostTargetTile();
        } else if (ghostType == GhostType.Pink)
        {
            targetTile = GetPinkGhostTargetTile();
        } else if (ghostType == GhostType.Blue)
        {
            targetTile = GetBlueGhostTargetTile();
        } else if (ghostType == GhostType.Orange)
        {
            targetTile = GetOrangeGhostTargetTile();
        }
        return targetTile;
    }

    Vector2 GetRandomTile()
    {
        int x = Random.Range(9, 37);
        int y = Random.Range(8, 36);

        return new Vector2(x, y);
    }

    void ReleasePinkGhost()
    {
        if(ghostType == GhostType.Pink && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost()
    {
        if (ghostType == GhostType.Blue && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }

    void ReleaseOrangeGhost()
    {
        if (ghostType == GhostType.Orange && isInGhostHouse)
        {
            isInGhostHouse = false;
        }
    }
    
    void ReleaseGhosts()
    {
        ghostReleaseTimer += Time.deltaTime;

        if (ghostReleaseTimer > pinkyReleaseTimer)
        {
            ReleasePinkGhost();
        }
        if (ghostReleaseTimer > inkyReleaseTimer)
        {
            ReleaseBlueGhost();
        }
        if (ghostReleaseTimer > clydeReleaseTimer)
        {
            ReleaseOrangeGhost();
        }
    }

    Node ChooseNextNode()
    {
        Vector2 targetTile = Vector2.zero;
        if (currentMode == Mode.Chase)
        {
            targetTile = GetTargetTile();
        }
        else if (currentMode == Mode.Scatter)
        {
            targetTile = homeNode.transform.position;
        }
        else if (currentMode == Mode.Frightened)
        {
            targetTile = GetRandomTile();
        }
        else if (currentMode == Mode.Consumed)
        {
            targetTile = ghostHouse.transform.position;
        }

        Node moveToNode = null;

        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        for (int i = 0; i < currentNode.neighbors.Length; i++)
        {
            if(currentNode.validDirections [i] != direction * -1)
            {
                if (currentMode != Mode.Consumed)
                {
                    GameObject tile = GetTileAtPosition(currentNode.transform.position);
                    if (tile.transform.GetComponent<Tiles>().isGhostHouseEntrance == true)
                    {
                        if (currentNode.validDirections[i] != Vector2.down)
                        {
                            foundNodes[nodeCounter] = currentNode.neighbors[i];
                            foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                            nodeCounter++;
                        }
                    }
                    else
                    {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                }
                else
                {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }

            }
        }
        if(foundNodes.Length == 1)
        {
            moveToNode = foundNodes[0];
            direction = foundNodesDirection[0];
        }
        if (foundNodes.Length > 1)
        {
            float leastDistance = 100000f;

            for (int i = 0; i < foundNodes.Length; i++)
            {
                if (foundNodesDirection [i] != Vector2.zero)
                {
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);

                    if (distance < leastDistance)
                    {
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }
        return moveToNode;
    }

    Node GetNodeAtPosition (Vector2 pos)
    {
        GameObject tile = GameObject.Find("GameBoard").GetComponent<GameBoard>().board[(int)pos.x, (int)pos.y];

        if (tile != null)
        {
            if (tile.GetComponent<Node>() != null)
            {
                return tile.GetComponent<Node>();
            }
        }
        return null;
    }

   GameObject GetTileAtPosition (Vector2 pos)
    {
        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameObject.Find("GameBoard").transform.GetComponent<GameBoard>().board[tileX, tileY];
        if (tile != null)
            return tile;
        return null;
    }

    float LengthFromNode (Vector2 targetPosition)
    {
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }
    bool OverShotTarget()
    {
        float nodeToTarget = LengthFromNode(targetNode.transform.position);
        float nodeToSelf = LengthFromNode(transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float GetDistance (Vector2 posA, Vector2 posB)
    {
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;

        float distance = Mathf.Sqrt(dx * dx + dy * dy);

        return distance;
    }
}
