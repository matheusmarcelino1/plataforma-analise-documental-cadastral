namespace AnaliseDocumental.Contratos.Eventos;

public sealed record DocumentoCadastralRecebidoV1(
    Guid EventoId,
    Guid CadastroId,
    Guid DocumentoId,
    string Bucket,
    string ChaveS3,
    string CorrelationId,
    DateTimeOffset OcorreuEm
);