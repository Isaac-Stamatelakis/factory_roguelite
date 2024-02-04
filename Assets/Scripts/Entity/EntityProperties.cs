using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityProperties : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    public virtual void Update()
    {
        setEntityToCurrentChunk();
    }

    public virtual void initalize() {
        
    }

    public void setLocation(float x, float y) {
        transform.position = new Vector3(x,y,Global.EntityZ);
    }

    public void setParent(Transform parentTransform) {
        transform.parent = parentTransform;
    }
    protected virtual void setEntityToCurrentChunk() {
        /*
        GameObject chunkObject = ChunkHelper.snapChunk(transform.position.x, transform.position.y);
        if (chunkObject == null) {
            Destroy(gameObject);
            return;
        }
        this.setParent(chunkObject.GetComponent<Chunk>().EntityContainer);
        */
    }

}
