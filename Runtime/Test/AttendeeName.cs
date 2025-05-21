using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Venti;

public class AttendeeName : MonoBehaviour
{
    TMPro.TMP_Text attendeeNameText;

    void Start()
    {
        attendeeNameText = GetComponent<TMPro.TMP_Text>();
    }

    public void OnSessionStart(Session session)
    {
        attendeeNameText.text = session.attendee.firstName;
    }

    public void OnSessionEnd()
    {
        attendeeNameText.text = "";
    }
}
