using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AstronautSelector : MonoBehaviour {

    public AstronautManager astronautManager;
    public Button Astronaut0Button;
    public Button Astronaut1Button;
    public Button Astronaut2Button;
    public Button Astronaut3Button;
    public Button Astronaut4Button;
    public Button Astronaut5Button;
    public Button Astronaut6Button;
    public Button Astronaut7Button;

    private int selectedAstronaut; //-1 => No se selecciona astronauta
    List<bool> selectedAstronauts = new List<bool>();
    private Color pressedColor = Color.green;

    private void Start()
    {
        selectedAstronaut = -1;
        //Ninguno es seleccionado en un principio
        for(int i=0; i<8; i++)
        {
            selectedAstronauts.Add(false);
        }
    }

    public int GetSelectedAstronaut()
    {
        return this.selectedAstronaut;
    }

    public void SetSelectedAstronaut(int num)
    {
        this.selectedAstronaut = num;
    }

    public void SelectAstronaut0(bool defend = false)
    {
        if(defend)
        {
            astronautManager.astronauts[0].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[0])
        {
            selectedAstronauts[0] = false;
            selectedAstronaut = -1;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for(int i=0; i<8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[0] = true;
            selectedAstronaut = 0;
            Astronaut0Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut1(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[1].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[1])
        {
            selectedAstronauts[1] = false;
            selectedAstronaut = -1;
            Astronaut1Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[1] = true;
            selectedAstronaut = 1;
            Astronaut1Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut2(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[2].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[2])
        {
            selectedAstronauts[2] = false;
            selectedAstronaut = -1;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[2] = true;
            selectedAstronaut = 2;
            Astronaut2Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut3(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[3].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[3])
        {
            selectedAstronauts[3] = false;
            selectedAstronaut = -1;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[3] = true;
            selectedAstronaut = 3;
            Astronaut3Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut4(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[4].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[4])
        {
            selectedAstronauts[4] = false;
            selectedAstronaut = -1;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[4] = true;
            selectedAstronaut = 4;
            Astronaut4Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut5(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[5].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[5])
        {
            selectedAstronauts[5] = false;
            selectedAstronaut = -1;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[5] = true;
            selectedAstronaut = 5;
            Astronaut5Button.GetComponent<Image>().color = pressedColor;
        }
    }

    public void SelectAstronaut6(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[6].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[6])
        {
            selectedAstronauts[6] = false;
            selectedAstronaut = -1;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[6] = true;
            selectedAstronaut = 6;
            Astronaut6Button.GetComponent<Image>().color = pressedColor;
        }
    }
    
    public void SelectAstronaut7(bool defend = false)
    {
        if (defend)
        {
            astronautManager.astronauts[7].GetComponent<PlayerController>().SetDefendThis();
            return;
        }
        if (selectedAstronauts[7])
        {
            selectedAstronauts[7] = false;
            selectedAstronaut = -1;
            Astronaut7Button.GetComponent<Image>().color = Color.white;
        }
        else
        {
            for (int i = 0; i < 8; i++)
            {
                selectedAstronauts[i] = false;
            }
            Astronaut1Button.GetComponent<Image>().color = Color.white;
            Astronaut2Button.GetComponent<Image>().color = Color.white;
            Astronaut3Button.GetComponent<Image>().color = Color.white;
            Astronaut4Button.GetComponent<Image>().color = Color.white;
            Astronaut5Button.GetComponent<Image>().color = Color.white;
            Astronaut6Button.GetComponent<Image>().color = Color.white;
            Astronaut0Button.GetComponent<Image>().color = Color.white;
            selectedAstronauts[7] = true;
            selectedAstronaut = 7;
            Astronaut7Button.GetComponent<Image>().color = pressedColor;
        }
    }
}
