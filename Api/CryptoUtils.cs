using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Api;

public class CryptoUtils(Config diConfig)
{
    private Config _config =  diConfig;

    public string Sign(object ob)
    {
        // Convert object to json string 
        string json = JsonSerializer.Serialize(ob, Config.JsonMinOptions);
        byte[] key = Encoding.UTF8.GetBytes(_config.AuthRequirements.OtpSecret);
        
        using HMACSHA256 hmac = new(key);
        string hashed = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(json)));
        return hashed;
    }

    /// <summary>
    /// Verifies that the provided hash is a valid signature of the provided object. 
    /// </summary>
    /// <param name="ob"></param>
    /// <param name="hash"></param>
    /// <returns></returns>
    public bool VerifySignature(object ob,  string hash)
    {
        return Sign(ob) == hash;
    }
}