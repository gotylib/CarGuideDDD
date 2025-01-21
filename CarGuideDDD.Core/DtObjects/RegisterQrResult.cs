

using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Core.DtObjects
{
    public class RegisterQrResult
    {
        public IActionResult? ActionResults {  get; set; }
        public MemoryStream? QrCodeStream { get; set; }


    }
}
