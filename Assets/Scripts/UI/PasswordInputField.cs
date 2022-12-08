using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PasswordInputField : MonoBehaviour {

    public TMP_InputField confirmPasswordInputField;
    public Button createWalletButton;

    private bool m_passwordConfirmed = false;

    private void Start()
    {
        GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        confirmPasswordInputField.contentType = TMP_InputField.ContentType.Password;

        createWalletButton.interactable = false;
    }

    public bool passwordConfirmed()
    {
        return m_passwordConfirmed;
    }

    public void resetFields()
    {
        gameObject.GetComponent<TMP_InputField>().text = "";
        confirmPasswordInputField.text = "";
    }

    public string passwordString()
    {
        return GetComponent<TMP_InputField>().text;
    }

    public void validatePasswordConfirmation()
    {
        m_passwordConfirmed = passwordString().Equals(confirmPasswordInputField.text);
        createWalletButton.interactable = m_passwordConfirmed;
    }
}
