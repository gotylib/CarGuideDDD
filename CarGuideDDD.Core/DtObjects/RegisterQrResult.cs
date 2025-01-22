
using CarGuideDDD.Core.AnswerObjects;
using CarGuideDDD.Core.EntityObjects.Interfaces;


namespace CarGuideDDD.Core.DtObjects
{
    public class RegisterQrResult<T, E, R>
        where T : IEntity
        where E : Exception
        where R : IEntity
    {
        public ServiceResult<T, E, R>? ActionResults { get; set; }
        public MemoryStream? QrCodeStream { get; set; }
    }

}
