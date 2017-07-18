using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


//PROBABLY NOT NEEDED FOR THE BASICS OF GETTING THIS TO WORK
public class CameraControls : MonoBehaviour {
    public BoxCollider2D CameraBounds;
    public FocalPoint focus;
    public Rigidbody2D selfBody { get; private set; }

    private Camera cameraComponent;
    private float original_camera_size;
    //private float min_camera_size;
    //private float max_camera_size;
    private float target_camera_size;

    public float zoomSpeed = 20f;
    public float maxZoomFOV = 10f;

    /* camera moving constants */
    private const float Z_OFFSET = -10;

    /*CONSTANTS*/
    private const float TARGETING_LOWER_BOUND = 0.0f;
    private const float TARGETING_UPPER_BOUND = 1.0f;
    private const float ZOOM_IN_LOWER_BOUND = 0.3f;
    private const float ZOOM_IN_UPPER_BOUND = 0.7f;
    private const float ZOOM_OUT_LOWER_BOUND = 0.2f;
    private const float ZOOM_OUT_UPPER_BOUND = 0.8f;

    private const float ZOOM_RATE = 0.02f;

    private const float PAN_SPEED = 5.0f;

	// Use this for initialization
	void Start () {
        cameraComponent = GetComponent<Camera>();
        selfBody = GetComponent<Rigidbody2D>();

        focus.addTargets(GameManager.instance.p1.gameObject);
        focus.addTargets(GameManager.instance.p2.gameObject);
        //foreach (AIPlayer player in GameManager.AIPlayers)
        //{
        //    focus.addTargets(player);
        //}

        original_camera_size = cameraComponent.orthographicSize;
        //min_camera_size = 0.75f * original_camera_size;
        //max_camera_size = 2.0f * original_camera_size;
        target_camera_size = original_camera_size;

        transform.position = focus.transform.position + new Vector3(0, 0, Z_OFFSET);
	}
	
	void FixedUpdate () {
        //Now follow the target
        if (transform.position != focus.transform.position + new Vector3(0, 0, Z_OFFSET))
        {
            float x = ((focus.transform.position + new Vector3(0, 0, Z_OFFSET)) - transform.position).x;
            float y = ((focus.transform.position + new Vector3(0, 0, Z_OFFSET)) - transform.position).y;
            selfBody.velocity = new Vector2(x * PAN_SPEED, y * PAN_SPEED);
        }
        else
        {
            selfBody.velocity.Set(0.0f, 0.0f);
        }
    
        //Keep the camera in bounds
        Vector3 pos;
        pos.x = Mathf.Clamp(transform.position.x, CameraBounds.bounds.min.x + cameraComponent.orthographicSize * Screen.width / Screen.height,
                                                  CameraBounds.bounds.max.x - cameraComponent.orthographicSize * Screen.width / Screen.height);
        pos.y = Mathf.Clamp(transform.position.y, CameraBounds.bounds.min.y + cameraComponent.orthographicSize,
                                                  CameraBounds.bounds.max.y - cameraComponent.orthographicSize);
        pos.z = transform.position.z;
        transform.position = pos;
    
        //external gradual resizing
        float cameraSize = cameraComponent.orthographicSize;
        //Current issue is that if the character moves too quickly, the opponent then leaves the FOV too quickly, resulting in an awkward camera
        if (cameraSize < target_camera_size)
            cameraComponent.orthographicSize = Mathf.MoveTowards(cameraSize, cameraSize * 1 + ZOOM_RATE, zoomSpeed * Time.deltaTime);
        if (cameraSize > target_camera_size)
            cameraComponent.orthographicSize = Mathf.MoveTowards(cameraSize, cameraSize * 1 - ZOOM_RATE, zoomSpeed * Time.deltaTime);
    }

}
