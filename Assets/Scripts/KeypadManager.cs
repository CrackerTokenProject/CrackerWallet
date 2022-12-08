using UnityEngine.UI;
using UnityEngine;
using TMPro;
using CandyCoded.HapticFeedback;

public class KeypadManager : MonoBehaviour {

	private string amount;

    public Button confirmButton;

    public TMP_Text keyPadText, sendingAmount;

    public GameObject TransactionScreen, HomeScreen;

    private void FixedUpdate()
    {
        if(amount == "" || amount == null)
        {
            confirmButton.enabled = false;
        }
        else
        {
            confirmButton.enabled = true;
        }

        keyPadText.text = amount;

        sendingAmount.text = amount;
    }

    public void AddKey(int key)
    {
        amount += key;
    }

    public void Confirm()
    {
        TransactionScreen.SetActive(true);
        HomeScreen.SetActive(false);
    }

    HapticFeedbackController controller;

    private void Start()
    {
        controller = new();
    }

    public void Delete()
    {
        if(amount.Length > 0) amount = amount.Substring(0, amount.Length - 1);
    }

    public void HapticFeedback()
    {
        controller.HeavyFeedback();
    }
}
