using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private Text _dashText;
    // Start is called before the first frame update
    void Start()
    {
        _dashText.text = "Dash: Available";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DashAvailability(bool isDashOnCooldown)
    {
        if(isDashOnCooldown==false)
        {
            _dashText.text = "Dash: Available";
        }
        else
        {
            _dashText.text = "Dash: On Cooldown";
        }
    }
}
