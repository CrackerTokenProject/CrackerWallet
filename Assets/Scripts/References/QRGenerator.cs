using ZXing;
using ZXing.QrCode;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Collections;

public class QRGenerator : MonoBehaviour
{
    public Image QRImage;
    private WebCamTexture camTexture;
    private Rect screenRect;
    void Start()
    {
        /*
        screenRect = new Rect(0, 0, Screen.width, Screen.height);
        camTexture = new WebCamTexture();
        camTexture.requestedHeight = Screen.height;
        camTexture.requestedWidth = Screen.width;
        if (camTexture != null)
        {
            camTexture.Play();
        }
        */
    }

    private static Color32[] Encode(string textForEncoding,
      int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    public void Generate()
    {
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

            var qrtext = Encrypt.Encrypts(wallets[WalletManager.Instance.walletSelectionDropdown.value]);

            Texture2D qr = generateQR(qrtext);
            Rect rec = new Rect(0, 0, qr.width, qr.height);
            QRImage.sprite = Sprite.Create(qr, rec, new Vector2(0,0));

            NativeGallery.SaveImageToGallery(qr.EncodeToPNG(), "Cracker Wallet", WalletManager.Instance.walletList[WalletManager.Instance.walletSelectionDropdown.value].name + "qrcode.png");

            StartCoroutine(Dissappear());
        }
    }

    private IEnumerator Dissappear()
    {
        yield return new WaitForSeconds(2.4f);
        transform.Find("Panel").gameObject.SetActive(false);
    }


    public Texture2D generateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }
}