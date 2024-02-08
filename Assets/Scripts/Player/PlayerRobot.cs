using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RobotModule;

public class PlayerRobot : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private PolygonCollider2D polygonCollider;
    void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = robot.defaultSprite;
        polygonCollider = GetComponent<PolygonCollider2D>();
        robot.init(gameObject);
        rebuildCollider();
    }
    [SerializeField]
    public Robot robot;

    public void FixedUpdate() {
        robot.handleMovement(transform);
    }

    public void setRobot(Robot robot) {

    }

    protected void rebuildCollider() {
        Sprite sprite = spriteRenderer.sprite;
        polygonCollider.pathCount = sprite.GetPhysicsShapeCount();
        Debug.Log(polygonCollider.pathCount);
        List<Vector2> path = new List<Vector2>();
            for (int i = 0; i < polygonCollider.pathCount; i++) {
            path.Clear();
            sprite.GetPhysicsShape(i, path);
            polygonCollider.SetPath(i, path.ToArray());
        }
    }
}
