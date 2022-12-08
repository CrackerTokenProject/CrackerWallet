using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Nethereum.Unity.Rpc;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using Nethereum.Util;
using TMPro;
using Nethereum.KeyStore;

using acc = Nethereum.Web3.Accounts.Account;

// TODO: IMPORTANT! A serialization bug sometimes makes the walletcache.data broken, fix this!
// for now, always backup walletcache.data

[System.Serializable]
public class WalletData
{
    public string name;
    public string address;

    // TODO: stored for convenience, may need to remove for security
    public string cachedPassword;
    public string encryptedJson;
    public string privateKey;
}

public class WalletManager : MonoBehaviour {

    // create class as singleton
    private static WalletManager instance;
    public static WalletManager Instance { get { return instance; } }
    public void Awake() { if (instance == null) instance = this; }
    public void OnDestroy() { if (instance == this) instance = null; }

    [Header("Config")]

    public string networkUrl;

    [Header("UI Components")]

    public PasswordInputField passwordInputField;
    public LogText logText;
    public TMP_Dropdown recepientAddressDropdown;
    public TMP_Text fundTransferAmountText;
    public GameObject createWalletPanel;
    public GameObject loadingIndicatorPanel;
    public GameObject operationsPanel;
    public TMP_Dropdown walletSelectionDropdown;
    public TMP_Text CustomTokenBalanceText;
    public TMP_Text walletInfoText;
    public TMP_Text walletAddressText;

    public TMP_Text CurrencyInfoText;
    public GameObject LoadingIndicator;
    //public Button ShowQRCodeButton;


    public QRCodeDisplay QRPanel;

    public GameObject MainPanel;
    public GameObject QRScannerPanel;
    public TMP_Text importPrivateKey;
    //public Button closeQRScannerButton;

    private bool isPaused = false;
    private bool dataSaved = false;

    // events
    static UnityEvent newAccountAdded;
    static UnityEvent loadingFinished;

    public List<WalletData> walletList = new List<WalletData>();

    // used for saving 
    private StreamReader sr;
    private string fileContent;
    private string filePath;
    private const string fileName = "Walletdata.wallet";

    // show QR Scanner
    public void ToggleQRScannerDisplay(bool forceMain = false)
    {
        if (forceMain)
        {
            MainPanel.SetActive(true);
            QRScannerPanel.SetActive(false);
        }

        else
        {
            MainPanel.SetActive(!MainPanel.activeSelf);
            QRScannerPanel.SetActive(!QRScannerPanel.activeSelf);
        }
    }

    //// show QR code display
    //public void ToogleQRCodeDisplay()
    //{
    //    currencyInfoScrollView.SetActive(!currencyInfoScrollView.activeSelf);
    //    QRPanel.gameObject.SetActive(!currencyInfoScrollView.activeSelf);
    //}

    // copy account address to clipboard
    public void CopyToClipboard()
    {
        CopyToClipboard(walletList[walletSelectionDropdown.value].address);
    }

    public void CopyToClipboard(string s)
    {
        TextEditor te = new TextEditor();
        te.text = s;
        te.SelectAll();
        te.Copy();
    }

    public void RefreshBalance()
    {
        foreach (WalletData d in walletList)
        {
            if(d.address == walletSelectionDropdown.options[walletSelectionDropdown.value].text)
            {
                StartCoroutine(CheckAccountBalanceCoroutine(d.address));
            }
        }
    }

    public void ImportWallet()
    {
        var keystoreservice = new KeyStoreService();

        acc a = new(importPrivateKey.text);

        var encryptedJson = keystoreservice.EncryptAndGenerateDefaultKeyStoreAsJson("", System.Text.Encoding.UTF8.GetBytes(a.PrivateKey), a.Address);
        Debug.Log(a.Address);

        Debug.Log(encryptedJson);

        WalletData w = new WalletData();
        w.name = "Imported Account: " + Random.Range(1000, 9999);
        w.address = a.Address;
        w.cachedPassword = "";
        w.encryptedJson = encryptedJson;
        w.privateKey = a.PrivateKey;

        walletList.Add(w);

        newAccountAdded.Invoke();
        loadingFinished.Invoke();
    }

