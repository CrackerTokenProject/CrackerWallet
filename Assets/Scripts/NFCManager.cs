using System.IO;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System.Security.Cryptography;

public class NFCManager : MonoBehaviour
{
    public bool isOn = false;

    public Color off = new(120, 120, 120);
    public Color on = Color.white;

    public GameObject existingData, newData;
    public TMP_Text existing, newD;

    private void FixedUpdate()
    {
        if (isOn)
        {
            GetComponent<Image>().color = on;

            AndroidNFCReader.enableBackgroundScan();
            AndroidNFCReader.ScanNFC(gameObject.name, "OnFinishScan");
        }
        else
        {
            GetComponent<Image>().color = off;

            AndroidNFCReader.disableBackgroundScan();
        }
    }

    public void ButtonClicked()
    {
        isOn = !isOn;
    }

    // NFC callback
    public void OnFinishScan(string result)
    {
        transform.Find("Panel").gameObject.SetActive(true);

        if (result == AndroidNFCReader.CANCELLED)
        {
            return;
        }
        else if (result == AndroidNFCReader.ERROR)
        {
            return;
        }
        else if (result == AndroidNFCReader.NO_HARDWARE)
        {
            return;
        }
        else if (result == "")
        {
            //No Data FOUND
            newData.SetActive(true);

            //Get wallet data to paste onto nfc
            var filePath = Application.persistentDataPath + "/" + "Walletdata.wallet";
            if (File.Exists(filePath))
            {
                var sr = new StreamReader(filePath);

                var fileContent = sr.ReadToEnd();

                sr.Close();

                fileContent = Encrypt.Decrypt(fileContent);
                string[] wallets = fileContent.Split("||", System.StringSplitOptions.RemoveEmptyEntries);

                if (wallets == null || wallets[0] == "") return;

                newD.text = Encrypt.Encrypts(wallets[WalletManager.Instance.walletSelectionDropdown.value]);
            }
        }
        else
        {
            //gotta contain something
            existingData.SetActive(true);

            //get nfc result
            existing.text = result;
        }
    }

    public void WriteDataToNFC()
    {
        //figure this out later
    }

    public void ImportNFCWallet()
    {
        var filePath = Application.persistentDataPath + "/" + "Walletdata.wallet";

        string[] decrypted = Encrypt.Decrypt(newD.text).Split("|", System.StringSplitOptions.RemoveEmptyEntries);

        WalletManager.Instance.walletList.Add(new WalletData
        {
            name = decrypted[0],
            address = decrypted[1],
            cachedPassword = decrypted[2],
            encryptedJson = decrypted[3],
            privateKey = decrypted[4]
        });

        foreach(WalletData d in WalletManager.Instance.walletList)
        {
            string encrypted = Encrypt.Encrypts(new string(d.name + "|" + d.address + "|" + d.cachedPassword + "|" + d.encryptedJson + "|" + d.privateKey + "||"));
            File.WriteAllText(filePath, encrypted);
        }
    }
}
