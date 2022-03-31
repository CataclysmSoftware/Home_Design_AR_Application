using System.Collections;
using UnityEngine;

namespace AF
{
	public class CreateWallsManager : MonoBehaviour
	{
		private const float WALL_LENGHT = 4f;

		// Managers
		private InputManager inputManager;
		private WallsManager wallsManager;

		// Resources
		private GameObject startWallResource;
		private GameObject endWallResource;
		private GameObject previewWallResource;
		private SimpleWallController simpleWallResource;

		// Game Objects
		private GameObject startWallGO;
		private GameObject endWallGO;
		private GameObject previewWallGO;

		private bool isCreating;

		private void Start()
		{
			InitializeManagers();
			InitializeResources();
		}

		private void InitializeManagers()
		{
			inputManager = FindObjectOfType<InputManager>();
			wallsManager = FindObjectOfType<WallsManager>();
		}

		private void InitializeResources()
		{
			startWallResource = Resources.Load<GameObject>(WallPaths.START_WALL_PATH);
			endWallResource = Resources.Load<GameObject>(WallPaths.END_WALL_PATH);
			simpleWallResource = Resources.Load<SimpleWallController>(WallPaths.SIMPLE_WALL_PATH);
			previewWallResource = Resources.Load<GameObject>(WallPaths.PREVIEW_WALL_PATH);
		}

		private void Update()
		{
			DrawWall();
		}

		private void DrawWall()
		{
			if (Input.GetMouseButtonDown(0) && !inputManager.IsPointerOverUIElement())
			{
				startWallGO = Instantiate(startWallResource, Vector3.zero, Quaternion.identity);
				endWallGO = Instantiate(endWallResource, Vector3.zero, Quaternion.identity);
				SetStartWallPosition();
			}
			else if (isCreating)
			{
				if (Input.GetMouseButtonUp(0))
				{
					CreateWall();
				}
				else
				{
					CreatePreviewWalls();
				}
			}
		}

		private void SetStartWallPosition()
		{
			previewWallGO = Instantiate(previewWallResource, startWallResource.transform.position, Quaternion.identity);
			var hit = inputManager.GetWorldPoint();
			isCreating = true;
			startWallGO.transform.position = hit;
		}

		private void CreateWall()
		{
			isCreating = false;
			endWallGO.transform.position = inputManager.GetWorldPoint();

			// Calculate the numbers of completed walls
			var distance = Vector3.Distance(startWallGO.transform.position, endWallGO.transform.position);
			var numberOfWalls = (int)(distance / WALL_LENGHT);
			var difference = distance - numberOfWalls * WALL_LENGHT;

			// Create a wall Controller
			var wallController = new GameObject().AddComponent<WallController>();

			// Create the Left Wall
			var leftWallPosition = startWallGO.transform.position + previewWallGO.transform.forward * (difference / 4);
			var leftWall = Instantiate(simpleWallResource, leftWallPosition, previewWallGO.transform.rotation);
			leftWall.transform.localScale = new Vector3(leftWall.transform.localScale.x, leftWall.transform.localScale.y, difference / 8);
			wallController.AddNewSimpleWalls(leftWall);

			// Create the completed walls
			var wallForward = previewWallGO.transform.forward;
			var startWallPosition = startWallGO.transform.position;
			for (int wallNumber = 0; wallNumber < numberOfWalls; wallNumber++)
			{
				var startPosition = wallForward * (2 * 2 * wallNumber + difference / 2 + 2) + startWallPosition;
				var wall = Instantiate(simpleWallResource, startPosition, previewWallGO.transform.rotation);
				wallController.AddNewSimpleWalls(wall);
			}

			// Create the Right Wall
			var rightWallPosition = wallForward * (difference / 2 + difference / 4 + WALL_LENGHT * numberOfWalls) + startWallPosition;
			var rightWall = Instantiate(simpleWallResource, rightWallPosition, previewWallGO.transform.rotation);
			rightWall.transform.localScale = new Vector3(leftWall.transform.localScale.x, leftWall.transform.localScale.y, difference / 8);
			wallController.AddNewSimpleWalls(rightWall);

			wallsManager.AddNewWall(wallController);

			Destroy(previewWallGO.gameObject);
			Destroy(startWallGO.gameObject);
			Destroy(endWallGO.gameObject);
		}

		private void CreatePreviewWalls()
		{
			endWallGO.transform.position = inputManager.GetWorldPoint();

			startWallGO.transform.LookAt(endWallGO.transform.position);
			endWallGO.transform.LookAt(startWallGO.transform.position);
			var distance = (startWallGO.transform.position - endWallGO.transform.position).magnitude;

			previewWallGO.transform.position = startWallGO.transform.position + distance / 2 * startWallGO.transform.forward;
			previewWallGO.transform.rotation = startWallGO.transform.rotation;
			previewWallGO.transform.localScale = new Vector3(previewWallGO.transform.localScale.x, previewWallGO.transform.localScale.y, distance);
		}
	}
}