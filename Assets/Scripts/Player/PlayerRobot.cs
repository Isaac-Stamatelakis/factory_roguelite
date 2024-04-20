using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;
using ItemModule;
using ChunkModule.ClosedChunkSystemModule;
using ChunkModule;
using TileMapModule;
using TileEntityModule;

public class PlayerRobot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private Rigidbody2D rb;
    private int noCollisionWithPlatformCounter;
    private GlobalUIContainer globalUIContainer;
    private bool onGround;
    public bool OnGround { get => onGround; set => onGround = value; }
    public int NoCollisionWithPlatformCounter { get => noCollisionWithPlatformCounter; set => noCollisionWithPlatformCounter = value; }
    private bool climbing;    
    [SerializeField] public RobotItem robotItem;
    [SerializeField] public Robot overrideRobot;
    private Robot currentRobot;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        globalUIContainer = GlobalUIContainer.getInstance();
    }
    
    

    

    public void FixedUpdate() {
        canStartClimbing();
        float playerWidth = spriteRenderer.sprite.bounds.extents.x;
        
        if (climbing) {
            handleClimbing();
            return;
        }
        noCollisionWithPlatformCounter--;
        Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y);
        
        int layers = (1 << LayerMask.NameToLayer("Block") | 1 << LayerMask.NameToLayer("Platform") | 1 << LayerMask.NameToLayer("SlipperyBlock"));
        RaycastHit2D raycastHit = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,0.1f),0,Vector2.zero,Mathf.Infinity,layers);
        if (raycastHit.collider != null) {
            onGround = true;
        }
        int platformLayer = (1 << LayerMask.NameToLayer("Platform"));
        RaycastHit2D platformCast = Physics2D.BoxCast(bottomCenter,new Vector2(playerWidth,0.2f),0,Vector2.zero,Mathf.Infinity,layers);
        if (platformCast.collider == null) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),LayerMask.NameToLayer("Platform"), true);
        } else {
            if (noCollisionWithPlatformCounter > 0) {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),LayerMask.NameToLayer("Platform"), true);
            }
            if (rb.velocity.y > 0.2) {
                noCollisionWithPlatformCounter=5;
            }
            if (noCollisionWithPlatformCounter == 0) {
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),LayerMask.NameToLayer("Platform"), false);
            }
        }
        if (!globalUIContainer.isActive()) {
            if (currentRobot == null) {
                handleEngineerMovement();
            } else {
                currentRobot.handleMovement(transform);
            }
        }
        
    }

    private void handleEngineerMovement() {
        
    }

    private void canStartClimbing() {
        bool climbKeyInput = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S);
        if (climbing || !climbKeyInput) {
            return;
        }
        if (!standingOnClimbable()) {
            return;
        }
        climbing = true;
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = 0;
        Vector3 position = transform.position;
        float x = position.x;
        x = Mathf.Floor(x*2)/2f+0.25f;
        position.x = x;
        transform.position = position;
    }
    private bool standingOnClimbable() {
        int objectLayer = (1 << LayerMask.NameToLayer("Object"));
        RaycastHit2D objHit = Physics2D.BoxCast(transform.position,new Vector2(0.5f,0.1f),0,Vector2.zero,Mathf.Infinity,objectLayer);
        if (objHit.collider == null) {
            return false;
        }
        TileGridMap tileGridMap = objHit.collider.GetComponent<TileGridMap>();
        if (tileGridMap == null) {
            return false;
        }
        TileItem tileItem = tileGridMap.getTileItem(Global.getCellPositionFromWorld(transform.position));
        if (tileItem == null || tileItem.tileEntity == null) {
            return false;
        }
        return tileItem.tileEntity is IClimableTileEntity;
    }

    private void handleClimbing() {
        int speed = 7;
        Vector2 velocity = rb.velocity;
        rb.velocity = velocity;
        if (Input.GetKey(KeyCode.W)) {
            velocity.y = speed;
        } else if (Input.GetKey(KeyCode.S)) {
            velocity.y = -speed;
        } else {
            velocity.y = 0;
        }
        rb.velocity = velocity;
        
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D) || !standingOnClimbable()) {
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            climbing = false;
            rb.gravityScale = 3;
        } 
    }

    public void setRobot(RobotItem robotItem) {
        if (spriteRenderer == null) {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
        if (rb == null) {
            rb = GetComponent<Rigidbody2D>();
        }
        if (overrideRobot != null) {
            currentRobot = overrideRobot;
        } else {
            this.robotItem = robotItem;
            currentRobot = robotItem.robot;
        }
        
        if (currentRobot == null) {
            // Play as engineer
        } else {
            currentRobot.init(gameObject);
            spriteRenderer.sprite = currentRobot.defaultSprite;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        /*
        if (collision.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            Debug.Log(true);
        }
        */
    }

    protected void rebuildCollider() {
        Sprite sprite = spriteRenderer.sprite;
        polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
        List<Vector2> path = new List<Vector2>();
            for (int i = 0; i < polygonCollider.pathCount; i++) {
            path.Clear();
            sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }
}
