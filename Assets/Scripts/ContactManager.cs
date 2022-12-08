using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ContactManager : MonoBehaviour
{
    private string filePath;
    private string fileName = "Contacts.bin";

    public TMP_InputField contactName;
    public TMP_InputField contactAddress;
    public Button confirmButton;

    public TMP_Dropdown recipientDropdown;

    private StreamReader sr;

    public List<ContactData> contactList;

    [System.Serializable]
    public class ContactData
    {
        public string name;
        public string address;
    }

    public void LoadContacts()
    {
        filePath = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(filePath))
        {
            contactList.Clear();

            sr = new StreamReader(filePath);

            var fileContent = sr.ReadToEnd();

            fileContent = Encrypt.Decrypt(fileContent);
            string[] contacts = fileContent.Split("||", System.StringSplitOptions.RemoveEmptyEntries);

            if (contacts == null || contacts[0] == "") return;
            foreach (string s in contacts)
            {
                ContactData w = new();
                var st = s.Split("|");
                w.name = st[0];
                w.address = st[1];

                contactList.Add(w);
            }

            Debug.Log("Contacts loaded from: " + filePath);
            sr.Close();
        }
    }

    public void SaveDataToFile()
    {
        filePath = Application.persistentDataPath + "/" + fileName;

        foreach (ContactData d in contactList)
        {
            string encrypted = Encrypt.Encrypts(new string(d.name + "|" + d.address + "||"));
            File.WriteAllText(filePath, encrypted);
        }
    }

    private void Start()
    {
        SaveDataToFile();

        InvokeRepeating("RefreshContacts", 5, 10);
    }

    public void RefreshContacts()
    {
        foreach(ContactData d in contactList)
        {
            recipientDropdown.options.Add(new(d.name));
        }
    }

    private void FixedUpdate()
    {
        if(contactAddress.text != null || contactAddress.text != "")
        {
            if (contactAddress.text.Substring(0, 2) == "0x") confirmButton.enabled = false;
        }
    }

    public void NewContact()
    {
        ContactData contact = new ContactData
        {
            name = contactName.text,
            address = contactAddress.text
        };

        foreach(ContactData d in contactList)
        {
            if (d.name == contactName.text)
            {
                contactName.text = "Name already exists!";
                return;
            }
        }

        contactList.Add(contact);
    }
}
