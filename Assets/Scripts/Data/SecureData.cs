using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class SecureData
{
    private static readonly byte[] claveAES = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF"); // clave para el cifrado AES
    private static readonly byte[] claveHMAC = Encoding.UTF8.GetBytes("FEDCBA9876543210FEDCBA9876543210"); // clave para firmar HMAC

    /**
        cifra un texto en JSON y lo firma, devuelve un array de bytes que contiene todo el resultado
    **/
    public static byte[] Encriptar(string json)
    {
        //encriptar
        using var aes = Aes.Create(); // crea una instancia de AES y lo hace en un bloque using para que se liberen recursos al final del bloque
        aes.Key = claveAES; // asigna la clave
        aes.GenerateIV(); // genera un IV

        var jsontext = Encoding.UTF8.GetBytes(json); // convierte el json pasado por parametros en un array de bytes
        var cifrado = aes.CreateEncryptor().TransformFinalBlock(jsontext, 0, jsontext.Length); // cifra el json text usando con AES usando la clave y el IV

        var cifradoIV = new byte[aes.IV.Length + cifrado.Length]; // contiene el json ya cifrado con AES e IV y ademas el IV para despues poder desencriptarlo ya que se necesita el mismo IV
        Buffer.BlockCopy(aes.IV, 0, cifradoIV, 0, aes.IV.Length);
        Buffer.BlockCopy(cifrado, 0, cifradoIV, aes.IV.Length, cifrado.Length);

        //firmar
        using var hmac = new HMACSHA256(claveHMAC); // crea una instancia de HMAC y asigna la clave
        var firmado = hmac.ComputeHash(cifradoIV); // firma el archivo cifrado

        //combina el archivo encriptado con la firma
        var cifradoyfirmado = new byte[cifradoIV.Length + firmado.Length];
        Buffer.BlockCopy(cifradoIV, 0, cifradoyfirmado, 0, cifradoIV.Length);
        Buffer.BlockCopy(firmado, 0, cifradoyfirmado, cifradoIV.Length, firmado.Length);
        return cifradoyfirmado;
    }

    /**
        verifica la firma, descifra los datos y devuelve un json legible
    **/
    public static string Desencriptar(byte[] bytes)
    {
        var cifrado = new byte[bytes.Length - 32]; // le resta los bytes de firma 256 bits pero en bytes 32
        var firma = new byte[32];
        Buffer.BlockCopy(bytes, 0, cifrado, 0, cifrado.Length);
        Buffer.BlockCopy(bytes, cifrado.Length, firma, 0, firma.Length);

        using var hmac = new HMACSHA256(claveHMAC);
        var esperado = hmac.ComputeHash(cifrado); //calcula la firma esperada
        if (!CryptographicOperations.FixedTimeEquals(esperado, firma)) // verificacion de que la firma esperada sea igual a la firma del archivo
            throw new Exception("los datos han sido manipulados");

        using var aes = Aes.Create();
        aes.Key = claveAES;
        var ivlongitud = aes.BlockSize / 8;
        aes.IV = cifrado.Take(ivlongitud).ToArray(); // obtiene el IV con el que se ha cifrado
        var cipher = cifrado.Skip(ivlongitud).ToArray(); // separa el contenido cifrado del IV

        var json = aes.CreateDecryptor().TransformFinalBlock(cipher, 0, cipher.Length); // desencripta a json
        return Encoding.UTF8.GetString(json);
    }
}
