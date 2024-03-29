﻿using System.Collections.Generic;
using UnityEngine;

public class DrawCar : MonoBehaviour
{
    [Header("Car")]
    [Range(0f, 100f)]
    public float carSpeed = 20f;

    [Header("Prefabs")]
    public GameObject linePrefab;
    public GameObject wheelPrefab;
    public GameObject obstacle;

    [Header("Line")]
    public GameObject currentLine;
    public LineRenderer lineRenderer;
    public EdgeCollider2D edgeCollider;
    public List<Vector2> fingerPositions;

    private Rigidbody2D rb2D;
    private List<GameObject> wheels = new List<GameObject>();
    private GameObject wheel;
    private Vector3 wheelPos;
    private Camera cam;

    private bool moveControl = false;

    private void Start()
    {
        rb2D = gameObject.GetComponent<Rigidbody2D>();
        cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    private void Update()
    {
        UserInput();
    }

    private void FixedUpdate()
    {
        MoveCar();
    }

    private void UserInput()
    {
        if (UIControl.UIManager.drawControl && transform.childCount == 0) //draw car
        {
            if (Input.GetMouseButtonDown(0))
            {
                CreateLine();
            }

            if (Input.GetMouseButton(0))
            {
                Vector2 tempFingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                if (Vector2.Distance(tempFingerPos, fingerPositions[fingerPositions.Count - 1]) > .1f)
                {
                    UpdateLine(tempFingerPos);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                //if (transform.childCount > 0) //if there is already a car
                //{
                //    foreach (Transform child in transform)
                //    {
                //        wheels.Clear();
                //        Destroy(child.gameObject);
                //    }
                //}
                CreateCar();

                UIControl.UIManager.RaceStarted();
            }
        }
        else if(UIControl.UIManager.drawControl) //draw path
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 pos = cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 5f));
                Instantiate(obstacle, pos, Quaternion.identity);
                UIControl.UIManager.stopWatch.StopWatchState("increase");
            }
        }
    }

    private void CreateLine()
    {
        currentLine = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
        lineRenderer = currentLine.GetComponent<LineRenderer>();
        edgeCollider = currentLine.GetComponent<EdgeCollider2D>();
        fingerPositions.Clear();
        fingerPositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        fingerPositions.Add(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        lineRenderer.SetPosition(0, fingerPositions[0]);
        lineRenderer.SetPosition(1, fingerPositions[1]);
        edgeCollider.points = fingerPositions.ToArray();
    }

    private void UpdateLine(Vector2 newFingerPos)
    {
        fingerPositions.Add(newFingerPos);
        lineRenderer.positionCount++;
        lineRenderer.SetPosition(lineRenderer.positionCount - 1, newFingerPos);
        edgeCollider.points = fingerPositions.ToArray();
    }

    private void CreateCar()
    {
        CreateWheel();
        (currentLine as GameObject).transform.parent = gameObject.transform;
        moveControl = true;
    }

    private void CreateWheel()
    {
        for (int i = 0; i < 2; i++)
        {
            if (i == 0){
                wheelPos = new Vector3(fingerPositions[0].x, fingerPositions[0].y);
                //wheelPos = new Vector3(lineRenderer.GetPosition(0).x, lineRenderer.GetPosition(0).y);
            }
            else if (i == 1)
            {
                wheelPos = new Vector3(fingerPositions[fingerPositions.Count - 1].x, fingerPositions[fingerPositions.Count - 1].y);
            }
            wheel = Instantiate(wheelPrefab, wheelPos, Quaternion.identity);
            (wheel as GameObject).transform.parent = gameObject.transform;
            wheels.Add(wheel as GameObject);
        }
    }

    private void MoveCar()
    {
        if (moveControl)
        {
            rb2D.bodyType = RigidbodyType2D.Dynamic;
            rb2D.AddForce(transform.right * carSpeed * Time.fixedDeltaTime * 100f, ForceMode2D.Force);
            RotateWheel();
        }
    }

    private void RotateWheel()
    {
        for (int i = 0; i < wheels.Count; i++)
        {
            wheels[i].transform.Rotate(new Vector3(0, 0, Random.Range(0f, 360f)) * Time.fixedDeltaTime * 3f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.transform.tag == "flagTag")
        {
            moveControl = false;
            carSpeed = 0f;
        }
    }
}
