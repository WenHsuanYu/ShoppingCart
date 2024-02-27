using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using static System.Net.Mime.MediaTypeNames;
namespace ShoppingCartUI.Attributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        public string GetErrorMessage()
        {
            return $"This photo extension is not allowed!";
        }
        protected override ValidationResult? IsValid(
                object? value, ValidationContext validationContext)
        {
            IFormFile? file = value as IFormFile;
            bool flag = false;
            if (file != null)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    return new ValidationResult(GetErrorMessage());
                }
                //Check file signature (Byte Order Mark)
                using var memoryStream = new MemoryStream();
                file.CopyTo(memoryStream);
                byte[] fileData = memoryStream.ToArray();
                List<byte[]> sig = fileSignature[extension.ToUpper()];
                
                foreach (byte[] b in sig)
                {
                    var curFileSig = new byte[b.Length];
                    Array.Copy(fileData, curFileSig, b.Length);
                    if (curFileSig.SequenceEqual(b))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag == true ? ValidationResult.Success : new ValidationResult(GetErrorMessage());
        }

        protected static Dictionary<string, List<byte[]>> fileSignature = new Dictionary<string, List<byte[]>>
        {
            { ".PNG", new List<byte[]> 
                                    { 
                                        new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } 
                                    }
            },
            { ".JPG", new List<byte[]>
                                    {
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
                                    }
            },
            { ".JPEG", new List<byte[]>
                                    {
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                                        new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
                                    }
            },
            { ".WEBP", new List<byte[]>

                                    {
                                        new byte[] { 0x52, 0x49, 0x46, 0x46 },
                                        new byte[] { 0x57, 0x45, 0x42, 0x50 }
                                    }
            }
        };
    }
}