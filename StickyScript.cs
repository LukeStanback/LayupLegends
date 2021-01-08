using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script controls the sticky hand after the player has activated the ability


public class StickyScript : MonoBehaviour
{

    public GameObject rod;
    public GameObject hand;
    public GameObject Hitbox;
    public GameObject boxcol;
    public GameObject hitboxprefab;
    private GameObject heldPlayer;
    private PlayerMovement parent;

    public Sprite frame1;
    public Sprite frame2;
    public Sprite frame3;

    private Vector2 target;
    private Vector2 offset = new Vector2();
    private Vector3 hitOffset = new Vector3(6, -3, 0);

    public int teamID;
    public int playerID;
    public int direction;
    public float length;
    public bool hitboxActive;
    private bool hit = false;

    //This method will activate the stickyhands hitbox, checking for collisions against players and solids
    public void enableHitbox(int team, int player, PlayerMovement p) {
        hitboxActive = true;
        teamID = team;
        playerID = player;
        parent = p;
    }

    //This method disables the hitbox
    public void endHitbox() {
        hitboxActive = false;
    }

    //Grab References to the objects that this script controls
    private void OnEnable()
    {
        rod = transform.Find("Sprites").Find("rodimg").gameObject;
        hand = transform.Find("Sprites").Find("Hand").gameObject;
        GameObject h = Instantiate(hitboxprefab);
        Hitbox = h.transform.Find("Hitbox").gameObject;
        boxcol = h.transform.Find("BoxCol").gameObject;
    }

    //This method is called by the owner's playermovement script every frame.
    public void updateHand(float scale, int dir) {
        //Update the direction of the stickyhand to match the player owner, if flipped(facing left), flip the hand sprite
        direction = dir;
        if (dir == -1) {
            hand.GetComponent<SpriteRenderer>().flipX = true;
        }
        
        //As the stickyhand extends, we need to adjust its position and length.
        rod.transform.localScale = new Vector2(scale, 1);
        rod.transform.localPosition = new Vector2((scale / 2f) * dir, 0);
        hand.transform.localPosition = new Vector2((scale + 11f) * dir, 14f) + offset;

        //As long as we havent hit on object, we need to update the hitbox position
        if(!hit) Hitbox.transform.parent.transform.position = hand.transform.position;
        

    }
    

    // Update is called once per frame
    void FixedUpdate()
    {
        if (hitboxActive && !hit)
        {
            //If our hitbox is active and we havent hit something yet, check collisions
            checkCol();
        }
        if(hit && heldPlayer != null){
            //If we hit a player and its reference still exists, we need to update the player position as theyre being dragged
            Hitbox.transform.parent.transform.position = hand.transform.position;
            grabPlayer(heldPlayer.GetComponent<PlayerMovement>());
            
        }
    }

