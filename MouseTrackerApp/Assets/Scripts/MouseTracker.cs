using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;

public class MouseTracker : MonoBehaviour
{
	// WebSocket connection settings
	[Header("WebSocket Settings")]
	[SerializeField] private string serverUrl = "ws://localhost:8080";
	[SerializeField] private float reconnectDelay = 3.0f;

	// Mouse tracking settings
	[Header("Mouse Tracking Settings")]
	[SerializeField] private float sendInterval = 0.05f; // 20 updates per second
	[SerializeField] private float minDistanceThreshold = 2.0f; // Min pixels moved before sending update

	// Debug visualization (optional)
	[Header("Debug Visualization")]
	[SerializeField] private bool showDebugVisuals = true;
	[SerializeField] private int maxDebugPoints = 100;

	// Private variables
	private WebSocket ws;
	private bool isConnected = false;
	private Vector3 lastSentPosition;
	private Queue<string> messageQueue = new Queue<string>();
	private List<Vector3> debugPoints = new List<Vector3>();

	// Unity lifecycle methods
	void Start()
	{
		Debug.Log("MouseTracker starting...");

		// Attempt to connect to WebSocket server
		ConnectToServer();

		// Start our coroutines
		StartCoroutine(SendMousePositionRoutine());
		StartCoroutine(ProcessQueueRoutine());

		// Initialize with current mouse position
		lastSentPosition = Input.mousePosition;

		Debug.Log("MouseTracker initialized successfully");
	}

	void Update()
	{
		// Only update debug visualization if enabled
		if (showDebugVisuals)
		{
			// Add current mouse position to debug points if it has moved significantly
			Vector3 currentMousePos = Input.mousePosition;
			if (Vector3.Distance(currentMousePos, lastSentPosition) > minDistanceThreshold)
			{
				debugPoints.Add(currentMousePos);

				// Limit number of debug points to avoid performance issues
				if (debugPoints.Count > maxDebugPoints)
				{
					debugPoints.RemoveAt(0);
				}
			}
		}
	}

	void OnGUI()
	{
		// Draw connection status
		GUI.color = isConnected ? Color.green : Color.red;
		GUI.Label(new Rect(10, 10, 200, 20), isConnected ? "Connected to server" : "Disconnected");

		// Draw message queue status
		GUI.Label(new Rect(10, 30, 200, 20), "Messages in queue: " + messageQueue.Count);

		// Draw debug visualization if enabled
		if (showDebugVisuals && debugPoints.Count > 1)
		{
			// Use a more visible color for our lines
			Texture2D texture = new Texture2D(1, 1);
			texture.SetPixel(0, 0, new Color(1, 0.5f, 0, 0.8f)); // Orange with some transparency
			texture.Apply();

			// Draw lines between debug points
			for (int i = 1; i < debugPoints.Count; i++)
			{
				Vector3 start = debugPoints[i - 1];
				Vector3 end = debugPoints[i];

				// Convert to screen space for GUI drawing
				// Note: GUI has origin at top-left, mouse position has origin at bottom-left
				Vector2 startGUI = new Vector2(start.x, Screen.height - start.y);
				Vector2 endGUI = new Vector2(end.x, Screen.height - end.y);

				DrawLine(startGUI, endGUI, texture, 2);
			}
		}
	}

	void OnApplicationQuit()
	{
		// Clean disconnect when application closes
		if (ws != null && ws.ReadyState == WebSocketState.Open)
		{
			Debug.Log("Closing WebSocket connection");
			ws.Close();
		}
	}

	// WebSocket connection management
	void ConnectToServer()
	{
		try
		{
			Debug.Log("Attempting to connect to " + serverUrl);

			// Initialize WebSocket
			ws = new WebSocket(serverUrl);

			// Set up event handlers
			ws.OnOpen += (sender, e) => {
				Debug.Log("Connected to WebSocket server");
				isConnected = true;
			};

			ws.OnClose += (sender, e) => {
				Debug.Log("Disconnected from WebSocket server: " + e.Reason);
				isConnected = false;
				// Try to reconnect after delay
				Invoke("ConnectToServer", reconnectDelay);
			};

			ws.OnError += (sender, e) => {
				Debug.LogError("WebSocket error: " + e.Message);
			};

			// Try to connect
			ws.Connect();
		}
		catch (Exception e)
		{
			Debug.LogError("Error connecting to server: " + e.Message);
			// Try again after delay
			Invoke("ConnectToServer", reconnectDelay);
		}
	}

	// Coroutine to capture and queue mouse positions
	IEnumerator SendMousePositionRoutine()
	{
		Debug.Log("Starting mouse position tracking");

		while (true)
		{
			// Get current mouse position
			Vector3 mousePos = Input.mousePosition;

			// Only send if position has changed significantly to reduce traffic
			if (Vector3.Distance(mousePos, lastSentPosition) > minDistanceThreshold)
			{
				// Create JSON message with timestamp
				string message = JsonUtility.ToJson(new MousePositionMessage
				{
					x = mousePos.x,
					y = mousePos.y,
					screenWidth = Screen.width,
					screenHeight = Screen.height,
					timestamp = DateTime.Now.Ticks
				});

				// Add to queue instead of sending directly
				messageQueue.Enqueue(message);
				lastSentPosition = mousePos;

				// Debug message for testing
				if (messageQueue.Count % 10 == 0)
				{
					Debug.Log("Queue size: " + messageQueue.Count);
				}
			}

			// Wait for next interval
			yield return new WaitForSeconds(sendInterval);
		}
	}

	// Coroutine to process queued messages
	IEnumerator ProcessQueueRoutine()
	{
		Debug.Log("Starting message queue processor");

		while (true)
		{
			// Process messages if connected and queue has items
			if (isConnected && messageQueue.Count > 0)
			{
				try
				{
					string message = messageQueue.Dequeue();
					ws.Send(message);
				}
				catch (Exception e)
				{
					Debug.LogError("Error sending message: " + e.Message);
					// If send fails, disconnect to trigger reconnect
					isConnected = false;
				}
			}

			// Yield to avoid blocking the main thread
			yield return null;
		}
	}

	// Helper method to draw lines in GUI
	void DrawLine(Vector2 start, Vector2 end, Texture2D texture, float width)
	{
		Vector2 d = end - start;
		float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

		GUIUtility.RotateAroundPivot(angle, start);

		float length = d.magnitude;
		GUI.DrawTexture(new Rect(start.x, start.y, length, width), texture);

		GUIUtility.RotateAroundPivot(-angle, start);
	}
}

// Data structure for mouse position messages
[Serializable]
public class MousePositionMessage
{
	public float x;
	public float y;
	public int screenWidth;
	public int screenHeight;
	public long timestamp;
}