    //public void ConnectMetamask()
    //{
    //    if (MetamaskInterop.IsMetamaskAvailable())
    //    {
    //        MetamaskInterop.EnableEthereum(gameObject.name, "", "");
    //    }
    //    else
    //    {
    //        Debug.Log("Metamask is not available, please install it");
    //    }
    //}

    //private bool _isMetamaskInitialised = false;

    //public void EthereumEnabled()
    //{
    //    if (!_isMetamaskInitialised)
    //    {
    //        MetamaskInterop.EthereumInit(gameObject.name, "", "");
    //        MetamaskInterop.GetChainId(gameObject.name, "", "");
    //        _isMetamaskInitialised = true;
    //    }
    //}

    public void Start()
    {
        subscribeToEvents();    
        LoadWalletsFromFile();
        RefreshTopPanelView();

        RefreshWalletText();

        RefreshBalance();
    }

    public void RefreshWalletText()
    {
        if (walletList.Count > 0)
        {
            walletAddressText.text = walletList[walletSelectionDropdown.value].address;
        }
        else
        {
            walletAddressText.text = "";
        }
    }

    private void subscribeToEvents()
    {
        newAccountAdded = new UnityEvent();
        loadingFinished = new UnityEvent();

        newAccountAdded.AddListener(RefreshWalletAccountDropdown);
        newAccountAdded.AddListener(RefreshTopPanelView);

        loadingFinished.AddListener(hideLoadingIndicatorPanel);
    }

    void LoadWalletsFromFile()
    {
        filePath = Application.persistentDataPath + "/" + fileName;
        if (File.Exists(filePath))
        {
            sr = new StreamReader(filePath);

            fileContent = sr.ReadToEnd();

            sr.Close();

            fileContent = Encrypt.Decrypt(fileContent);
            string[] wallets = fileContent.Split("||", System.StringSplitOptions.RemoveEmptyEntries);

            if (wallets == null || wallets[0] == "") return;
            foreach (string s in wallets)
            {
                WalletData w = new();
                var st = s.Split("|");
                w.name = st[0];
                w.address = st[1];
                w.cachedPassword = st[2];
                w.encryptedJson = st[3];
                w.privateKey = st[4];

                walletList.Add(w);
            }

            Debug.Log("Wallet loaded from: " + filePath);
            sr.Close();
        }
        RefreshWalletAccountDropdown();
    }


    public void RefreshWalletAccountDropdown()
    {
        walletSelectionDropdown.ClearOptions();

        foreach (WalletData w in walletList)
        {
            walletSelectionDropdown.AddOptions(new List<string> { w.address });            
        }

        if(walletList.Count <= 0)
        {
            walletSelectionDropdown.AddOptions(new List<string> { "No Wallets Found" });
        }
    }

    public void RefreshTopPanelView()
    {
        passwordInputField.resetFields();

        int index = walletSelectionDropdown.value;

        if (index >= walletSelectionDropdown.options.Count - 1)
        {
            createWalletPanel.SetActive(true);
            operationsPanel.SetActive(false);

            CustomTokenBalanceText.text = "Available CRK: 0";
            //CopyToClipboardButton.interactable = false;
            //ShowQRCodeButton.interactable = false;

            //currencyInfoScrollView.SetActive(true);
            //QRPanel.gameObject.SetActive(false);

        }

        else
        {
            createWalletPanel.SetActive(false);
            operationsPanel.SetActive(true);

            //CopyToClipboardButton.interactable = true;
            //ShowQRCodeButton.interactable = true;

            //QRPanel.RenderQRCode(walletList[index].address);
        }
    }


    void SaveDataToFile()
    {
        filePath = Application.persistentDataPath + "/" + fileName;

        foreach(WalletData d in walletList)
        {
            string encrypted = Encrypt.Encrypts(new string(d.name + "|" + d.address + "|" + d.cachedPassword + "|" + d.encryptedJson + "|" + d.privateKey + "||"));
            File.WriteAllText(filePath, encrypted);
        }

        dataSaved = true;
    }

    void OnApplicationFocus(bool hasFocus)
    {
        isPaused = !hasFocus;
    }

    void OnApplicationPause(bool pauseStatus)
    {
        isPaused = pauseStatus;

        if (isPaused)
            SaveDataToFile();
        else
            dataSaved = false;
    }

    void OnApplicationQuit()
    {
        if (!dataSaved)
            SaveDataToFile();
    }


    private void disableOperationPanels()
    {
        createWalletPanel.SetActive(false);
    }

