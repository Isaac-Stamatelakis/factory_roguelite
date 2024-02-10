using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;
using ItemModule;

public class PlayerRobot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    private Rigidbody2D rb;
    private int noCollisionWithPlatformCounter;
    private bool onGround;
    public bool OnGround { get => onGround; set => onGround = value; }
    public int NoCollisionWithPlatformCounter { get => noCollisionWithPlatformCounter; set => noCollisionWithPlatformCounter = value; }

    [SerializeField]
    public RobotItem robotItem;
    [SerializeField]
    public Robot overrideRobot;
    private Robot currentRobot;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }
    
    

    

    public void FixedUpdate() {
        
        noCollisionWithPlatformCounter--;
        Vector2 bottomCenter = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y - spriteRenderer.sprite.bounds.extents.y);
        float playerWidth = spriteRenderer.sprite.bounds.extents.x;
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
        
        if (currentRobot == null) {
            handleEngineerMovement();
        } else {
            currentRobot.handleMovement(transform);
        }
    }

    private void handleEngineerMovement() {

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
