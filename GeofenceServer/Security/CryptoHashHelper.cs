using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using System.Diagnostics;

public class CryptoHashHelper
{
    private AesCryptoServiceProvider Provider;
    private ICryptoTransform Encryptor;
    private ICryptoTransform Decryptor;
    private SHA256 Hasher;
    private string SecretFolderpath = "D:/RaresLaptop/8Uni/Materiale/DezvoltareaAplicatiilorMobile/VideoWIzardServer/VideoWIzardServer/Security/";

    public CryptoHashHelper()
    {
        try
        {
            Provider = new AesCryptoServiceProvider();
        }
        catch(Exception ex)
        {
            Trace.TraceError("Exception thrown while instantiating crypto provider.\n" +
                 "Message: " + ex.Message +
                 "\nStack trace: " + ex.StackTrace);
        }
        Provider.Key = Convert.FromBase64String(File.ReadAllText(SecretFolderpath + "key.txt"));
        Provider.IV = Convert.FromBase64String(File.ReadAllText(SecretFolderpath + "IV.txt"));
        Encryptor = Provider.CreateEncryptor();
        Decryptor = Provider.CreateDecryptor(Provider.Key, Provider.IV);

        try
        {
            Hasher = SHA256.Create();
        }
        catch (Exception ex)
        {
            Trace.TraceError("Exception thrown while instantiating crypto provider.\n" +
                 "Message: " + ex.Message +
                 "\nStack trace: " + ex.StackTrace);
        }
    }
    public string Encrypt(string text)
    {
        byte[] clearBytes = Encoding.UTF8.GetBytes(text);
        byte[] encryptedByte;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream
                , Encryptor
                , CryptoStreamMode.Write))
            {
                cryptoStream.Write(clearBytes, 0, clearBytes.Length);
                cryptoStream.Close();
            }
            encryptedByte = memoryStream.ToArray();
        }
        return Convert.ToBase64String(encryptedByte);
    }
    public string Decrypt(string encryptedText)
    {
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] clearBytes;

        using (MemoryStream memoryStream = new MemoryStream())
        {
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream
                    , Decryptor
                    , CryptoStreamMode.Write))
            {
                cryptoStream.Write(encryptedBytes, 0, encryptedBytes.Length);
                cryptoStream.Close();
            }
            clearBytes = memoryStream.ToArray();
        }
        return Encoding.UTF8.GetString(clearBytes);
    }
    public string GetHash(string text)
    {
        byte[] bytes = Hasher.ComputeHash(Encoding.UTF8.GetBytes(text));
        StringBuilder stringBuilder = new StringBuilder();
        for (int i = 0; i < bytes.Length; ++i)
        {
            stringBuilder.Append(bytes[i].ToString("x2"));
        }
        return stringBuilder.ToString();
    }
}