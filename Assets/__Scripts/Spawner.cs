using System.Collections;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public static Spawner Instance { get; set; } = null;

    private Bounds spawnArea;
    public Ball ballPrefab;
    public GameObject bombPrefab, explosionPrefab;

    float spawnTimer, defaultSpawnTimer = 2f;

    public void Init() {
        Instance = this;
        spawnArea = CalcSpawnArea();
        enabled = true;
        Ball.ballsTr = GameObject.Find("Balls").transform;
    }

    Bounds CalcSpawnArea() {
        return Utils.GetCameraBounds(Camera.main);
    }

    public Ball SpawnBall(BallType ballType) {
        Vector2 pos;
        float diameter;
        while (true) {
            pos = new Vector2(Random.Range(spawnArea.min.x, spawnArea.max.x), Random.Range(spawnArea.min.y, spawnArea.max.y));
            diameter = Random.Range(1f, 1.2f);

            // check if not colliding with border
            if ((Mathf.Abs(pos.x - spawnArea.min.x) > diameter && Mathf.Abs(pos.y - spawnArea.min.y) > 1f
                && Mathf.Abs(pos.x - spawnArea.max.x) > diameter && Mathf.Abs(pos.y - spawnArea.max.y) > 1f) == false)
                continue;

            // check if not colliding with other balls
            bool posOk = true;
            for (int i = 0; i < Ball.SpawnedBalls.Count; i++) {
                if (Vector2.Distance(pos, Ball.SpawnedBalls[i].transform.position) <= diameter / 2f + Ball.SpawnedBalls[i].Diameter / 2f + 0.25f) {
                    posOk = false;
                    break;
                }
            }

            if (posOk)
                break;
        }
        var ball = Instantiate(ballPrefab.gameObject, pos, Quaternion.identity).GetComponent<Ball>();
        ball.Init(ballType, diameter);
        Ball.SpawnedBalls.Add(ball);
        return ball;
    }

    public void SpawnBomb(Ball atBall) {
        var bombParticle = Instantiate(bombPrefab, atBall.transform.position, Quaternion.identity).transform.GetChild(0).GetComponent<ParticleSystem>();
        var main = bombParticle.main;
        main.startColor = Background.Instance.CurrentColor * 1.5f;
        bombParticle.Play();
    }

    public void SpawnExplosion(Ball atBall) {
        Instantiate(explosionPrefab, atBall.transform.position + new Vector3(0, 0, -3f), Quaternion.identity).transform.GetChild(0).GetComponent<ParticleSystem>();
    }

    private void FixedUpdate() {
        if ((spawnTimer -= Time.deltaTime) <= 0f) {
            int count = 1;
            float bonusTime = 0f;

            if (GameManager.Score >= 10f) {
                if (Random.Range(0f, 1f) <= 0.4f) {
                    count = 2;
                    bonusTime = 0.5f;
                }
            }

            if (GameManager.Score >= 30f) {
                if (Random.Range(0f, 1f) <= 0.25f) {
                    count = 3;
                    bonusTime = Random.Range(1f, 2f);
                }
            }

            if (GameManager.Score >= 60f) {
                if (Random.Range(0f, 1f) <= 0.2f) {
                    count = 4;
                    bonusTime = 2f;
                }
            }
            StartCoroutine(SpawnBalls(count));
            spawnTimer = defaultSpawnTimer + bonusTime;
        }

        // moving all balls by accelometer
        float range = 0.18f;
        var accPos = new Vector3((Input.acceleration.x - Utils.DefaultAccelometerPos.x) * range, (Input.acceleration.y - Utils.DefaultAccelometerPos.y) * range);
        Ball.ballsTr.transform.position = Vector3.Lerp(Ball.ballsTr.transform.position, accPos, Time.deltaTime * 3f / Time.timeScale);
    }

    IEnumerator SpawnBalls(int count) {
        for (int i = 0; i < count; i++) {
            if (Ball.SpawnedBalls.Count < 6) {
                SpawnBall(Random.Range(0.0f, 1.0f) <= 0.1f ? BallType.black : BallType.normal);
                AudioManager.Instance.PlaySpawnSfx();
                yield return new WaitForSeconds(0.25f);
            } else
                break;
        }
    }

}
