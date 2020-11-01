using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class Player : NetworkBehaviour
{

    void HandleMovement() {
        if (isLocalPlayer) {
            float moveH = Input.GetAxis("Horizontal") * 0.2f;
            float moveV = Input.GetAxis("Vertical") * 0.2f;
            Vector3 move = new Vector3(moveH, moveV, 0);
            transform.position += move;
        }
    }

    private void Update() {
        HandleMovement();
    }
}
