using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class FloorOrganizer : MonoBehaviour {

    List<List<FloorTile>> floorTileMatrix;
    public FloorTile floorTile;
    public float tileHeight;
    public float tileWidth;

    public int height;
    public int width;

    public int xOrigin;
    public int yOrigin;


    public float spreadDelay;

	void Start () {
        floorTileMatrix = new List<List<FloorTile>>();

	    for(int i = 0; i < height; i++)
        {
            floorTileMatrix.Add(new List<FloorTile>());
            for(int j = 0; j < width; j++)
            {
                FloorTile newTile = Instantiate(floorTile);
                newTile.transform.parent = this.gameObject.transform;
                newTile.name = (i * height + j).ToString();
                newTile.transform.position = new Vector3(xOrigin + j * tileWidth, yOrigin + i * tileHeight, 0);
                newTile.parentOrganizer = this;
                newTile.xPos = i;
                newTile.yPos = j;
                floorTileMatrix[i].Add(newTile);
            }
        }
	}
	
	// Update is called once per frame
	void Update () {
	}

    public IEnumerator DropletPattern(int x, int y, Direction dir = Direction.None, int energy = 2)
    {
        int mappedX = x + width / 2;
        int mappedY = y;
        if (!(mappedX < 0 || mappedX >= width || mappedY < 0 || mappedY >= height || energy < 0))
        {
            if(!floorTileMatrix[mappedY][mappedX].triggered)
            {
                floorTileMatrix[mappedY][mappedX].toggle();
                energy--;

                yield return new WaitForSeconds(spreadDelay);
                if (dir != Direction.E)
                    StartCoroutine(DropletPattern(x - 1, y, Direction.W, energy));
                if (dir != Direction.W)
                    StartCoroutine(DropletPattern(x + 1, y, Direction.E, energy));
                if (dir != Direction.N)
                    StartCoroutine(DropletPattern(x, y + 1, Direction.W, energy));
                if (dir != Direction.S)
                    StartCoroutine(DropletPattern(x, y - 1, Direction.N, energy));
            }
        }
        else
            yield return null;
    }

    public void GameOfLifePattern(int x, int y) {

    }

    public enum Direction{
        None,
        N,
        S,
        E,
        W
    }
}
