using System.Security.Cryptography;

namespace Bewirtung.Webserver.Data
{
    /// <summary>
    /// Dienst zum sicheren Hashen und Verifizieren von Passwörtern (PBKDF2 mit Salt).
    /// Das Ergebnis wird als "iterationen.salt.hash" (Base64) gespeichert.
    /// </summary>
    public class PasswordHasher
    {
        private const int SaltSize = 16;      // 128 Bit
        private const int KeySize = 32;       // 256 Bit
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

        /// <summary>
        /// Erzeugt einen verschlüsselten (gehashten) Wert für das übergebene Passwort.
        /// </summary>
        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(SaltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// Prüft, ob das übergebene Passwort zum gespeicherten Hash passt.
        /// </summary>
        public bool Verify(string password, string? gespeichert)
        {
            if (string.IsNullOrEmpty(gespeichert))
            {
                return false;
            }

            var teile = gespeichert.Split('.', 3);
            if (teile.Length != 3 || !int.TryParse(teile[0], out var iterationen))
            {
                return false;
            }

            try
            {
                var salt = Convert.FromBase64String(teile[1]);
                var erwartet = Convert.FromBase64String(teile[2]);
                var aktuell = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterationen, Algorithm, erwartet.Length);
                return CryptographicOperations.FixedTimeEquals(aktuell, erwartet);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Prüft, ob ein gespeicherter Wert bereits ein gültiger PBKDF2-Hash im erwarteten Format ist.
        /// </summary>
        public static bool IstGehasht(string? wert)
        {
            if (string.IsNullOrEmpty(wert))
            {
                return false;
            }

            var teile = wert.Split('.', 3);
            return teile.Length == 3 && int.TryParse(teile[0], out _);
        }
    }
}
