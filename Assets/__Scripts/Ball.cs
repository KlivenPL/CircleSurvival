using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BallType { normal, black }

[RequireComponent(typeof(SpriteRenderer))]
public class Ball : MonoBehaviour {
    public static List<Ball> SpawnedBalls { get; set; } = new List<Ball>();

    public static Transform ballsTr;

    public Action OnTap { get; set; } = null;
    public Action OnTimeout { get; set; } = null;

    public BallType BallType { get; set; }

    private float timer, defTimer;
    public float Diameter { get; set; }

    Renderer fillRenderer;
    Color baseFillColor;
    Vector2 spawnPos;

    public void Init(BallType ballType, float diameter) {
        this.BallType = ballType;
        this.Diameter = diameter;
        spawnPos = transform.position;
        transform.SetParent(ballsTr, true);
        switch (BallType) {
            case BallType.normal:
                GetComponent<SpriteRenderer>().color = Menu.Instance.themedCirclesChk.isOn ? Background.Instance.CurrentColor * 1.25f : Color.green;
                GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, 0.3f);
                defTimer = 2f + (diameter - 1f) / .2f * 2f;

                OnTap += () => {
                    AudioManager.Instance.PlaySfx(SoundEffect.pop, 0.4f);
                    Spawner.Instance.SpawnBomb(this);
                    DestroyBall();
                };


                OnTimeout += () => {
                    GameManager.Instance.GameOver(this);
                    AudioManager.Instance.PlaySfx(UnityEngine.Random.Range(0f, 1f) >= 0.5f ? SoundEffect.explosion1 : SoundEffect.explosion2, 1f);
                };
                StartCoroutine(Wiggle());
                break;
            case BallType.black:
                AudioManager.Instance.PlaySfx(SoundEffect.blackHole, 0.6f);
                GetComponent<SpriteRenderer>().color = Color.black;
                GetComponent<SpriteRenderer>().sortingOrder = 2;

                defTimer = 3.0f * Time.timeScale;

                OnTap += () => {
                    GameManager.Instance.GameOver(this);
                    AudioManager.Instance.PlaySfx(SoundEffect.bassDrop, 0.7f);
                };
                OnTimeout += () => StartCoroutine(ScaleDown(true));

                transform.GetChild(0).gameObject.SetActive(false);
                transform.GetChild(1).gameObject.SetActive(true);
                var main = transform.GetChild(1).GetChild(0).GetComponent<ParticleSystem>().main;
                main.simulationSpeed /= Time.timeScale;
                StartCoroutine(Pulse());
                break;
        }
        fillRenderer = transform.GetChild(0).GetComponent<Renderer>();
        fillRenderer.material.SetFloat("_Cutoff", 1f);
        baseFillColor = fillRenderer.material.GetColor("_Emission");
        gameObject.transform.localScale = Vector3.one * 0.01f;
        timer = defTimer;
        StartCoroutine(ScaleUp());
    }

    private void FixedUpdate() {
        if (GameManager.Instance.gameOver)
            return;
        if (timer > 0)
            timer -= Time.deltaTime;
        else if (timer != -1f) {
            timer = -1f;
            OnTimeout();
        }

        if (BallType == BallType.normal) {
            fillRenderer.material.SetFloat("_Cutoff", 1f - ((defTimer - timer) / defTimer) / 2f);
            float colSin = Mathf.Sin((defTimer - timer) * (12f * ((defTimer - timer) / defTimer))) / 15f;
            fillRenderer.material.SetColor("_Emission", baseFillColor + new Color(colSin, colSin, colSin));
        }
    }

    IEnumerator ScaleUp() {
        while (true) {
            if (transform.localScale.x < Diameter) {
                transform.localScale += Vector3.one * Time.deltaTime / Time.timeScale * 10f;
                yield return null;
            } else {
                transform.localScale = Vector3.one * Diameter;
                enabled = true;
                break;
            }
        }
    }

    IEnumerator Wiggle() {
        float wiggleX = UnityEngine.Random.Range(0f, 1f) > 0.5f ? -0.025f : 0.025f;
        float wiggleSpeed = 0.75f;
        while (true) {
            transform.position = Vector3.MoveTowards(transform.position, (Vector3)spawnPos - transform.root.position + new Vector3(wiggleX, 0),
                ((defTimer - timer) / defTimer) * Time.deltaTime / Time.timeScale * wiggleSpeed);
            if (Vector2.Distance(spawnPos - (Vector2)transform.root.position + new Vector2(wiggleX, 0), transform.position) <= 0.001f)
                wiggleX *= -1;

            yield return null;
        }
    }

    IEnumerator Pulse() {
        bool grow = true;
        while (true) {
            if (Diameter == transform.localScale.x) {
                grow = true;
            }
            if (grow)
                transform.localScale += Vector3.one * Time.deltaTime / Time.timeScale * 2f;
            else {
                transform.localScale -= Vector3.one * Time.deltaTime / Time.timeScale * 0.15f;
                if (Mathf.Abs(transform.localScale.x - Diameter) < 0.05f)
                    transform.localScale = Vector3.one * Diameter;
            }

            if (transform.localScale.x >= Diameter * 1.15f)
                grow = false;
            yield return null;
        }
    }

    public IEnumerator ScaleDown(bool destroy) {
        while (true) {
            if (transform.localScale.x > 0.01f) {
                transform.localScale -= Vector3.one * Time.deltaTime / Time.timeScale * 10f;
                yield return null;
            } else {
                transform.localScale = Vector3.zero;
                if (destroy)
                    DestroyBall();
                break;
            }
        }
    }

    public IEnumerator OnBlackHomeGameOver(Ball blackHole) {
        float dist = Vector2.Distance(blackHole.transform.position, transform.position);
        //Vector2 dir = (blackHole.transform.position - transform.position).normalized;
        float speed = 0f;
        while (true) {
            speed += Time.deltaTime / Time.timeScale * 9.81f;
            if (blackHole == this) {
                if (transform.localScale.x >= Diameter * 1.75f == false)
                    transform.localScale += Vector3.one * Time.deltaTime / Time.timeScale * speed;
                else
                    break;
                yield return null;
            } else {
                transform.position = Vector3.MoveTowards(transform.position, blackHole.transform.position, Time.deltaTime / Time.timeScale * speed);
                float currDist = Vector2.Distance(blackHole.transform.position, transform.position);
                if (currDist <= 0.15f) {
                    DestroyBall();
                    break;
                }
                transform.localScale = Mathf.Min((currDist / 1.75f), Diameter) * Vector3.one;
                yield return null;
            }
        }
    }

    public IEnumerator OnTimeoutGameOver(Ball ball) {
        if (ball == this) {
            Spawner.Instance.SpawnExplosion(this);
            gameObject.SetActive(false);
        } else {
            Vector2 explosionDirection = (transform.position - ball.transform.position).normalized;
            float explosionForce = 5f - Mathf.Min(Vector2.Distance(ball.transform.position, transform.position) / 2f, 3f) / 2f;
            while (explosionForce > 0) {
                transform.Translate(explosionDirection * (explosionForce -= Time.deltaTime / Time.timeScale * 2) * Time.deltaTime / Time.timeScale, Space.World);
                yield return null;
            }
        }
        
    }

    public void DestroyBall() {
        SpawnedBalls.Remove(this);
        Destroy(gameObject);
    }

}
