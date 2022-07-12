using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{
    // [SyncVar(hook = nameof(SetPlayerColor))]
    // public Color playerColor = Color.white;

    public float playerSpeed = 10.0f;

    // public GameObject cameraPrefab = null;

    // [SerializeField]
    // private GameObject cameraNode = null;

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // playerColor = Random.ColorHSV(0f, 1f, 0.9f, 0.9f, 1f, 1f);
    }

    public override void OnStartLocalPlayer()
    {
        // if(Camera.main == null)
        // {
        //     var camObject = Instantiate(cameraPrefab);
        //     camObject.SetActive(true);

        //     var cam = camObject.GetComponent<Camera>();
        //     cam.enabled = true;
        // }
    }



    // Update is called once per frame
    void Update()
    {
        UpdateMovement();
    }

    void UpdateMovement()
    {
        if (isLocalPlayer)
        {
            // Camera.main.gameObject.transform.LookAt(this.transform);
            Move();
        }
    }

    void Move()
    {
        float verticalAxis = Input.GetAxis("Vertical");
        float horizontalAxis = Input.GetAxis("Horizontal");

        if (verticalAxis != 0 || horizontalAxis != 0)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            Vector3 movement = (forward * verticalAxis + right * horizontalAxis) * playerSpeed * Time.deltaTime;
            transform.Translate(movement);
        }

    }

    // void SetPlayerColor(Color oldColor, Color newColor)
    // {
    //     var renderer = gameObject.GetComponent<Renderer>();
    //     renderer.material.SetColor("_Color", newColor);
    // }
}