    //Check collisions against players and solids
    private void checkCol() {
        GameObject[] hurtboxes = GameObject.FindGameObjectsWithTag("Hurtbox");
        for (int i = 0; i < hurtboxes.Length; i++) {
            if (Hitbox.GetComponent<CircleCollider2D>().bounds.Intersects(hurtboxes[i].GetComponent<CapsuleCollider2D>().bounds)) {
                PlayerMovement player = hurtboxes[i].GetComponentInParent<PlayerMovement>();
                //We only want to hit the player if they dont have invinciblity, and they are on a different team
                if (player.TeamID != teamID && player.iFrames <= 0 && !hit) {
                    heldPlayer = player.gameObject;
                    hit = true;
                    if (parent != null) {
                        //Shake the camera and spawn particle FX
                        Camera.main.GetComponent<CameraController>().startShake(5, 8, new Vector2(1, 1), 0);
                        GameObject p = Camera.main.GetComponent<ParticleHandler>().spawnParticle("Stickies", parent.getTimeScale(), hand.transform.position);
                        
                        //Adjust the position of the particle based on the direction of the hand
                        p.transform.position = new Vector2(p.transform.position.x + (-3 * direction), p.transform.position.y);
                        if (direction == -1) {
                            p.transform.eulerAngles = new Vector2(0, 180);
                        }
                        //Callback to the playerowner to let their playermovement script know that they grabbed someone
                        parent.stickyHIT();

                        //Spawn hit FX
                        FX f = new FX();
                        f.airburst = true;
                        f.background = true;
                        f.shockwave = true;
                        f.stickyFX = true;
                        f.strength = 120;
                        f.dir = direction * -1;
                        f.angle = 0;
                        f.lines = true;
                        f.baseOrder = heldPlayer.GetComponent<PlayerMovement>().body.GetComponent<SpriteRenderer>().sortingOrder;
                        Camera.main.GetComponent<ParticleHandler>().spawnHitFX(f, Hitbox.transform.position, Hitbox.transform.position);

                    }
                    //If we hit something, we dont need to keep checking collisions, and we can return out of the method.
                    return;
                }
            }
        }
        GameObject[] colliders = GameObject.FindGameObjectsWithTag("Physical Object");
        for (int i = 0; i < colliders.Length; i++)
        {
            //If the solid were colliding with is named NoSticky, we can ignore that collision
            if (colliders[i].name != "NoSticky") {
                if (boxcol.GetComponent<BoxCollider2D>().bounds.Intersects(colliders[i].GetComponent<Collider2D>().bounds))
                {
                    solidCollision();
                    //Hit something, so we can return out of the method
                    return;
                }
            }
            
        }
        colliders = GameObject.FindGameObjectsWithTag("BottomCollider");
        for (int i = 0; i < colliders.Length; i++)
        {
            if (boxcol.GetComponent<BoxCollider2D>().bounds.Intersects(colliders[i].GetComponent<Collider2D>().bounds))
            {
                solidCollision();
                //Hit something, so we can return out of the method
                return;
            }
        }

        colliders = GameObject.FindGameObjectsWithTag("stickyCollider");
        for (int i = 0; i < colliders.Length; i++)
        {
            if (boxcol.GetComponent<BoxCollider2D>().bounds.Intersects(colliders[i].GetComponent<Collider2D>().bounds))
            {
                solidCollision();
                
            }
        }

    }

    //This method is called when the hitbox collides with a solid
    private void solidCollision() {
        hit = true;
        if (parent != null)
        {
            //If the player owner still exists, let it's playermovement script know that we grabbed a solid
            GameObject p = Camera.main.GetComponent<ParticleHandler>().spawnParticle("Stickies", parent.getTimeScale(), hand.transform.position);
            parent.stickyGRAPPLE();
        }
        Destroy(Hitbox);
        Destroy(boxcol);
    }

    //Method called by the player Owner to end the super ability
    public void finish() {
        Destroy(this.gameObject);
    }

    //If were attached to a player, this method keeps them stunned and updates their position
    private void grabPlayer(PlayerMovement p) {
        hitbox n = new hitbox();
        n.stunAmount = 45;
        n.knockbackPower = 110;
        n.knockbackAngle = 25;
        n.stunLagFrames = 3;
        n.smoke = true;
        n.TUMBLE = true;
        n.untechable = true;
        n.PLAYERID = playerID;
        n.superName = "Sticky Hand";
        n.ownerTimeScale = 1;
        n.ID = 0;
        n.direction = direction;
        p.startStun(n.stunLagFrames, n.stunAmount, n.knockbackAngle, n.knockbackPower, n.superName, n.direction, n.TUMBLE, n.untechable, n.smoke, 0.75f, true, false);
        heldPlayer.transform.position = Hitbox.transform.position + new Vector3(hitOffset.x * direction, hitOffset.y, 0);
    }

    public void setLength(float l) {
        length = l;
    }

    public void updateSprite(int s) {
        switch (s) {
            case 1:
                hand.GetComponent<SpriteRenderer>().sprite = frame1;
                break;
            case 2:
                hand.GetComponent<SpriteRenderer>().sprite = frame2;
                break;
            case 3:
                hand.GetComponent<SpriteRenderer>().sprite = frame3;
                offset = new Vector2(0, -4);
                break;
            default:
                hand.GetComponent<SpriteRenderer>().sprite = frame1;
                break;

        }
    }

}
