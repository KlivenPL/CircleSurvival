using UnityEngine;

public class Autodestruction : MonoBehaviour {

    public float timer;
    void Update() {
        timer -= Time.deltaTime;
        if (timer <= 0 && timer != -1) {
            timer = -1;
            Destroy(gameObject);
        }
    }
}
