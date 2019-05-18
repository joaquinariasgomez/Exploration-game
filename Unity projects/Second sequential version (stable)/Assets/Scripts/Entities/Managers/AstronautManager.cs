using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AstronautManager : MonoBehaviour {

    public GameObject[] astronauts;
    public GameObject StopExploringButton;
    public GameObject SwordButtons;
    public GameObject ShieldButtons;
    public GameObject WeaponImages;
    public AlienManager alienManager;
    public GameObject progressBar;

    private List<PlayerController> astronautControllers = new List<PlayerController>();
    private int numAstronauts;
    private bool startPSO = false;
    private bool goForAliens = false;
    private float inertia;
    private bool setSoundBattle = true;

    PSO pso;

    // Use this for initialization
    void Start () {
        numAstronauts = astronauts.Length;
        foreach (GameObject astronaut in astronauts)
        {
            astronautControllers.Add(astronaut.GetComponent<PlayerController>());
        }
        //Initialize
        int counter = 0;
        foreach(PlayerController controller in astronautControllers)
        {
            controller.Initialize(counter);
            counter++;
        }

        SetAstronautsInPlace();
        pso = new PSO(astronautControllers, progressBar, StopExploringButton);
        StopExploringButton.SetActive(false);
    }

    void SetAstronautsInPlace()
    {
        //Set Astronauts in place forming a circle, for example
        float radius = 5f;

        astronautControllers[0].SetInPlace(-radius, 0f, -90f);
        astronautControllers[1].SetInPlace(-radius * 3f / 4f, radius * 3f / 4f, -45f);
        astronautControllers[2].SetInPlace(0f, radius, 0f);
        astronautControllers[3].SetInPlace(radius * 3f / 4f, radius * 3f / 4f, 45f);
        astronautControllers[4].SetInPlace(radius, 0f, 90f);
        astronautControllers[5].SetInPlace(radius * 3f / 4f, -radius * 3f / 4f, 135f);
        astronautControllers[6].SetInPlace(0f, -radius, 180f);
        astronautControllers[7].SetInPlace(-radius * 3f / 4f, -radius * 3f / 4f, -135f);
    }

    public void onOK()
    {
        startPSO = true;
        inertia = GameObject.Find("WSliderText").GetComponent<ShowWInSlider>().value;
        pso.SetInertiaAstronaut(inertia);

        StopExploringButton.SetActive(true);
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().Unpoint("button");

        //Destroy UI elements
        GameObject.Find("WSlider").gameObject.SetActive(false); //Destroys WSliderText because its child of WSlider
        GameObject.Find("WSliderOK").gameObject.SetActive(false);
    }

    public void onStopExploring()
    {
        pso.StopExploringAstronaut();

        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().Unpoint("button");

        //Destroy UI element
        StopExploringButton.SetActive(false);
    }

    private void DestroyButtonsToImage()
    {
        //Destroy buttons
        foreach(Transform child in SwordButtons.transform){
            child.gameObject.SetActive(false);
        }
        foreach (Transform child in ShieldButtons.transform)
        {
            child.gameObject.SetActive(false);
        }
        //Put image
        List<string> weapons = new List<string>();
        foreach(PlayerController controller in astronautControllers)
        {
            string weapon = controller.GetWeapon();
            weapons.Add(weapon);
        }
        int counter = 0;
        foreach(Transform child in WeaponImages.transform)
        {
            child.gameObject.GetComponent<ShowWeaponImage>().SetImage(weapons[counter]);
            counter++;
        }
        GameObject.Find("Mouse").GetComponent<MouseSkinManager>().Unpoint("button");
    }
	
    public GameObject[] GetAstronauts()
    {
        return astronauts;
    }

	// Update is called once per frame
	void Update () {
        if(PauseMenu.GamePaused)
        {
            return;
        }
        if(startPSO)
        {
            bool weaponsAssigned = pso.UpdateAstronauts();
            if(weaponsAssigned)
            {
                if(setSoundBattle)
                {
                    FindObjectOfType<AudioManager>().PlayBattle();
                    setSoundBattle = false;
                }
                startPSO = false;
                //Destroy Weapon Buttons
                DestroyButtonsToImage();
                //Send signal to go for aliens
                goForAliens = true;
                //Call AlienManager
                alienManager.Initialize(pso.GetTargetCoordinates());
            }
        }
        if(goForAliens)
        {

        }
    }
}
