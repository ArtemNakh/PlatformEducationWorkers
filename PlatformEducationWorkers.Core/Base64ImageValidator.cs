using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlatformEducationWorkers.Core
{
    public class Base64ImageValidator
    {
        public static bool IsBase64Image(string base64String)
        {
            if (string.IsNullOrEmpty(base64String))
                return false;

            try
            {
                // Відділяємо частину base64 від префікса "data:image..."
                var base64Data = base64String.Substring(base64String.IndexOf(',') + 1);

                // Пробуємо декодувати base64
                byte[] imageBytes = Convert.FromBase64String(base64Data);

               
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
