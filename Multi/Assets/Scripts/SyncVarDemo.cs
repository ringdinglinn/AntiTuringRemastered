using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncVarDemo : NetworkBehaviour {
    [SyncVar(hook = nameof(SetColor))]
    private Color32 _color = Color.red;

    public override void OnStartServer() {
        base.OnStartServer();
        StartCoroutine(_RandomizeColor());
    }

    private void SetColor(Color32 oldColor, Color32 newColor) {
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.color = newColor;
    }

    IEnumerator _RandomizeColor() {
        WaitForSeconds wait = new WaitForSeconds(2f);

        while (true) {
            _color = Random.ColorHSV();
        }
    }
}
