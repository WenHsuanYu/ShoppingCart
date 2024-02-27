using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Reflection;

namespace ShoppingCartUI.Utilities
{
    public static class FileHelpers
    {
        public static async Task<byte[]> ProcessFormFile<T>(IFormFile? formFile,
            ModelStateDictionary modelState)
        {
            
            if (formFile == null)
            {
                return [];
            }
            
            var fieldDisplayName = string.Empty;
            // Use reflection to obtain the display name for the model
            // property associated with this IFormFile. If a display
            // name isn't found, error messages simply won't show
            // a display name.
            MemberInfo? property = typeof(T).GetProperty(formFile.Name[
                (formFile.Name.IndexOf('.', StringComparison.Ordinal) + 1)..]);
            if (property is not null)
            {
                if (property.GetCustomAttribute(typeof(DisplayAttribute)) is
                    DisplayAttribute displayAttribute)
                {
                    fieldDisplayName = $"{displayAttribute.Name} ";
                }
            }

            // Don't trust the file name sent by the client. To display
            // the file name, HTML-encode the value.
            var trustedFileNameForDisplay = WebUtility.HtmlEncode(
                formFile.FileName);

            // Check the file length. This check doesn't catch files that only have
            // a BOM as their content.
            if (formFile.Length == 0)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");

                return [];
            }

            try
            {
                using var memoryStream = new MemoryStream();
                await formFile.CopyToAsync(memoryStream);

                // Check the content length in case the file's only
                // content was a BOM and the content is actually
                // empty after the BOM was removed.
                if (memoryStream.Length == 0)
                {
                    modelState.AddModelError(formFile.Name,
                        $"{fieldDisplayName}({trustedFileNameForDisplay}) is empty.");
                }
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                modelState.AddModelError(formFile.Name,
                    $"{fieldDisplayName}({trustedFileNameForDisplay}) upload failed. " +
                    $"Please contact the Help Desk for support. Error: {ex.HResult}");
            }

            return [];
        }
    }
}