

using CarGuideDDD.Core.AnswerObjects;
using Microsoft.AspNetCore.Mvc;

namespace CarGuideDDD.Core.DtObjects
{
    public class RegisterQrResult
    {
        public ServiceResult? ActionResults {  get; set; }
        public MemoryStream? QrCodeStream { get; set; }


    }
}
