using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerObjectType Type = PlayerObjectType.ServerObject;
    public Vector3 UserInput;
    public event Action<Vector3> OnFixedUpdate;
    public Vector3 Position, OldPosition;
    private float lastFixedUpdate;

    private void Start()
    {
        lastFixedUpdate = Time.time;
    }

    private void FixedUpdate()
    {
        lastFixedUpdate = Time.time;

        if (Type == PlayerObjectType.ThisPlayer)
        {
            UserInput = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0);
            OnFixedUpdate(UserInput);

            OldPosition = Position;
            Position += UserInput * Consts.ClientSpeed;
            transform.position = Position;
        }
        else if (Type == PlayerObjectType.ServerObject)
        {
            OldPosition = Position;
            Position += UserInput * Consts.ClientSpeed;
            transform.position = Position;
        }
        else
        {

        }
    }

    private void Update()
    {
        if (Type == PlayerObjectType.ThisPlayer) // local interpolation
        {
            var timeFactor = (Time.time - lastFixedUpdate) / Time.fixedDeltaTime;
            transform.position = Vector3.LerpUnclamped(OldPosition, Position, 1 + timeFactor);
        }
        else if (Type == PlayerObjectType.ServerObject)
        {

        }
        else
        {

        }

        GetComponentInChildren<TextMesh>().text = transform.position.ToString() + "\n" + UserInput.ToString();
    }

}