    private void hideLoadingIndicatorPanel()
    {
        loadingIndicatorPanel.SetActive(false);
    }

    private void showLoadingIndicatorPanel()
    {
        loadingIndicatorPanel.SetActive(true);
    }

    public void CreateWallet()
    {
        if (passwordInputField.passwordConfirmed())
        {
            disableOperationPanels();
            showLoadingIndicatorPanel();

            // Here we call CreateAccount() and we send it a password to encrypt the new account
            StartCoroutine(CreateAccountCoroutine(passwordInputField.passwordString(),
                "Account " + (walletList.Count + 1))); 
        }

        else
        {
            return;
        }
    }

    public void AddInfoText(string text, bool clear = false)
    {
        if (clear)
            CurrencyInfoText.text = "";
        else
            CurrencyInfoText.text += "\n";

        CurrencyInfoText.text += text;

    }

    public IEnumerator CheckAccountBalanceCoroutine(string address)
    {
        CustomTokenBalanceText.text = "Loading Balance...";

        yield return 0; // allow UI to update

        string customTokenBalance;

        var tokenBalanceRequest = new EthCallUnityRequest(networkUrl);

        // get custom token balance (uint 256)
        yield return tokenBalanceRequest.SendRequest(TokenContractService.Instance.CreateCallInput("balanceOf", address), 
            BlockParameter.CreateLatest());

        customTokenBalance = UnitConversion.Convert.FromWei(
            TokenContractService.Instance.DecodeVariable<BigInteger>("balanceOf", tokenBalanceRequest.Result), 
            TokenContractService.Instance.TokenInfo.decimals).ToString();

        CustomTokenBalanceText.text = "Available " + TokenContractService.Instance.TokenInfo.symbol + ": " + customTokenBalance;
    }

    // We create the function which will check the balance of the address and return a callback with a decimal variable
    public IEnumerator CreateAccountCoroutine(string password, string accountName)
    {
        yield return 0; // allow UI to update

        CreateAccount(password, (address, encryptedJson, privateKey) =>
        {
            // We just print the address and the encrypted json we just created
            Debug.Log(address);
            Debug.Log(encryptedJson);

            WalletData w = new WalletData();
            w.name = accountName;
            w.address = address;
            w.cachedPassword = password;
            w.encryptedJson = encryptedJson;
            w.privateKey = privateKey;

            walletList.Add(w);

            newAccountAdded.Invoke();
            loadingFinished.Invoke();
        });
    }

    private void FixedUpdate()
    {
        if(walletSelectionDropdown.options.Count > 0)
        {
            walletAddressText.text = walletSelectionDropdown.options[walletSelectionDropdown.value].text;
        }
    }

    // This function will just execute a callback after it creates and encrypt a new account
    public void CreateAccount(string password, System.Action<string, string, string> callback)
    {
        // We use the Nethereum.Signer to generate a new secret key
        Nethereum.Signer.EthECKey.DEFAULT_PREFIX = 0xC1;
        var ecKey = Nethereum.Signer.EthECKey.GenerateKey(System.Text.Encoding.UTF8.GetBytes("0xC1"));

        // After creating the secret key, we can get the public address and the private key with
        // ecKey.GetPublicAddress() and ecKey.GetPrivateKeyAsBytes()
        // (so it return it as bytes to be encrypted)
        var address = ecKey.GetPublicAddress();
        var privateKeyBytes = ecKey.GetPrivateKeyAsBytes();
        var privateKey = ecKey.GetPrivateKey();

        // Then we define a new KeyStore service
        var keystoreservice = new KeyStoreService();

        // And we can proceed to define encryptedJson with EncryptAndGenerateDefaultKeyStoreAsJson(),
        // and send it the password, the private key and the address to be encrypted.
        var encryptedJson = keystoreservice.EncryptAndGenerateDefaultKeyStoreAsJson(password, privateKeyBytes, address);
        // Finally we execute the callback and return our public address and the encrypted json.
        // (you will only be able to decrypt the json with the password used to encrypt it)
        callback(address, encryptedJson, privateKey);
    }

    public WalletData GetSelectedWalletData()
    {
        int index = walletSelectionDropdown.value;

        if (index >= walletSelectionDropdown.options.Count - 1)
            return null;

        else
            return walletList[index];

    }



}
