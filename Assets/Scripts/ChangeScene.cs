using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour {
    new Collider collider;
    public List<string> scenes;
    public Transform player;
    bool InOpenWorld;
	// Use this for initialization
	void Start () {
		if(player==null)
        {
            player = GameObject.FindWithTag("Player").transform;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider == player.GetComponent<Collider>())
        {
            Scene currentScene=SceneManager.GetActiveScene();
            string otherscenename =scenes.Find(x => x != currentScene.name);
            //Scene otherscene = SceneManager.GetSceneByName(otherscenename);
            //If we want to do the crazy deload surface and load new area thing we change this line
            SceneManager.LoadScene(otherscenename, LoadSceneMode.Single);            
        }
    }


}
