using System.Collections;
using UnityEngine;

public class EthlasBlock : MonoBehaviour
{

    [SerializeField]
    private ParticleSystem fireworks;
    
    public GameObject parentBlock;

    private Vector3 position, localScale;
    private Vector3 parentPosition, parentLocalScale;
    private Vector3 startPos, endPos;
    private IEnumerator movementCoroutine;

    private float bufferSize, speed, newBlockWaitTime;
    private Color colour;

    private void Start()
    {
        GameManager.Instance.InputReceived.AddListener(FreezeAndSlice);
        this.movementCoroutine = MoveBlock();

        this.position = this.transform.position;
        this.localScale = this.transform.localScale;

        this.parentPosition = this.parentBlock.transform.position;
        this.parentLocalScale = this.parentBlock.transform.localScale;

        int direction = Random.Range(0, 2);
        if (direction == 0) { direction = -1; }

        // Offset in the x-axis.
        if (Random.Range(0, 2) == 0)
        {
            this.startPos = new Vector3(this.position.x + GameManager.Instance.BlockStartOffset * direction, this.position.y, this.position.z);
            this.endPos = new Vector3(this.position.x - GameManager.Instance.BlockStartOffset * direction, this.position.y, this.position.z);
        }
        // Offset in the z-axis.
        else
        {
            this.startPos = new Vector3(this.position.x, this.position.y, this.position.z + GameManager.Instance.BlockStartOffset * direction);
            this.endPos = new Vector3(this.position.x, this.position.y, this.position.z - GameManager.Instance.BlockStartOffset * direction);
        }

        this.bufferSize = GameManager.Instance.BlockBufferSize;
        this.speed = GameManager.Instance.BlockSpeed;
        this.newBlockWaitTime = GameManager.Instance.NewBlockWaitTime;
        this.transform.position = startPos;
        this.colour = this.GetRandomColour();
        this.GetComponent<Renderer>().material.color = this.colour;

        this.StartCoroutine(movementCoroutine);
    }

    private void FreezeAndSlice()
    {
        GameManager.Instance.InputReceived.RemoveListener(FreezeAndSlice);
        this.StopCoroutine(movementCoroutine);
        this.StartCoroutine(SliceBlock());
    }

    private Color GetRandomColour()
    {
        Color colour = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
        return colour;
    }

    private IEnumerator SliceBlock()
    {
        // Booleans to check if the block is perfectly aligned with the parent block.
        bool sameMinX = false, sameMaxX = false,
            sameMinZ = false, sameMaxZ = false;

        this.position = this.gameObject.transform.position;

        // Determine the positions of the edges of the block.
        float minX = this.position.x - this.localScale.x * 0.5f;
        float maxX = this.position.x + this.localScale.x * 0.5f;
        float minZ = this.position.z - this.localScale.z * 0.5f;
        float maxZ = this.position.z + this.localScale.z * 0.5f;

        // Determine the positions of the edges of the parent block.
        float parentMinX = this.parentPosition.x - this.parentLocalScale.x * 0.5f;
        float parentMaxX = this.parentPosition.x + this.parentLocalScale.x * 0.5f;
        float parentMinZ = this.parentPosition.z - this.parentLocalScale.z * 0.5f;
        float parentMaxZ = this.parentPosition.z + this.parentLocalScale.z * 0.5f;

        // First, check if the x and z positions fall within the buffer range.
        // If they fall within an acceptable buffer range, we'll assume it is the same as the parent's.
        if (minX <= parentMinX + bufferSize && minX >= parentMinX - bufferSize)
        {
            sameMinX = true;
            minX = parentMinX;
        }
        if (maxX <= parentMaxX - bufferSize && maxX >= parentMaxX + bufferSize)
        {
            sameMaxX = true;
            maxX = parentMaxX;
        }
        if (minZ <= parentMinZ + bufferSize && minZ >= parentMinZ - bufferSize)
        {
            sameMinZ = true;
            minZ = parentMinZ;
        }
        if (maxZ <= parentMaxZ - bufferSize && maxZ >= parentMaxZ + bufferSize)
        {
            sameMaxZ = true;
            maxZ = parentMaxZ;
        }

        // Next, check if the block has zero overlaps with the parent block on either the x or z axis.
        if ((maxX < parentMinX || minX > parentMaxX) || (maxZ < parentMinZ || minZ > parentMaxZ))
        {
            // end the game
            GameManager.Instance.GameOver();
            yield break;
        }

        // Next, check for where the block exceeds the parent block's x/z positions and cut them off.
        if (minX < parentMinX) { minX = parentMinX; }
        if (maxX > parentMaxX) { maxX = parentMaxX; }
        if (minZ < parentMinZ) { minZ = parentMinZ; }
        if (maxZ > parentMaxZ) { maxZ = parentMaxZ; }

        Vector3 newBlockPos;

        // If the block is perfectly aligned with the parent block, trigger an event.
        if (sameMinX && sameMaxX && sameMinZ && sameMaxZ)
        {
            GameManager.Instance.TriggerPerfectAlignment();
            // Reposition the current block.
            this.transform.position = new Vector3(this.parentPosition.x, this.transform.position.y, this.parentPosition.z);
            newBlockPos = new Vector3(this.parentPosition.x, this.transform.position.y + this.transform.localScale.y, this.parentPosition.z);
        }
        // Otherwise, recreate the trimmed block.
        else
        {
            // Calculate the new local scales based on ratio differences.
            float xDiff = (maxX - minX) / (parentMaxX - parentMinX);
            float newXScale = parentBlock.transform.localScale.x * xDiff;

            float zDiff = (maxZ - minZ) / (parentMaxZ - parentMinZ);
            float newZScale = parentBlock.transform.localScale.z * zDiff;
            
            // Calculate the new x and z positions.
            float newXPos = minX + (maxX - minX) * 0.5f;
            float newZPos = minZ + (maxZ - minZ) * 0.5f;

            // Reposition and resize the current block.
            this.transform.position = new Vector3(newXPos, this.transform.position.y, newZPos);
            this.transform.localScale = new Vector3(newXScale, this.localScale.y, newZScale);
            yield return new WaitForEndOfFrame();

            newBlockPos = new Vector3(newXPos, this.transform.position.y + this.transform.localScale.y, newZPos);
        }

        // Create a new block on a higher y axis.
        EthlasBlock newBlock = Instantiate(this.gameObject, newBlockPos, Quaternion.identity, parentBlock.transform.parent).GetComponent<EthlasBlock>();
        newBlock.parentBlock = this.gameObject;
        GameManager.Instance.UpdateScene();

        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] {
            new GradientColorKey(this.colour, 1.0f),
            new GradientColorKey(this.colour, 0.0f) },
            new GradientAlphaKey[]
            { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });

        var col = fireworks.colorOverLifetime;
        col.color = grad;
        fireworks.Play();
    }

    private IEnumerator MoveBlock()
    {
        yield return new WaitForSeconds(this.newBlockWaitTime);

        float startTime = Time.time;
        float maxTime = GameManager.Instance.BlockStartOffset / speed;
        float elapsedTime = Time.time - startTime;

        while (elapsedTime < maxTime)
        {
            this.transform.position = Vector3.Lerp(startPos, endPos, 1 / maxTime * (Time.time - startTime));
            yield return null;
            elapsedTime = Time.time - startTime;
        }

        this.transform.position = endPos;
    }
}
