using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody))]
public class playerPhysicsMovement : MonoBehaviour
{
    [Header("Basic Initialization Stuff")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float groundCheckDistance = 0.15f;
    [SerializeField] private float groundAngleThreshHold = 50f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float tpMaxRange = 20f;
    [SerializeField] private float maxCharges = 3f;
    [SerializeField] private float chargeCooldown = 3f;
    [SerializeField] private float tpBuffer = 0.5f;
    [SerializeField] private int endScreen = 2;
    

   
    [SerializeField] private float fallGravityMultiplier = 5f;   
    [SerializeField] private float lowJumpMultiplier = 3f;      
    [SerializeField] private float riseGravityMultiplier = 2.2f; 

    private Rigidbody rb;
    private CapsuleCollider col;
    private Vector2 moveInput;
    private Camera playerCamera;
    private orbBehavior orb;
    private GameObject projection;
    
    
    private bool isGrounded;
    private bool jumpQueued;
    private bool jumpHeld;
    private float tpRange = 10f;
    private float actualCooldown = 3f;
    private float actualCharges = 3f;
    private float actualTpBuffer = 0f;
    private float pickups = 0f;
    private float tpFallMult = 1f;

    void Awake(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        col = GetComponent<CapsuleCollider>();
        playerCamera = Camera.main;
        orb = GetComponent<orbBehavior>();
        projection = CreateProjection();
    }

    void Update(){
        CheckGround();
        AdjustRange();
        if (jumpQueued && isGrounded){
            jumpQueued = false;
            Jump();
        }
        if (actualCooldown > 0.0f && actualCharges < maxCharges)
        {
            actualCooldown -= Time.deltaTime;
        }
        if (actualTpBuffer > 0.0f)
        {
            actualTpBuffer -= Time.deltaTime;
        }
        if (actualCooldown <= 0.0f && actualCharges < maxCharges)
        {
            actualCooldown = chargeCooldown;
            actualCharges++;
            Debug.Log("Added Charge");

        }
        if (actualTpBuffer <= 0.0f)
        {
            fallGravityMultiplier = 5f;
            moveSpeed = 8f;
        }
        if (Mouse.current.leftButton.wasPressedThisFrame && actualTpBuffer <= 0.0f)
        {
            Teleport();
            fallGravityMultiplier = tpFallMult;
            moveSpeed = 2f;
        }
        if (Mouse.current.rightButton.isPressed)
        {
            playerCamera.fieldOfView = 30f;
        }
        else
        {
            playerCamera.fieldOfView = 60f;
        }
        if (orb.isVisible)
        {
            projection.SetActive(true);
            UpdateProjection(projection);
        }
        else
        {
            projection.SetActive(false);
        }
    }

    void FixedUpdate(){
        Move();
        BetterJumpFeel();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pickup"))
        {
            pickups++;
            other.gameObject.SetActive(false);
            switch (pickups)
            {
                case 1:
                    tpMaxRange += 20;
                    break;
                case 2:
                    maxCharges += 2;
                    break;
                case 3:
                    tpFallMult = 0.1f;
                    break;
            }
        }
        if (other.gameObject.CompareTag("PickupOrb"))
        {
            pickups++;
            other.gameObject.SetActive(false);
            orb.hasOrb = true;
            orb.activate();
            
        }
         if (other.gameObject.CompareTag("youwinn"))
        {
            
            SceneManager.LoadScene(endScreen);
            
        }

       
        
    }

    public void OnMove(InputAction.CallbackContext context){
        moveInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context){
        if (context.started){
            jumpQueued = true;
            jumpHeld = true;
        }
        if (context.canceled){
            jumpHeld = false;
        }
    }

    private void Move(){
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 desired = move * moveSpeed;

        rb.linearVelocity = new Vector3(desired.x, rb.linearVelocity.y, desired.z);
    }

    private void Jump(){
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void Teleport() {
        if (!orb.isVisible || actualCharges < 1)
        {
            Debug.Log("Not enough charges");
            return; 
        }
        RaycastHit hit;
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.Normalize();
        Vector3 tpDest = rb.transform.position + cameraForward * tpRange;
        if (Physics.Linecast(transform.position, tpDest, out hit, groundLayer))
        {
            rb.MovePosition(hit.point + hit.normal * 1f);
        }
        else
        {
            rb.MovePosition(tpDest);
        }
        actualCharges--;
        actualTpBuffer = tpBuffer;
    }

    private void AdjustRange()
    {
        if (!orb.isVisible)
        {
            return;
        }
        Vector2 scrollDelta = Mouse.current.scroll.ReadValue();
        tpRange += scrollDelta.y;
        if (tpRange > tpMaxRange)
        {
            tpRange = tpMaxRange;
        }
        else if (tpRange < 10f)
        {
            tpRange = 10f;
        }
    }

    private GameObject CreateProjection()
    {
        RaycastHit hit;
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.Normalize();
        Vector3 tpDest = rb.transform.position + cameraForward * tpRange;
        GameObject projection = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Renderer rend = projection.GetComponent<Renderer>();
        projection.transform.localScale = new Vector3(1.2f, 2.5f, 1.2f);
        BoxCollider coll = projection.GetComponent<BoxCollider>();
        if (coll != null)
        {
            Destroy(coll);
        }
        if (rend != null)
        {
            rend.material.color = Color.cyan;
            Color color = rend.material.color;
            color.a = 0.5f;
            rend.material.color = color;
        }
        if (Physics.Linecast(transform.position, tpDest, out hit, groundLayer))
        {
            projection.transform.position = (hit.point + hit.normal * 1f);
        }
        else
        {
            projection.transform.position = tpDest;
        }
        return projection;
    }

    private void UpdateProjection(GameObject projection)
    {
        RaycastHit hit;
        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.Normalize();
        Vector3 tpDest = rb.transform.position + cameraForward * tpRange;
        if (Physics.Linecast(transform.position, tpDest, out hit, groundLayer))
        {
            projection.transform.position = (hit.point + hit.normal * 1f);
        }
        else
        {
            projection.transform.position = tpDest;
        }
    }

    private void CheckGround(){
    RaycastHit hit;
    Vector3 origin = col.bounds.center;
    float radius = col.radius * 0.95f;
    float castDist = (col.bounds.extents.y - radius) + groundCheckDistance;
    if(Physics.SphereCast(origin, radius, Vector3.down, out hit, castDist, groundLayer)){
        if(Vector3.Angle(hit.normal, Vector3.up) <= groundAngleThreshHold){
            isGrounded = true;
        }else{
            isGrounded = false;
        }
    }else{
        isGrounded = false;
    }
    
    }

    private void BetterJumpFeel(){
        float yVel = rb.linearVelocity.y;
        if (yVel < 0f){
            rb.AddForce(Physics.gravity * (fallGravityMultiplier - 1f), ForceMode.Acceleration);
        }
        else if (yVel > 0f){
            float mult = jumpHeld ? riseGravityMultiplier : lowJumpMultiplier;
            rb.AddForce(Physics.gravity * (mult - 1f), ForceMode.Acceleration);
        }
    }
}
