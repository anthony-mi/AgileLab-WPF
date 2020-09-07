using AgileLab.Data;
using AgileLab.Security;
using AgileLab.Services.Logging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;

namespace AgileLab.Services.Authentication
{
    class AuthenticationService
    {
        private IUsersDataModel _dataModel = ComponentsContainer.Get<IUsersDataModel>();
        private IHasher _hasher = ComponentsContainer.Get<IHasher>();
        private ILogger _logger = ComponentsContainer.Get<ILogger>();

        public bool LogIn(string username, SecureString password, ref string message)
        {
            string inputHash = _hasher.CalculateHash(password);
            return LogIn(username, inputHash, ref message);
        }

        public bool LogIn(string username, string passwordHash, ref string message)
        {
            try
            {
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(passwordHash))
                {
                    throw new ArgumentNullException("User log in error. Username or passwordHash is null or empty.");
                }

                if (!_dataModel.UserExists(username))
                {
                    message = $"User `{username}` doesn't exists.";
                    return false;
                }

                string fromDbHash = _dataModel.GetPasswordHashByUsername(username);

                if (string.IsNullOrEmpty(fromDbHash))
                {
                    throw new ArgumentNullException($"User log in error. Password hash of user `{username}` doesn't found.");
                }

                bool equals = Equals(fromDbHash, passwordHash);

                if (!equals)
                {
                    message = "Username and password doesn't match";
                }

                return Equals(fromDbHash, passwordHash);
            }
            catch(Exception ex)
            {
                _logger.Fatal("User log in error");
                _logger.Fatal(ex);

                throw new AuthenticationException("A technical error has occurred.");
            }
        }

        public string GetPasswordHash(SecureString password)
        {
            return _hasher.CalculateHash(password);
        }

        public static ushort PasswordMinimalLength
        {
            get
            {
                return Properties.Settings.Default.passwordMinimalLength;
            }
        }

        public void CreateNewUser(string firstName, string lastName, string username, SecureString password, SecureString retypedPassword)
        {
            try
            {
                bool areValidationMistakes = false;

                firstName = firstName.Trim();
                lastName = lastName.Trim();
                username = username.Trim();

                string validationMistakes = string.Empty;
                string tmpMessage = string.Empty;

                if (!IsValidName(firstName, ref tmpMessage))
                {
                    validationMistakes = tmpMessage;
                    areValidationMistakes = true;
                }

                if (!IsValidName(lastName, ref tmpMessage))
                {
                    validationMistakes += (areValidationMistakes) ? Environment.NewLine : string.Empty;
                    validationMistakes += tmpMessage;

                    areValidationMistakes = true;
                }

                if (!IsUsernameValid(username, ref tmpMessage))
                {
                    validationMistakes += (areValidationMistakes) ? Environment.NewLine : string.Empty;
                    validationMistakes += tmpMessage;

                    areValidationMistakes = true;
                }

                if (!IsPasswordValid(password, ref tmpMessage))
                {
                    validationMistakes += (areValidationMistakes) ? Environment.NewLine : string.Empty;
                    validationMistakes += tmpMessage;
                    areValidationMistakes = true;
                }

                if(!ArePasswordsEqual(password, retypedPassword))
                {
                    validationMistakes += (areValidationMistakes) ? Environment.NewLine : string.Empty;
                    validationMistakes += "Passwords are not equal.";
                    areValidationMistakes = true;
                }

                if (_dataModel.UserExists(username))
                {
                    string errorText = $"User `{username}` already exists.";

                    validationMistakes += (areValidationMistakes) ? Environment.NewLine : string.Empty;
                    validationMistakes += errorText;

                    areValidationMistakes = true;
                }

                if (areValidationMistakes)
                {
                    throw new AuthenticationException(validationMistakes);
                }

                try
                {
                    _dataModel.CreateNewUser(firstName, lastName, username, _hasher.CalculateHash(password));
                }
                catch (Exception ex)
                {
                    _logger.Fatal(ex);
                    throw new AuthenticationException("A technical error creating the user has occurred.");
                }
            }
            catch
            {
                throw;
            }
        }

        private bool ArePasswordsEqual(SecureString password1, SecureString password2)
        {
            string decryptedPassword1 = GetDecryptedString(password1);
            string decryptedPassword2 = GetDecryptedString(password2);

            return Equals(decryptedPassword1, decryptedPassword2);
        }

        private bool IsValidName(string name, ref string message)
        {
            bool isValid = true;

            message = string.Empty;

            if (string.IsNullOrEmpty(name))
            {
                message = "Some name is empty.";
                return false;
            }

            CharEnumerator enumerator = name.GetEnumerator();

            while (enumerator.MoveNext())
            {
                char ch = enumerator.Current;

                if (!char.IsLetter(ch) &&
                    ch != '\'' &&
                    ch != '`' &&
                    ch != '-')
                {
                    message += (isValid == false) ? Environment.NewLine : string.Empty;
                    message += $"Invalid character in name ('{ch}').";

                    isValid = false;
                }
            };

            return isValid;
        }

        private bool IsUsernameValid(string username, ref string message)
        {
            bool isValid = true;

            message = string.Empty;

            if (string.IsNullOrEmpty(username))
            {
                message = "Username is empty.";
                return false;
            }

            CharEnumerator enumerator = username.GetEnumerator();

            while (enumerator.MoveNext())
            {
                char ch = enumerator.Current;

                if (!char.IsLetter(ch) &&
                    !char.IsNumber(ch) && 
                    ch != '_')
                {
                    message += (isValid == false) ? Environment.NewLine : string.Empty;
                    message += $"Invalid character in username ('{ch}').";

                    isValid = false;
                }
            };

            return isValid;
        }

        private bool IsPasswordValid(SecureString password, ref string message)
        {
            bool isValid = true;

            string decryptedPassword = GetDecryptedString(password);

            message = string.Empty;

            if (decryptedPassword.Length < PasswordMinimalLength)
            {
                message = $"Password length must be equal or greater than {PasswordMinimalLength}.";
                isValid = false;
            }

            if(!decryptedPassword.Any(char.IsLower))
            {
                message += (isValid == false) ? Environment.NewLine : string.Empty;
                message += $"Password must include lowercase characters.";
                isValid = false;
            }

            if (!decryptedPassword.Any(char.IsUpper))
            {
                message += (isValid == false) ? Environment.NewLine : string.Empty;
                message += $"Password must include uppercase characters.";
                isValid = false;
            }

            if (!decryptedPassword.Any(char.IsNumber))
            {
                message += (isValid == false) ? Environment.NewLine : string.Empty;
                message += $"Password must include number characters.";
                isValid = false;
            }

            return isValid;
        }

        private string GetDecryptedString(SecureString password)
        {
            IntPtr stringPointer = Marshal.SecureStringToBSTR(password);
            string str = string.Empty;

            try
            {
                str = Marshal.PtrToStringBSTR(stringPointer);
            }
            finally
            {
                Marshal.ZeroFreeBSTR(stringPointer);
            }

            return str;
        }
    }
}
