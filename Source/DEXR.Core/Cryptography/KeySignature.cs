using System;
using System.Globalization;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace DEXR.Core.Cryptography
{
    public class KeySignature
    {
        public static PublicPrivateKeyPair GenerateKeyPairs()
        {
            string privateKey = GeneratePrivateKey();
            string publicKey = GetPublicKey(privateKey);

            return new PublicPrivateKeyPair()
            {
                PrivateKey = privateKey,
                PublicKey = publicKey
            };
        }

        public static string GeneratePrivateKey()
        {
            //return Guid.NewGuid().ToString().Replace("-", "");
            System.Threading.Thread.Sleep(50);
            Random rand = new Random();
            return 
                rand.Next(10000000, 99999999).ToString() + 
                Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds) + 
                rand.Next(10000000, 99999999).ToString();
        }

        public static string GetPublicKey(string privateKey)
        {
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
            
            var d = new Org.BouncyCastle.Math.BigInteger(privateKey);
            var q = domain.G.Multiply(d);

            var publicKey = new ECPublicKeyParameters(q, domain);
            return Base58Encoding.Encode(publicKey.Q.GetEncoded());
        }

        private static string GetPublicKeyHex(string privateKey)
        {
            var p = BigInteger.Parse("0FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFEFFFFFC2F", NumberStyles.HexNumber);
            var b = (BigInteger)7;
            var a = BigInteger.Zero;
            var Gx = BigInteger.Parse("79BE667EF9DCBBAC55A06295CE870B07029BFCDB2DCE28D959F2815B16F81798", NumberStyles.HexNumber);
            var Gy = BigInteger.Parse("483ADA7726A3C4655DA4FBFC0E1108A8FD17B448A68554199C47D08FFB10D4B8", NumberStyles.HexNumber);

            CurveFp curve256 = new CurveFp(p, a, b);
            Point generator256 = new Point(curve256, Gx, Gy);

            var secret = BigInteger.Parse(privateKey, NumberStyles.HexNumber);
            var pubkeyPoint = generator256 * secret;
            return pubkeyPoint.X.ToString("X") + pubkeyPoint.Y.ToString("X");
        }

        public static string Sign(string privateKey, string content)
        {
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            var keyParameters = new
                    ECPrivateKeyParameters(new Org.BouncyCastle.Math.BigInteger(privateKey),
                    domain);

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");

            signer.Init(true, keyParameters);
            signer.BlockUpdate(Encoding.ASCII.GetBytes(content), 0, content.Length);
            var signature = signer.GenerateSignature();
            return Base58Encoding.Encode(signature);
        }

        public static bool VerifySignature(string publicKey, string content, string signature)
        {
            var curve = SecNamedCurves.GetByName("secp256k1");
            var domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

            var publicKeyBytes = Base58Encoding.Decode(publicKey);

            var q = curve.Curve.DecodePoint(publicKeyBytes);

            var keyParameters = new
                    Org.BouncyCastle.Crypto.Parameters.ECPublicKeyParameters(q,
                    domain);

            ISigner signer = SignerUtilities.GetSigner("SHA-256withECDSA");

            signer.Init(false, keyParameters);
            signer.BlockUpdate(Encoding.ASCII.GetBytes(content), 0, content.Length);

            var signatureBytes = Base58Encoding.Decode(signature);

            return signer.VerifySignature(signatureBytes);
        }
    }
}
