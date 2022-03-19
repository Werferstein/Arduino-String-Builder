/*
    Copyright (c) 2022 
    Ingolf Hill, Zum Werferstein 36, DE-51570 Windeck-Werfen, i.hill@werferstein.org
    
    Permission is hereby granted, free of charge, to any person obtaining
    a copy of this software and associated documentation files (the "Software"),
    to deal in the Software without restriction, including without limitation
    the rights to use, copy, modify, merge, publish, distribute, sublicense,
    and/or sell copies of the Software, and to permit persons to whom the Software
    is furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
    EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
    OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
    IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
    CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
    TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE
    OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;

namespace CPP_Arduino_String_Builder
{

    #region Speichern und Laden der App-Daten

    /// <summary>
    /// Klasse zum Speichern der App-Daten über XML, die erbt sich selber (Generic Object)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AppSettings<T> where T : new()
    {
        const string CONST_DefaultConfigFilename = "NewProject.aduword";

        /// <summary>
        /// XML Konstruktor der Klasse
        /// </summary>
        public AppSettings()
        { }
        /// <summary>
        /// Save to xml file
        /// </summary>
        /// <param name="fileName"></param>
        public bool Save(out string error, string fileName = CONST_DefaultConfigFilename)
        {
            if (CONST_DefaultConfigFilename == string.Empty) fileName = CONST_DefaultConfigFilename;
            return AppXML.SaveToXML(out error, fileName, false, this);
        }
        /// <summary>
        /// Save to xml file
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public bool Save(string fileName = CONST_DefaultConfigFilename)
        {            
            return AppXML.SaveToXML(error: out _, path: fileName, Compression: false, newObject: this);
        }




        /// <summary>
        /// Load from XML
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:", Justification = "<>")]
        public static T Load(out string error, string fileName = CONST_DefaultConfigFilename)
        {
            error = string.Empty;

            T t = new T();

            if (File.Exists(fileName))
                t = (T)AppXML.LoadFromXML(out error, fileName, false, typeof(T));
            if (t == null) t = new T();
            return t;
        }


        /// <summary>
        /// Load from XML
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1000:")]
        public static T Load(string fileName = CONST_DefaultConfigFilename)
        {
            return Load(out _, fileName);
        }
    }

    #endregion

    public static class AppXML
    {
        /// <summary>
        /// Buffer size for zip
        /// </summary>
        private const int ZIP_BUFFER_SIZE = 1048576;

        #region Get zip original fileSize
        /// <summary>
        /// Extracts the original filesize of the compressed file.
        /// </summary>
        /// <param name="fi">GZip file to handle</param>
        /// <returns>Size of the compressed file, when its decompressed.</returns>
        /// <remarks>More information at <a href="http://tools.ietf.org/html/rfc1952">http://tools.ietf.org/html/rfc1952</a> section 2.3</remarks>
        public static int GetGzOriginalFileSize(FileInfo fi)
        {
            if (fi == null) throw new Exception("GetGzOriginalFileSize(FileInfo fi) fi is null!");
            int filesize = -1;
            try
            {


                FileStream fs;
                using (fs = fi.OpenRead())
                {

                    byte[] fh = new byte[3];
                    fs.Read(fh, 0, 3);
                    if (fh[0] == 31 && fh[1] == 139 && fh[2] == 8) //If magic numbers are 31 and 139 and the deflation id is 8 then...
                    {
                        byte[] ba = new byte[4];
                        fs.Seek(-4, SeekOrigin.End);
                        fs.Read(ba, 0, 4);
                        filesize = BitConverter.ToInt32(ba, 0);
                    }
                    //fs.Close();
                }
            }
            catch { return -1; }
            //finally
            //{
            //    if (fs != null)
            //        fs.Dispose();
            //}

            return filesize;
        }
        #endregion

        #region Load and Save XML
        public static bool SaveToXML(out string error, string path, bool Compression, object newObject)
        {
            error = string.Empty;
            if (newObject == null) { error = "ERROR: Object == null"; return false; }


            try
            {
                if (Compression)
                {
                    XmlSerializer serializer = new XmlSerializer(newObject.GetType());
                    MemoryStream stream = new MemoryStream();
                    serializer.Serialize(stream, newObject);

                    //die unkomprimierte Größe der Datei ermitteln
                    long originalFileSize = stream.Position;
                    if (originalFileSize < 0) originalFileSize = 0;

                    stream.Position = 0;
                    FileStream fileStream = File.Create(path);

                    GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress);
                    byte[] buffer = new byte[ZIP_BUFFER_SIZE];

                    int numRead;
                    long bufferLength = 0;

                    while ((numRead = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        bufferLength += buffer.Length;
                        gzipStream.Write(buffer, 0, numRead);
                    }
                    gzipStream.Dispose();
                    //fileStream.Dispose();
                }
                else
                {

                    Type ntype = newObject.GetType();
                    XmlSerializer serializer = new XmlSerializer(newObject.GetType());
                    FileStream fs = new FileStream(path, FileMode.Create);
                    serializer.Serialize(fs, newObject);
                    fs.Close();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message + " ->" + ex.InnerException;
                Console.WriteLine(error);
                return false;
            }
            return true;
        }


        public static object LoadFromXML(out string error, string path, bool Compression, Type objectType)
        {
            object result = null;
            FileStream fs = null;
            try
            {
                if (Compression)
                {
                    //die unkomprimierte Größe der Datei ermitteln
                    long originalFileSize = GetGzOriginalFileSize(new FileInfo(path));
                    if (originalFileSize < 0) originalFileSize = 0;

                    FileStream compressedStream = new FileInfo(path).OpenRead();

                    MemoryStream destinationStream = new MemoryStream();
                    GZipStream gzipStream = new GZipStream(compressedStream, CompressionMode.Decompress);

                    byte[] buffer = new byte[ZIP_BUFFER_SIZE];

                    int numRead;

                    while ((numRead = gzipStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        destinationStream.Capacity += numRead;
                        destinationStream.Write(buffer, 0, numRead);
                    }

                    gzipStream.Dispose();
                    //compressedStream.Dispose();

                    destinationStream.Position = 0;

                    XmlSerializer deserializer = new XmlSerializer(objectType);
                    result = deserializer.Deserialize(destinationStream);
                }
                else
                {
                    XmlSerializer serializer = new XmlSerializer(objectType);
                    fs = new FileStream(path, FileMode.Open);
                    result = serializer.Deserialize(fs);
                    fs.Close();
                    fs = null;
                }
                error = string.Empty;
            }
            catch (Exception ex)
            {
                error = ex.Message + " ->" + ex.InnerException;
                Console.WriteLine(error);

                if (fs != null)
                {
                    fs.Close();
                }
            }

            return result;
        }
        #endregion

        #region Encrypt / Decrypt

        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 256;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        public static string Encrypt(string plainText, string passPhrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                //memoryStream.Close();
                                //cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            byte[] plainTextBytes = null;
            Int32 decryptedByteCount = -1;

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;

                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                plainTextBytes = new byte[cipherTextBytes.Length];
                                decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                //cryptoStream.Close();
                                //cryptoStream.Dispose();
                            }
                            //memoryStream.Close();
                            //memoryStream.Dispose();
                        }
                    }
                    //symmetricKey.Dispose();                    
                }
                //password.Dispose();
            }

            if (plainTextBytes != null) return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
            return string.Empty;
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
        #endregion

        #region De/Serialize
        public static T Deserialize<T>(this string toDeserialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StringReader textReader = new StringReader(toDeserialize);
                return (T)xmlSerializer.Deserialize(textReader);
            }

            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }

        public static string Serialize<T>(this T toSerialize)
        {
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                StringWriter textWriter = new StringWriter();
                xmlSerializer.Serialize(textWriter, toSerialize);
                return textWriter.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                throw new Exception(ex.Message);
            }
        }
        #endregion

    }




//    #region XML
//    public static class XmlHelper
//    {
//        #region Load / Save
//        public static Project Load(string path, string key = "")
//        {
//            if (string.IsNullOrEmpty(path) || !System.IO.File.Exists(path))
//            {
//                return null;
//            }

//            try
//            {
//                Project t = null;
//                if (!string.IsNullOrWhiteSpace(key))
//                {
//                    string con = System.IO.File.ReadAllText(path, Encoding.UTF8);
//                    con = AppXML.Decrypt(con, key);
//                    t = AppXML.Deserialize<Project>(con);
                    
//                    return t;
//                }
//                t = AppXML.Deserialize<Project>(System.IO.File.ReadAllText(path, Encoding.UTF8));
                
//                return t;
//            }
//#if DEBUG
//            catch (Exception ex)
//            {
//                Console.WriteLine("ERROR: " + ex.Message);
//                return null;
//            }
//#else
//        catch { return null; }
//#endif
//        }
//        public static bool Save(Project codeControlFormat, string path, string key = "")
//        {
//            if (codeControlFormat == null || !System.IO.Directory.Exists(Path.GetDirectoryName(path)))
//            {
//                return false;
//            }

//            try
//            {
//                if (!string.IsNullOrWhiteSpace(key))
//                {
//                    string con = AppXML.Serialize<Project>(codeControlFormat);
//                    con = AppXML.Encrypt(con, key);
//                    System.IO.File.WriteAllText(path, con, Encoding.UTF8);
//                }
//                else
//                {
//                    System.IO.File.WriteAllText(path, AppXML.Serialize<Project>(codeControlFormat), Encoding.UTF8);
//                }
//                return true;
//            }
//#if DEBUG
//            catch (Exception ex)
//            {
//                Console.WriteLine("ERROR: " + ex.Message);
//                return false;
//            }
//#else
//        catch { return false; }
//#endif
//        }
//        #endregion
//    }
//    #endregion
}
